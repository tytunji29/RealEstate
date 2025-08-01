using System.Text;
using System.Text.Json;

namespace RealEstate.Repository;

public interface IPinnacleService
{
    Task<string> GenerateVirtualAccountAsync(string payload);
}
public class PinnacleService: IPinnacleService
{
    private readonly HttpClient _httpClient;

    public PinnacleService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<string> GenerateVirtualAccountAsync(string json)
    {
        var url = "https://pinnacle.ng/genvirtacct.ashx";

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        // Add custom header
        content.Headers.Add("PINNACLE_KEY_HASH", "9720e1ab-ef71-4360-a206-3debcb774d18");

        var response = await _httpClient.PostAsync(url, content);
        response.EnsureSuccessStatusCode();

        var responseBody = await response.Content.ReadAsStringAsync();
        return responseBody;
    }
}