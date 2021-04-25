using System;
using System.Threading.Tasks;
using NServiceBus;

namespace HelloWorld
{
    public class BasicQueuing
    {
        public BasicQueuing()
        {
        }

        public static void DisplayMessageHeaders(IMessageHandlerContext context)
        {
            foreach (var i in context.MessageHeaders.Keys)
            {
                string value = "";
                if(context.MessageHeaders.TryGetValue(i, out value))
                {
                    Console.WriteLine(i + ": " + value);
                }                 
            }
        }

        public static async Task<IEndpointInstance> ConfigureBasicEndpoint(string queueName)
        {
            var endpointConfiguration = new EndpointConfiguration(queueName);            
            var transport = endpointConfiguration.UseTransport<RabbitMQTransport>();
            transport.ConnectionString("amqp://localhost/MyFirstHost");            
            transport.UseDirectRoutingTopology();                
            return await Endpoint.Start(endpointConfiguration)
                .ConfigureAwait(false);
        }

        public static async Task RunLoop(IEndpointInstance endpointInstance)
        {
            var rand = new Random();
            while (true)
            {
                Console.WriteLine("- Press 'P' to delay, 'M' to fast, or 'Q' to quit!");
                var key = Console.ReadKey();
                Console.WriteLine();

                switch (key.Key)
                {
                    case ConsoleKey.M:
                        // Instantiate the command
                        var cmd1 = new DoSomethingFast
                        {
                            SomeProperty = Guid.NewGuid().ToString(),
                            SomeValue = rand.Next()
                        };                        
                        await endpointInstance
                            .SendLocal(cmd1)
                            .ConfigureAwait(false);
                        break;
                    case ConsoleKey.P:
                        // Instantiate the command
                        var command = new DoSomethingDelayed
                        {
                            SomeProperty = Guid.NewGuid().ToString()
                        };
                        await endpointInstance.SendLocal(command)
                            .ConfigureAwait(false);
                        break;

                    case ConsoleKey.Q:
                        return;

                    default:
                        break;
                }
            }
        }
    }
}
