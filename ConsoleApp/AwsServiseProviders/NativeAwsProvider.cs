using Amazon;
using Amazon.Runtime;
using Amazon.SQS;
using System;

namespace ConsoleApp3
{
    internal class NativeAwsProvider
    {
        public static IAmazonSQS GetSqsClient()
        {
            var awsAccessKey = Environment.GetEnvironmentVariable("YANS_AWS_ACCESS_KEY", EnvironmentVariableTarget.User);
            var awsSecretKey = Environment.GetEnvironmentVariable("YANS_AWS_SECRET_KEY", EnvironmentVariableTarget.User);

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
}
