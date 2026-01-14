using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using ContosoPizza.Client;
using ContosoPizza.Client.Services;

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

await builder.Build().RunAsync();
