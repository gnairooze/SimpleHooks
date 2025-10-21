using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using OpenIddict.Validation.AspNetCore;
using SimpleTools.SimpleHooks.SampleListener.SampleListenerAPI.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers().AddNewtonsoftJson();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


ConfigurationManager configuration = builder.Configuration;
builder.Services.AddDbContext<SampleDbContext>(options =>
    options.UseSqlServer(configuration.GetConnectionString("SampleDbConnection")));

// Configure authentication to use OpenIddict validation
builder.Services.AddAuthentication(OpenIddictValidationAspNetCoreDefaults.AuthenticationScheme);

// Configure OpenIddict Validation with introspection (required for encrypted tokens)
builder.Services.AddOpenIddict()
    .AddValidation(options =>
    {
        options.SetIssuer(builder.Configuration["IdentityServer:Authority"]!);
        options.AddAudiences(builder.Configuration["IdentityServer:Audience"]!);

        // Configure the validation handler to use introspection for encrypted tokens
        options.UseIntrospection()
            .SetClientId(builder.Configuration["IdentityServer:ClientId"]!)
            .SetClientSecret(builder.Configuration["IdentityServer:ClientSecret"]!);

        // Configure the validation handler to use ASP.NET Core.
        options.UseAspNetCore();

        // Configure the validation handler to use System.Net.Http for introspection.
        options.UseSystemNetHttp();
    });

builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireSample", policy =>
        policy.RequireClaim("scope", "samplelistener_api.sample"));
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();

    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
