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
        public static async Task RegisterSRPInSubscription(SubscriptionResource subscription)
        {
            ResourceProviderResource resourceProvider =
                await subscription.GetResourceProviderAsync("Microsoft.Storage");

            // Check the registration state of the resource provider and register, if needed
            if (resourceProvider.Data.RegistrationState == "NotRegistered")
                resourceProvider.Register();
        }

        public static async Task<StorageAccountResource> CreateStorageAccount(
            ResourceGroupResource resourceGroup,
            string storageAccountName)
        {
            // Define the settings for the storage account
            StorageAccountCreateOrUpdateContent parameters = GetStorageAccountParameters();

            // Create a storage account with defined account name and settings
            StorageAccountCollection accountCollection = resourceGroup.GetStorageAccounts();
            ArmOperation<StorageAccountResource> acccountCreateOperation = 
                await accountCollection.CreateOrUpdateAsync(WaitUntil.Completed, storageAccountName, parameters);
            StorageAccountResource storageAccount = acccountCreateOperation.Value;

            return storageAccount;
        }

        public static StorageAccountCreateOrUpdateContent GetStorageAccountParameters()
        {
            AzureLocation location = AzureLocation.EastUS;
            StorageSku sku = new(StorageSkuName.StandardLrs);
            StorageKind kind = StorageKind.StorageV2;

            StorageAccountCreateOrUpdateContent parameters = new(sku, kind, location)
            {
                AccessTier = StorageAccountAccessTier.Cool,
                AllowSharedKeyAccess = false,
            };

            return parameters;
        }

        public static async Task ListStorageAccountsInResourceGroup(ResourceGroupResource resourceGroup)
        {
            await foreach (StorageAccountResource storageAcct in resourceGroup.GetStorageAccounts())
            {
                Console.WriteLine($"\t{storageAcct.Id.Name}");
            }
        }

        public static async Task ListStorageAccountsForSubscription(SubscriptionResource subscription)
        {
            await foreach (StorageAccountResource storageAcctSub in subscription.GetStorageAccountsAsync())
            {
                Console.WriteLine($"\t{storageAcctSub.Id.Name}");
            }
        }

        public static async Task ListStorageAccountKeysAsync(StorageAccountResource storageAccount)
           {
            AsyncPageable<StorageAccountKey> acctKeys = storageAccount.GetKeysAsync();
            await foreach (StorageAccountKey key in acctKeys)
            {
                Console.WriteLine($"\tKey name: {key.KeyName}");
                Console.WriteLine($"\tKey value: {key.Value}");
            }
        }

        public static async Task RegenerateStorageAccountKey(StorageAccountResource storageAccount)
        {
            StorageAccountRegenerateKeyContent regenKeyContent = new("key1");
            AsyncPageable<StorageAccountKey> regenAcctKeys = storageAccount.RegenerateKeyAsync(regenKeyContent);
            await foreach (StorageAccountKey key in regenAcctKeys)
            {
                Console.WriteLine($"\tKey name: {key.KeyName}");
                Console.WriteLine($"\tKey value: {key.Value}");
            }
        }

        public static async Task UpdateStorageAccountSkuAsync(
            StorageAccountResource storageAccount,
            StorageAccountCollection accountCollection)
        {
            // Update storage account SKU
            var currentSku = storageAccount.Data.Sku.Name;  // capture the current Sku value before updating
            var kind = storageAccount.Data.Kind ?? StorageKind.StorageV2;
            var location = storageAccount.Data.Location;
            StorageSku updatedSku = new(StorageSkuName.StandardGrs);
            StorageAccountCreateOrUpdateContent updatedParams = new(updatedSku, kind, location);
            await accountCollection.CreateOrUpdateAsync(WaitUntil.Completed, storageAccount.Data.Name, updatedParams);
            Console.WriteLine($"SKU on storage account updated from {currentSku} to {storageAccount.Get().Value.Data.Sku.Name}");
        }

        public static async Task FailoverStorageAccountAsync(StorageAccountResource storageAccount)
        {
            await storageAccount.FailoverAsync(WaitUntil.Completed, StorageAccountFailoverType.Planned);
        }

        public static async Task DeleteStorageAccountAsync(StorageAccountResource storageAccount)
        {
            await storageAccount.DeleteAsync(WaitUntil.Completed);
        }
    }
}
