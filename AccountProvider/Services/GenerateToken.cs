using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;

namespace AccountProvider.Services;

public class GenerateToken
{
    private readonly HttpClient _httpClient;

    public GenerateToken(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<HttpResponseMessage> GenerateTokenAsync(string email, string userId)
    {
        var tokenRequest = new { Email = email, UserId = userId };
        var json = JsonConvert.SerializeObject(tokenRequest);
        var content = new StringContent(json, Encoding.UTF8, "application/json");

        var tokenProviderUrl = "https://tokenprovider--silicon.azurewebsites.net/api/GenerateToken?code=h6JSG711uVK_bfONCqCY7lVJdVhyQTjNifgc_YWmJWufAzFu6WRE6A%3D%3D";

        var requestMessage = new HttpRequestMessage(HttpMethod.Post, tokenProviderUrl)
        {
            Content = content
        };
        requestMessage.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return await _httpClient.SendAsync(requestMessage);
    }
}
