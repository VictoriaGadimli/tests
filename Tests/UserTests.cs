using System.Net;
using Newtonsoft.Json;
using ProjectX.Models;
using ProjectX.Services;

namespace ProjectX.Tests;

public class UserTests
{
    private readonly RestClient _httpClient;
    private readonly UserService _userService;

    public UserTests(RestClient httpClient, UserService userService)
    {
        _httpClient = httpClient;
        _userService = userService;
    }

    [Fact]
    public async Task VerifyUserProfileIsSuccess()
    {
        UserRegistrationResult userRegistrationResult = await _userService.RegisterUser();

        var request = new RestRequest("/api/user/profile", Method.Get);
        request.AddHeader("Authorization", $"Bearer {userRegistrationResult.AccessToken}");

        var response = await _httpClient.ExecuteAsync(request);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //Verifying that the correct user profile has been received
        var userProfileResponse = JsonConvert.DeserializeObject<Response<UserProfileDto>>(response.Content);
        Assert.NotNull(userProfileResponse);
        Assert.True(userProfileResponse.isSuccess);
        Assert.NotNull(userProfileResponse.Data);

        Assert.Equal(userRegistrationResult.RegistredUser.Email, userProfileResponse.Data.Username);
        Assert.Equal(userRegistrationResult.RegistredUser.DisplayName, userProfileResponse.Data.DisplayName);
        Assert.Equal(userRegistrationResult.RegistredUser.Gender, userProfileResponse.Data.Gender);
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_When_AccessTokenIsInvalid() //401
    {
        var invalidAccessToken =
            "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJuYW1laWQiOiJ2aWxsLnNoYXJwQHRlc3QuY29tIiwibmJmIjoxNzI5NTMyMTM5LCJleHAiOjE3Mjk1NTAxMzksImlhdCI6MTcyOTUzMjEzOSwiaXNzIjoiWWVydGFwIiwiYXVkIjoieWVydGFwLWN1c3RvbWVycyJ9.gW0Zz2NGkrYg5jlqrSMLM_kQNWwQZKWsTXp";

        var request = new RestRequest("/api/user/profile", Method.Get);
        request.AddHeader("Authorization", $"Bearer {invalidAccessToken}");

        var response = await _httpClient.ExecuteAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_ReturnUnauthorized_When_AccessTokenIsMissing() //401
    {
        var request = new RestRequest("/api/user/profile", Method.Get);

        var response = await _httpClient.ExecuteAsync(request);
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Should_UpdateUserProfileSuccessfully()
    {
        UserRegistrationResult userRegistrationResult = await _userService.RegisterUser();

        var updateRequest = new RestRequest("/api/user/profile", Method.Put);
        updateRequest.AddHeader("Authorization", $"Bearer {userRegistrationResult.AccessToken}");
        updateRequest.AddJsonBody(new { DisplayName = "Updated Gomi Sharp" });

        var updateResponse = await _httpClient.ExecuteAsync(updateRequest);
        Assert.Equal(HttpStatusCode.OK, updateResponse.StatusCode);

        var updatedProfile = JsonConvert.DeserializeObject<Response<UserProfileDto>>(updateResponse.Content);

        // Assertions to verify the profile was updated correctly
        Assert.NotNull(updatedProfile);
        Assert.Equal("Updated Gomi Sharp", updatedProfile.Data.DisplayName);
    }
}