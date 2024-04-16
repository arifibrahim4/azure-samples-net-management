using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Communication;
using Azure.ResourceManager.EventGrid;
using Azure.ResourceManager.EventGrid.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Identity.Client;
using Samples.Utilities;
using System.Text;

namespace manage_communication_event_subscriptions
{
    public class Program
    {
        public static async Task Main()
        {
            await CreateEventSubscription();

            await GetEventSubscription();

            //await UpdateEventSubscription();

            //await DeleteEventSubscription();
        }

        public static async Task CreateEventSubscription()
        {
            var subscriptionId = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
            var resourceGroupName = Environment.GetEnvironmentVariable("RESOURCE_GROUP_NAME");
            var resName = Environment.GetEnvironmentVariable("COMMUNICATION_RESOURCE_NAME");
            var eventSubscriptionName = Environment.GetEnvironmentVariable("COMMUNICATION_EVENT_SUBSCRIPTION_NAME");
            var webhook = Environment.GetEnvironmentVariable("EVENT_SUBSCRIPTION_WEBHOOK_URL");


            // Create Management Client
            ArmClient client = new ArmClient(new DefaultAzureCredential(), subscriptionId);

            // Use subscription and resource group to find the targeted communication resource
            ResourceIdentifier resourceGroupResourceId = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroupName);
            ResourceGroupResource resourceGroupResource = client.GetResourceGroupResource(resourceGroupResourceId);
            var commResource = resourceGroupResource.GetCommunicationServiceResource(resName).Value;

            Utilities.PrintCommunicationServiceResource(commResource);

            // get event subscription collection for the communication resource
            var eventSubscriptionCollection = client.GetEventSubscriptions(commResource.Id);

            // Start the event subscription process for the communication resource           

            // set the events which you want to receive
            var eventFilter = new EventSubscriptionFilter();
            eventFilter.IncludedEventTypes.Add("Microsoft.Communication.SMSDeliveryReportReceived");

            //Add webhook & filter detail needed for your event subscription
            var eventSubscription = new EventGridSubscriptionData()
            {
                Destination = new WebHookEventSubscriptionDestination()
                {
                    Endpoint = new Uri(webhook),
                },
                Filter = eventFilter,
            };

            // Create Event subscription
            ArmOperation<EventSubscriptionResource> lro = await eventSubscriptionCollection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, eventSubscriptionName, eventSubscription);
            EventSubscriptionResource result = lro.Value;

            EventGridSubscriptionData resourceData = result.Data;
            // for demo we just print out the id
            Utilities.Log($"Succeeded on id: {resourceData.Id}");

        }

        public static async Task GetEventSubscription()
        {
            var subscriptionId = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
            var resourceGroupName = Environment.GetEnvironmentVariable("RESOURCE_GROUP_NAME");
            var resName = Environment.GetEnvironmentVariable("COMMUNICATION_RESOURCE_NAME");
            var eventSubscriptionName = Environment.GetEnvironmentVariable("COMMUNICATION_EVENT_SUBSCRIPTION_NAME");

            // Create Management Client
            ArmClient client = new ArmClient(new DefaultAzureCredential(), subscriptionId);

            // Use subscription and resource group to find the targeted communication resource
            ResourceIdentifier resourceGroupResourceId = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroupName);
            ResourceGroupResource resourceGroupResource = client.GetResourceGroupResource(resourceGroupResourceId);
            var commResource = resourceGroupResource.GetCommunicationServiceResource(resName).Value;

            // get event subscription collection for the communication resource
            var eventSubscriptionCollection = client.GetEventSubscriptions(commResource.Id);

            // invoke the operation
           
            EventSubscriptionResource response = await eventSubscriptionCollection.GetAsync(eventSubscriptionName);
            EventGridSubscriptionData eventSubscriptionData = response.Data;

            Utilities.PrintEventGridSubscriptionData(eventSubscriptionData);

        }

        public static async Task UpdateEventSubscription()
        {
            var subscriptionId = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
            var resourceGroupName = Environment.GetEnvironmentVariable("RESOURCE_GROUP_NAME");
            var resName = Environment.GetEnvironmentVariable("COMMUNICATION_RESOURCE_NAME");
            var eventSubscriptionName = Environment.GetEnvironmentVariable("COMMUNICATION_EVENT_SUBSCRIPTION_NAME");

            // Create Management Client
            ArmClient client = new ArmClient(new DefaultAzureCredential(), subscriptionId);

            // Use subscription and resource group to find the targeted communication resource
            ResourceIdentifier resourceGroupResourceId = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroupName);
            ResourceGroupResource resourceGroupResource = client.GetResourceGroupResource(resourceGroupResourceId);
            var commResource = resourceGroupResource.GetCommunicationServiceResource(resName).Value;

            // get event subscription collection for the communication resource
            var eventSubscriptionCollection = client.GetEventSubscriptions(commResource.Id);

            // Get event grid which needs update            
            EventSubscriptionResource response = await eventSubscriptionCollection.GetAsync(eventSubscriptionName);
            EventGridSubscriptionData eventSubscriptionOldData = response.Data;

            // Get Event Subscription resource
            var eventSubscription = client.GetEventSubscriptionResource(eventSubscriptionOldData.Id);

            // add the additonal event type you want to include
            eventSubscriptionOldData.Filter.IncludedEventTypes.Add("Microsoft.Communication.SMSReceived");


            EventGridSubscriptionPatch patch = new EventGridSubscriptionPatch()
            {
                Filter = eventSubscriptionOldData.Filter
            };

            ArmOperation<EventSubscriptionResource> lro = await eventSubscription.UpdateAsync(WaitUntil.Completed, patch);
            EventSubscriptionResource result = lro.Value;

            Utilities.PrintEventGridSubscriptionData(result.Data);
        }

        public static async Task DeleteEventSubscription()
        {
            var subscriptionId = Environment.GetEnvironmentVariable("SUBSCRIPTION_ID");
            var resourceGroupName = Environment.GetEnvironmentVariable("RESOURCE_GROUP_NAME");
            var resName = Environment.GetEnvironmentVariable("COMMUNICATION_RESOURCE_NAME");
            var eventSubscriptionName = Environment.GetEnvironmentVariable("COMMUNICATION_EVENT_SUBSCRIPTION_NAME");

            // Create Management Client
            ArmClient client = new ArmClient(new DefaultAzureCredential(), subscriptionId);

            // Use subscription and resource group to find the targeted communication resource
            ResourceIdentifier resourceGroupResourceId = ResourceGroupResource.CreateResourceIdentifier(subscriptionId, resourceGroupName);
            ResourceGroupResource resourceGroupResource = client.GetResourceGroupResource(resourceGroupResourceId);
            var commResource = resourceGroupResource.GetCommunicationServiceResource(resName).Value;

            // get event subscription collection for the communication resource
            var eventSubscriptionCollection = client.GetEventSubscriptions(commResource.Id);

            // Get event grid which needs update
           
            EventSubscriptionResource response = await eventSubscriptionCollection.GetAsync(eventSubscriptionName);
            EventGridSubscriptionData eventSubscriptionOldData = response.Data;

            // Get Event Subscription resource
            var eventSubscription = client.GetEventSubscriptionResource(eventSubscriptionOldData.Id);

            // Invoke Delete operation
            await eventSubscription.DeleteAsync(WaitUntil.Completed);

            Utilities.Log($"Deleted Event subscription : {eventSubscriptionOldData.Name}");
        }        
    }

}
