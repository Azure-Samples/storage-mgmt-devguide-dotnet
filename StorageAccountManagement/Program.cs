using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Storage;
using Azure.ResourceManager.Storage.Models;

using StorageAccountManagement;

const string subscriptionId = "<subscription-id>";
const string rgName = "<resource-group-name>";
string storageAccountName = "<account-name>";

// Authenticate to Azure and create the top-level ArmClient
ArmClient armClient = new ArmClient(new DefaultAzureCredential());

// Create a resource identifier and get the subscription resource
ResourceIdentifier resourceIdentifier = new ResourceIdentifier($"/subscriptions/{subscriptionId}");
SubscriptionResource subscription = armClient.GetSubscriptionResource(resourceIdentifier);

// Register the Storage resource provider in the subscription
ResourceProviderResource resourceProvider = await subscription.GetResourceProviderAsync("Microsoft.Storage");
resourceProvider.Register();

// Create a new resource group - if one already exists then it's updated
ArmOperation<ResourceGroupResource> rgOperation = await subscription.GetResourceGroups().CreateOrUpdateAsync(WaitUntil.Completed, rgName, new ResourceGroupData(location));
ResourceGroupResource resourceGroup = rgOperation.Value;
Console.WriteLine($"Resource group: {resourceGroup.Id.Name}");

// Check if the account name is available
bool? nameAvailable = subscription.CheckStorageAccountNameAvailability(new StorageAccountNameAvailabilityContent(storageAccountName)).Value.IsNameAvailable;

StorageAccountResource storageAccount = await ManagementTasks.CreateStorageAccount(resourceGroup, storageAccountName);

// Get all the storage accounts for a given subscription
await ManagementTasks.ListStorageAccountsForSubscription(subscription);

// Get a list of storage accounts within a specific resource group
await ManagementTasks.ListStorageAccountsInResourceGroup(resourceGroup);

// List the storage account keys for a given account
await ManagementTasks.ListStorageAccountKeys(storageAccount);

// Regenerate an account key for a given account
await ManagementTasks.RegenerateStorageAccountKey(storageAccount);

//Update the storage account for a given account name and resource group
await UpdateStorageAccountSkuAsync(storageAccount, accountCollection);

// Delete a storage account with the given account name and a resource group
storageAccount = await accountCollection.GetAsync(storAccountName);
Console.WriteLine($"Deleting storage account {storageAccount.Id.Name}");
await storageAccount.DeleteAsync(WaitUntil.Completed);

Console.ReadLine();