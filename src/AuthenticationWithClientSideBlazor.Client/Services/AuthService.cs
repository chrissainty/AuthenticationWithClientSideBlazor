using AuthenticationWithClientSideBlazor.Shared;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;

using System.Threading.Tasks;

namespace AuthenticationWithClientSideBlazor.Client.Services
{
    public class AuthService : IAuthService
    {
        private readonly HttpClient _httpClient;
        private readonly AuthenticationStateProvider _authenticationStateProvider;
        private readonly ILocalStorageService _localStorage;

        public AuthService(HttpClient httpClient,
                           AuthenticationStateProvider authenticationStateProvider,
                           ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _authenticationStateProvider = authenticationStateProvider;
            _localStorage = localStorage;
        }

        public async Task<RegisterResult> Register(RegisterModel registerModel)
        {
            var response = await _httpClient.PostAsJsonAsync("api/accounts", registerModel);
            var result = await response.Content.ReadAsJsonAsync<RegisterResult>();

            return result;
        }

        public async Task<LoginResult> Login(LoginModel loginModel)
        {
             var response = await _httpClient.PostAsJsonAsync("api/Login", loginModel);

             var loginResult = await response.Content.ReadAsJsonAsync<LoginResult>();

            if (!response.IsSuccessStatusCode)
            {
                return loginResult;
            }

            await _localStorage.SetItemAsync("authToken", loginResult.Token);
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(loginModel.Email);
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", loginResult.Token);

            return loginResult;
        }

        public async Task Logout()
        {
            await _localStorage.RemoveItemAsync("authToken");
            ((ApiAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
            _httpClient.DefaultRequestHeaders.Authorization = null;
        }
    }
}
