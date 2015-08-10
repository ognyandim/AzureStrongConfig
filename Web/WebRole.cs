using System.Linq;
using System.Reflection;
using Microsoft.WindowsAzure.ServiceRuntime;

namespace Web
{
    public class WebRole : RoleEntryPoint
    {
        public override bool OnStart()
        {
            var assLocation = Assembly.GetExecutingAssembly().Location; // TODO locate config and try editing it in order to get event fired
            RoleEnvironment.Changing += RoleEnvironmentOnChanging;
            // For information on handling configuration changes
            // see the MSDN topic at http://go.microsoft.com/fwlink/?LinkId=166357.

            return base.OnStart();
        }

        private void RoleEnvironmentOnChanging(object sender, RoleEnvironmentChangingEventArgs roleEnvironmentChangingEventArgs)
        {
            // In order to handle changing configuration
            // this can be avoided if the config is not "singleton" but it is recreated via this event
            // https://msdn.microsoft.com/en-us/library/azure/gg432963.aspx
            // https://alexandrebrisebois.wordpress.com/2013/09/29/handling-cloud-service-role-configuration-changes-in-windows-azure/


            var configurationChanges = roleEnvironmentChangingEventArgs.Changes
                                    .OfType<RoleEnvironmentConfigurationSettingChange>()
                                    .ToList();

            if (!configurationChanges.Any())
            {
                return;
            }

            // TODO for specific settings which are uesd to instantiate any cached objects
            //if(configurationChanges.Any(c => c.ConfigurationSettingName == "StorageAccount"))
            roleEnvironmentChangingEventArgs.Cancel = true;
        
        }
    }
}
