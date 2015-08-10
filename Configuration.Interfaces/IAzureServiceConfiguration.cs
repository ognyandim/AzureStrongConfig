namespace Configuration.Interfaces
{
    public interface IAzureServiceConfiguration
    {
        string SubscriptionId { get; set; }
        string BlobConnectionString { get; set; }
        string ServiceBusConnectionString { get; set; }

        string CloudStorageAccountKey { get; set; }
        string CloudStorageAccountName { get; set; }

        string CommonBlobContainer { get; set; }
    }
}