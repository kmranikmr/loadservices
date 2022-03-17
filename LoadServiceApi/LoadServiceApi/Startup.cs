using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using DataAccess.DTO;
using DataAccess.Models;
using LoadServiceApi.Shared;
using LoadServiceApi;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Coravel;
using Akka.Actor;
using LoadServiceApi.Previews;
using DataAnalyticsPlatform.Actors.Utils;
using LoadServiceApi.Load;
using DataAnalyticsPlatform.Shared.DataModels;
using DataAnalyticsPlatform.Shared.DataAccess;
using Akka.Bootstrap.Docker;

namespace WebApplication1
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {

            services.ConfigureAuth(Configuration);
            services.AddSingleton<ActorSystem>(_ => ActorSystem.Create("PreviewService"));
            
            services.AddPreviewServices();
            services.AddControllers().AddNewtonsoftJson();
            var config = HoconLoader.ParseConfig("webapi.hocon");
            services.AddSingleton<ActorSystem>(_ => ActorSystem.Create("dap-actor-system", config.BootstrapFromDocker()));
            services.AddLoadDataService();
            services.AddSignalR();
            services.AddQueue();
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                    .AllowAnyOrigin()
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    );
            });//.AllowCredentials()
            services.AddAutoMapper(cfg =>
            {
                cfg.CreateMap<SchemaDTO, ProjectSchema>();
                cfg.CreateMap<SchemaModelDTO, SchemaModel>();
                cfg.CreateMap<ModelMetadataDTO, ModelMetadata>();
                cfg.CreateMap<ProjectFileDTO, ProjectFile>();
                cfg.CreateMap<ReaderDTO, Reader>();
                cfg.CreateMap<JobDTO, Job>();
            }, AppDomain.CurrentDomain.GetAssemblies());


            //services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            services.AddMvc(option => option.EnableEndpointRouting = false);
            var connectionString = Configuration.GetConnectionString("localDb");
            var postgresConnectionString = Configuration.GetConnectionString("postgresdb");
             var elasticSearchString = Configuration.GetConnectionString("elasticSearch");
            var mongoDB = Configuration.GetConnectionString("mongoDB");
            services.Configure<ConnectionStringsConfig>(option => option.DefaultConnection = connectionString );
            services.Configure<ConnectionStringsConfig>(option => option.PostgresConnection = postgresConnectionString);
            services.Configure<ConnectionStringsConfig>(option => option.ElasticSearchString = elasticSearchString);
            services.Configure<ConnectionStringsConfig>(option => option.MongoDBString = mongoDB);
            //  var repository = new PgRepository<object>(postgresConnectionString, "");
            //  repository.CreateSchema("schema1_1014_2");
            //var connection = @"Server=(localdb)\mssqllocaldb;Database=Blogging;Trusted_Connection=True;ConnectRetryCount=0";
            services.AddDbContext<DAPDbContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<IRepository, Repository>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();
            app.UseCors(
                     builder =>
                     {
                         builder.AllowAnyOrigin()
                         .AllowAnyHeader().AllowAnyMethod();
                     }
                     );//AllowCredentials();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            //app.UseEndpoints(routes =>
            //{

            //   routes.MapHub<ProgressHub>("/processing");
            //});
            // app.UseHttpsRedirection();
            app.UseMvc();
        }
    }
}
