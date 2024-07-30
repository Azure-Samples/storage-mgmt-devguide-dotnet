using Azure;
using Azure.Core;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage.Models;
using Azure.ResourceManager.Storage;

namespace StorageAccountManagement
{
    public static class ManagementTasks
    {
        // <Snippet_RegisterSRP>
        public static async Task RegisterSRPInSubscription(SubscriptionResource subscription)
        {
            ResourceProviderResource resourceProvider =
                await subscription.GetResourceProviderAsync("Microsoft.Storage");

            // Check the registration state of the resource provider and register, if needed
            if (resourceProvider.Data.RegistrationState == "NotRegistered")
                resourceProvider.Register();
        }
        // </Snippet_RegisterSRP

        // <Snippet_CreateStorageAccount>
        public static async Task<StorageAccountResource> CreateStorageAccount(
            ResourceGroupResource resourceGroup,
            string storageAccountName)
        {
            // Define the settings for the storage account
            AzureLocation location = AzureLocation.EastUS;
            StorageSku sku = new(StorageSkuName.StandardLrs);
            StorageKind kind = StorageKind.StorageV2;

            // Set other properties as needed
            StorageAccountCreateOrUpdateContent parameters = new(sku, kind, location)
            {
                AccessTier = StorageAccountAccessTier.Cool,
                AllowSharedKeyAccess = false,
            };

            // Create a storage account with defined account name and settings
            StorageAccountCollection accountCollection = resourceGroup.GetStorageAccounts();
            ArmOperation<StorageAccountResource> accountCreateOperation = 
                await accountCollection.CreateOrUpdateAsync(WaitUntil.Completed, storageAccountName, parameters);
            StorageAccountResource storageAccount = accountCreateOperation.Value;

            return storageAccount;
        }
        // </Snippet_CreateStorageAccount>

        // <Snippet_ListAccountsResourceGroup>
        public static async Task ListStorageAccountsInResourceGroup(ResourceGroupResource resourceGroup)
        {
            await foreach (StorageAccountResource storageAccount in resourceGroup.GetStorageAccounts())
            {
                Console.WriteLine($"\t{storageAccount.Id.Name}");
            }
        }
        // </Snippet_ListAccountsResourceGroup>

        // <Snippet_ListAccountsSubscription>
        public static async Task ListStorageAccountsForSubscription(SubscriptionResource subscription)
        {
            await foreach (StorageAccountResource storageAccount in subscription.GetStorageAccountsAsync())
            {
                Console.WriteLine($"\t{storageAccount.Id.Name}");
            }
        }
        // </Snippet_ListAccountsSubscription>

        // <Snippet_GetAccountKeys>
        public static async Task GetStorageAccountKeysAsync(StorageAccountResource storageAccount)
           {
            AsyncPageable<StorageAccountKey> acctKeys = storageAccount.GetKeysAsync();
            await foreach (StorageAccountKey key in acctKeys)
            {
                Console.WriteLine($"\tKey name: {key.KeyName}");
                Console.WriteLine($"\tKey value: {key.Value}");
            }
        }
        // </Snippet_GetAccountKeys>

        // <Snippet_RegenerateAccountKey>
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
        // </Snippet_RegenerateAccountKey>

        // <Snippet_UpdateAccountSKU>
        public static async Task UpdateStorageAccountSkuAsync(
            StorageAccountResource storageAccount,
            StorageAccountCollection accountCollection)
        {
            // Update storage account SKU
            var currentSku = storageAccount.Data.Sku.Name;  // capture the current SKU value before updating
            var kind = storageAccount.Data.Kind ?? StorageKind.StorageV2;
            var location = storageAccount.Data.Location;
            StorageSku updatedSku = new(StorageSkuName.StandardGrs);
            StorageAccountCreateOrUpdateContent updatedParams = new(updatedSku, kind, location);
            await accountCollection.CreateOrUpdateAsync(WaitUntil.Completed, storageAccount.Data.Name, updatedParams);
            Console.WriteLine($"SKU on storage account updated from {currentSku} to {storageAccount.Get().Value.Data.Sku.Name}");
        }
        // </Snippet_UpdateAccountSKU>

        // <Snippet_DeleteStorageAccount>
        public static async Task DeleteStorageAccountAsync(StorageAccountResource storageAccount)
        {
            await storageAccount.DeleteAsync(WaitUntil.Completed);
        }
        // </Snippet_DeleteStorageAccount>
    }
}
