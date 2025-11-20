namespace MiniHR.WebAPI.Extensions
{
    public static class AuthorizationExtensions
    {
        public static IServiceCollection AddMiniHRAuthorization(this IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminOnly", policy =>
                {
                    policy.RequireRole("Admin");
                });
            });

            return services;
        }
    }
}
