using Microsoft.Extensions.DependencyInjection;
using EconomyViewerWeb.Application.Validators.Items;
using EconomyViewerWeb.Application.Mapping;
using FluentValidation;

namespace EconomyViewerWeb.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CreateItemRequestValidator>();

        services.AddAutoMapper(
            cfg => { },
            typeof(ItemProfile));

        return services;
    }
}
