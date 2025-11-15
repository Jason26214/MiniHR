
using Autofac.Extensions.DependencyInjection;

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

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
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
