using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TodoList.Models;

namespace TodoList
{
    public class Startup
    {
        readonly string TodolistOrigins = "_TodolistOrigins";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContextPool<TodoContext>(options => options.UseNpgsql(Configuration.GetConnectionString("DefaultConnection")));
            services.AddCors(options =>
                {
                    options.AddPolicy(TodolistOrigins,
                    builder =>
                    {
                        CorsPolicyBuilder corsPolicyBuilder = builder.WithOrigins("http://localhost:3000").AllowAnyHeader().AllowAnyMethod();
                    });
                });
            // For 3.0+ add argument `option => option.EnableEndpointRouting = false` to AddMvc
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
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseCors(TodolistOrigins);
            app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
