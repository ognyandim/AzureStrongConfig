# AzureStrongConfig
Azure Service With Strong Typed Configuration via Castle Dicitonary Adapter

How to use : 

1. Define configuration interfaces like 

`namespace Configuration.Interfaces
{
    public interface IAzureServiceConfiguration
    {
        string SubscriptionId { get; set; }
        string StorageConnectionString { get; set; }
        string ServiceBusConnectionString { get; set; }

        string CloudStorageAccountKey { get; set; }
        string CloudStorageAccountName { get; set; }

        string CommonBlobContainer { get; set; }
    }
}`
