using Amazon.SQS;
using Amazon.SQS.Model;
using ConsoleApp3;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace AwsEmulators.Demo.ConsoleApp
{
    public class SqsClient
    {
        private readonly IConfiguration _configuration;
        private readonly SqsServiceProviders _providerType;
        private IAmazonSQS _sqsClient;

        public IAmazonSQS Client
        {
            get
            {
                if (_sqsClient == null)
                {
                    this._sqsClient = GetSqsClient(_providerType, _configuration);
                }

                return _sqsClient;
            }
        }

        public SqsClient(IConfiguration configuration, SqsServiceProviders providerType)
        {

            this._configuration = configuration;
            this._providerType = providerType;
        }

        public async Task<string> CreateQueueAsync(string s_QueueName, CancellationToken cancellationToken = default)
        {
            var response = await Client.CreateQueueAsync(s_QueueName, cancellationToken);
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
                    sqsClient = new LocalStack(configuration).GetSqsClient();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(sqsProviderType));
            }

            return sqsClient;
        }

        public async Task<List<string>> GetQueuesAsync()
        {
            var response = await Client.ListQueuesAsync(string.Empty);
            return response.QueueUrls;
        }

        public async Task<SendMessageResponse> SendMessageAsync(string queueName)
        {
            //Create the request to send
            var sendRequest = new SendMessageRequest
            {
                QueueUrl = $"{Client.Config.ServiceURL}/queues/{queueName}",
                MessageBody = "Curret local time is: " + DateTime.Now.ToLongDateString()
            };

            var sendMessageResponse = await Client.SendMessageAsync(sendRequest);
            return sendMessageResponse;
        }

        public async Task<ReceiveMessageResponse> ReadMessagesAsync(string queueName)
        {
            //Create a receive requesdt to see if there are any messages on the queue
            var receiveMessageRequest = new ReceiveMessageRequest();
            receiveMessageRequest.QueueUrl = $"{Client.Config.ServiceURL}/queues/{queueName}";

            //Send the receive request and wait for the response
            var response = await Client.ReceiveMessageAsync(receiveMessageRequest);
            return response;
        }

        public async Task<DeleteMessageResponse> DeleteMessageFrowQueueAsync(string queueName, string receiptHandle)
        {
            //Remove it from the queue as we don't want to see it again
            var deleteMessageRequest = new DeleteMessageRequest
            {
                QueueUrl = $"{Client.Config.ServiceURL}/queues/{queueName}",
                ReceiptHandle = receiptHandle
            };

            var result = await Client.DeleteMessageAsync(deleteMessageRequest);
            return result;
        }
    }
}
