using EventsAndPolls.Application.Services;
using EventsAndPolls.Infrastructure.Data;
using EventsAndPolls.Infrastructure.Repositories;
using EventsAndPolls.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using EventsAndPolls.Application.Export;
using EventsAndPolls.Application.Facade;
using EventsAndPolls.Application.Decorators;
using EventsAndPolls.Application.Proxy;
using Microsoft.Extensions.Caching.Memory;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddMemoryCache();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddSwaggerGen(c =>
{
     c.SwaggerDoc("v1", new OpenApiInfo
     {
          Title = "Events & Polls API",
          Version = "v1",
          Description = "API for managing events and polls"
     });
});

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IPollRepository, PollRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IVoteService, VoteService>();

builder.Services.AddScoped<IPollService>(provider =>
{
     var pollRepository = provider.GetRequiredService<IPollRepository>();
     var voteRepository = provider.GetRequiredService<IVoteRepository>();
     var eventRepository = provider.GetRequiredService<IEventRepository>();
     var cache = provider.GetRequiredService<IMemoryCache>();

     var loggingLogger = provider.GetRequiredService<ILogger<LoggingPollServiceDecorator>>();
     var cachingLogger = provider.GetRequiredService<ILogger<CachingPollServiceDecorator>>();
     var proxyLogger = provider.GetRequiredService<ILogger<PollServiceProtectionProxy>>();

     IPollService service = new PollService(pollRepository, voteRepository, eventRepository);

     service = new LoggingPollServiceDecorator(service, loggingLogger);

     service = new CachingPollServiceDecorator(service, cache, cachingLogger);

     service = new PollServiceProtectionProxy(service, voteRepository, proxyLogger);

     return service;
});

builder.Services.AddScoped<IPollFacade, PollFacade>();
builder.Services.AddScoped<IExportAdapter, JsonExportAdapter>();
builder.Services.AddScoped<IExportAdapter, PlainTextExportAdapter>();
builder.Services.AddScoped<IExportService, ExportService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
     app.UseExceptionHandler("/Error");
     app.UseHsts();
}

if (app.Environment.IsDevelopment())
{
     app.UseDeveloperExceptionPage();
     app.UseSwagger();
     app.UseSwaggerUI(c =>
     {
          c.SwaggerEndpoint("/swagger/v1/swagger.json", "Events & Polls API V1");
          c.RoutePrefix = "swagger";
     });
}

var appConfig = AppConfiguration.Instance;
appConfig.LoadFromConfiguration(builder.Configuration);
appConfig.DisplaySettings();

var maxEvents = AppConfiguration.Instance.GetSetting<int>("MaxEventsPerUser");

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

app.Run();