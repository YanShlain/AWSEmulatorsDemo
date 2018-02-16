using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using Amazon.SQS.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleApp3
{
    internal class GoAws
    {
        public const string ServiceUrl = "http://localhost:4100";
        public const string QueueUrl = "http://localhost:4576/queue/";

        public static IAmazonSQS GetSqsClient()
        {
            return GetAmazonClient(ServiceUrl);
        }

        private static IAmazonSQS GetAmazonClient(string serviceUrl)
        {
            var clientConfig = new AmazonSQSConfig { ServiceURL = serviceUrl };
            return new AmazonSQSClient(clientConfig);
        }
    }

    internal class LocalStack
    {
        public const string ServiceUrl = "http://localhost:4576";
        public const string QueueUrl = "http://localhost:4576/queue/";

        public static IAmazonSQS GetSqsClient()
        {
            return GetAmazonClient(ServiceUrl);
        }

        private static IAmazonSQS GetAmazonClient(string serviceUrl)
        {
            var clientConfig = new AmazonSQSConfig { ServiceURL = serviceUrl };
            return new AmazonSQSClient(clientConfig);
        }
    }

    internal class NativeAwsProvider
    {
        public static IAmazonSQS GetSqsClient()
        {
            var awsAccessKey = Environment.GetEnvironmentVariable("AWS_ACCESS_KEY", EnvironmentVariableTarget.User);
            var awsSecretKey = Environment.GetEnvironmentVariable("AWS_SECRET_KEY", EnvironmentVariableTarget.User);

            var sqsClient = GetAmazonClient(awsAccessKey, awsSecretKey, RegionEndpoint.USEast1);
            return sqsClient;
        }

        private static AmazonSQSClient GetAmazonClient(string awsAccessKey, string awsSecretKey, RegionEndpoint regionEndpoint)
        {
            Console.WriteLine("Creating Client and request");

            var awsCreds = new BasicAWSCredentials(awsAccessKey, awsSecretKey);
            var amazonSQSClient = new AmazonSQSClient(awsCreds, regionEndpoint);

            return amazonSQSClient;
        }
    }

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

        static async Task Main(string[] args)
        {
            try
            {
                Console.WriteLine("Queue Test Starting!");
                IAmazonSQS sqsClient = GetSqsClient(SqsServiceProviders.LocalStack);

                var queues = await GetQueuesAsync(sqsClient);
                string queueUrl = queues.FirstOrDefault(queueName => queueName == s_QueueName) ?? await CreateQueueAsync(sqsClient, s_QueueName);

                await SendMessageAsync(sqsClient, queueUrl);
                await ReadMessagesAsync(sqsClient, queueUrl);

            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        private static async Task<string> CreateQueueAsync(IAmazonSQS sqsClient, string s_QueueName, CancellationToken cancellationToken = default)
        {
            var response = await sqsClient.CreateQueueAsync(s_QueueName, cancellationToken);
            return response.QueueUrl;
        }

        private static IAmazonSQS GetSqsClient(SqsServiceProviders sqsProviderType)
        {
            IAmazonSQS sqsClient = null;

            switch (sqsProviderType)
            {
                case SqsServiceProviders.Amazon:
                    sqsClient = NativeAwsProvider.GetSqsClient();
                    break;
                case SqsServiceProviders.GoAws:
                    sqsClient = GoAws.GetSqsClient();
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
