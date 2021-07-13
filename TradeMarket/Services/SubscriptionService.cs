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
    public class SubscriptionService<SubscribeRequest, SubscribeReply,LocalClass, Service>
        where SubscribeRequest : Google.Protobuf.IMessage<SubscribeRequest>
        where SubscribeReply : Google.Protobuf.IMessage<SubscribeReply>
    {
        private Subscriber<LocalClass> _subscriber;

        private ILogger<Service> _logger;

        public static Converter<LocalClass, SubscribeReply> _converter;

        public SubscriptionService(Subscriber<LocalClass> subscriber, ILogger<Service> logger, Converter<LocalClass, SubscribeReply> converter)
        {
            _logger = logger;
            _subscriber = subscriber;
            _converter = converter;

        }

        public async Task Subscribe(SubscribeRequest request, IServerStreamWriter<SubscribeReply> responseStream, ServerCallContext context)
        {
            _subscriber.Changed += async (sender, args) =>
            {
                await WriteStreamAsync(responseStream, _converter(args.Changed));
            };
            await AwaitCancellation(context.CancellationToken);

        }

        public async Task WriteStreamAsync(IServerStreamWriter<SubscribeReply> stream, SubscribeReply reply)
        {
            try
            {
                await stream.WriteAsync(reply);
            }
            catch (Exception exception)
            {
                //TODO что делать когда разорвется соеденение ?
                _logger.LogWarning("Connection was interrupted by network servicies.");
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
