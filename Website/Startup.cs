using System;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Prometheus;
using Prometheus.SystemMetrics;
using Serilog;

namespace Website
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;

			// Получение строки подключения к Seq.
			string seqConnection = Environment.GetEnvironmentVariable("SEQ_CONNECTION_STRING");
			if (seqConnection == null)
				seqConnection = configuration.GetConnectionString("Seq");

			// Добавление нового логгера, который будет выводить всю информацию в консоль.
			Log.Logger = new LoggerConfiguration()
				.Enrich.FromLogContext()
				.MinimumLevel.Debug()
				.WriteTo.Console()
				.WriteTo.Seq(seqConnection)
				.CreateLogger();
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme).AddCookie(options =>
			{
				options.LoginPath = new PathString("/authorization/login");
				options.LogoutPath = new PathString("/authorization/logout");
			});
			services.AddControllersWithViews();
			services.AddServerSideBlazor();
			services.AddHttpContextAccessor();
			services.AddSystemMetrics();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();
			app.UseStaticFiles();
			app.UseRouting();
			app.UseAuthentication();
			app.UseAuthorization();
			app.UseHttpMetrics();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapControllerRoute(
					name: "default",
					pattern: "{controller=Home}/{action=Index}/{id?}");
				endpoints.MapBlazorHub();
			});
		}
	}
}
