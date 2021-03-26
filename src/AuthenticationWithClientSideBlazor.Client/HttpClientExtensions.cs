using System;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace AuthenticationWithClientSideBlazor.Client
{


    public static class HttpClientExtensions
    {

      

        public static async Task<T> ReadAsJsonAsync<T>(this HttpContent content)
        {
            var dataAsString = "";
            try
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                dataAsString = await content.ReadAsStringAsync();
                if (string.IsNullOrEmpty(dataAsString)) return default;
                return JsonSerializer.Deserialize<T>(dataAsString, options);
            }
            catch (Exception e)
            {
                throw new ArgumentException($"Invalid json: '{dataAsString}'", e.InnerException);
            }
        }

      
    }
}
