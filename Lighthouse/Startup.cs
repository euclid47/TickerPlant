using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http.Connections;
using Lighthouse.Managers;
using System.Threading;
using System.Net.WebSockets;
using TickerPlant.Interfaces;
using TickerPlant;
using Lighthouse.Interfaces;

namespace Lighthouse
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddSingleton<IPlant, Plant>();
			services.AddSingleton<IUserManager, UserManager>();

			services.Configure<CookiePolicyOptions>(options =>
			{
				// This lambda determines whether user consent for non-essential cookies is needed for a given request.
				options.CheckConsentNeeded = context => true;
				options.MinimumSameSitePolicy = SameSiteMode.None;
			});


			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IHostingEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Home/Error");
			}

			app.UseStaticFiles();
			app.UseCookiePolicy();
			app.UseWebSockets(new Microsoft.AspNetCore.Builder.WebSocketOptions { KeepAliveInterval = TimeSpan.FromMinutes(2), ReceiveBufferSize = 4 * 1024 });

			app.Use(async (context, next) => {
				if (context.Request.Path == @"/ws" && context.WebSockets.IsWebSocketRequest)
				{
					var socket = await context.WebSockets.AcceptWebSocketAsync();
					var userManager = app.ApplicationServices.GetRequiredService<IUserManager>();
					userManager.AddSocket(context.Connection.Id, socket);

					do
					{
					} while (!socket.CloseStatus.HasValue);

					try
					{
						await socket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None);
					}
					catch (Exception)
					{

					}
					finally
					{
						socket.Dispose();
					}
				}
			});

			app.UseMvc(routes =>
			{
				routes.MapRoute(
					name: "default",
					template: "{controller=Home}/{action=Index}/{id?}");
			});
		}
	}
}
