using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Microsoft.Azure.ServiceBus;

namespace POCPubSub
{
    class Program
    {
        static string connString = "Endpoint=sb://demoappnamespace.servicebus.windows.net/;SharedAccessKeyName=AppPolicy;SharedAccessKey=qThd4bqApP3SCwv6X9gtsvpb1kKxt2HLbHNWfO+qdYA=;";
        static string queueName = "demoqueue";

        
        //static string subscriptionName = "Visual Studio Professional Subscription";
        static async Task Main()
        {
            //await SendMessageToTopicAsyncString();
            await SendMessageToTopicAsyncBytes();
        }

        #region Method 1 - to send the message without bytes formatting
        static async Task SendMessageToTopicAsyncString()
        {
            await using (ServiceBusClient serviceBusClient = new ServiceBusClient(connString))
            {
                ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(queueName);
                ServiceBusMessage serviceBusMessage = new ServiceBusMessage("Hello !");

                await serviceBusSender.SendMessageAsync(serviceBusMessage);
                Console.WriteLine("Message sent to queue {0}", queueName);

            };

        }
        #endregion

        #region Method 2 - to send the message with bytes formatting
        static async Task SendMessageToTopicAsyncBytes()
        {
            IQueueClient queueClient = new QueueClient(connString,queueName);

            Person person = new Person();
            Message message = new Message(Encoding.UTF8.GetBytes(person.ID.ToString() + ' ' + person.name.ToString()  ));
            Console.WriteLine("Sending message");
            await queueClient.SendAsync(message);

        }
        #endregion

        #region Sending messages as batch
        static Queue<ServiceBusMessage> CreateMessages()
        {
            Queue<ServiceBusMessage> serviceBusMessages = new Queue<ServiceBusMessage>();
            serviceBusMessages.Enqueue(new ServiceBusMessage("First msg"));
            serviceBusMessages.Enqueue(new ServiceBusMessage("Second msg"));

            return serviceBusMessages;
        }

        static async Task SendMessageBatchToTopicAsync()
        {
            await using (ServiceBusClient serviceBusClient = new ServiceBusClient(connString))
            {
                ServiceBusSender serviceBusSender = serviceBusClient.CreateSender(queueName);
                // Get the messages to be queued               
                Queue<ServiceBusMessage> busMessages = CreateMessages();
                var messageCount = busMessages.Count;
                
                while (messageCount>0)
                {
                    // create serviceBus message batch
                    ServiceBusMessageBatch serviceBusMessageBatch = await serviceBusSender.CreateMessageBatchAsync();

                    //try to add first message in the batch
                    if (serviceBusMessageBatch.TryAddMessage(busMessages.Peek()))
                    {
                        //if messgae is added, we will dequeue it from service bus queue so that it won't be added into batch again
                        busMessages.Dequeue();
                    }
                    else
                    {
                        // this means message is so large that it can't be fitted in batch
                        throw new Exception("Queue is uanble to read first message");
                    }

                    // if there are more messages and will add it in Batch
                    while(messageCount>0 && serviceBusMessageBatch.TryAddMessage(busMessages.Peek()))
                    {
                        busMessages.Dequeue();
                    }

                    //send the batch
                    await serviceBusSender.SendMessagesAsync(serviceBusMessageBatch);
                }


                Console.WriteLine("Messages sent via Batch");
            }
        }
        #endregion

    }
}
