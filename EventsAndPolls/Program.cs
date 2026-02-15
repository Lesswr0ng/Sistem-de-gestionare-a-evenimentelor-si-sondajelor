using EventsAndPolls.Application.Services;
using EventsAndPolls.Infrastructure.Data;
using EventsAndPolls.Infrastructure.Repositories;
using EventsAndPolls.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddRazorPages();
builder.Services.AddControllers();

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<IEventRepository, EventRepository>();
builder.Services.AddScoped<IPollRepository, PollRepository>();
builder.Services.AddScoped<IVoteRepository, VoteRepository>();

builder.Services.AddScoped<IEventService, EventService>();
builder.Services.AddScoped<IPollService, PollService>();
builder.Services.AddScoped<IVoteService, VoteService>();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
     app.UseExceptionHandler("/Error");
     app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();
app.MapControllers();
app.MapRazorPages();

app.Run();