namespace OrderService.Extensions;

public static class ApiDocumentationExtensions
{
    public static void AddApiDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc(
                    "v1",
                    new()
                    {
                        Title = "Discussion Platform API",
                        Description = "REST API for discussion platform",
                        Version = "1.0"
                    }
                );
            }
        );
    }

    public static void UseApiDocumentation(this IApplicationBuilder app, IWebHostEnvironment environment)
    {
        if (!environment.IsDevelopment())
        {
            return;
        }

        app.UseSwagger();
        app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders API v1");
                options.RoutePrefix = "swagger";
            }
        );
    }
}