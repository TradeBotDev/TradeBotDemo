using Bitmex.Client.Websocket;
using Bitmex.Client.Websocket.Client;
using Bitmex.Client.Websocket.Websockets;
using Grpc.Net.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Prometheus;
using Prometheus.SystemMetrics;
using StackExchange.Redis;
using System;
using TradeBot.Account.AccountService.v1;
using TradeMarket.Clients;
using TradeMarket.DataTransfering.Bitmex.Rest.Client;
using TradeMarket.Model.TradeMarkets;
using TradeMarket.Model.UserContexts.Builders;
using TradeMarket.Services;

namespace TradeMarket
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddSingleton(new AccountClient(new ExchangeAccess.ExchangeAccessClient(GrpcChannel.ForAddress(Environment.GetEnvironmentVariable("ACCOUNT_CONNECTION_STRING")))));
            services.AddSingleton<IConnectionMultiplexer>(options => ConnectionMultiplexer.Connect(Environment.GetEnvironmentVariable("REDIS_CONNECTION_STRING")));
            services.AddSingleton<TradeMarketFactory>();
            services.AddSingleton<ContextDirector>();
            services.AddSingleton<CommonContextBuilder>();
            services.AddSingleton<ContextBuilder>();
            services.AddSingleton((sp) => {
                var communicator = new BitmexWebsocketCommunicator(BitmexValues.ApiWebsocketTestnetUrl);
                var res = new BitmexWebsocketClient(communicator);
                communicator.Start();
                return res;
            });
            services.AddSingleton(new RestfulClient(BitmexRestufllLink.Testnet));
            services.AddSystemMetrics();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();
            app.UseGrpcMetrics();

            app.UseEndpoints(endpoints =>
            {
                //endpoints.MapGrpcService<GreeterService>();
                endpoints.MapGrpcService<TradeMarketService>();


                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
