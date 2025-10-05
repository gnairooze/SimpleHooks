using Microsoft.Extensions.Configuration;
using OpenIddict.Validation.AspNetCore;
using SimpleTools.SimpleHooks.AuthApi;
using SimpleTools.SimpleHooks.AuthApi.Helper;
using SimpleTools.SimpleHooks.Log.Interface;

SimpleTools.SimpleHooks.Log.SQL.Logger _logger;
var _logCorrelationId = Guid.NewGuid();


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

var _configurationHelper = new ConfigurationHelper(builder.Configuration);

ReadEnvVarAndSetConfig();

_logger = new SimpleTools.SimpleHooks.Log.SQL.Logger()
{
    MinLogType = (SimpleTools.SimpleHooks.Log.Interface.LogModel.LogTypes)Enum.Parse(typeof(SimpleTools.SimpleHooks.Log.Interface.LogModel.LogTypes), _configurationHelper.LoggerMinLogLevel, true),
    ConnectionString = _configurationHelper.ConnectionStringLog,
    FunctionName = _configurationHelper.LoggerFunction
};

_logger.Add(new SimpleTools.SimpleHooks.Log.Interface.LogModel()
{
    CodeReference = "SimpleHooks.AuthApi.Program",
    Correlation = _logCorrelationId,
    Counter = 1,
    CreateDate = DateTime.UtcNow,
    Duration = 0,
    Location = "SimpleHooks.AuthApi.Program",
    LogType = SimpleTools.SimpleHooks.Log.Interface.LogModel.LogTypes.Information,
    Machine = Environment.MachineName,
    NotesA = "First",
    NotesB = string.Empty,
    Operation = "Starting",
    Owner = "System",
    ReferenceName = "Application",
    ReferenceValue = "SimpleHooks.AuthApi",
    Step = "First"
});

builder.Services.AddSingleton<SimpleTools.SimpleHooks.Log.Interface.ILog>(provider => new SimpleTools.SimpleHooks.Log.SQL.Logger()
{
    MinLogType = (SimpleTools.SimpleHooks.Log.Interface.LogModel.LogTypes)Enum.Parse(typeof(SimpleTools.SimpleHooks.Log.Interface.LogModel.LogTypes), _configurationHelper.LoggerMinLogLevel, true),
    ConnectionString = _configurationHelper.ConnectionStringLog,
    FunctionName = _configurationHelper.LoggerFunction
});
builder.Services.AddSingleton<SimpleTools.SimpleHooks.Interfaces.IConnectionRepository>(provider => new SimpleTools.SimpleHooks.Repo.SQL.SqlConnectionRepo()
{
    ConnectionString = _configurationHelper.ConnectionStringSimpleHooks
});
builder.Services.AddSingleton<SimpleTools.SimpleHooks.Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Instance.EventInstance>, SimpleTools.SimpleHooks.Repo.SQL.EventInstanceDataRepo>();
builder.Services.AddSingleton<SimpleTools.SimpleHooks.Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Instance.ListenerInstance>, SimpleTools.SimpleHooks.Repo.SQL.ListenerInstanceDataRepo>();
builder.Services.AddSingleton<SimpleTools.SimpleHooks.HttpClient.Interface.IHttpClient, SimpleTools.SimpleHooks.HttpClient.Simple.SimpleClient>();
builder.Services.AddSingleton<SimpleTools.SimpleHooks.Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Definition.EventDefinition>, SimpleTools.SimpleHooks.Repo.SQL.EventDefinitionDataRepo>();
builder.Services.AddSingleton<SimpleTools.SimpleHooks.Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Definition.ListenerDefinition>, SimpleTools.SimpleHooks.Repo.SQL.ListenerDefinitionDataRepo>();
builder.Services.AddSingleton<SimpleTools.SimpleHooks.Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Definition.EventDefinitionListenerDefinition>, SimpleTools.SimpleHooks.Repo.SQL.EventIistenerDefinitionDataRepo>();
builder.Services.AddSingleton<SimpleTools.SimpleHooks.Interfaces.IDataRepository<SimpleTools.SimpleHooks.Models.Definition.AppOption>, SimpleTools.SimpleHooks.Repo.SQL.AppOptionDataRepo>();

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

ConfigureIdentityService();




var app = builder.Build();

