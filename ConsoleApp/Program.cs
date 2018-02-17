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

namespace AwsEmulators.Demo.ConsoleApp
{

    public enum SqsServiceProviders
    {
        Undefined = 0,
        Amazon = 1,
        GoAws = 2,
        LocalStack = 3
    }


    class Program
    {
        private const string s_QueueName = "local_test_queue1";
        private const SqsServiceProviders s_EmulatorName = SqsServiceProviders.LocalStack;
        public static IConfiguration Configuration { get; set; }

        static async Task Main(string[] args)
        {
            try
            {
                Configuration = ReadConfiguration("/Configuration", "appsettings.json");
                var sqsClient = new SqsClient(Configuration, s_EmulatorName);

                var queues = await sqsClient.GetQueuesAsync();
                string queueUrl = queues.FirstOrDefault(queueName => queueName == s_QueueName) ?? await sqsClient.CreateQueueAsync(s_QueueName);

                await sqsClient.SendMessageAsync(s_QueueName);
                await sqsClient.ReadMessagesAsync(s_QueueName);
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
    }
}
