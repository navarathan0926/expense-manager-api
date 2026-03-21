using ExpenseManager.Infrastructure.Persistence;
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

			return services;
		}
	}
}
