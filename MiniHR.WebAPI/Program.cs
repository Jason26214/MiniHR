using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniHR.Application.Profiles;
using MiniHR.Infrastructure.Persistence;
using MiniHR.WebAPI.ExceptionHandlers;
using MiniHR.WebAPI.Extensions;
using MiniHR.WebAPI.Modules;
using Serilog;

namespace MiniHR.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Serilog configuration
            // 1. Read configuration from appsettings (if any)
            // 2. Automatically record context information (Enrich)
            // 3. Output to console (Console)
            // 4. Output to file (File) - daily rolling, saved in the logs folder
            builder.Host.UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration)
                        .Enrich.FromLogContext()
                        .WriteTo.Console()
                        .WriteTo.File("logs/minihr.txt", rollingInterval: RollingInterval.Day));

            // setting autofac
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());
            builder.Host.ConfigureContainer<ContainerBuilder>(containerBuilder =>
            {
                containerBuilder.RegisterModule(new AutofacModule());
            });

            // CORS
            var corsPolicyName = "AllowSpecificOrigin";
            builder.Services.AddCors(options =>
            {
                options.AddPolicy(name: corsPolicyName, policy =>
                            {
                                policy.WithOrigins("http://localhost:3000")
                                    .WithMethods("GET", "POST", "PUT", "DELETE")
                                    .WithHeaders("Authorization", "Content-Type");
                            });
            });

            // DbContext
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<MiniHrDbContext>(options =>
                options.UseNpgsql(connectionString));

            // AutoMapper registration
            builder.Services.AddAutoMapper(cfg =>
            {
                cfg.AddProfile<MappingProfile>();
            }, typeof(MappingProfile).Assembly);

            // WebAPI.Extensions.IdentityServiceExtensions
            builder.Services.AddIdentityServices(builder.Configuration);

            builder.Services.AddControllers();

            // Register Global Exception Handler
            builder.Services.AddProblemDetails();
            builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

            // To disable automatic model state validation
            builder.Services.Configure<ApiBehaviorOptions>(options =>
            {
                options.SuppressModelStateInvalidFilter = true;
            });

            builder.Services.AddEndpointsApiExplorer();

            // MiniHR.WebAPI.Extensions.SwaggerServiceExtensions
            builder.Services.AddMiniHRSwagger();

            /* -------------------------------------- */
            /* app */
            var app = builder.Build();

            app.UseSerilogRequestLogging();

            app.UseExceptionHandler();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseCors(corsPolicyName);

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
