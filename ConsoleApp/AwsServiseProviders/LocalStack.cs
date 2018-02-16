using Amazon.SQS;

namespace ConsoleApp3
{
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
}
