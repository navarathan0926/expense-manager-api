using ExpenseManager.Application.Interfaces;
using ExpenseManager.Application.Repositories;
using ExpenseManager.Infrastructure.Persistence;
using ExpenseManager.Infrastructure.Repositories;
using ExpenseManager.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseManager.Infrastructure
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddInfrastructure(
		this IServiceCollection services,
		IConfiguration configuration)
		{
			services.AddDbContext<ExpenseManagerDbContext>(options =>
					options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
					npgsql => npgsql.MigrationsAssembly(typeof(ExpenseManagerDbContext).Assembly.FullName)
				)
			);

            services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
            services.AddScoped<IExpenseRepository, ExpenseRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddScoped<ICsvExportService, CsvExportService>();
            services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
            services.AddScoped<IPasswordHasher, PasswordHasher>();

			return services;
		}
	}
}
