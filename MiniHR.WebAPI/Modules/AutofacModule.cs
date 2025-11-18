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
            builder.RegisterAssemblyTypes(infrastructureAssembly)
                   .Where(t => t.Name.EndsWith("Repository"))
                   .AsImplementedInterfaces()
                   .InstancePerLifetimeScope();

            var applicationAssembly = typeof(EmployeeService).Assembly;
            builder.RegisterAssemblyTypes(applicationAssembly)
                   .Where(t => t.Name.EndsWith("Service"))
                   .AsImplementedInterfaces() 
                   .InstancePerLifetimeScope();
        }
    }
}