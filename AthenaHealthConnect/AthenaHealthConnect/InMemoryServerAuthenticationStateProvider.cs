using Duende.IdentityModel.Client;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using System.Security.Claims;

namespace AthenaHealthConnect;

public class InMemoryServerAuthenticationStateProvider : ServerAuthenticationStateProvider
{
    private readonly ILogger<InMemoryServerAuthenticationStateProvider> logger;
    private readonly IConfiguration configuration;

    private InMemoryTokenStorage TokenStorage { get; }

    public InMemoryServerAuthenticationStateProvider(ILogger<InMemoryServerAuthenticationStateProvider> logger, IConfiguration configuration, InMemoryTokenStorage tokenStorage)
    {
        this.logger = logger;
        this.configuration = configuration;
        this.TokenStorage = tokenStorage;
    }

    public void SetCodeAsync(string code)
    {
        NotifyAuthenticationStateChanged(GetAuthenticationStateFromCode(code));
    }
    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(CreatePrincipal(TokenStorage.TokenResponse)));
    }

    private async Task<AuthenticationState> GetAuthenticationStateFromCode(string code)
    {
        var storedTokenResponse = TokenStorage.TokenResponse;
        if (storedTokenResponse != null)
        {
            return new AuthenticationState(CreatePrincipal(storedTokenResponse));
        }

        var client = new TokenClient(
            new HttpClient() { BaseAddress = new Uri(FhirSample.AthenaServer + "/oauth2/token") },
            new TokenClientOptions()
            {
                ClientId = configuration["AthenaHealth:ClientId"],
                ClientSecret = configuration["AthenaHealth:ClientSecret"]
            });
        var tokenResponse = await client.RequestAuthorizationCodeTokenAsync(code, configuration["AthenaHealth:RedirectUrl"]);
        if (tokenResponse.Json.HasValue && !tokenResponse.IsError)
        {
            TokenStorage.TokenResponse = tokenResponse;
        }

        return new AuthenticationState(CreatePrincipal(tokenResponse));
    }

    ClaimsPrincipal CreatePrincipal(TokenResponse? tokenResponse)
    {
        if (tokenResponse == null)
        {
            return new ClaimsPrincipal(new ClaimsIdentity());
        }
        if (tokenResponse.Json.HasValue && !tokenResponse.IsError)
        {
            var PatientId = tokenResponse.Json.Value.GetProperty("patient").GetString() ?? "-";
            var TokenScopes = tokenResponse.Json.Value.GetProperty("scope").GetString() ?? "-";
            var Uuid = tokenResponse.Json.Value.GetProperty("uuid").GetString() ?? "-";
            var NeedPatientBanner = tokenResponse.Json.Value.GetProperty("need_patient_banner").GetBoolean();
            var SmartUrl = tokenResponse.Json.Value.GetProperty("smart_style_url").GetString() ?? "-";
            return new ClaimsPrincipal(
                new ClaimsIdentity(
                    [
                        new Claim("PatientId", PatientId),
                        new Claim("Uuid", Uuid),
                        new Claim("SmartUrl", SmartUrl),
                        new Claim("NeedPatientBanner", NeedPatientBanner.ToString()),
                        new Claim("TokenScopes", TokenScopes),
                        new Claim("access_token", tokenResponse.AccessToken),
                        new Claim("id_token", tokenResponse.IdentityToken),
                    ], "Bearer"));
        }

        if (tokenResponse.IsError)
        {
            this.logger.LogError(tokenResponse.ErrorType + ": " + tokenResponse.Error + " " + tokenResponse.ErrorDescription);
        }

        return new ClaimsPrincipal(new ClaimsIdentity());
    }
}
