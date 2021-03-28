using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using DataAnalyticsPlatform.Actors;
using DataAnalyticsPlatform.Actors.Preview;
using DataAnalyticsPlatform.Shared.Models;

namespace LoadServiceApi.Previews
{
    public static class PreviewServices
    {
        public static void AddPreviewServices(this IServiceCollection services)
        {
            services.AddSingleton<PreviewActorProvider>();
            services.AddSingleton<PreviewRegistry>();
            services.AddSingleton<GetModels>();
            services.AddSingleton<GenerateModel>();
            services.AddSingleton<UpdateModel>();
            services.AddMemoryCache();
        }

    }
}
