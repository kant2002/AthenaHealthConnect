using AthenaHealthConnect;
using AthenaHealthConnect.Components;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Server;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents()
    .AddAuthenticationStateSerialization(
        options => options.SerializeAllClaims = true);

builder.Services.AddSingleton<InMemoryTokenStorage>();

_ = builder.Services.AddAuthentication()
    //.AddOAuth("AthenaHealth", "Connect using Athena Health", options =>
    //{
    //    options.ClientId = builder.Configuration["AthenaHealth:ClientId"];
    //    options.ClientSecret = builder.Configuration["AthenaHealth:ClientSecret"];
    //    options.AuthorizationEndpoint = builder.Configuration["AthenaHealth:AuthorizationEndpoint"];
    //    options.TokenEndpoint = builder.Configuration["AthenaHealth:TokenEndpoint"];
    //    options.CallbackPath = builder.Configuration["AthenaHealth:CallbackPath"];

    //    options.SaveTokens = true;
    //    options.Scope.Add("openid");
    //    options.Scope.Add("profile");
    //    options.Scope.Add("patient/*.read");
    //    options.Scope.Add("launch/patient");

    //    options.ClaimActions.MapJsonKey("urn:athenahealth:patient_id", "patient_id");
    //    options.ClaimActions.MapJsonKey("urn:athenahealth:patient_name", "patient_name");
    //})
    ;

builder.Services.AddAuthorization();
builder.Services.AddCascadingAuthenticationState();
builder.Services.AddScoped<AuthenticationStateProvider, ServerAuthenticationStateProvider>();
//builder.Services.AddScoped<AuthenticationStateProvider, InMemoryServerAuthenticationStateProvider>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseWebAssemblyDebugging();
}
else
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();


// For authentication
app.UseAuthentication();
app.UseAuthorization();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(AthenaHealthConnect.Client._Imports).Assembly);

app.Run();
