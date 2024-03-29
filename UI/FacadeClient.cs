﻿using System;
using System.Threading.Tasks;
using System.Windows.Forms;
using Grpc.Core;
using Grpc.Net.Client;
using TradeBot.Common.v1;
using TradeBot.Facade.FacadeService.v1;
using UpdateServerConfigRequest = TradeBot.Facade.FacadeService.v1.UpdateServerConfigRequest;
using System.Threading;

namespace UI
{
    class FacadeClient
    {
        private readonly FacadeService.FacadeServiceClient _client = new(GrpcChannel.ForAddress("http://23.88.34.174:5002"));
        private Metadata _meta;

        public delegate void OrdersUpdate(PublishOrderEvent order, Metadata metadata);
        public OrdersUpdate HandleOrderUpdate;

        public delegate void BalanceUpdate(PublishBalanceEvent balance, Metadata metadata);
        public BalanceUpdate HandleBalanceUpdate;
        
        private CancellationTokenSource _token;

        public async Task<bool> RegisterLicense()
        {
            var sessionId = _meta.GetValue("sessionid");
            try
            {
                var checkHasLicense = await _client.CheckLicenseAsync(new()
                {
                    Product = ProductCode.Tradebot,
                    SessionId = sessionId
                });
                if (!checkHasLicense.HaveAccess)
                {
                    var setLicenseResponse = await _client.SetLicenseAsync(new()
                    {
                        CardNumber = "4276450016026862",
                        Cvv = 123,
                        Date = DateTime.Now.AddYears(2).Second,
                        Product = ProductCode.Tradebot,
                        SessionId = sessionId,
                    });
                    return setLicenseResponse.Code == LicenseCode.Successful || setLicenseResponse.Code == LicenseCode.HaveAccess;
                }
                else
                {
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

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
            _token = new CancellationTokenSource();
            using var call = _client.SubscribeEvents(new SubscribeEventsRequest { Sessionid = _meta.GetValue("sessionid") }, _meta);
            var slotName = _meta.GetValue("slot");
            var sessionid = _meta.GetValue("sessionid");
            var responseHeaders = await call.ResponseHeadersAsync;
            while (await call.ResponseStream.MoveNext(_token.Token))
            {
                switch (call.ResponseStream.Current.EventTypeCase)
                {
                    case SubscribeEventsResponse.EventTypeOneofCase.Balance:
                        if (call.ResponseStream.Current.Balance.Slot == slotName &&
                            call.ResponseStream.Current.Balance.Sessionid == sessionid) 
                            HandleBalanceUpdate?.Invoke(call.ResponseStream.Current.Balance, responseHeaders);
                        break;
                    case SubscribeEventsResponse.EventTypeOneofCase.Order:
                        
                        if (call.ResponseStream.Current.Order.Slot == slotName &&
                            call.ResponseStream.Current.Order.Sessionid == sessionid) 
                            HandleOrderUpdate?.Invoke(call.ResponseStream.Current.Order, responseHeaders);
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
            _token.Cancel();
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
                var response = await _client.RegisterAsync(new RegisterRequest
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
                var sessionId = logResponse.SessionId;
                _meta = new Metadata
                {
                    { "sessionid", sessionId },
                    { "slot", "XBTUSD" },
                    { "trademarket", "bitmex" },
                    { "userid", logResponse.AccountId.ToString()}
                };

                await RegisterLicense();

                _client.AddExchangeAccess(new AddExchangeAccessRequest
                {
                    SessionId = sessionId,
                    Token = key,
                    Secret = secret,
                    Code = ExchangeAccessCode.Bitmex,
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
                await _client.LogoutAsync(new LogoutRequest { SessionId = _meta.GetValue("sessionid") });
                return new DefaultResponse { Code = ReplyCode.Succeed, Message = ""};
            }
            catch
            {
                return new DefaultResponse { Code = ReplyCode.Failure, Message = ""};
            }
        }
    }
}
