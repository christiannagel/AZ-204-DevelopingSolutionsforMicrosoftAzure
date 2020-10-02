using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace WebAppUsingCache
{
    public class Startup
    {
        public IConfiguration Configuration { get;  }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddSession().AddStackExchangeRedisCache(setup =>
            {
                setup.Configuration = Configuration["RedisConnection"];
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseSession();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGet("session1", async context =>
                {
                    context.Session.SetString("mydata", "42");
                    await context.Response.WriteAsync("<h1>session set</h1>");
                });
                endpoints.MapGet("session2", async context =>
                {
                    var value = context.Session.GetString("mydata");
                    await context.Response.WriteAsync($"<h1>session read: {value}</h1>");
                });

                endpoints.MapGet("/", async context =>
                {
                    await context.Response.WriteAsync("Hello World!");
                });
            });
        }
    }
}
