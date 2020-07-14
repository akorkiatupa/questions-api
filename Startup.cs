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
            var connectionString = this.Configuration.GetConnectionString("DefaultConnection");

            EnsureDatabase.For.SqlDatabase(connectionString);
            // deploy changes to sql database with scripts embedded in to the project, as db transaction.
            var upgrader = DeployChanges.To.SqlDatabase(connectionString).WithScriptsEmbeddedInAssembly(System.Reflection.Assembly.GetExecutingAssembly()).WithTransaction().Build();
            // TODO - Do a database migration if there are any pending SQL 

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
            services.AddCors(options => options.AddPolicy("CorsPolicy", builder => builder.AllowAnyMethod().AllowAnyHeader().WithOrigins("http://127.0.0.1:3000").AllowCredentials()));

            services.AddSignalR();
            services.AddMemoryCache();

            services.AddSingleton<IQuestionCache, QuestionCache>();

            services.AddAuthentication(options =>
           {
               options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;

               options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
           }).AddJwtBearer(options => {
               options.Authority = this.Configuration["Auth0:Authority"];
               options.Audience = this.Configuration["Auth0:Audience"];
           });
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
