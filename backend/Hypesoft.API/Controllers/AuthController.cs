using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Hypesoft.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[AllowAnonymous]
public class AuthController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<AuthController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var tokenEndpoint = $"{_configuration["Keycloak:Authority"]}/protocol/openid-connect/token";
            var clientId = _configuration["Keycloak:ClientId"];
            var clientSecret = _configuration["Keycloak:ClientSecret"];

            var client = _httpClientFactory.CreateClient();
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "password"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("username", request.Username),
                new KeyValuePair<string, string>("password", request.Password)
            });

            var response = await client.PostAsync(tokenEndpoint, requestContent);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Falha no login para o usuário {Username}. Resposta: {Response}", 
                    request.Username, content);
                return Unauthorized(new { message = "Usuário ou senha inválidos" });
            }

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content);
            return Ok(tokenResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar login para o usuário {Username}", request.Username);
            return StatusCode(500, new { message = "Ocorreu um erro ao processar sua solicitação" });
        }
    }

    [HttpPost("refresh-token")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        try
        {
            var tokenEndpoint = $"{_configuration["Keycloak:Authority"]}/protocol/openid-connect/token";
            var clientId = _configuration["Keycloak:ClientId"];
            var clientSecret = _configuration["Keycloak:ClientSecret"];

            var client = _httpClientFactory.CreateClient();
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("grant_type", "refresh_token"),
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("refresh_token", request.RefreshToken)
            });

            var response = await client.PostAsync(tokenEndpoint, requestContent);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Falha ao atualizar token. Resposta: {Response}", content);
                return Unauthorized(new { message = "Token de atualização inválido ou expirado" });
            }

            var tokenResponse = JsonSerializer.Deserialize<TokenResponse>(content);
            return Ok(tokenResponse);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar token");
            return StatusCode(500, new { message = "Ocorreu um erro ao processar sua solicitação" });
        }
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        try
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var logoutEndpoint = $"{_configuration["Keycloak:Authority"]}/protocol/openid-connect/logout";
            var clientId = _configuration["Keycloak:ClientId"];
            var clientSecret = _configuration["Keycloak:ClientSecret"];

            var client = _httpClientFactory.CreateClient();
            var requestContent = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("refresh_token", token)
            });

            var response = await client.PostAsync(logoutEndpoint, requestContent);
            
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Falha ao fazer logout. Status: {StatusCode}", response.StatusCode);
            }

            return Ok(new { message = "Logout realizado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao processar logout");
            return StatusCode(500, new { message = "Ocorreu um erro ao processar o logout" });
        }
    }
}

public class LoginRequest
{
    public string Username { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RefreshTokenRequest
{
    public string RefreshToken { get; set; } = string.Empty;
}

public class TokenResponse
{
    [JsonPropertyName("access_token")]
    public string AccessToken { get; set; } = string.Empty;
    
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; set; }
    
    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; set; }
    
    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = string.Empty;
    
    [JsonPropertyName("token_type")]
    public string TokenType { get; set; } = string.Empty;
    
    [JsonPropertyName("id_token")]
    public string IdToken { get; set; } = string.Empty;
    
    [JsonPropertyName("not-before-policy")]
    public int NotBeforePolicy { get; set; }
    
    [JsonPropertyName("session_state")]
    public string SessionState { get; set; } = string.Empty;
    
    [JsonPropertyName("scope")]
    public string Scope { get; set; } = string.Empty;
}
