using HomeLibrary.Core.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Text;

namespace HomeLibrary.Data
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDataLayer(this IServiceCollection services, string connectionString)
        {
            services.AddScoped<IBookRepository>(_ => new BookRepository(connectionString));
            return services;
        }
    }
}
