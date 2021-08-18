﻿using System;
using System.Threading.Tasks;
using Grpc.Core;
using Grpc.Net.Client;
using TradeBot.Common.v1;
using TradeBot.Facade.FacadeService.v1;
using UpdateServerConfigRequest = TradeBot.Facade.FacadeService.v1.UpdateServerConfigRequest;

namespace UI
{
    class FacadeClient
    {
        private readonly FacadeService.FacadeServiceClient _client = new(GrpcChannel.ForAddress("http://localhost:5002"));
        private Metadata _meta;

        public delegate void OrdersUpdate(PublishOrderEvent order);
        public OrdersUpdate HandleOrderUpdate;

        public delegate void BalanceUpdate(PublishBalanceEvent balance);
        public BalanceUpdate HandleBalanceUpdate;

        public async Task<DefaultResponse> StartBot(string slotName, Config configuration)
        {
            _meta[1] = new Metadata.Entry("slot", slotName);
            try
            {
                await _client.StartBotAsync(new SwitchBotRequest { Config = configuration }, _meta);
                await _client.UpdateServerConfigAsync(
                    new UpdateServerConfigRequest
                    {
                        Request = new TradeBot.Common.v1.UpdateServerConfigRequest
                            { Config = configuration, Switch = false }
                    }, _meta);
                SubscribeEvents();
                return new DefaultResponse { Code = ReplyCode.Succeed, Message = ""};
            }
            catch
            {
                return new DefaultResponse { Code = ReplyCode.Failure, Message = ""};
            }
        }

        private async Task SubscribeEvents()
        {
            using var call = _client.SubscribeEvents(new SubscribeEventsRequest { Sessionid = _meta.GetValue("sessionid") }, _meta);

            while (await call.ResponseStream.MoveNext())
            {
                switch (call.ResponseStream.Current.EventTypeCase)
                {
                    case SubscribeEventsResponse.EventTypeOneofCase.Balance:
                        HandleBalanceUpdate?.Invoke(call.ResponseStream.Current.Balance);
                        break;
                    case SubscribeEventsResponse.EventTypeOneofCase.Order:
                        HandleOrderUpdate?.Invoke(call.ResponseStream.Current.Order);
                        break;
                    case SubscribeEventsResponse.EventTypeOneofCase.None:
                        break;
                    default:
                        throw new ArgumentOutOfRangeException();
                }
            }
        }

        public async Task<DefaultResponse> RemoveMyOrders()
        {
            try
            {
                await _client.DeleteOrderAsync(new DeleteOrderRequest(), _meta);
                return new DefaultResponse { Code = ReplyCode.Succeed, Message = ""};
            }
            catch
            {
                return new DefaultResponse { Code = ReplyCode.Failure, Message = ""};
            }
        }

        public async Task<DefaultResponse> UpdateConfig(Config configuration)
        {
            try
            {
                await _client.UpdateServerConfigAsync(new UpdateServerConfigRequest { Request = new TradeBot.Common.v1.UpdateServerConfigRequest { Config = configuration, Switch = false } }, _meta);
                return new DefaultResponse { Code = ReplyCode.Succeed, Message = ""};
            }
            catch
            {
                return new DefaultResponse { Code = ReplyCode.Failure, Message = ""};
            }
        }

        public async Task<DefaultResponse> StopBot(string slotName, Config configuration)
        {
            _meta[1] = new Metadata.Entry("slot", slotName);
            try
            {
                await _client.StopBotAsync(new StopBotRequest { Request = new TradeBot.Common.v1.UpdateServerConfigRequest { Config = configuration, Switch = true } }, _meta);
                await _client.DeleteOrderAsync(new DeleteOrderRequest(), _meta);
                return new DefaultResponse { Code = ReplyCode.Succeed, Message = ""};
            }
            catch
            {
                return new DefaultResponse { Code = ReplyCode.Failure, Message = ""};
            }
        }

        public async Task<DefaultResponse> RegisterAccount(string email, string password, string verifypassword)
        {
            try
            {
                await _client.RegisterAsync(new RegisterRequest
                {
                    Email = email,
                    Password = password,
                    VerifyPassword = verifypassword
                });
                return new DefaultResponse { Code = ReplyCode.Succeed, Message = ""};
            }
            catch
            {
                return new DefaultResponse { Code = ReplyCode.Failure, Message = ""};
            }
        }

        public async Task<DefaultResponse> SigningIn(string login, string password, string key, string secret)
        {
            //чё за эксченж
            try
            {
                var logResponse = await _client.LoginAsync(new LoginRequest
                {
                    Email = login,
                    Password = password
                });
                //MessageBox.Show(password);
                var sessionId = logResponse.SessionId;
                _meta = new Metadata
                {
                    { "sessionid", sessionId },
                    { "slot", "XBTUSD" },
                    { "trademarket", "bitmex" }
                };

                _client.AddExchangeAccess(new AddExchangeAccessRequest
                {
                    SessionId = sessionId,
                    Token = key,
                    Secret = secret,
                    Code = ExchangeCode.Bitmex,
                    ExchangeName = "BitMEX"
                });
                return new DefaultResponse { Code = ReplyCode.Succeed, Message = sessionId};
            }
            catch
            {
                return new DefaultResponse { Code = ReplyCode.Failure, Message = ""};
            }
        }

        public async Task<DefaultResponse> SigningOut()
        {
            try
            {
                await _client.LogoutAsync(new SessionRequest { SessionId = _meta.GetValue("sessionid") });
                return new DefaultResponse { Code = ReplyCode.Succeed, Message = ""};
            }
            catch
            {
                return new DefaultResponse { Code = ReplyCode.Failure, Message = ""};
            }
            
        }


    }
}
