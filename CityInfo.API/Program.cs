using CityInfo.API;
using CityInfo.API.DbContexts;
using CityInfo.API.Services;
using Microsoft.AspNetCore.StaticFiles;
using Microsoft.EntityFrameworkCore;
using Serilog;

Log.Logger=new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .WriteTo.File("logs/cityinfo.txt",rollingInterval:RollingInterval.Month)
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
//builder.Logging.ClearProviders();
//builder.Logging.AddConsole();

builder.Host.UseSerilog();

// Add services to the container.

builder.Services.AddControllers(options =>
{
    options.ReturnHttpNotAcceptable=true;
}).AddNewtonsoftJson()
  .AddXmlDataContractSerializerFormatters();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddSingleton<FileExtensionContentTypeProvider>();

builder.Services.AddTransient<IMailService,LocalMailService>();
builder.Services.AddSingleton<CitiesDataStore>();

// IF You want to use SQL Server Database without SQLite By You must install Nuget Package for SQL Server (Microsoft.EntityFrameworkCore.Sqlserver)
//builder.Services.AddDbContext<CityInfoContext>(options => options.UseSqlServer(builder.Configuration.GetConnectionString("CityInfoConnectionString")));


builder.Services.AddDbContext<CityInfoContext>(
    dbContextOptions=>dbContextOptions.UseSqlite(builder.Configuration.GetConnectionString("ConnectionStrings:CityInfoDBConnectionString")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseRouting();

app.UseAuthorization();

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
});

app.Run();
