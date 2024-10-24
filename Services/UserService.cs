using Bogus;
using Newtonsoft.Json;
using ProjectX.Models;

namespace ProjectX.Services;

public class UserService
{
    private readonly RestClient _httpClient;

    public UserService(RestClient httpClient)
    {
        _httpClient = httpClient;
    }

    private User GenerateFakeUser()
    {
        var userFaker = new Faker<User>()
            .RuleFor(u => u.Email, f => f.Internet.Email())
            .RuleFor(u => u.Password, f => "Password123") // Fixed password for testing
            .RuleFor(u => u.ConfirmPassword, (f, u) => u.Password)
            .RuleFor(u => u.DisplayName, f => f.Name.FullName())
            .RuleFor(u => u.Gender, f => f.PickRandom(new[] { "Male", "Female" }))
            .RuleFor(u => u.PreferredCategoryKeys, f => new[] { f.Random.Word(), f.Random.Word() })
            .RuleFor(u => u.CountryKey, f => f.Address.Country());

        return userFaker.Generate();
    }

    public async Task<UserRegistrationResult> RegisterUser()
    {
        var newUser = GenerateFakeUser();

        var request = new RestRequest("/api/user/identity/register", Method.Post);
        request.AddJsonBody(newUser);
        var response = await _httpClient.ExecuteAsync(request);

        var tokenDto = JsonConvert.DeserializeObject<Response<TokenDto>>(response.Content);
        //return tokenDto.Data.AccessToken
            
        return new UserRegistrationResult
        {
            RegistredUser = newUser,
            AccessToken = tokenDto.Data.AccessToken,
        };
    }
}

public class UserRegistrationResult
{
    public User RegistredUser { get; set; }
    public string AccessToken { get; set; }
}