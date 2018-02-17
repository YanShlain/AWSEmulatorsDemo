using Amazon.SQS;
using Microsoft.Extensions.Configuration;
using System;

namespace ConsoleApp3
{
    internal class GoAws
    {
        private const string SECTION_NAME = "GoAws";
        public string ServiceUrl { get; }
        public string QueueUrl { get; }

        public GoAws(IConfiguration configuration)
        {
            var configurationSection = configuration.GetSection(SECTION_NAME);
            ServiceUrl = configurationSection["ServiceUrl"];
            QueueUrl = configurationSection["QueueUrl"];
        }

        public IAmazonSQS GetSqsClient()
        {
            return GetAmazonClient(ServiceUrl);
        }

        private static IAmazonSQS GetAmazonClient(string serviceUrl)
        {
            var awsAccessKey = Environment.GetEnvironmentVariable("YANS_AWS_ACCESS_KEY", EnvironmentVariableTarget.User);
            var awsSecretKey = Environment.GetEnvironmentVariable("YANS_AWS_SECRET_KEY", EnvironmentVariableTarget.User);

            var clientConfig = new AmazonSQSConfig { ServiceURL = serviceUrl };
            return new AmazonSQSClient(awsAccessKey, awsSecretKey, clientConfig);
        }
    }
}
