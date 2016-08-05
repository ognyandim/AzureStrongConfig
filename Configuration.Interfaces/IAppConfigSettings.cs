namespace Configuration.Interfaces
{
    public interface IAppConfigSettings
    {
        string SubscriptionId { get; set; }
        string ManagementCertContents { get; set; }
        string CloudServiceName { get; set; }
        string ServiceConfigurationNamespace { get; set; }
    }
}