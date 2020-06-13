using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using DbUp;
using netcore_api.Data;

namespace netcore_api
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
            var connectionString = Configuration.GetConnectionString("DefaultConnection");

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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthorization();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
