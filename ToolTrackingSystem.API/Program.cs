using Microsoft.EntityFrameworkCore;
using ToolTrackingSystem.API.Data;
using ToolTrackingSystem.API.Models.Entities;

namespace ToolTrackingSystem.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            // Configure the HTTP request pipeline
            ConfigureMiddleware(app, builder.Environment);

            // Initialize database and seed data
            await InitializeDatabaseAsync(app);

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            // Add DbContext with SQL Server
            services.AddDbContext<AppDbContext>(options =>
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection")));

            // Add controllers and JSON options
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null; // Preserve PascalCase
                });

            // Add Swagger/OpenAPI
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new()
                {
                    Title = "Tool Tracking System API",
                    Version = "v1",
                    Description = "API for managing tool inventory, issuance, and returns"
                });
            });

            // Add AutoMapper
            services.AddAutoMapper(typeof(Program));

            // Add CORS policy (adjust for production)
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });

            // Add application services (to be implemented)
            // services.AddScoped<IToolService, ToolService>();
            // services.AddScoped<IAuthService, AuthService>();
        }

        private static void ConfigureMiddleware(WebApplication app, IWebHostEnvironment env)
        {
            // Configure the HTTP request pipeline
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c =>
                {
                    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Tool Tracking API v1");
                });
            }

            app.UseHttpsRedirection();
            app.UseCors("AllowAll");
            app.UseRouting();

            // Add authentication/authorization middleware
            // app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllers();
        }

        private static async Task InitializeDatabaseAsync(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;

            try
            {
                var context = services.GetRequiredService<AppDbContext>();

                // Apply migrations (instead of EnsureCreated)
                await context.Database.MigrateAsync();

                // Seed initial data
                if (app.Environment.IsDevelopment())
                {
                    DbInitializer.Initialize(context);
                }
            }
            catch (Exception ex)
            {
                var logger = services.GetRequiredService<ILogger<Program>>();
                logger.LogError(ex, "An error occurred while initializing the database.");
                throw;
            }
        }
    }
}