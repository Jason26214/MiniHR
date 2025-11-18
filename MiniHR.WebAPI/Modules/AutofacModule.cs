using Autofac;
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
        }
    }
}