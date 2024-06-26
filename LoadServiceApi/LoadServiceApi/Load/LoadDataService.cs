using DataAnalyticsPlatform.Actors.Master;
using Microsoft.Extensions.DependencyInjection;

namespace LoadServiceApi.Load
{
    public static class LoadDataService
    {
        public static void AddLoadDataService(this IServiceCollection services)
        {

            services.AddSingleton<LoadActorProvider>();
            services.AddSingleton<LoadModels>();
        }
    }


}
