using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniHR.Application.Profiles;
using MiniHR.Infrastructure.Persistence;
using MiniHR.WebAPI.Middleware;
using MiniHR.WebAPI.Modules;

namespace MiniHR.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // setting autofac
            #region autofac
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                containerBuilder.RegisterModule(new AutofacModule());
            });
            #endregion

            // CORS
            #region CORS
            var corsPolicyName = "AllowSpecificOrigin";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: corsPolicyName,
                                  policy =>
                                  {
                                      policy.WithOrigins("http://localhost:3000")
                                            .WithMethods("GET", "POST", "PUT", "DELETE")
                                            .WithHeaders("Authorization", "Content-Type");
                                  });
            });
            #endregion

            // Add services to the container.

            // DbContext
            #region DbContext
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<MiniHrDbContext>(options =>
                options.UseNpgsql(connectionString));
            #endregion

            // AutoMapper registration
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            }, typeof(MappingProfile).Assembly);

            builder.Services.AddControllers();

            // To disable automatic model state validation
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            /* app */
            var app = builder.Build();

            app.UseMiddleware<ExceptionMiddleware>();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors(corsPolicyName);

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
