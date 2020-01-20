using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace StravaDiscordBot.Services
{
    public interface IRemoteItService
    {
        Task<string> GetCurrentProxyUrl();
    }

    public class RemoteItService : IRemoteItService
    {
        private const string BASE_REMOTE_IT_URL = "https://api.remot3.it";
        private readonly IOptionsMonitor<AppOptions> _options;

        public RemoteItService(IOptionsMonitor<AppOptions> options)
        {
            _options = options;
        }

        public async Task<string> GetCurrentProxyUrl()
        {
            var token = await GetToken();

            using(var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("developerkey", _options.CurrentValue.Remote.DeveloperKey);
                http.DefaultRequestHeaders.Add("token", token);
                var requestBody = new ConnectToDeviceRequest
                {
                    DeviceAddress = _options.CurrentValue.Remote.DeviceServiceId,
                    Wait = true,
                    HostIp = "0.0.0.0"
                };

                var response = await http.PostAsync($"{BASE_REMOTE_IT_URL}/apv/v27/user/login", new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                    throw new Exception("Failed to fetch Remote access token");

                var responseContentString = await response.Content.ReadAsStringAsync();
                var responseDeserialized = JsonConvert.DeserializeObject<ConnectToDeviceReponse>(responseContentString);

                return responseDeserialized.Connection.Proxy;
            }
        }

        private async Task<string> GetToken()
        {
            using (var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("developerkey", _options.CurrentValue.Remote.DeveloperKey);
                var requestBody = new RemoteAuthRequest
                {
                    Username = _options.CurrentValue.Remote.Username,
                    Password = _options.CurrentValue.Remote.Password
                };

                var response = await http.PostAsync($"{BASE_REMOTE_IT_URL}/apv/v27/user/login", new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));
                if (response.IsSuccessStatusCode)
                    throw new Exception("Failed to fetch Remote access token");

                var responseContentString = await response.Content.ReadAsStringAsync();
                var responseDeserialized = JsonConvert.DeserializeObject<RemoteAuthResponse>(responseContentString);

                return responseDeserialized.Token;
            }
        }

        private class RemoteAuthRequest
        {
            public string Username { get; set; }
            public string Password { get; set; }
        }

        private class RemoteAuthResponse 
        { 
            public bool Status { get; set; }
            public string Token { get; set; }
            public string Email { get; set; }
        }

        private class ConnectToDeviceRequest
        {
            public string DeviceAddress { get; set; }
            public bool Wait { get; set; }
            public string HostIp { get; set; }
        }

        private class ConnectToDeviceReponse
        {
            public RemoteConnection Connection { get; set; }
            public bool Status { get; set; }
        }

        private class RemoteConnection
        {
            public string DeviceAddress { get; set; }
            public string ExpirationSec { get; set; }
            public string Proxy { get; set; }
            public string ConnectionId { get; set; }
        }
    }
}
