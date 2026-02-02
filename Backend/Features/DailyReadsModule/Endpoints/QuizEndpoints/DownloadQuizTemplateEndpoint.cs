using FastEndpoints;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Features.DailyReadsModule.Endpoints.QuizEndpoints;

public class DownloadQuizTemplateEndpoint : EndpointWithoutRequest<object>
{
    public override void Configure()
    {
        Get("quiz/template");
        Group<DailyReadsEndpointGroup>();
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        string binDirectory = Path.GetDirectoryName(AppDomain.CurrentDomain.BaseDirectory)!;
        var filePath = Path.Combine(binDirectory, "Templates", "QuestionListTemplate.xlsx");

        if (!File.Exists(filePath))
        {
            await Send.ResultAsync(
                TypedResults.NotFound<ApiResponse>(
                    Result.Failure(new Error("Template.NotFound", "Template file not found"))
                )
            );
            return;
        }

        var file = new FileStream(filePath, FileMode.Open, FileAccess.Read);

        await Send.StreamAsync(
            stream: file,
            fileName: "QuestionListTemplate.xlsx",
            fileLengthBytes: file.Length,
            contentType: "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
        );
    }
}
