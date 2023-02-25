using Hammerfest.GameSpy.QueryReport;
using Microsoft.Extensions.DependencyInjection;

namespace Hammerfest.GameSpy;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddGameSpyServices(this IServiceCollection services) =>
        services.AddHostedService<QueryReportServer>();
}
