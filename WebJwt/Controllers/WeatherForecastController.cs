using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace WebJwt.Controllers;

[ApiController]
[Route("[controller]")]
public class WeatherForecastController : ControllerBase
{
    private readonly IConfiguration _configuration;

    private static readonly string[] Summaries =
    [
        "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
    ];

    [Authorize]
    [HttpGet(Name = "GetWeatherForecast")]
    public IEnumerable<string> Get()
    {
        var name = HttpContext.User.Claims.Where(s => s.Type == ClaimTypes.Name).FirstOrDefault();
        var userName = name.Value;
        
        return
            Enumerable.Concat<string>([userName],
            Enumerable.Range(1, 5).Select(index => 
                Summaries[Random.Shared.Next(Summaries.Length)]
            ))
            .ToArray();
    }

    public WeatherForecastController(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    [HttpPost]
    public ActionResult<string> Auth([FromBody] LoginData data)
    {
        if (data.UserName != "1" || data.Password != "2")
            return Unauthorized("пользователь не найден");

        Claim[] userClaims = [
            new Claim(ClaimTypes.PrimarySid, "12112"), 
            new Claim(ClaimTypes.Name, "местный дурачок")
        ];

        JwtSecurityToken options = new JwtSecurityToken(
            claims: userClaims,
            expires: DateTime.Now.AddMinutes(30),
            signingCredentials: 
                new (new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(_configuration["JWT:SecretKey"])
                ), SecurityAlgorithms.HmacSha256)
        );
   
        var result = new JwtSecurityTokenHandler().
            WriteToken(options);
        
        return Ok(result);
    }
}