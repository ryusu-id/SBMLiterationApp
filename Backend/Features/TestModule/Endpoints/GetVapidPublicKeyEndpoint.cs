using FastEndpoints;
using Microsoft.Extensions.Options;
using PureTCOWebApp.Core;
using PureTCOWebApp.Core.Models;

namespace PureTCOWebApp.Features.TestModule.Endpoints;

public record GetVapidPublicKeyResponse(string PublicKey, bool IsConfigured);

/// <summary>
/// Returns the VAPID public key needed by the browser to create a push subscription.
///
/// Call this from the frontend with:
///   const res = await fetch('/api/test-items/push-notification/vapid-key')
///   const { publicKey } = await res.json()
///   registration.pushManager.subscribe({ userVisibleOnly: true, applicationServerKey: publicKey })
///
/// Route: GET /api/test-items/push-notification/vapid-key
/// </summary>
public class GetVapidPublicKeyEndpoint(
    IOptions<VapidSettings> vapidOptions
) : EndpointWithoutRequest<ApiResponse<GetVapidPublicKeyResponse>>
{
    public override void Configure()
    {
        Get("push-notification/vapid-key");
        Group<TestModuleEndpointGroup>();
        AllowAnonymous();
        Summary(s =>
        {
            s.Summary = "[Test] Get the VAPID public key to use when subscribing in the browser.";
            s.Description =
                "Returns the VAPID public key from configuration. " +
                "Use this key in pushManager.subscribe({ applicationServerKey }) on the frontend.";
        });
    }

    public override async Task HandleAsync(CancellationToken ct)
    {
        var settings = vapidOptions.Value;

        await Send.OkAsync(
            Result.Success(new GetVapidPublicKeyResponse(
                PublicKey: settings.PublicKey,
                IsConfigured: settings.IsConfigured
            )),
            cancellation: ct);
    }
}
