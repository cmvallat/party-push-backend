using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Net;
using DataLayer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IDatabaseConnectionFactory, PartyConnectionFactory>();
builder.Services.AddSingleton<IPartyService, PartyService>();
builder.Services.AddMediator();

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder.WithOrigins("http://localhost:3000"));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors(options =>
{
    options.WithOrigins("http://localhost:3000")
           .WithHeaders("X-Requested-With", "Content-Type", "Accept", "Authorization")
           .WithMethods("GET", "POST", "PUT", "DELETE")
           .WithExposedHeaders("content-disposition")
           .WithExposedHeaders("content-length")
           .WithExposedHeaders("content-encoding")
           .WithExposedHeaders("content-type")
           .WithExposedHeaders("x-filename")
           .WithExposedHeaders("x-filesize")
           .WithExposedHeaders("access-control-allow-origin");
});

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/healthcheck");

app.Run();

public partial class Program {  }