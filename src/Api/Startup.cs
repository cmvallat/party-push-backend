using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace MyNamespace
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
            services.AddCors(options =>
            {
                options.AddPolicy("AllowSpecificOrigin",
                    builder => builder
                    .WithOrigins(
                        "http://localhost:3000",
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
                    .AllowAnyHeader()
                    .AllowAnyMethod());
                });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors("AllowSpecificOrigin");

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
