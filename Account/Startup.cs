using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.IO;
using Account.Models;
using System.Text.Json;

namespace Account
{
    public class Startup
    {
        // Конструктор перед запуском получает всю информацию о состоянии входов в аккаунт из файла.
        public Startup()
        {
            if (File.Exists(State.LoggedInFilename))
            {
                var state = FileManagement.ReadFile<Dictionary<string, LoggedAccount>>(State.LoggedInFilename);
                State.loggedIn = state;
            }
            else
            {
                State.loggedIn = new Dictionary<string, LoggedAccount>();
                FileManagement.WriteFile(State.LoggedInFilename, State.loggedIn);
            }
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddGrpc();
            services.AddDbContext<AccountContext>();
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
                endpoints.MapGrpcService<AccountService>();
                endpoints.MapGrpcService<ExchangeAccessService>();

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Communication with gRPC endpoints must be made through a gRPC client. To learn how to create a client, visit: https://go.microsoft.com/fwlink/?linkid=2086909");
                });
            });
        }
    }
}
