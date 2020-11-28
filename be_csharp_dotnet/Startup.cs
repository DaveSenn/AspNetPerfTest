using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TodoList.Controllers;

namespace TodoList
{
    public class Startup
    {
        private const String TodoListOrigins = "_TodolistOrigins";
        private readonly IConfiguration _configuration;

        public Startup( IConfiguration configuration ) =>
            _configuration = configuration;

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure( IApplicationBuilder app, IHostEnvironment env )
        {
            if ( env.IsDevelopment() )
                app.UseDeveloperExceptionPage();

            app.UseCors( TodoListOrigins );
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseEndpoints( endpoints => endpoints.MapControllers() );
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices( IServiceCollection services )
        {
            services
                .AddCors( options => options
                              .AddPolicy( TodoListOrigins,
                                          builder => builder
                                                     .WithOrigins( "http://localhost:3000" )
                                                     .AllowAnyHeader()
                                                     .AllowAnyMethod() ) );
            services.AddControllers();

            services.AddSingleton<IRepo, Repo>();
            //services.AddTransient<IRepo, Repo>();
        }
    }
}