using System.Text;
using ExpenseManager.API.Middleware;
using ExpenseManager.Application;
using ExpenseManager.Infrastructure;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

namespace ExpenseManager.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // 1. Serilog
            builder.Host.UseSerilog((context, config) =>
            {
                config
                    .ReadFrom.Configuration(context.Configuration)
                    .WriteTo.Console()
                    .WriteTo.File("logs/expense-manager-.txt",
                        rollingInterval: RollingInterval.Day);
            });

            // Add services to the container.

            // 2. CORS
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("NextJsPolicy", policy =>
                {
                    var allowedOrigins = builder.Configuration
                        .GetSection("Cors:AllowedOrigins")
                        .Get<string[]>()!;

                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod();
                });
            });

            // 3. JWT Authentication
            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                        ValidateAudience = true,
                        ValidAudience = builder.Configuration["JwtSettings:Audience"],
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = new SymmetricSecurityKey(
                            Encoding.UTF8.GetBytes(
                                builder.Configuration["JwtSettings:Secret"]!))
                    };
                });

            builder.Services.AddControllers();
            builder.Services.AddFluentValidationAutoValidation();


            // 4. Swagger with JWT Support
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "ExpenseManager API",
                    Version = "v1"
                });
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,
                    Scheme = "bearer",
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "Enter 'Bearer {token}'"
                });
                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });
            });

            // 5. Dependency Injection
            builder.Services.AddApplication();
            builder.Services.AddInfrastructure(builder.Configuration);

            var app = builder.Build();

            // 6. Middleware Pipeline Order
            // Order is critical — do not change
            app.UseMiddleware<GlobalExceptionMiddleware>(); // must be first
            app.UseSerilogRequestLogging();

            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseHttpsRedirection();
            }
            app.UseCors("NextJsPolicy");
            app.UseAuthentication();  // must come before UseAuthorization
            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
