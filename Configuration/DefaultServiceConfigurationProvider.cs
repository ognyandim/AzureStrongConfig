using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Xml;
using System.Xml.Linq;
using Configuration.Interfaces;
using Microsoft.Azure;
using Microsoft.WindowsAzure.Management.Compute;
using Microsoft.WindowsAzure.Management.Compute.Models;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Configuration
{
    public class DefaultServiceConfigurationProvider : IServiceConfigurationProvider
    {
        static string subscriptionId = ConfigurationManager.AppSettings["subscriptionId"];// your subscription-id
        // The Base64 Encoded Management Certificate string from Azure Publish Settings file 
        // download from https://manage.windowsazure.com/publishsettings/index
        static string managementCertContents = ConfigurationManager.AppSettings["managementCertContents"];
        static string cloudServiceName = ConfigurationManager.AppSettings["cloudServiceName"];// "<your cloud service name>"
        static string ns = "http://schemas.microsoft.com/ServiceHosting/2008/10/ServiceConfiguration";

        public Dictionary<string, Dictionary<string, string>> GetConfig()
        {
            var configuration = new Dictionary<string, Dictionary<string, string>>();
            var configXml = GetConfigXml();
            var roles = configXml.Descendants(XName.Get("Role", ns));

            foreach(var role in roles)
            {
                var roleConfiguration = new Dictionary<string, string>();
                var roleName = role.Attribute("name").Value;
                var configurationSettings = role.Element(XName.Get("ConfigurationSettings", ns));
                foreach(var element in configurationSettings.Elements(XName.Get("Setting", ns)))
                {
                    var settingName = element.Attribute("name").Value;
                    var settingValue = element.Attribute("value").Value;
                    roleConfiguration.Add(settingName, settingValue);
                }
                configuration.Add(roleName, roleConfiguration);
            }
            return configuration;
        }

        private static XElement GetConfigXml()
        {
            XElement configXml = null;

            if(!RoleEnvironment.IsAvailable/*as local web project*/ || RoleEnvironment.IsEmulated /*as azure emulator project*/)
            {
                var localConfigFile =
                    new System.IO.DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory).Parent.EnumerateFiles(
                        "*Local.cscfg", System.IO.SearchOption.AllDirectories).FirstOrDefault();
                XmlDocument doc = new XmlDocument();
                doc.Load(localConfigFile.FullName);
                configXml = XElement.Parse(doc.InnerXml);
            }
            else
            {
                var managementCertificate = new X509Certificate2(Convert.FromBase64String(managementCertContents));
                var credentials = new CertificateCloudCredentials(subscriptionId, managementCertificate);

                var computeManagementClient = new ComputeManagementClient(credentials);
                var response = computeManagementClient.HostedServices.GetDetailed(cloudServiceName);
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