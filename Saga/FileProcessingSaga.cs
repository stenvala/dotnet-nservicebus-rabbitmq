using System;
using System.Threading;
using System.Threading.Tasks;
using NServiceBus;

namespace Saga
{

    // This will do the actual processing. It can't be inside Saga, because it'll lock the processing.
    public class ProcessFile : ICommand
    {
        public string BatchId { get; set; }
        public string ItemId { get; set; }
        public string FileName { get; set; }
        public int BatchSize { get; set; }        
    }

    // This will be in Saga
    public class FileFinalized : ICommand
    {
        public string BatchId { get; set; }
        public int BatchSize { get; set; }
    }

    // This will do the combination of the results when ready
    public class CombineResult : ICommand
    {
        public string BatchId { get; set; }
    }

    // This will only care about finalized actions and will just follow message processing.
    // Keep everything here super fast.
    public class FileProcessingBatch : ContainSagaData
    {
        public string BatchId { get; set; }
        public int NumberOfFilesToBeProcessed { get; set; }
        public int ProcessedFiles { get; set; }
        public bool IsReady { get; set; }        
    }

    // Definition of the Saga. Considers locking, so only one execution of handle is ongoing at max simultaneously successfully (per batch)
    public class FileProcessingSaga : Saga<FileProcessingBatch>,
        IAmStartedByMessages<FileFinalized>
    {

        protected override void ConfigureHowToFindSaga(SagaPropertyMapper<FileProcessingBatch> mapper)
        {
            mapper.ConfigureMapping<FileFinalized>(message => message.BatchId)
                .ToSaga(sagaData => sagaData.BatchId);            
        }

        public Task Handle(FileFinalized message, IMessageHandlerContext context)
        {
            Data.NumberOfFilesToBeProcessed = message.BatchSize;            
            Data.ProcessedFiles++;            
            Data.IsReady = Data.ProcessedFiles == Data.NumberOfFilesToBeProcessed;
            Console.WriteLine($"Processed {Data.ProcessedFiles}/{Data.NumberOfFilesToBeProcessed}. Is ready: {Data.IsReady}.");
            return ProcessBatch(context);
        }

        private async Task ProcessBatch(IMessageHandlerContext context)
        {         
             if (Data.IsReady)
             {
                 // This could be sent to the worker queue again
                 Console.WriteLine("***** That was last. Saga completed. Sending message to combiner.");
                 await context.SendLocal(new CombineResult() { BatchId = Data.BatchId });
                 MarkAsComplete();
             }
        }        
    }

    // Batch finalizer, longish operation, so don't do inside Saga (e.g. in ProcessBatch).
    // It might be good to do this in another queue, but now done in the same example as the Saga
    public class BatchFinalizer : IHandleMessages<CombineResult>
    {
        public Task Handle(CombineResult message, IMessageHandlerContext context)
        {
            Console.WriteLine($"***** All files processed in batch {message.BatchId}. Merging them to zip.");
            return Task.CompletedTask;
        }
    }

    // Actual file processor, longish operation and may or may not have preprocessed data ready (and thus resend the message after delay)
    public class FileProcessor : IHandleMessages<ProcessFile>
    {
        // Normally dependency inject these way or another
        public static IEndpointInstance EndpointInstanceRetry;
        public static IEndpointInstance EndpointInstanceSaga;

        public async Task Handle(ProcessFile message, IMessageHandlerContext context)
        {
            var rd = new Random();
            Console.WriteLine($"Starting processing file {message.FileName} Id {message.ItemId} in batch {message.BatchId}.");

            // Check if async pre-processing is ready or not (randomly)
            var isReady = rd.Next(1, 5) < 4;
            if (!isReady)
            {
                var options = new SendOptions();
                options.RouteToThisEndpoint();
                // Doesn't work with my rabbit:                
                // 'NOT_FOUND - no exchange 'nsb.delay-delivery' in vhost 'MyFirstHost'
                //options.DelayDeliveryWith(TimeSpan.FromSeconds(10));                                                               
                Console.WriteLine($"File preprocessing not ready for {message.ItemId}. Retry in 5 s.");
                Thread.Sleep(5000);
                Console.WriteLine("Send retry messages");
                // This should be done with delay queue, but doesn't work now
                await EndpointInstanceRetry
                    .Send(message, options)
                    .ConfigureAwait(false);
                return;
            }

            // Wait some time as the processing takes some time, more or less random few seconds            
            int processingDuration = rd.Next(1000, 10000);
            Thread.Sleep(processingDuration);
            Console.WriteLine($"{message.FileName} processed successfully in {processingDuration} ms. Send message to Saga.");
            // Tell to Saga that this file is now finished
            var cmd = new FileFinalized
            {
                BatchId = message.BatchId,
                BatchSize = message.BatchSize
            };
            await EndpointInstanceSaga
                .SendLocal(cmd)
                .ConfigureAwait(false);            
        }        
    }
}
