using EventsAndPolls.Application.Services;
using EventsAndPolls.Infrastructure.Data;
using EventsAndPolls.Infrastructure.Repositories;
using EventsAndPolls.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi;
using Microsoft.OpenApi.Models;
using EventsAndPolls.Application.Export;
using EventsAndPolls.Application.Facade;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

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
builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddScoped<IVoteService, VoteService>();
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