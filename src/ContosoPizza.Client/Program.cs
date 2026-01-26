using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ContosoPizza.Client;
using ContosoPizza.Client.Services;
using Blazorise;
using Blazorise.Bootstrap5;
using Blazorise.Icons.FontAwesome;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

var apiBaseUrl = builder.Configuration["ApiBaseUrl"];
if (string.IsNullOrWhiteSpace(apiBaseUrl))
{
    var frontendScheme = new Uri(builder.HostEnvironment.BaseAddress, UriKind.Absolute).Scheme;
    apiBaseUrl = frontendScheme.Equals("https", StringComparison.OrdinalIgnoreCase)
        ? builder.Configuration["ApiBaseUrlHttps"]
        : builder.Configuration["ApiBaseUrlHttp"];
}

apiBaseUrl ??= builder.HostEnvironment.BaseAddress;
builder.Services.AddScoped(_ => new HttpClient { BaseAddress = new Uri(apiBaseUrl, UriKind.Absolute) });
builder.Services.AddScoped<PizzaApiClient>();

builder.Services
    .AddBlazorise( options =>
    {
        options.Immediate = true;
    } )
    .AddBootstrap5Providers()
    .AddFontAwesomeIcons();

await builder.Build().RunAsync();
