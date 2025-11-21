using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Text.Json;
using System.Text;

namespace KNVBChatbot.Controllers
{
    [ApiController]
    [Route("chat")]
    public class ChatController : ControllerBase
    {
        private readonly HttpClient _httpClient;

        public ChatController(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request)
        {
            var searchResults = await SearchKNVBRules(request.Question);
            var answer = await GenerateAnswer(request.Question, searchResults);
            return Ok(new { answer });
        }

        private async Task<string> SearchKNVBRules(string query)
        {
            var url = $"{Environment.GetEnvironmentVariable("AZURE_SEARCH_ENDPOINT")}/indexes/spelregels-index/docs?api-version=2021-04-30-Preview&search={query}";
            var req = new HttpRequestMessage(HttpMethod.Get, url);
            req.Headers.Add("api-key", Environment.GetEnvironmentVariable("AZURE_SEARCH_KEY"));
            var res = await _httpClient.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            var passages = doc.RootElement.GetProperty("value")
                .EnumerateArray()
                .Take(3)
                .Select(e => e.GetProperty("content").GetString());
            return string.Join("\n", passages);
        }

        private async Task<string> GenerateAnswer(string question, string context)
        {
            var payload = new
            {
                messages = new[]
                {
                    new { role = "system", content = "Je bent een behulpzame assistent." },
                    new { role = "user", content = $"Context:\n{context}\nVraag:\n{question}" }
                },
                max_tokens = 500
            };

            var req = new HttpRequestMessage(HttpMethod.Post,
                $"{Environment.GetEnvironmentVariable("AZURE_OPENAI_ENDPOINT")}/openai/deployments/gpt-4o-mini/chat/completions?api-version=2024-02-15-preview");
            req.Headers.Add("api-key", Environment.GetEnvironmentVariable("AZURE_OPENAI_KEY"));
            req.Content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");

            var res = await _httpClient.SendAsync(req);
            var json = await res.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString();
        }
    }
}
