using Akka.Actor;
using Akka.Bootstrap.Docker;
using AutoMapper;
using Coravel;
using DataAccess.DTO;
using DataAccess.Models;
using DataAnalyticsPlatform.Actors.Utils;
using DataAnalyticsPlatform.Shared.DataModels;
using LoadServiceApi.Load;
using LoadServiceApi.Previews;
using LoadServiceApi.Shared;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace LoadServiceApi
{
    /// <summary>
    /// Startup class for configuring services and the app's request pipeline
    /// </summary>
    public class Startup
    {
        /// <summary>
        /// Initializes a new instance of the Startup class
        /// </summary>
        /// <param name="configuration">Application configuration</param>
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        }

        /// <summary>
        /// Gets the configuration for the application
        /// </summary>
        public IConfiguration Configuration { get; }

        /// <summary>
        /// This method gets called by the runtime. Use this method to add services to the container.
        /// </summary>
        /// <param name="services">The service collection</param>
        public void ConfigureServices(IServiceCollection services)
        {
            // Configure authentication
            services.ConfigureAuth(Configuration);
            
            // Configure Actor System
            services.AddSingleton<ActorSystem>(_ => ActorSystem.Create("PreviewService"));
            var config = HoconLoader.ParseConfig("webapi.hocon");
            services.AddSingleton<ActorSystem>(_ => ActorSystem.Create("dap-actor-system", config.BootstrapFromDocker()));
            
            // Add API services
            services.AddPreviewServices();
            services.AddLoadDataService();
            services.AddControllers().AddNewtonsoftJson();
            services.AddSignalR();
            services.AddQueue();
            
            // Configure CORS
            services.AddCors(options =>
            {
                options.AddPolicy("CorsPolicy",
                    builder => builder
                        .AllowAnyOrigin()
                        .AllowAnyMethod()
                        .AllowAnyHeader());
            });
            
            // Configure AutoMapper
            services.AddAutoMapper(cfg =>
            {
                cfg.CreateMap<SchemaDTO, ProjectSchema>();
                cfg.CreateMap<SchemaModelDTO, SchemaModel>();
                cfg.CreateMap<ModelMetadataDTO, ModelMetadata>();
                cfg.CreateMap<ProjectFileDTO, ProjectFile>();
                cfg.CreateMap<ReaderDTO, Reader>();
                cfg.CreateMap<JobDTO, Job>();
            }, AppDomain.CurrentDomain.GetAssemblies());
            
            // Configure MVC
            services.AddMvc(option => option.EnableEndpointRouting = false);
            
            // Configure connection strings
            var connectionString = Configuration.GetConnectionString("localDb");
            var postgresConnectionString = Configuration.GetConnectionString("postgresdb");
            var elasticSearchString = Configuration.GetConnectionString("elasticSearch");
            var mongoDB = Configuration.GetConnectionString("mongoDB");
            
            services.Configure<ConnectionStringsConfig>(option => 
            {
                option.DefaultConnection = connectionString;
                option.PostgresConnection = postgresConnectionString;
                option.ElasticSearchString = elasticSearchString;
                option.MongoDBString = mongoDB;
            });
            
            // Configure database context
            services.AddDbContext<DAPDbContext>(options => options.UseSqlServer(connectionString));
            services.AddScoped<IRepository, Repository>();
        }

        /// <summary>
        /// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        /// </summary>
        /// <param name="app">The application builder</param>
        /// <param name="env">The hosting environment</param>
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days for production scenarios
                app.UseHsts();
            }
            
            // Configure CORS
            app.UseCors(builder =>
            {
                builder.AllowAnyOrigin()
                       .AllowAnyHeader()
                       .AllowAnyMethod();
            });
            
            // Configure authentication
            app.UseAuthentication();
            
            // Configure MVC
            app.UseMvc();
        }
    }
}
