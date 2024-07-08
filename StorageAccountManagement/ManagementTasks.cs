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

            StorageAccountCreateOrUpdateContent parameters = new StorageAccountCreateOrUpdateContent(sku, kind, location)
            {
                AccessTier = StorageAccountAccessTier.Hot,
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
            AsyncPageable<StorageAccountResource> storageAcctsSub = subscription.GetStorageAccountsAsync();
            Console.WriteLine($"List of storage accounts in subscription {subscription.Get().Value.Data.DisplayName}:");
            await foreach (StorageAccountResource storageAcctSub in storageAcctsSub)
            {
                Console.WriteLine($"\t{storageAcctSub.Id.Name}");
            }
        }

        public static async Task ListStorageAccountKeys(StorageAccountResource storageAccount)
           {
            Pageable<StorageAccountKey> acctKeys = storageAccount.GetKeys();
            foreach (StorageAccountKey key in acctKeys)
            {
                Console.WriteLine($"\tKey name: {key.KeyName}");
                Console.WriteLine($"\tKey value: {key.Value}");
            }
        }

        public static async Task RegenerateStorageAccountKey(StorageAccountResource storageAccount)
        {
            StorageAccountRegenerateKeyContent regenKeyContent = new StorageAccountRegenerateKeyContent("key1");
            Pageable<StorageAccountKey> regenAcctKeys = storageAccount.RegenerateKey(regenKeyContent);
        }

        public static async Task UpdateStorageAccountSkuAsync(StorageAccountResource storageAccount, StorageAccountCollection accountCollection)
        {
            // Update storage account sku
            var currentSku = storageAccount.Get().Value.Data.Sku.Name;  // capture the current Sku value before updating
            StorageSku updateSku = new StorageSku(StorageSkuName.StandardLrs);
            StorageAccountCreateOrUpdateContent updateParams = new StorageAccountCreateOrUpdateContent(updateSku, kind, location);
            await accountCollection.CreateOrUpdateAsync(WaitUntil.Completed, storAccountName, updateParams);
            Console.WriteLine($"Sku on storage account updated from {currentSku} to {storageAccount.Get().Value.Data.Sku.Name}");
        }

        public static async Task DeleteStorageAccountAsync(StorageAccountResource storageAccount)
        {
            await storageAccount.DeleteAsync(WaitUntil.Completed);
        }
    }
}
