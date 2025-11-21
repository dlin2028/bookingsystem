using BookingSystem.DataAccess;
using BookingSystem.DataAccess.InMemory;
using BookingSystem.DataAccess.Sql;
using BookingSystem.Services.Booking;
using BookingSystem.Services.Payment;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace BookingSystem
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            ConfigureServices(builder.Services, builder.Configuration);

            var app = builder.Build();

            if (true) //eventually change to use app.Environment.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Booking System API v1"));
            }

            app.UseHttpsRedirection();
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }

        private static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddControllers()
                .AddJsonOptions(options =>
                {
                    options.JsonSerializerOptions.PropertyNamingPolicy = null;
                    options.JsonSerializerOptions.WriteIndented = true;
                    options.JsonSerializerOptions.Converters.Add(new Models.Seating.SeatingTypeJsonConverter());
                });

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
                {
                    Title = "Booking System API",
                    Version = "v1",
                    Description = "Event booking system with OOD, payment integration, and advanced data access"
                });

                var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = System.IO.Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });

            var dataAccessMode = configuration.GetValue<string>("DataAccessMode") ?? "InMemory";

            if (dataAccessMode.Equals("SQL", StringComparison.OrdinalIgnoreCase))
            {
                var connectionString = configuration.GetConnectionString("DefaultConnection");
                if (string.IsNullOrEmpty(connectionString))
                {
                    throw new InvalidOperationException(
                        "SQL mode requires a connection string named 'DefaultConnection' in appsettings.json");
                }

                services.AddScoped<IUserRepository>(provider => new SqlUserRepository(connectionString));
                services.AddScoped<IVenueRepository>(provider => new SqlVenueRepository(connectionString));
                services.AddScoped<IEventRepository>(provider => new SqlEventRepository(connectionString));
                services.AddScoped<IBookingRepository>(provider => new SqlBookingRepository(connectionString));
            }
            else
            {
                services.AddSingleton<IUserRepository, InMemoryUserRepository>();
                services.AddSingleton<IVenueRepository, InMemoryVenueRepository>();
                services.AddSingleton<IEventRepository, InMemoryEventRepository>();
                services.AddSingleton<IBookingRepository, InMemoryBookingRepository>();
            }

            var paymentMode = configuration.GetValue<string>("PaymentMode") ?? "Simulated";

            if (paymentMode.Equals("External", StringComparison.OrdinalIgnoreCase))
            {
                var paymentApiBaseUrl = configuration.GetValue<string>("PaymentApiBaseUrl");
                if (string.IsNullOrEmpty(paymentApiBaseUrl))
                {
                    throw new InvalidOperationException(
                        "External payment mode requires 'PaymentApiBaseUrl' in appsettings.json");
                }

                services.AddHttpClient<IPaymentGateway, PaymentGateway>((serviceProvider, client) =>
                {
                    client.BaseAddress = new Uri(paymentApiBaseUrl);
                    client.Timeout = TimeSpan.FromSeconds(30);
                });
            }
            else
            {
                services.AddScoped<IPaymentGateway, SimulatedPaymentGateway>();
            }

            services.AddScoped<IBookingService, BookingService>();

            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder.AllowAnyOrigin()
                           .AllowAnyMethod()
                           .AllowAnyHeader();
                });
            });
        }
    }
}

public partial class Program { }
