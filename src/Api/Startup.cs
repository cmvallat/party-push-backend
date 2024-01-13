using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Diagnostics;
using System.Net;
using DataLayer;
using Mediator;
using Microsoft.OpenApi.Models;
using System;
using MediatR;

namespace Api
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
              // Register MediatR
            services.AddMediator();

            // Add other services
            services.AddSingleton<IDatabaseConnectionFactory, PartyConnectionFactory>();
            services.AddSingleton<IPartyService, PartyService>();

            // Add controllers, health checks, and API explorer
            services.AddControllers();
            services.AddHealthChecks();
            services.AddEndpointsApiExplorer();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder
                        .WithOrigins(
                            "http://localhost:3000",
                            "http://54.208.105.152:3000",
                            "https://livepartyhelper.com",
                            "https://www.livepartyhelper.com",
                            "http://party-push-backend.us-east-1.elasticbeanstalk.com",
                            "https://party-push-backend.us-east-1.elasticbeanstalk.com",
                            "party-push-backend.us-east-1.elasticbeanstalk.com",
                            "http://party-push.us-east-1.elasticbeanstalk.com",
                            "https://party-push.us-east-1.elasticbeanstalk.com",
                            "party-push.us-east-1.elasticbeanstalk.com",
                            "https://api.twilio.com",
                            "http://api.twilio.com")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
            });

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = "http://party-push-backend.us-east-1.elasticbeanstalk.com",
                        ValidAudience = "http://party-push-backend.us-east-1.elasticbeanstalk.com",
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("6xGrJLpenwmJXCIAlLPbdfbgctrbgvrtgcrtdbgvgrdffvxrsvfdfrcvftryr65gr4sdrger"))
                    };
                });

            services.AddControllers();

            // Swagger configuration
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "Your API", Version = "v1" });
            });
        }


        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "Your API V1");
            });

            app.UseHttpsRedirection();

            app.UseCors("AllowSpecificOrigin");

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }

    }
}
