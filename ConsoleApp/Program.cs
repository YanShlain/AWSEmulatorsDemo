using Amazon.SQS;
using Amazon.SQS.Model;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp3
{

    internal enum SqsServiceProviders
    {
        Undefined = 0,
        Amazon = 1,
        GoAws = 2,
        LocalStack = 3
    }


    class Program
    {
        private const string s_QueueName = "local_test_queue1";
        private const SqsServiceProviders s_EmulatorName = SqsServiceProviders.GoAws;
        public static IConfiguration Configuration { get; set; }

        static async Task Main(string[] args)
        {
            try
            {
                if (Directory.Exists(@"/Configuration"))
                {
                    string filePath = Path.Combine(@"/Configuration", "appsettings.json");
                    if (File.Exists(filePath))
                    {
                        var content = File.ReadAllText(filePath);
                    }
                }
                Configuration = ReadConfiguration("/Configuration", "appsettings.json");

                IAmazonSQS sqsClient = GetSqsClient(s_EmulatorName, Configuration);


                var queues = await GetQueuesAsync(sqsClient);
                string queueUrl = queues.FirstOrDefault(queueName => queueName == s_QueueName) ?? await CreateQueueAsync(sqsClient, s_QueueName);

                //TODO: Remove below line.
                queueUrl = queueUrl.Replace("/:/", "/172.25.39.209:4100/");

                await SendMessageAsync(sqsClient, queueUrl);
                await ReadMessagesAsync(sqsClient, queueUrl);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static IConfiguration ReadConfiguration(string dirPath, string configurationFileName)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(dirPath)
                .AddJsonFile(configurationFileName);

            return builder.Build();
        }

        private static async Task<string> CreateQueueAsync(IAmazonSQS sqsClient, string s_QueueName, CancellationToken cancellationToken = default)
        {
            var response = await sqsClient.CreateQueueAsync(s_QueueName, cancellationToken);
            return response.QueueUrl;
        }

        private static IAmazonSQS GetSqsClient(SqsServiceProviders sqsProviderType, IConfiguration configuration)
        {
            IAmazonSQS sqsClient = null;

            switch (sqsProviderType)
            {
                case SqsServiceProviders.Amazon:
                    sqsClient = NativeAwsProvider.GetSqsClient();
                    break;
                case SqsServiceProviders.GoAws:
                    sqsClient = new GoAws(configuration).GetSqsClient();
                    break;
                case SqsServiceProviders.LocalStack:
                    sqsClient = LocalStack.GetSqsClient();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sqsProviderType));
            }

            return sqsClient;
        }

        public static async Task<List<string>> GetQueuesAsync(IAmazonSQS sqsClient)
        {
            var response = await sqsClient.ListQueuesAsync(string.Empty);
            return response.QueueUrls;
        }

        private static async Task SendMessageAsync(IAmazonSQS sqsClient, string queueUrl)
        {
            //Create the request to send
            var sendRequest = new SendMessageRequest
            {
                QueueUrl = queueUrl,
                MessageBody = "Curret local time is: " + DateTime.Now.ToLongDateString()
            };

            //Send the message to the queue and wait for the result
            Console.WriteLine("Sending Message");
            var sendMessageResponse = await sqsClient.SendMessageAsync(sendRequest);
        }

        private static async Task ReadMessagesAsync(IAmazonSQS amazonSQSClient, string queueUrl)
        {
            Console.WriteLine("Receiving Message");

            //Create a receive requesdt to see if there are any messages on the queue
            var receiveMessageRequest = new ReceiveMessageRequest();
            receiveMessageRequest.QueueUrl = queueUrl;

            //Send the receive request and wait for the response
            var response = await amazonSQSClient.ReceiveMessageAsync(receiveMessageRequest);

            //If we have any messages available
            foreach (var message in response.Messages)
            {
                //Spit it out
                Console.WriteLine(message.Body);

                //Remove it from the queue as we don't want to see it again
                var deleteMessageRequest = new DeleteMessageRequest();
                deleteMessageRequest.QueueUrl = queueUrl;
                deleteMessageRequest.ReceiptHandle = message.ReceiptHandle;

                var result = amazonSQSClient.DeleteMessageAsync(deleteMessageRequest).Result;
            }
        }
    }
}
