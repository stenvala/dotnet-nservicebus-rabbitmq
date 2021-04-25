using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;

namespace HelloWorld
{
    public class DoSomethingDelayed :
    ICommand
    {
        public string SomeProperty { get; set; }
    }

    public class DoSomethingFast :
    ICommand
    {
        public string SomeProperty { get; set; }
        public int SomeValue { get; set; }
    }


    public class TaskHandler :
        IHandleMessages<DoSomethingDelayed>,
        IHandleMessages<DoSomethingFast>
    {        

        public Task Handle(DoSomethingDelayed message, IMessageHandlerContext context)
        {            
            Console.WriteLine("* Received DoSomething message with property that will be printed in 2 s (sleep, not delayed message):");
            BasicQueuing.DisplayMessageHeaders(context);
            Thread.Sleep(2000);
            Console.WriteLine(message.SomeProperty);         
            return Task.CompletedTask;
        }

        public Task Handle(DoSomethingFast message, IMessageHandlerContext context)
        {
            Console.WriteLine("* Received DoSomethingElse with property and value:");
            BasicQueuing.DisplayMessageHeaders(context);
            Console.WriteLine(message.SomeProperty);
            Console.WriteLine(message.SomeValue);
            return Task.CompletedTask;
        }
    }
}
