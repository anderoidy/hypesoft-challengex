using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Hypesoft.API.Controllers.Base;

namespace Hypesoft.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public class UsersController : BaseAuthController
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<UsersController> _logger;

    public UsersController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<UsersController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
    }

    [HttpGet("me")]
    public IActionResult GetCurrentUser()
    {
        try
        {
            var userInfo = new
            {
                Id = UserId,
                Email = UserEmail,
                Username = UserName,
                Roles = UserRoles.ToList()
            };

            return Ok(userInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao obter informações do usuário atual");
            return StatusCode(500, new { message = "Ocorreu um erro ao obter as informações do usuário" });
        }
    }

    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetUsers()
    {
        try
        {
            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var usersEndpoint = $"{_configuration["Keycloak:Authority"]}/admin/realms/{_configuration["Keycloak:Realm"]}/users";
            
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            var response = await client.GetAsync(usersEndpoint);
            var content = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Falha ao obter lista de usuários. Status: {StatusCode}, Resposta: {Response}", 
                    response.StatusCode, content);
                return StatusCode((int)response.StatusCode, new { message = "Falha ao obter lista de usuários do Keycloak" });
            }

            var users = JsonSerializer.Deserialize<List<KeycloakUser>>(content);
            var simplifiedUsers = users?.Select(u => new
            {
                u.Id,
                u.Username,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Enabled,
                u.EmailVerified
            });

            return Ok(simplifiedUsers);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao listar usuários");
            return StatusCode(500, new { message = "Ocorreu um erro ao listar os usuários" });
        }
    }

    [HttpPut("me")]
    public async Task<IActionResult> UpdateCurrentUser([FromBody] UpdateUserRequest request)
    {
        try
        {
            if (string.IsNullOrEmpty(UserId))
            {
                return Unauthorized();
            }

            var token = HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
            var userEndpoint = $"{_configuration["Keycloak:Authority"]}/admin/realms/{_configuration["Keycloak:Realm"]}/users/{UserId}";
            
            var client = _httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            
            // Primeiro, obtenha o usuário atual
            var getUserResponse = await client.GetAsync(userEndpoint);
            if (!getUserResponse.IsSuccessStatusCode)
            {
                _logger.LogWarning("Usuário não encontrado no Keycloak. UserId: {UserId}", UserId);
                return NotFound(new { message = "Usuário não encontrado" });
            }

            var user = JsonSerializer.Deserialize<KeycloakUser>(
                await getUserResponse.Content.ReadAsStringAsync());

            // Atualize apenas os campos fornecidos
#pragma warning disable CS8602 // Desreferência de uma referência possivelmente nula.
            if (request.FirstName != null) user.FirstName = request.FirstName ;
#pragma warning restore CS8602 // Desreferência de uma referência possivelmente nula.
#pragma warning disable CS8602 // Desreferência de uma referência possivelmente nula.
            if (request.LastName != null) user.LastName = request.LastName;
#pragma warning restore CS8602 // Desreferência de uma referência possivelmente nula.
#pragma warning disable CS8602 // Desreferência de uma referência possivelmente nula
            if (request.Email != null) user.Email = request.Email;
            
            // Atualize o usuário no Keycloak
            var updateResponse = await client.PutAsJsonAsync(userEndpoint, user);
            
            if (!updateResponse.IsSuccessStatusCode)
            {
                var errorContent = await updateResponse.Content.ReadAsStringAsync();
                _logger.LogWarning("Falha ao atualizar usuário. Status: {StatusCode}, Resposta: {Response}", 
                    updateResponse.StatusCode, errorContent);
                return StatusCode((int)updateResponse.StatusCode, 
                    new { message = "Falha ao atualizar o usuário no Keycloak" });
            }

            return Ok(new { message = "Usuário atualizado com sucesso" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar usuário");
            return StatusCode(500, new { message = "Ocorreu um erro ao atualizar o usuário" });
        }
    }
}

public class UpdateUserRequest
{
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
}

public class KeycloakUser
{
    public string? Id { get; set; }
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public bool Enabled { get; set; } = true;
    public bool EmailVerified { get; set; }
    public List<string>? RequiredActions { get; set; }
    public Dictionary<string, List<string>>? Attributes { get; set; }
    public List<KeycloakCredential>? Credentials { get; set; }
    public List<string>? RealmRoles { get; set; }
    public Dictionary<string, List<string>>? ClientRoles { get; set; }
    public List<string>? Groups { get; set; }
    public bool? EmailVerifiedBool => EmailVerified;
    public bool? EnabledBool => Enabled;
}

public class KeycloakCredential
{
    public string? Type { get; set; }
    public string? Value { get; set; }
    public bool Temporary { get; set; }
}
