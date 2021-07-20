using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TradeMarket.DataTransfering;
using TradeMarket.Model;

namespace TradeMarket.Services
{
    /// <summary>
    /// Класс который отвечает на ответы запросов по подписке .
    /// </summary>
    ///
    //TODO LocalClass такое себе название !
    public class SubscriptionService<TSubscribeRequest, TSubscribeReply,TLocalClass, TService>
        where TSubscribeRequest : Google.Protobuf.IMessage<TSubscribeRequest>
        where TSubscribeReply : Google.Protobuf.IMessage<TSubscribeReply>
    {
        private readonly IPublisher<TLocalClass> _subscriber;

        private readonly ILogger<TService> _logger;

        private static Converter<TLocalClass, TSubscribeReply> _converter;

        public SubscriptionService(IPublisher<TLocalClass> subscriber, ILogger<TService> logger, Converter<TLocalClass, TSubscribeReply> converter)
        {
            _logger = logger;
            _subscriber = subscriber;
            _converter = converter;

        }

        public async Task Subscribe(TSubscribeRequest request, IServerStreamWriter<TSubscribeReply> responseStream, ServerCallContext context)
        {
            _subscriber.Changed += async (sender, args) =>
            {
                _logger.LogInformation($"Recieved Order {args.Changed} ");

                await WriteStreamAsync(responseStream, _converter(args.Changed));
            };
            await AwaitCancellation(context.CancellationToken);

        }

        public async Task WriteStreamAsync(IServerStreamWriter<TSubscribeReply> stream, TSubscribeReply reply)
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
