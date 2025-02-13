using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SimpleHooks.Web
{
    public class Startup
    {
        private readonly Helper.ConfigurationHelper _Config;
        private readonly Log.SQL.Logger _Logger;
        private readonly Guid Log_CorrelationId = Guid.NewGuid();

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
            _Config = new Helper.ConfigurationHelper(configuration);
            _Logger = new Log.SQL.Logger()
            {
                MinLogType = (Log.Interface.LogModel.LogTypes)Enum.Parse(typeof(Log.Interface.LogModel.LogTypes), _Config.Logger_MinLogLevel, true),
                ConnectionString = _Config.ConnectionString_Log,
                FunctionName = _Config.Logger_Function
            };

            _Logger.Add(new Log.Interface.LogModel()
            {
                CodeReference = "SimpleHooks.Web.Startup",
                Correlation = Log_CorrelationId,
                Counter = 1,
                CreateDate = DateTime.UtcNow,
                Duration = 0,
                Location = "SimpleHooks.Web.Startup",
                LogType = Log.Interface.LogModel.LogTypes.Information,
                Machine = Environment.MachineName,
                NotesA = "Startup",
                NotesB = string.Empty,
                Operation = "Startup",
                Owner = "System",
                ReferenceName = "Application",
                ReferenceValue = "SimpleHooks.Web",
                Step = "Startup"
            });
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _Logger.Add(_Logger.Add(new Log.Interface.LogModel()
            {
                CodeReference = "SimpleHooks.Web.Startup",
                Correlation = Log_CorrelationId,
                Counter = 1,
                CreateDate = DateTime.UtcNow,
                Duration = 0,
                Location = "SimpleHooks.Web.Startup",
                LogType = Log.Interface.LogModel.LogTypes.Information,
                Machine = Environment.MachineName,
                NotesA = "ConfigureServices",
                NotesB = string.Empty,
                Operation = "ConfigureServices",
                Owner = "System",
                ReferenceName = "Application",
                ReferenceValue = "SimpleHooks.Web",
                Step = "ConfigureServices"
            }));

            services.AddControllersWithViews();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            _Logger.Add(new Log.Interface.LogModel()
            {
                CodeReference = "SimpleHooks.Web.Startup",
                Correlation = Log_CorrelationId,
                Counter = 1,
                CreateDate = DateTime.UtcNow,
                Duration = 0,
                Location = "SimpleHooks.Web.Startup",
                LogType = Log.Interface.LogModel.LogTypes.Information,
                Machine = Environment.MachineName,
                NotesA = "Configure started",
                NotesB = string.Empty,
                Operation = "Configure",
                Owner = "System",
                ReferenceName = "Application",
                ReferenceValue = "SimpleHooks.Web",
                Step = "Configure started"
            });

            try
            {
                if (env.IsDevelopment())
                {
                    _Logger.Add(new Log.Interface.LogModel()
                    {
                        CodeReference = "SimpleHooks.Web.Startup",
                        Correlation = Log_CorrelationId,
                        Counter = 2,
                        CreateDate = DateTime.UtcNow,
                        Duration = 0,
                        Location = "SimpleHooks.Web.Startup",
                        LogType = Log.Interface.LogModel.LogTypes.Information,
                        Machine = Environment.MachineName,
                        NotesA = "environment is development",
                        NotesB = string.Empty,
                        Operation = "Configure",
                        Owner = "System",
                        ReferenceName = "Application",
                        ReferenceValue = "SimpleHooks.Web",
                        Step = "environment is development"
                    });
                    app.UseDeveloperExceptionPage();
                }
                else
                {
                    _Logger.Add(new Log.Interface.LogModel()
                    {
                        CodeReference = "SimpleHooks.Web.Startup",
                        Correlation = Log_CorrelationId,
                        Counter = 2,
                        CreateDate = DateTime.UtcNow,
                        Duration = 0,
                        Location = "SimpleHooks.Web.Startup",
                        LogType = Log.Interface.LogModel.LogTypes.Information,
                        Machine = Environment.MachineName,
                        NotesA = "environment is not development",
                        NotesB = string.Empty,
                        Operation = "Configure",
                        Owner = "System",
                        ReferenceName = "Application",
                        ReferenceValue = "SimpleHooks.Web",
                        Step = "environment is not development"
                    });

                    app.UseExceptionHandler("/Home/Error");
                    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                    app.UseHsts();
                }
                app.UseHttpsRedirection();
                app.UseStaticFiles();

                app.UseRouting();

                app.UseAuthorization();

                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapControllerRoute(
                        name: "default",
                        pattern: "{controller=Home}/{action=Index}/{id?}");
                });

            }
            catch (Exception ex)
            {
                _Logger.Add(new Log.Interface.LogModel() 
                {
                    CodeReference = "SimpleHooks.Web.Startup",
                    Correlation = Log_CorrelationId,
                    Counter = 3,
                    CreateDate = DateTime.UtcNow,
                    Duration = 0,
                    Location = "SimpleHooks.Web.Startup",
                    LogType = Log.Interface.LogModel.LogTypes.Error,
                    Machine = Environment.MachineName,
                    NotesA = ex.Message,
                    NotesB = Newtonsoft.Json.JsonConvert.SerializeObject(ex),
                    Operation = "Configure",
                    Owner = "System",
                    ReferenceName = "Application",
                    ReferenceValue = "SimpleHooks.Web",
                    Step = "Error in Configure"
                });
                throw;
            }

            _Logger.Add(new Log.Interface.LogModel()
            {
                CodeReference = "SimpleHooks.Web.Startup",
                Correlation = Log_CorrelationId,
                Counter = 3,
                CreateDate = DateTime.UtcNow,
                Duration = 0,
                Location = "SimpleHooks.Web.Startup",
                LogType = Log.Interface.LogModel.LogTypes.Information,
                Machine = Environment.MachineName,
                NotesA = "Configure completed",
                NotesB = string.Empty,
                Operation = "Configure",
                Owner = "System",
                ReferenceName = "Application",
                ReferenceValue = "SimpleHooks.Web",
                Step = "Configure completed"
            });
        }
    }
}
