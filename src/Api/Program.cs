using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using System.Diagnostics;
using System.Net;
using DataLayer;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IDatabaseConnectionFactory, PartyConnectionFactory>();
builder.Services.AddSingleton<IPartyService, PartyService>();
builder.Services.AddMediator();

builder.Services.AddControllers();
builder.Services.AddHealthChecks();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer(options => {
    options.TokenValidationParameters = new TokenValidationParameters {
        ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = "http://party-push-backend.us-east-1.elasticbeanstalk.com/",
            ValidAudience = "http://party-push-backend.us-east-1.elasticbeanstalk.com/",
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("6xGrJLpenwmJXCIAlLPbdfbgctrbgvrtgcrtdbgvgrdffvxrsvfdfrcvftryr65gr4sdrger"))
    };
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpecificOrigin",
        builder => builder
        .WithOrigins("http://localhost:3000",
                    "http://54.208.105.152:3000",
                    "https://livepartyhelper.com",
                    "https://www.livepartyhelper.com",
                    "https://api.twilio.com",
                    "http://api.twilio.com",
                    "http://party-push-backend.us-east-1.elasticbeanstalk.com",
                    "https://party-push-backend.us-east-1.elasticbeanstalk.com",
                    "party-push-backend.us-east-1.elasticbeanstalk.com",
                    "http://party-push.us-east-1.elasticbeanstalk.com",
                    "https://party-push.us-east-1.elasticbeanstalk.com",
                    "party-push.us-east-1.elasticbeanstalk.com"));
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.UseHttpsRedirection();

app.UseCors(options =>
{
    options.WithOrigins("http://localhost:3000",
                    "http://54.208.105.152:3000",
                    "https://livepartyhelper.com",
                    "https://www.livepartyhelper.com",
                    "https://api.twilio.com",
                    "http://api.twilio.com",
                    "http://party-push-backend.us-east-1.elasticbeanstalk.com",
                    "https://party-push-backend.us-east-1.elasticbeanstalk.com",
                    "party-push-backend.us-east-1.elasticbeanstalk.com",
                    "http://party-push.us-east-1.elasticbeanstalk.com",
                    "https://party-push.us-east-1.elasticbeanstalk.com",
                    "party-push.us-east-1.elasticbeanstalk.com")
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

app.UseAuthentication();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/healthcheck");

app.Run();

public partial class Program {  }