using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TradeMarket;
using TradeBot.Common;
using static TradeMarket.UIInfoService;

namespace Facade
{
    public class UIInfoService : TradeMarket.UIInfoService.UIInfoServiceBase
    {
        //private UIInfoServiceClient clientTM = new UIInfoServiceClient(GrpcChannel.ForAddress(""/*����� ��*/));
        //TODO ������� ����� ���� � ������� ���� private UIInfoServiceClient clientRelay = new UIInfoServiceClient(GrpcChannel.ForAddress(""/*����� Relay*/));
        private readonly ILogger<UIInfoService> _logger;
        public UIInfoService(ILogger<UIInfoService> logger)
        {
            _logger = logger;
        }
        /// <summary>
        /// �������� �� ������
        /// </summary>
        /// TODO �� ������� ������� 
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<SubscribeBalanceReply> SubscribeBalance(SubscribeBalanceRequest request, ServerCallContext context)
        {
            //var response = clientTM.SubscribeBalance(new SubscribeBalanceRequest { });
            return Task.FromResult(new SubscribeBalanceReply
            {
                //TODO ��� �������
            });
        }
        /// <summary>
        /// �������� �� ����
        /// </summary>
        /// TODO �����?
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<SubscribeLogsReply> SubscribeLogs(SubscribeLogsRequest request, ServerCallContext context)
        {
            //var response = clientTM.SubscribeBalance(new SubscribeBalanceRequest { });
            return Task.FromResult(new SubscribeLogsReply
            {
                //TODO ��� �������
            });
        }

        /// <summary>
        /// �������� ����� ������������ � ������ ������
        /// </summary>
        /// TODO ������������ � ��-� 
        /// <param name="request"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        public override Task<AuthenticateTokenReply> AuthenticateToken(AuthenticateTokenRequest request, ServerCallContext context)
        {
            //var response = clientTM.AuthenticateToken(new AuthenticateTokenRequest {Token = request.Token});

            return Task.FromResult(new AuthenticateTokenReply
            {
                
                Reply = new DefaultReply { Message = "asd", Code = ReplyCode.Succeed }
                //Reply = response.Reply
            }); 
        }

    }

}
