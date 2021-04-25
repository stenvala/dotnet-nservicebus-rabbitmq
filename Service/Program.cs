using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;

namespace Client
{
    public class Program
    {

        static void Main(string[] args)
        {
            AsyncMain().GetAwaiter().GetResult();
        }

        static async Task AsyncMain()
        {
            Console.WriteLine("Initializing queue!");                       

            var endpointInstance = await BasicQueuing.ConfigureBasicEndpoint("MyFirstQueue");            

            Console.WriteLine("Starting looping!");
            await BasicQueuing.RunLoop(endpointInstance).ConfigureAwait(false);
            // When done
            await endpointInstance.Stop().ConfigureAwait(false);            
        }               
    }    
}
