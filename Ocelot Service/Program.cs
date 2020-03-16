using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;

namespace Ocelot_Service
{
    public class Program
    {
        private static IConfiguration Configuration = null;

        public static void Main(string[] args)
        {
            new WebHostBuilder()
                .UseKestrel()
                .UseContentRoot(Directory.GetCurrentDirectory())
                .ConfigureAppConfiguration((hostingContext, config) =>
                {
                    config
                        .SetBasePath(hostingContext.HostingEnvironment.ContentRootPath)
                        .AddJsonFile("appsettings.json", true, true)
                        .AddJsonFile($"appsettings.{hostingContext.HostingEnvironment.EnvironmentName}.json", true,
                            true)
                        .AddJsonFile("ocelot.json")
                        .AddEnvironmentVariables();
                    Configuration = config.Build();
                })
                .ConfigureServices(s =>
                {
                    var audienceConfig = Configuration.GetSection("Audience");
                    var signingKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(audienceConfig["Secret"]));
                    var tokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        IssuerSigningKey = signingKey,
                        ValidateIssuer = true,
                        ValidIssuer = audienceConfig["Iss"],
                        ValidateAudience = true,
                        ValidAudience = audienceConfig["Aud"],
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero,
                        RequireExpirationTime = true,
                    };
                    s.AddAuthentication(o => { o.DefaultAuthenticateScheme = "TestKey"; });

                    s.AddAuthentication()
                        .AddJwtBearer("TestKey", x =>
                        {
                            x.RequireHttpsMetadata = false;
                            x.TokenValidationParameters = tokenValidationParameters;
                        });

                    s.AddOcelot();
                })
                .UseIISIntegration()
                .Configure(app =>
                {
                    var configuration = new OcelotPipelineConfiguration
                    {
                        PreAuthenticationMiddleware = async (ctx, next) =>
                        {
                            var pre = ctx.HttpContext.Request.Headers["Pre"];
                            if (!string.IsNullOrEmpty(pre))
                            {
                                
                                await next.Invoke();
                            }
                            else
                            {
                                ctx.HttpContext.Response.StatusCode = 403;
                                await ctx.HttpContext.Response.WriteAsync("hello");
                            }
                            
                        }
                    };

                    app.UseHttpsRedirection();
                    app.UseRouting();
                    app.UseAuthorization();
                    app.UseEndpoints(endpoints => { endpoints.MapControllers(); });
                    app.UseOcelot(configuration).Wait();
                })
                .Build()
                .Run();
        }
    }
}