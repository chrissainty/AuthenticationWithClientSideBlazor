using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;

namespace AuthenticationWithClientSideBlazor.Client
{
    public class ApiAuthenticationStateProvider : AuthenticationStateProvider
    {
        private readonly HttpClient _httpClient;
        private readonly ILocalStorageService _localStorage;

        public ApiAuthenticationStateProvider(HttpClient httpClient, ILocalStorageService localStorage)
        {
            _httpClient = httpClient;
            _localStorage = localStorage;
        }
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            var savedToken = await _localStorage.GetItemAsync<string>("authToken");

            if (string.IsNullOrWhiteSpace(savedToken))
            {
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("bearer", savedToken);

            return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(savedToken), "jwt")));
        }

        public void MarkUserAsAuthenticated(string token)
        {
			var authenticatedUser = new ClaimsPrincipal(new ClaimsIdentity(ParseClaimsFromJwt(token), "jwt"));
			var authState = Task.FromResult(new AuthenticationState(authenticatedUser));
            NotifyAuthenticationStateChanged(authState);
        }

        public void MarkUserAsLoggedOut()
        {
            var anonymousUser = new ClaimsPrincipal(new ClaimsIdentity());
            var authState = Task.FromResult(new AuthenticationState(anonymousUser));
            NotifyAuthenticationStateChanged(authState);
        }

        private IEnumerable<Claim> ParseClaimsFromJwt(string jwtToken)
        {
			string[] tokenSegments = jwtToken.Split('.');
			if (tokenSegments.Length != 3)
				throw new ArgumentException("JWT Token should have three segments");

			var payload = tokenSegments[1];
			var jsonBytes = ParseBase64WithoutPadding(payload);
			var keyValuePairs = JsonSerializer.Deserialize<Dictionary<string, object>>(jsonBytes);

			var claims = new List<Claim>();
			if ((keyValuePairs.TryGetValue(ClaimTypes.Role, out object roles)) && (roles != null) && (roles is JsonElement))
			{
				JsonElement jsonRoles = (JsonElement)roles;
				if (jsonRoles.ValueKind == JsonValueKind.Array)
					claims.AddRange(jsonRoles.EnumerateArray().Select(role => new Claim(ClaimTypes.Role, role.GetString())));
				else
					claims.Add(new Claim(ClaimTypes.Role, roles.ToString()));

				// remove the roles claim from the  
				keyValuePairs.Remove(ClaimTypes.Role);
			}
			//add other claims as well
			claims.AddRange(keyValuePairs.Select(kvp => new Claim(kvp.Key, kvp.Value.ToString())));
			return claims;
		}

        private byte[] ParseBase64WithoutPadding(string base64)
        {
            switch (base64.Length % 4)
            {
                case 2: base64 += "=="; break;
                case 3: base64 += "="; break;
            }
            return Convert.FromBase64String(base64);
        }
    }
}
