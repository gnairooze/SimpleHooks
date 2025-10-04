using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using SimpleTools.SimpleHooks.Web.Helper;

namespace SimpleTools.SimpleHooks.Web
{
    public class Startup
    {
        private readonly Log.SQL.Logger _logger;
        private readonly Guid _logCorrelationId = Guid.NewGuid();
        private readonly ConfigurationHelper _configurationHelper;

        public Startup(IConfiguration configuration)
        {
            _configurationHelper = new ConfigurationHelper(configuration);

            ReadEnvVarAndSetConfig();

            _logger = new Log.SQL.Logger()
            {
                MinLogType = (Log.Interface.LogModel.LogTypes)Enum.Parse(typeof(Log.Interface.LogModel.LogTypes), _configurationHelper.LoggerMinLogLevel, true),
                ConnectionString = _configurationHelper.ConnectionStringLog,
                FunctionName = _configurationHelper.LoggerFunction
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

        private void ReadEnvVarAndSetConfig()
        {
            if (!String.IsNullOrEmpty(EnvironmentVariablesHelper.ConnectionStringSimpleHooks))
            {
                _configurationHelper.ConnectionStringSimpleHooks = EnvironmentVariablesHelper.ConnectionStringSimpleHooks;
            }

            if (!String.IsNullOrEmpty(EnvironmentVariablesHelper.ConnectionStringLog))
            {
                _configurationHelper.ConnectionStringLog = EnvironmentVariablesHelper.ConnectionStringLog;
            }

            if (!String.IsNullOrEmpty(EnvironmentVariablesHelper.LoggerMinLogLevel))
            {
                _configurationHelper.LoggerMinLogLevel = EnvironmentVariablesHelper.LoggerMinLogLevel;
            }

            if (!String.IsNullOrEmpty(EnvironmentVariablesHelper.LoggerFunction))
            {
                _configurationHelper.LoggerFunction = EnvironmentVariablesHelper.LoggerFunction;
            }
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

            services.AddSingleton<Log.Interface.ILog>(provider => new Log.SQL.Logger()
            {
                MinLogType = (Log.Interface.LogModel.LogTypes)Enum.Parse(typeof(Log.Interface.LogModel.LogTypes), _configurationHelper.LoggerMinLogLevel, true),
                ConnectionString = _configurationHelper.ConnectionStringLog,
                FunctionName = _configurationHelper.LoggerFunction
            });
            services.AddSingleton<Interfaces.IConnectionRepository>(provider => new Repo.SQL.SqlConnectionRepo()
            {
                ConnectionString = _configurationHelper.ConnectionStringSimpleHooks
            });
            services.AddSingleton<Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Instance.EventInstance>, Repo.SQL.EventInstanceDataRepo>();
            services.AddSingleton<Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Instance.ListenerInstance>, Repo.SQL.ListenerInstanceDataRepo>();
            services.AddSingleton<HttpClient.Interface.IHttpClient, HttpClient.Simple.SimpleClient>();
            services.AddSingleton<Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Definition.EventDefinition>, Repo.SQL.EventDefinitionDataRepo>();
            services.AddSingleton<Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Definition.ListenerDefinition>, Repo.SQL.ListenerDefinitionDataRepo>();
            services.AddSingleton<Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Definition.EventDefinitionListenerDefinition>, Repo.SQL.EventIistenerDefinitionDataRepo>();
            services.AddSingleton<Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Definition.AppOption>, Repo.SQL.AppOptionDataRepo>();

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
