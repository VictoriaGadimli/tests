using System.Net;
using Bogus;
using Bogus.DataSets;
using Newtonsoft.Json;
using ProjectX.Models;
using ProjectX.Services;

namespace ProjectX;

public class IdentityTests
{
    private readonly RestClient _httpClient;
    private readonly UserService _userService;

    public IdentityTests(RestClient httpClient, UserService userService)
    {
        _httpClient = httpClient;
        _userService = userService;
    }

    [Fact]
    public async Task RegisterUser_ShouldReturnOk()
    {
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Email, f => f.Person.Email)
            .RuleFor(u => u.Password, f => "Password123") // Fixed password for testing
            .RuleFor(u => u.ConfirmPassword, (f, u) => u.Password)
            .RuleFor(u => u.DisplayName, f => f.Name.FullName())
            .RuleFor(u => u.Gender, f => f.PickRandom(new[] { "Male", "Female" }))
            .RuleFor(u => u.PreferredCategoryKeys, f => new[] { f.Random.Word(), f.Random.Word() })
            .RuleFor(u => u.CountryKey, f => f.Address.Country());
        
        var newUser = userFaker.Generate();
        
        var request = new RestRequest("/api/user/identity/register", Method.Post);
        request.AddJsonBody(newUser);

        var response = await _httpClient.ExecuteAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Check that the returned content contains a registered user
        var tokenDto = JsonConvert.DeserializeObject<Response<TokenDto>>(response.Content);
        Assert.NotNull(tokenDto);
        Assert.NotEmpty(tokenDto.Data.AccessToken);
        Assert.NotEqual(0, tokenDto.Data.ExpiresIn);
        Assert.NotEmpty(tokenDto.Data.RefreshToken);
    }

    [Fact]
    public async Task RegisterUser_WithExistingEmail_ShouldReturnBadRequest()
    {
        var existingUser = new User
        {
            Email = "mari.doe@example.com", // Email already registered
            Password = "Password123",
            ConfirmPassword = "Password123",
            DisplayName = "Mari Doe",
            Gender = "Female",
            PreferredCategoryKeys = ["Key1", "Key2"],
            CountryKey = "UK"
        };

        var request = new RestRequest("/api/user/identity/register", Method.Post);
        request.AddJsonBody(existingUser);

        var response = await _httpClient.ExecuteAsync(request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode); //400 
    }

    [Fact]
    public async Task LoginUser_ShouldReturnToken()
    {
        var loginUser = new User
        {
            Email = "mari.doe@example.com",
            Password = "Password123"
        };

        var request = new RestRequest("/api/user/identity/login-email", Method.Post);
        request.AddJsonBody(loginUser);

        var response = await _httpClient.ExecuteAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tokenDto = JsonConvert.DeserializeObject<Response<TokenDto>>(response.Content);
        Assert.NotNull(tokenDto);
        Assert.NotEmpty(tokenDto.Data.AccessToken);
        Assert.NotEmpty(tokenDto.Data.RefreshToken);
    }

    [Theory]
    [InlineData("invalid-email@example.com", "Password123")]
    [InlineData("mari.doe@example.com", "WrongPassword")]
    public async Task LoginUser_WithInvalidCredentials_ShouldReturnUnauthorized(string email, string password)
    {
        var loginUser = new { Email = email, Password = password };

        var request = new RestRequest("/api/user/identity/login-email", Method.Post);
        request.AddJsonBody(loginUser);

        var response = await _httpClient.ExecuteAsync(request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); // 401
    }

    [Fact]
    public async Task Should_ReturnNewAccessToken_When_RefreshTokenIsUsed()
    {
        var refreshTokenRequest = new
        {
            RefreshToken =
                "CfDJ8NESHzjbvWpEntyEnBIJLWgMs1OzfCiycl6kKjiIQqSXdr_RHNo3UqL7ny8IimTpakQ6uAcLNcBb1DQKJOIdwc5WDtla2mvVtUWVd2jn-XaTYe_wlNd_weyam1uArSv2qw_9u_8GYpw97BuTPn0w1nHyr0FynVAJElZeeUj6n0NTVDta9JM0P2BejpAMqAyvdu1OjOWkHKXC1CfJ1mhezFYUX8MJ3pIuSxIh0_0TxWVYR-Tfif3gc3uA2yC0bLb1VSmi3gWgWSNHzvw63Ee3FCKAxlUSMZTHclyR1ohrt8pbVs4HVkCQasE5Zv0MGtgeyPBKI7dntHavE2QHQUZYDow"
        };

        var request = new RestRequest("/api/identity/token/refresh", Method.Post);
        request.AddJsonBody(refreshTokenRequest);

        var response = await _httpClient.ExecuteAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var tokenDto = JsonConvert.DeserializeObject<Response<TokenDto>>(response.Content);
        Assert.NotNull(tokenDto);
        Assert.NotEmpty(tokenDto.Data.AccessToken);
        Assert.NotEmpty(tokenDto.Data.RefreshToken);
    }

    [Fact]
    public async Task RefreshToken_WithInvalidToken_ShouldReturnUnauthorized()
    {
        var invalidTokenRequest = new
        {
            RefreshToken = "invalid-refresh-token"
        };

        var request = new RestRequest("/api/identity/token/refresh", Method.Post);
        request.AddJsonBody(invalidTokenRequest);

        var response = await _httpClient.ExecuteAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode); // 401
    }

    [Fact]
    public async Task GetUserByValidEmail_ShouldReturnOk()
    {
        //Register new user

        string testEmail = "gomi.boss@test.com";

        _ = await _userService.RegisterUser(); //Bogus

        var request = new RestRequest($"/api/user/identity/login-email?email={testEmail}", Method.Get);
        var response = await _httpClient.ExecuteAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUserByInvalidId_ShouldReturnNotFound()
    {
        string email = "test@test.com";
        var client = new RestClient("https://api.greenwater-c062bd3e.westeurope.azurecontainerapps.io");
        var request = new RestRequest($"/api/user/identity/login-email?email={email}", Method.Get);

        var response = await client.ExecuteAsync(request);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}