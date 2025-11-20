using Autofac;
using MiniHR.Application.Services;
using MiniHR.Infrastructure.Repositories;

namespace MiniHR.WebAPI.Modules
{
    public class AutofacModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            var infrastructureAssembly = typeof(EmployeeRepository).Assembly;
            // Register repositories
            builder.RegisterAssemblyTypes(infrastructureAssembly)
                   .Where(t => t.Name.EndsWith("Repository"))
                   .AsImplementedInterfaces()
                   .InstancePerLifetimeScope();

            // Register Authentication services
            builder.RegisterAssemblyTypes(infrastructureAssembly)
                   .Where(t => t.Name.EndsWith("Hasher") || t.Name.EndsWith("Generator"))
                   .AsImplementedInterfaces()
                   .InstancePerLifetimeScope();

            // Register services
            var applicationAssembly = typeof(EmployeeService).Assembly;
            builder.RegisterAssemblyTypes(applicationAssembly)
                   .Where(t => t.Name.EndsWith("Service"))
                   .AsImplementedInterfaces()
                   .InstancePerLifetimeScope();
        }
    }
}