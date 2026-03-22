using AutoMapper;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;
using ExpenseManager.Application.Interfaces;
using ExpenseManager.Application.Services;

namespace ExpenseManager.Application
{
	public static class DependencyInjection
	{
		public static IServiceCollection AddApplication(
		this IServiceCollection services)
		{
			services.AddAutoMapper(Assembly.GetExecutingAssembly());
			services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IExpenseService, ExpenseService>();
            services.AddScoped<IReportService, ReportService>();
            services.AddScoped<IUserService, UserService>();

			return services;
		}
	}
}