try
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        _logger.Add(new SimpleTools.SimpleHooks.Log.Interface.LogModel()
        {
            CodeReference = "SimpleHooks.AuthApi.Program",
            Correlation = _logCorrelationId,
            Counter = 2,
            CreateDate = DateTime.UtcNow,
            Duration = 0,
            Location = "SimpleHooks.AuthApi.Program",
            LogType = SimpleTools.SimpleHooks.Log.Interface.LogModel.LogTypes.Information,
            Machine = Environment.MachineName,
            NotesA = "environment is development",
            NotesB = string.Empty,
            Operation = "Starting",
            Owner = "System",
            ReferenceName = "Application",
            ReferenceValue = "SimpleHooks.AuthApi",
            Step = "Second"
        });
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    _logger.Add(new SimpleTools.SimpleHooks.Log.Interface.LogModel()
    {
        CodeReference = "SimpleHooks.AuthApi.Program",
        Correlation = _logCorrelationId,
        Counter = 2,
        CreateDate = DateTime.UtcNow,
        Duration = 0,
        Location = "SimpleHooks.AuthApi.Program",
        LogType = SimpleTools.SimpleHooks.Log.Interface.LogModel.LogTypes.Information,
        Machine = Environment.MachineName,
        NotesA = "environment is not development",
        NotesB = string.Empty,
        Operation = "Starting",
        Owner = "System",
        ReferenceName = "Application",
        ReferenceValue = "SimpleHooks.AuthApi",
        Step = "Second"
    });

    app.UseHttpsRedirection();

    app.UseAuthentication();
    app.UseAuthorization();

    app.MapControllers();

    _logger.Add(new SimpleTools.SimpleHooks.Log.Interface.LogModel()
    {
        CodeReference = "SimpleHooks.AuthApi.Program",
        Correlation = _logCorrelationId,
        Counter = 3,
        CreateDate = DateTime.UtcNow,
        Duration = 0,
        Location = "SimpleHooks.AuthApi.Program",
        LogType = SimpleTools.SimpleHooks.Log.Interface.LogModel.LogTypes.Information,
        Machine = Environment.MachineName,
        NotesA = "Starting completed. next app.run",
        NotesB = string.Empty,
        Operation = "Starting",
        Owner = "System",
        ReferenceName = "Application",
        ReferenceValue = "SimpleHooks.AuthApi",
        Step = "Starting completed"
    });

    app.Run();
}
catch (Exception ex)
{
    Console.WriteLine(ex);

    _logger.Add(new SimpleTools.SimpleHooks.Log.Interface.LogModel()
    {
        CodeReference = "SimpleHooks.AuthApi.Program",
        Correlation = _logCorrelationId,
        Counter = 3,
        CreateDate = DateTime.UtcNow,
        Duration = 0,
        Location = "SimpleHooks.AuthApi.Program",
        LogType = SimpleTools.SimpleHooks.Log.Interface.LogModel.LogTypes.Error,
        Machine = Environment.MachineName,
        NotesA = ex.Message,
        NotesB = Newtonsoft.Json.JsonConvert.SerializeObject(ex),
        Operation = "Starting",
        Owner = "System",
        ReferenceName = "Application",
        ReferenceValue = "SimpleHooks.AuthApi",
        Step = "Error in Starting"
    });
    throw;
}


void ReadEnvVarAndSetConfig()
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

    if (!String.IsNullOrEmpty(EnvironmentVariablesHelper.IdentityServerAuthority))
    {
        _configurationHelper.IdentityServerAuthority = EnvironmentVariablesHelper.IdentityServerAuthority;
    }

    if (!String.IsNullOrEmpty(EnvironmentVariablesHelper.IdentityServerAudience))
    {
        _configurationHelper.IdentityServerAudience = EnvironmentVariablesHelper.IdentityServerAudience;
    }

    if (!String.IsNullOrEmpty(EnvironmentVariablesHelper.IdentityServerIntrospectionEndpoint))
    {
        _configurationHelper.IdentityServerIntrospectionEndpoint = EnvironmentVariablesHelper.IdentityServerIntrospectionEndpoint;
    }

    if (!String.IsNullOrEmpty(EnvironmentVariablesHelper.IdentityServerClientId))
    {
        _configurationHelper.IdentityServerClientId = EnvironmentVariablesHelper.IdentityServerClientId;
    }

    if (!String.IsNullOrEmpty(EnvironmentVariablesHelper.IdentityServerClientSecret))
    {
        _configurationHelper.IdentityServerClientSecret = EnvironmentVariablesHelper.IdentityServerClientSecret;
    }
}

void ConfigureIdentityService()
{
    // Configure authentication to use OpenIddict validation
    builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

    // Configure OpenIddict Validation with introspection (required for encrypted tokens)
    builder.Services.AddOpenIddict()
        .AddValidation(options =>
        {
            options.SetIssuer(_configurationHelper.IdentityServerAuthority!);
            options.AddAudiences(_configurationHelper.IdentityServerAudience!);

            // Configure the validation handler to use introspection for encrypted tokens
            options.UseIntrospection()
                .SetClientId(_configurationHelper.IdentityServerClientId!)
                .SetClientSecret(_configurationHelper.IdentityServerClientSecret!);

            // Configure the validation handler to use ASP.NET Core.
            options.UseAspNetCore();

            // Configure the validation handler to use System.Net.Http for introspection.
            options.UseSystemNetHttp();
        });

    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy(AuthPolicies.REQUIRE_SIMPLEHOOKS_API_GET_EVENT_INSTANCE_STATUS, policy =>
            policy.RequireClaim("scope", "simplehooks_api.get_event_instance_status"));

        options.AddPolicy(AuthPolicies.REQUIRE_SIMPLEHOOKS_API_LOADDEFINITIONS, policy =>
            policy.RequireClaim("scope", "simplehooks_api.load_definitions"));

        options.AddPolicy(AuthPolicies.REQUIRE_SIMPLEHOOKS_API_TRIGGEREVENT, policy =>
            policy.RequireClaim("scope", "simplehooks_api.trigger_event"));
    });
}