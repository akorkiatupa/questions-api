using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DbUp;
using netcore_api.Data;
using netcore_api.Hubs;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using netcore_api.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace netcore_api
{
    public class Startup
    {

        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            this.Configuration = configuration;
        }

        

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            
            //TODO: is there a  way to find out if debug runtime is docker?
            //var connectionString = this.Configuration.GetConnectionString("DefaultConnectionDocker");
            var connectionString = this.Configuration.GetConnectionString("DefaultConnection");

            // this line upserts database, if db defined in connection string does't exist it creates it
            EnsureDatabase.For.SqlDatabase(connectionString);
            
            // deploy changes to sql database with scripts embedded in to the project, as db transaction. ( only as upsert )
            var upgrader = DeployChanges.To.SqlDatabase(connectionString).WithScriptsEmbeddedInAssembly(System.Reflection.Assembly.GetExecutingAssembly()).WithTransaction().Build();
            if(upgrader.IsUpgradeRequired())
            {
                upgrader.PerformUpgrade();
            }

            services.AddControllers();

            // AddScoped - Instance lasts for whole HTTP request lifecycle.
            // AddTransient - New instance is created every time when requested.
            // AddSingleton - Only one class instance for the lifetime of the whole app.
            services.AddScoped<IDataRepository, DataRepository>();

            // Add Cross origin policy, accept everything from dev environment
            services.AddCors(options => options.AddPolicy("CorsPolicy", builder => builder.AllowAnyMethod().AllowAnyHeader().WithOrigins(new string[] { "http://127.0.0.1:3000", "http://localhost:3000" }).AllowCredentials()));

            services.AddSignalR();
            services.AddMemoryCache();

            services.AddSingleton<IQuestionCache, QuestionCache>();

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme =
                  JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme =
                  JwtBearerDefaults.AuthenticationScheme;
            }).AddJwtBearer(options =>
            {
                options.Authority = this.Configuration["Auth0:Authority"];
                options.Audience = this.Configuration["Auth0:Audience"];
            });

            services.AddHttpClient();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("MustBeQuestionAuthor", policy => policy.Requirements.Add(new MustBeQuestionAuthorRequirement()));
            });

            services.AddScoped<IAuthorizationHandler, MustBeQuestionAuthorHandler>();
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {

            app.UseCors("CorsPolicy");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();               
            }

            app.UseHttpsRedirection();

            app.UseRouting();
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers(); // maps controllers
                endpoints.MapHub<QuestionsHub>("/questionshub"); // maps websoket to /questionshub
            });
        }
    }
}
