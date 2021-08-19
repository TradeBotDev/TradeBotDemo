using Former.Clients;
using Former.Models;
using Former.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Former
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();

            //проверяет, есть в редисе контексты формера, и если они есть, то это значит что сервис был остановлен аварийно
            //и следует его запустить с контекстами и настройками из редиса
            var contexts = RedisClient.ReadMeta().Result;
            foreach (var ctx in contexts)
            {
                var configuration = RedisClient.ReadConfiguration(ctx).Result;
                var userContext = Contexts.GetUserContext(ctx.Sessionid, ctx.Trademarket, ctx.Slot);
                userContext.SetConfiguration(configuration);
                userContext.SubscribeStorageToMarket();
            }
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<ListenerImpl>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
