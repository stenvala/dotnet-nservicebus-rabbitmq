using System;
using System.Threading.Tasks;
using NServiceBus;

namespace Saga
{
    class Program
    {
        static void Main(string[] args)
        {
            AsyncMain().GetAwaiter().GetResult();
        }        

        static async Task AsyncMain()
        {        
            Console.WriteLine("Initializing queue!");
           
            var endpointInstance = await ConfigureBasicEndpoint("MyFirstQueue");
            var endpointInstance2 = await ConfigureBasicEndpoint("MyFileSagaFinisher");
            FileProcessor.EndpointInstanceRetry = endpointInstance;
            FileProcessor.EndpointInstanceSaga = endpointInstance2;

            Console.WriteLine("Start batches!");
            await RunLoop(endpointInstance).ConfigureAwait(false);
            // When done
            await endpointInstance.Stop().ConfigureAwait(false);
            await endpointInstance2.Stop().ConfigureAwait(false);
        }

        public static async Task RunLoop(IEndpointInstance endpointInstance)
        {            
            var rd = new Random();            
            while (true)
            {
                Console.WriteLine("- Press 'B' to start batch or 'Q' to quit!");
                var key = Console.ReadKey();
                Console.WriteLine();
                switch (key.Key)
                {                    
                    case ConsoleKey.B:
                        var batchSize = rd.Next(2, 5);                        
                        var batchId = Guid.NewGuid().ToString();                       
                        for (var i = 0; i < batchSize; i++)
                        {
                            var cmd = new ProcessFile
                            {
                                BatchId = batchId,
                                ItemId = Guid.NewGuid().ToString(),
                                FileName = $"File{i+1}.txt",
                                BatchSize = batchSize
                            };
                            await endpointInstance
                                .SendLocal(cmd)
                                .ConfigureAwait(false);
                        }
                        break;
                    
                    case ConsoleKey.Q:
                        return;

                    default:
                        break;
                }
            }
        }

        public static async Task<IEndpointInstance> ConfigureBasicEndpoint(string queueName)
        {
            var endpointConfiguration = new EndpointConfiguration(queueName);
            var persistence = endpointConfiguration.UsePersistence<LearningPersistence>();

            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("amqp://localhost/MyFirstHost");
            transport.UseDirectRoutingTopology();

            return await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);
        }

    }
}
