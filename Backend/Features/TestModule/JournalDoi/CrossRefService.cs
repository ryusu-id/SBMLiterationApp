namespace PureTCOWebApp.Features.TestModule.JournalDoi;

public class CrossRefService(HttpClient httpClient)
{
    private readonly HttpClient _httpClient = httpClient;

    public async Task<WorkMessage?> GetWorkByDoiAsync(string doi)
    {
        var cleanDoi = ExtractDoi(doi);
        var url = $"works/{cleanDoi}";
        var response = await _httpClient.GetFromJsonAsync<CrossRefApiResponse>(url);
        return response?.Message;
    }

    private static string ExtractDoi(string input)
    {
        var doiIndex = input.IndexOf("doi.org/", StringComparison.OrdinalIgnoreCase);
        if (doiIndex >= 0)
        {
            return input.Substring(doiIndex + 8);
        }
        return input;
    }
}

internal record CrossRefApiResponse(string Status, string MessageType, string MessageVersion, WorkMessage Message);
