using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using SimpleHooks.Web.Helper;

namespace SimpleHooks.Web
{
    public class Startup
    {
        private readonly Log.SQL.Logger _logger;
        private readonly Guid _logCorrelationId = Guid.NewGuid();

        public Startup(IConfiguration configuration)
        {
            var config = new ConfigurationHelper(configuration);
            _logger = new Log.SQL.Logger()
            {
                MinLogType = (Log.Interface.LogModel.LogTypes)Enum.Parse(typeof(Log.Interface.LogModel.LogTypes), config.LoggerMinLogLevel, true),
                ConnectionString = config.ConnectionStringLog,
                FunctionName = config.LoggerFunction
            };

            _logger.Add(new Log.Interface.LogModel()
            {
                CodeReference = "SimpleHooks.Web.Startup",
                Correlation = _logCorrelationId,
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

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            _logger.Add(_logger.Add(new Log.Interface.LogModel()
            {
                CodeReference = "SimpleHooks.Web.Startup",
                Correlation = _logCorrelationId,
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
            _logger.Add(new Log.Interface.LogModel()
            {
                CodeReference = "SimpleHooks.Web.Startup",
                Correlation = _logCorrelationId,
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
                    _logger.Add(new Log.Interface.LogModel()
                    {
                        CodeReference = "SimpleHooks.Web.Startup",
                        Correlation = _logCorrelationId,
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
                    _logger.Add(new Log.Interface.LogModel()
                    {
                        CodeReference = "SimpleHooks.Web.Startup",
                        Correlation = _logCorrelationId,
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
                _logger.Add(new Log.Interface.LogModel() 
                {
                    CodeReference = "SimpleHooks.Web.Startup",
                    Correlation = _logCorrelationId,
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

            _logger.Add(new Log.Interface.LogModel()
            {
                CodeReference = "SimpleHooks.Web.Startup",
                Correlation = _logCorrelationId,
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
