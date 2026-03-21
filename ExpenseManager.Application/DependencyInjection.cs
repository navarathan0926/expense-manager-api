using Microsoft.Extensions.DependencyInjection;

namespace ExpenseManager.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(
		this IServiceCollection services)
		{
			// AutoMapper, FluentValidation, Services will be registered here later

			return services;
		}
	}
}
