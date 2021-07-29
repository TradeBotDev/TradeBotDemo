using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using System.Collections.Generic;
using System.IO;
using Serilog;

using AccountGRPC.Models;

namespace AccountGRPC
{
    public class Startup
    {
        // Конструктор перед запуском получает всю информацию о состоянии входов в аккаунт из файла.
        public Startup()
        {
            // Добавление нового логгера, который будет выводить всю информацию в консоль.
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .WriteTo.Seq("http://localhost:5341")
                .CreateLogger();

            // Проверяется существование файла, в котором хранятся данные. В случае, если он существует,
            // производится его чтение и запись состояния в коллекцию вошедших пользователей.
            if (File.Exists(State.LoggedInFilename))
            {
                var state = FileManagement.ReadFile<Dictionary<string, LoggedAccount>>(State.LoggedInFilename);
                State.loggedIn = state;
                Log.Information("Запуск сервиса Account: файл с данными обнаружен. Произведено чтение данных.");
            }
            // Иначе выделяется память под коллекцию вошедших пользователей и она сразу же записывается в файл.
            else
            {
                State.loggedIn = new Dictionary<string, LoggedAccount>();
                FileManagement.WriteFile(State.LoggedInFilename, State.loggedIn);
                Log.Information("Запуск сервиса Account: файл с данными не найден. Произведено создание нового файла.");
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
