using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
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
        private readonly AppOptions _options;
        private readonly ILogger<RemoteItService> _logger;

        public RemoteItService(AppOptions options, ILogger<RemoteItService> logger)
        {
            _options = options;
            _logger = logger;
        }

        public async Task<string> GetCurrentProxyUrl()
        {
            var token = await GetToken();
            var remoteDeviceIp = await GetLastDeviceIp(token);
            using(var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("developerkey", _options.Remote.DeveloperKey);
                http.DefaultRequestHeaders.Add("token", token);
                var requestBody = new ConnectToDeviceRequest
                {
                    DeviceAddress = _options.Remote.DeviceServiceId,
                    Wait = true,
                    HostIp = "80.163.18.77"
                };

                var response = await http.PostAsync($"{BASE_REMOTE_IT_URL}/apv/v27/device/connect", new StringContent(JsonConvert.SerializeObject(requestBody), Encoding.UTF8, "application/json"));
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to fetch RemoteIt device proxy, response: {response.StatusCode}");
                    throw new Exception("Failed to fetch Remote access token");
                }

                var responseContentString = await response.Content.ReadAsStringAsync();
                var responseDeserialized = JsonConvert.DeserializeObject<ConnectToDeviceReponse>(responseContentString);

                _logger.LogInformation($"New device's proxy url: {responseDeserialized.Connection.Proxy}");

                return responseDeserialized.Connection.Proxy;
            }
        }

        private async Task<string> GetToken()
        {
            using (var http = new HttpClient())
            {
                _logger.LogInformation("Fetching RemoteIt token");
                http.DefaultRequestHeaders.Add("developerkey", _options.Remote.DeveloperKey);
                http.DefaultRequestHeaders.Add("Accept", "*/*");
                var requestBody = new RemoteAuthRequest
                {
                    Username = _options.Remote.Username,
                    Password = _options.Remote.Password
                };

                var response = await http.PostAsync($"{BASE_REMOTE_IT_URL}/apv/v27/user/login", new StringContent(JsonConvert.SerializeObject(requestBody)));
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to fetch RemoteIt token, response: {response.StatusCode}");
                    throw new Exception("Failed to fetch Remote access token");
                }

                var responseContentString = await response.Content.ReadAsStringAsync();
                var responseDeserialized = JsonConvert.DeserializeObject<RemoteAuthResponse>(responseContentString);

                return responseDeserialized.Token;
            }
        }

        private async Task<string> GetLastDeviceIp(string token)
        {
            using(var http = new HttpClient())
            {
                http.DefaultRequestHeaders.Add("developerkey", _options.Remote.DeveloperKey);
                http.DefaultRequestHeaders.Add("token", token);

                var response = await http.GetAsync($"{BASE_REMOTE_IT_URL}/apv/v27/device/list/all");
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Failed to fetch RemoteIt device, response: {response.StatusCode}");
                    throw new Exception("Failed to fetch Remote device");
                }

                var responseContentString = await response.Content.ReadAsStringAsync();
                var responseDeserialized = JsonConvert.DeserializeObject<RemoteDeviceResponse>(responseContentString);

                return responseDeserialized.Devices.First(x => x.DeviceAlias == _options.Remote.DeviceName).DeviceLastIp;
            }
        }

        private class RemoteAuthRequest
        {
            [JsonProperty("username")]
            public string Username { get; set; }
            [JsonProperty("password")]
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
            [JsonProperty("deviceaddress")]
            public string DeviceAddress { get; set; }
            [JsonProperty("wait")]
            public bool Wait { get; set; }
            [JsonProperty("hostip")]
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

        private class RemoteDeviceResponse
        {
            public List<RemoteDevice> Devices { get; set; }
        }

        private class RemoteDevice
        {
            public string CreatedDate { get; set; }
            public string DeviceAddress { get; set; }
            public string DeviceAlias { get; set; }
            public string DeviceLastIp { get; set; }
            public string DeviceState { get; set; }
            public string DeviceType { get; set; }
            public string LastInternalIp { get; set; }
        }
    }
}
