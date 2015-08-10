namespace Configuration.Interfaces
{
    public interface IWebConfigSettings
    {
        string SubscriptionId { get; set; }
        string ManagementCertContents { get; set; }
        string CloudServiceName { get; set; }
        string ServiceConfigurationNamespace { get; set; }
    }
}