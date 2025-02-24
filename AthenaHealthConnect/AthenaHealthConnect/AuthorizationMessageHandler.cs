namespace AthenaHealthConnect;

public class AuthorizationMessageHandler : HttpClientHandler
{
    public string? AuthorizationToken { get; set; }
    protected async override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        if (AuthorizationToken != null)
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", AuthorizationToken);
        return await base.SendAsync(request, cancellationToken);
    }
}
