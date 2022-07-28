using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Web;
using System.Text.Json.Serialization;
using VersionCheckerApi.Analysing;
using VersionCheckerApi.Analysing.Actionables;
using VersionCheckerApi.Analysing.Modules;
using VersionCheckerApi.Analysing.Packages;
using VersionCheckerApi.Analysing.Packages.LatestVersionGetters;
using VersionCheckerApi.Analysing.Packages.Security;
using VersionCheckerApi.Analysing.Pipelines;
using VersionCheckerApi.Persistence;

var builder = WebApplication.CreateBuilder(args);

//Cors
builder.Services.AddCors(o =>
{
    o.AddPolicy("default", b =>
    {
        b.WithOrigins("https://localhost:7251");
        b.AllowAnyHeader();
        b.AllowAnyMethod();
        b.AllowCredentials();
    });
});

// Add services to the container.
// 3rd party services
builder.Services.AddHttpClient();
// Own services

builder.Services.AddSingleton<SecurityService>(); // Gets security vulnerabilities from github security advisory database and keeps in memory

builder.Services.AddScoped<LatestVersionGetterFactory>();
builder.Services.AddScoped<NugetService>();
builder.Services.AddScoped<NpmService>();
builder.Services.AddScoped<PackagistService>();

builder.Services.AddScoped<ProjectBuilder>();
builder.Services.AddScoped<ModuleBuilderFactory>();
builder.Services.AddScoped<ModuleUpdater>();
builder.Services.AddScoped<PackageBuilder>(); // builds packages with nugetservice/npmservice and securityservice and keeps in memory

builder.Services.AddScoped<ActionablesService>();
builder.Services.AddScoped<PipelineAnalyzer>();

//Db context
builder.Services.AddDbContext<ProjectContext>(o =>
{
    o.UseSqlServer(builder.Configuration.GetConnectionString("Database"));
    o.EnableSensitiveDataLogging();
});

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApi(builder.Configuration.GetSection("AzureAd"));

builder.Services.AddControllers().AddJsonOptions(options =>
        options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();
app.UseCors("default");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDeveloperExceptionPage();
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();