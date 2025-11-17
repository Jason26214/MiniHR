
using Autofac.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using MiniHR.Infrastructure.Persistence;

namespace MiniHR.WebAPI
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // setting autofac
            builder.Host.UseServiceProviderFactory(new AutofacServiceProviderFactory());

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

            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            /* app */
            var app = builder.Build();

            // Configure the HTTP request pipeline.
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
