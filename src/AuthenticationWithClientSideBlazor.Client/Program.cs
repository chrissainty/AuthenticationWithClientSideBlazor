using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net.Http;
using System.Threading.Tasks;
using AuthenticationWithClientSideBlazor.Client.Services;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace AuthenticationWithClientSideBlazor.Client
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddHttpClient("AuthenticationWithClientSideBlazor.ServerAPI",
                client => client.BaseAddress = new Uri(builder.HostEnvironment.BaseAddress));
            
            // Supply HttpClient instances that include access tokens when making requests to the server project
            builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>().CreateClient("AuthenticationWithClientSideBlazor.ServerAPI"));
            builder.Services.AddAuthorizationCore();
             builder.Services.AddBlazoredLocalStorage();
             builder.Services.AddScoped<AuthenticationStateProvider, ApiAuthenticationStateProvider>();

            builder.Services.AddScoped<IAuthService, AuthService>();
            await builder.Build().RunAsync();
        }
    }
}
