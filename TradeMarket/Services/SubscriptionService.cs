using Grpc.Core;

using Microsoft.Extensions.Logging;

using System;
using System.Threading;
using System.Threading.Tasks;

using TradeMarket.DataTransfering.Bitmex;

namespace TradeMarket.Services
{
    /// <summary>
    /// Класс который отвечает на ответы запросов по подписке .
    /// </summary>
    ///
    //TODO LocalClass такое себе название !
    public class SubscriptionService<TSubscribeRequest, TSubscribeReply, TLocalClass, TService>
        where TSubscribeRequest : Google.Protobuf.IMessage<TSubscribeRequest>
        where TSubscribeReply : Google.Protobuf.IMessage<TSubscribeReply>
    {
        private EventHandler<TLocalClass> _subscriber;

        private BitmexUserContext.TestDelegate _te;
        private readonly ILogger<TService> _logger;

        private static Converter<TLocalClass, TSubscribeReply> _converter;

        public SubscriptionService(BitmexUserContext.TestDelegate te, ILogger<TService> logger, Converter<TLocalClass, TSubscribeReply> converter)
        {
            _te = te;
            _logger = logger;
            _converter = converter;
        }

        public SubscriptionService(EventHandler<TLocalClass> subscriber, ILogger<TService> logger, Converter<TLocalClass, TSubscribeReply> converter)
        {
            _logger = logger;
            _subscriber = subscriber;
            _converter = converter;
        }

        public async Task Subscribe(TSubscribeRequest request, IServerStreamWriter<TSubscribeReply> responseStream, ServerCallContext context)
        {
            _te += async (sender, args) =>
            {
                /*                _logger.LogInformation($"Recieved Order {args.Changed} ");
                */
                await WriteStreamAsync(responseStream, _converter(args));
            };

            _subscriber += async (sender, args) =>
            {
                /*                _logger.LogInformation($"Recieved Order {args.Changed} ");
                */
                await WriteStreamAsync(responseStream, _converter(args));
            };
            await AwaitCancellation(context.CancellationToken);

        }

        private async Task WriteStreamAsync(IServerStreamWriter<TSubscribeReply> stream, TSubscribeReply reply)
        {
            try
            {
                await stream.WriteAsync(reply);
            }
            catch (Exception exception)
            {
                //TODO что делать когда разорвется соеденение ?
                _logger.LogWarning("Connection was interrupted by network services.");
            }
        }

        private static Task AwaitCancellation(CancellationToken token)
        {
            var completion = new TaskCompletionSource<object>();
            token.Register(() => completion.SetResult(null));
            return completion.Task;
        }
    }
}
