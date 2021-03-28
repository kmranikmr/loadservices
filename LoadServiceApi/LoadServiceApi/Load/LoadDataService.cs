using DataAnalyticsPlatform.Actors.Master;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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
