namespace PureTCOWebApp.Features.TestModule.GoogleBook;

public class GoogleBooksService
{
    private readonly HttpClient _httpClient;
    private readonly string _apiKey;

    public GoogleBooksService(HttpClient httpClient, IConfiguration configuration)
    {
        _httpClient = httpClient;
        _apiKey = configuration["GoogleBooks:ApiKey"] ?? throw new InvalidOperationException("Google Books API key not configured");
    }

    public async Task<GoogleBooksResponse?> SearchBooksAsync(string query, int maxResults = 10, int startIndex = 0)
    {
        var url = $"volumes?q={Uri.EscapeDataString(query)}&maxResults={maxResults}&startIndex={startIndex}&key={_apiKey}";
        return await _httpClient.GetFromJsonAsync<GoogleBooksResponse>(url);
    }

    public async Task<BookItem?> GetBookByIdAsync(string volumeId)
    {
        var url = $"volumes/{volumeId}?key={_apiKey}";
        return await _httpClient.GetFromJsonAsync<BookItem>(url);
    }
}
