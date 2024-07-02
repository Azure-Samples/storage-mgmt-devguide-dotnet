using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage.Models;
using Azure.ResourceManager.Storage;
using Azure;
using Azure.ResourceManager;
using Azure.Core;

namespace StorageAccountManagement
{
    public static class ManagementTasks
    {
        public static async Task<StorageAccountResource> CreateStorageAccount(
            ResourceGroupResource resourceGroup,
            string storageAccountName)
        {
            // Define the settings for the storage account
            StorageAccountCreateOrUpdateContent parameters = GetStorageAccountParameters();

            // Create a storage account with defined account name and settings
            StorageAccountCollection accountCollection = resourceGroup.GetStorageAccounts();
            ArmOperation<StorageAccountResource> acccountCreateOperation = await accountCollection.CreateOrUpdateAsync(WaitUntil.Completed, storageAccountName, parameters);
            StorageAccountResource storageAccount = acccountCreateOperation.Value;

            return storageAccount;
        }

        public static StorageAccountCreateOrUpdateContent GetStorageAccountParameters()
        {
            AzureLocation location = AzureLocation.WestUS;
            StorageSku sku = new StorageSku(StorageSkuName.StandardGrs);
            StorageKind kind = StorageKind.StorageV2;
            bool allowSharedKeyAccess = false;

            StorageAccountCreateOrUpdateContent parameters = new StorageAccountCreateOrUpdateContent(sku, kind, location);

            return parameters;
        }

        public static async Task GetStorageAccountsInResourceGroup(ResourceGroupResource resourceGroup)
        {
            Console.WriteLine($"List of storage accounts in {resourceGroup.Id.Name}:");
            await foreach (StorageAccountResource storAcct in resourceGroup.GetStorageAccounts())
            {
                Console.WriteLine($"\t{storAcct.Id.Name}");
            }
        }

        public static async Task GetStorageAccountsForSubscription(SubscriptionResource subscription)
        {
            AsyncPageable<StorageAccountResource> storAcctsSub = subscription.GetStorageAccountsAsync();
            Console.WriteLine($"List of storage accounts in subscription {subscription.Get().Value.Data.DisplayName}:");
            await foreach (StorageAccountResource storAcctSub in storAcctsSub)
            {
                Console.WriteLine($"\t{storAcctSub.Id.Name}");
            }
        }

        public static async Task UpdateStorageAccountSkuAsync(StorageAccountResource storageAccount, StorageAccountCollection accountCollection)
        {
            Console.WriteLine("Updating storage account...");
            // Update storage account sku
            var currentSku = storageAccount.Get().Value.Data.Sku.Name;  // capture the current Sku value before updating
            StorageSku updateSku = new StorageSku(StorageSkuName.StandardLrs);
            StorageAccountCreateOrUpdateContent updateParams = new StorageAccountCreateOrUpdateContent(updateSku, kind, location);
            await accountCollection.CreateOrUpdateAsync(WaitUntil.Completed, storAccountName, updateParams);
            Console.WriteLine($"Sku on storage account updated from {currentSku} to {storageAccount.Get().Value.Data.Sku.Name}");
        }
    }
}
