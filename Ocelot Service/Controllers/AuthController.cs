using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Ocelot_Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AuthController : ControllerBase
    {
        public AuthController(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        [HttpGet]
        public ActionResult<object> Index(string name, string pass)
        {
            var audienceConfig = Configuration.GetSection("Audience");

            if (name == "admin" && pass == "123")
            {
                var now = DateTime.Now;
                var claims = new Claim[]
                {
                    new Claim(JwtRegisteredClaimNames.Sub, name),
                    new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new Claim(JwtRegisteredClaimNames.Iat, now.ToUniversalTime().ToString(), ClaimValueTypes.Integer64),
                };

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
                var jwt = new JwtSecurityToken(
                    issuer: audienceConfig["Iss"],
                    audience: audienceConfig["Aud"],
                    claims: claims,
                    notBefore: now,
                    expires: now.Add(TimeSpan.FromMinutes(500)),
                    signingCredentials: new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256)
                );
                var encodedJwt = new JwtSecurityTokenHandler().WriteToken(jwt);
                var responseJson = new
                {
                    access_token = encodedJwt,
                    expires_in = (int) TimeSpan.FromMinutes(5000).TotalSeconds
                };

                return responseJson;
            }

            return "Not Auth";
        }

        public class Audience
        {
            public string Secret { get; set; }
            public string Iss { get; set; }
            public string Aud { get; set; }
        }
    }
}