using Microsoft.Azure.ServiceBus;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Worker.Service
{
    public class WorkRequestHandler : IHostedService
    {
        private readonly IQueueClient _queueClient;
        private readonly IQueueClient _queueClientOut;

        public WorkRequestHandler(IConfiguration configuration)
        {
            _queueClient = new QueueClient(configuration["ServiceBusConnectionString"], configuration["RequestQueue"]);
            _queueClientOut = new QueueClient(configuration["ServiceBusConnectionString"], configuration["ResponseQueue"]);
        }
        public Task StartAsync(CancellationToken cancellationToken)
        {
            _queueClient.RegisterSessionHandler(ProcessSessionMessagesAsync, ExceptionReceivedHandler);
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        async Task ProcessSessionMessagesAsync(IMessageSession session, Message message, CancellationToken token)
        {
            Console.WriteLine($"Received Session: {session.SessionId} message: SequenceNumber: {message.SystemProperties.SequenceNumber} Body:{Encoding.UTF8.GetString(message.Body)}");
            await session.CompleteAsync(message.SystemProperties.LockToken);

            //Send respnse
            var response = message.Clone();
            response.SessionId = message.SessionId;
            await _queueClientOut.SendAsync(response);
        }

        Task ExceptionReceivedHandler(ExceptionReceivedEventArgs exceptionReceivedEventArgs)
        {
            Console.WriteLine($"Message handler encountered an exception {exceptionReceivedEventArgs.Exception}.");
            var context = exceptionReceivedEventArgs.ExceptionReceivedContext;
            Console.WriteLine("Exception context for troubleshooting:");
            Console.WriteLine($"- Endpoint: {context.Endpoint}");
            Console.WriteLine($"- Entity Path: {context.EntityPath}");
            Console.WriteLine($"- Executing Action: {context.Action}");
            return Task.CompletedTask;
        }

    }
}
