using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Linq;
using Castle.Components.DictionaryAdapter;
using Configuration.Interfaces;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Management.Compute;
using Microsoft.WindowsAzure.Management.Compute.Models;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Configuration
{
    public class DefaultAzureServiceConfigurationProvider : IAzureServiceConfigurationProvider
    {
        private readonly string _subscriptionId;
        // The Base64 Encoded Management Certificate string from Azure Publish Settings file 
        // download from https://manage.windowsazure.com/publishsettings/index
        private readonly string _managementCertContents;
        private readonly string _cloudServiceName;
        private readonly string _serviceConfigurationNamespace;

        // We use webConfig to 
        // (1) to be able to use in test env. with only web instance with no azure emulator
        // (2) to hide some credentials in outside file in web.config
        public DefaultAzureServiceConfigurationProvider(IWebConfigSettings webConfigSettings)
        {
            _subscriptionId = webConfigSettings.SubscriptionId;
            _managementCertContents = webConfigSettings.ManagementCertContents;
            _cloudServiceName = webConfigSettings.CloudServiceName;
            _serviceConfigurationNamespace = webConfigSettings.ServiceConfigurationNamespace;
        }

        public Dictionary<string, Dictionary<string, string>> GetConfigRaw()
        {
            var configuration = new Dictionary<string, Dictionary<string, string>>();
            var configXml = GetConfigXml();
            var roles = configXml.Descendants(XName.Get("Role", _serviceConfigurationNamespace));

            foreach(var role in roles)
            {
                var roleConfiguration = new Dictionary<string, string>();
                var roleName = role.Attribute("name").Value;
                var configurationSettings = role.Element(XName.Get("ConfigurationSettings", _serviceConfigurationNamespace));
                foreach(var element in configurationSettings.Elements(XName.Get("Setting", _serviceConfigurationNamespace)))
                {
                    var settingName = element.Attribute("name").Value;
                    var settingValue = element.Attribute("value").Value;
                    roleConfiguration.Add(settingName, settingValue);
                }
                configuration.Add(roleName, roleConfiguration);
            }
            return configuration;
        }

        public IAzureServiceConfiguration GetConfig()
        {
            var configFactory = new DictionaryAdapterFactory();
            IAzureServiceConfiguration config;
            try
            {

                var rawAzureServiceConfig = GetConfigRaw();
                var rawAzureWebServiceConfig = rawAzureServiceConfig["Core.Web"];
                config = configFactory.GetAdapter<IAzureServiceConfiguration>(rawAzureWebServiceConfig);
                config = ComplementConfigurationFromConfigurationManager(config);
            }
            catch(Exception exception)
            {
                // happens in some projects when using Full Emulator
                // so we fallback to cloudconfigurationmanager
                // this is not bad since we have isolated it in configuration assembly

                Hashtable hashConfig = GetConfigFromConfigurationManager();
                config = configFactory.GetAdapter<IAzureServiceConfiguration>(hashConfig);
            }

            return config;
        }


        private IAzureServiceConfiguration ComplementConfigurationFromConfigurationManager(IAzureServiceConfiguration config)
        {
            Trace.WriteLine("Complementing configuration");
            var azureConfigType = config.GetType();
            foreach(PropertyInfo property in config.GetType().GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly))
            {
                var xmlConfigValue = CloudConfigurationManager.GetSetting(property.Name);

                var liveConfigPropValue = (string)azureConfigType.GetProperty(property.Name).GetValue(config, null);

                if(string.IsNullOrEmpty(liveConfigPropValue))
                {
                    Trace.WriteLine(property.Name + " in live config is empty. Complementing with '" + xmlConfigValue + "' from ConfigurationManager.");
                    property.SetValue(config, xmlConfigValue);
                }
                // do something with the property
            }

            return config;
        }

        private Hashtable GetConfigFromConfigurationManager()
        {
            Hashtable hashConfig = new Hashtable();
            var configProperties = typeof(IAzureServiceConfiguration).GetProperties();

            foreach(PropertyInfo prop in configProperties)
            {
                hashConfig.Add(prop.Name, CloudConfigurationManager.GetSetting(prop.Name));
            }
            return hashConfig;
        }

        private XElement GetConfigXml()
        {
            XElement configXml = null;

            if(!RoleEnvironment.IsAvailable/*as local web project*/ || RoleEnvironment.IsEmulated /*as azure emulator project*/)
            {
                try
                {
                    var localConfigFile =
                        new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.EnumerateFiles(
                            "*Local.cscfg", SearchOption.AllDirectories).FirstOrDefault();
                    XmlDocument doc = new XmlDocument();
                    doc.Load(localConfigFile.FullName);
                    configXml = XElement.Parse(doc.InnerXml);
                }
                catch(Exception) // happens in some projects when using Full Emulator
                {
                    throw; // intended - just marking - will catch it above
                }
            }
            else
            {
                var managementCertificate = new X509Certificate2(Convert.FromBase64String(_managementCertContents));
                var credentials = new CertificateCloudCredentials(_subscriptionId, managementCertificate);

                var computeManagementClient = new ComputeManagementClient(credentials);
                var response = computeManagementClient.HostedServices.GetDetailed(_cloudServiceName);
                var deployment = response.Deployments.FirstOrDefault(d => d.DeploymentSlot == DeploymentSlot.Production);
                if(deployment != null)
                {
                    var config = deployment.Configuration;
                    configXml = XElement.Parse(config);
                }
            }
            return configXml;
        }
    }
}