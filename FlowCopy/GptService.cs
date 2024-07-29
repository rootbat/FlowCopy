using Newtonsoft.Json.Linq;
using System.Net.Http.Headers;
using System.Net.Http;
using System.Text;

public class GptService
{
    private readonly HttpClient _client;

    public GptService(string apiKey)
    {
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", apiKey);
    }

    public async Task<string> GetChatGptResponse(string prompt)
    {
        var requestBody = new
        {
            model = "text-davinci-003",
            prompt = prompt,
            max_tokens = 150
        };

        var content = new StringContent(JObject.FromObject(requestBody).ToString(), Encoding.UTF8, "application/json");
        var response = await _client.PostAsync("https://api.openai.com/v1/completions", content);
        var responseString = await response.Content.ReadAsStringAsync();
        var responseJson = JObject.Parse(responseString);

        return responseJson["choices"][0]["text"].ToString().Trim();
    }
}
