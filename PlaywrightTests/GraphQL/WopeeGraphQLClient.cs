using System.Text;
using System.Text.Json;

namespace PlaywrightTests.GraphQL
{
    public static class WopeeGraphQLClient
    {
        private static HttpClient? _httpClient;
        private static string? _wopeeApiUrl;
        private static string? _wopeeApiKey;
        private static bool _isInitialized = false;

        /// <summary>
        /// Call once (e.g., in [AssemblyInitialize]) after DotNetEnv.Env.Load().
        /// This populates _wopeeApiUrl, _wopeeApiKey, and creates the HttpClient.
        /// </summary>
        public static void Initialize()
        {
            if (_isInitialized) 
                return;  // Prevent double-initialization if MSTest calls multiple times.

            _wopeeApiUrl = Environment.GetEnvironmentVariable("WOPEE_API_URL")
                ?? throw new InvalidOperationException("WOPEE_API_URL not set.");

            _wopeeApiKey = Environment.GetEnvironmentVariable("WOPEE_API_KEY")
                ?? throw new InvalidOperationException("WOPEE_API_KEY not set.");

            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("api_key", _wopeeApiKey);

            _isInitialized = true;
        }

        /// <summary>
        /// Send a GraphQL query or mutation with optional variables.
        /// Must call Initialize() before this if environment variables are not set globally.
        /// </summary>
        public static async Task<string> SendRequestAsync(string query, object? variables = null)
        {

            if (!_isInitialized)
            {
                throw new InvalidOperationException(
                    "WopeeGraphQLClient not initialized. " +
                    "Call WopeeGraphQLClient.Initialize() before sending requests."
                );
            }

            var requestBody = new
            {
                query,
                variables
            };

            var json = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            // _httpClient and _wopeeApiUrl are guaranteed non-null now if _isInitialized = true
            var response = await _httpClient!.PostAsync(_wopeeApiUrl, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (responseString.Contains("Not Authorised!"))
            {
                throw new UnauthorizedAccessException("GraphQL responded with 'Not Authorised!'");
            }


            if (!response.IsSuccessStatusCode)
            {
                throw new HttpRequestException(
                    $"GraphQL request failed.\nStatus: {response.StatusCode}\nBody: {responseString}"
                );
            }

            return responseString;
        }

        /// <summary>
        /// Helper to parse any field from the response JSON:
        /// e.g. objectName = "createIntegrationScenario", fieldName = "uuid"
        /// </summary>
        public static string? ExtractFieldFromResponse(
            string response,
            string objectName,
            string fieldName)
        {
            using var doc = JsonDocument.Parse(response);
            var data = doc.RootElement.GetProperty("data");

            if (data.TryGetProperty(objectName, out var obj))
            {
                if (obj.TryGetProperty(fieldName, out var fieldValue))
                {
                    return fieldValue.GetString();
                }
            }
            return null;
        }
    }
}
