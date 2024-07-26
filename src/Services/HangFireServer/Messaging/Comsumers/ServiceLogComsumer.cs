using System.Text.Json;
using Common.Enums;
using Common.Shared.Models.Logs;
using MassTransit;
using Serilog;

namespace HangFireServer.Messaging.Comsumers
{
    public class ServiceLogComsumer : IConsumer<ServiceLogMessage>
    {
        public async Task Consume(ConsumeContext<ServiceLogMessage> context)
        {
            var serviceLogMessage = context.Message;
            switch (serviceLogMessage.LogLevel)
            {
                case ELogLevel.Information:
                    Log.Logger.Information(
                        "[{@Environment} {@ServiceName}] {@EventName} - {@Description}",
                        serviceLogMessage.Environment,
                        serviceLogMessage.ServiceName, serviceLogMessage.EventName,
                        serviceLogMessage.Description, serviceLogMessage);
                    break;
                case ELogLevel.Warning:
                    Log.Logger.Warning("[{@Environment} {@ServiceName}] {@EventName} - {@Description}",
                        serviceLogMessage.Environment,
                        serviceLogMessage.ServiceName, serviceLogMessage.EventName,
                        serviceLogMessage.Description, serviceLogMessage);
                    break;
                case ELogLevel.Error:
                    Log.Logger.Error(
                        "[{@Environment} {@ServiceName}] {@EventName} - {@Description} - Detail",
                        serviceLogMessage.Environment,
                        serviceLogMessage.ServiceName, serviceLogMessage.EventName,
                        serviceLogMessage.Description,
                        serviceLogMessage);
                    break;
            }
            // Cheat warning async/await
            await Task.FromResult(0);
        }
    }
}
