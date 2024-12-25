using Newtonsoft.Json;
using RestSharp;

namespace Mixvel
{
    public class ProviderClient<T, S> : IDisposable
        where T : class
        where S : class
    {
        private readonly RestClient _client;

        private const string pingEndpoint = "ping";
        private const string searchEndpoint = "search";

        public ProviderClient(string baseUrl)
        {
            _client = new RestClient(baseUrl);
        }
        
        public void Dispose()
        {
            _client?.Dispose();
        }

        public Task<RestResponse> PingAsync(CancellationToken token)
        {
            var request = new RestRequest();
            request.Resource = pingEndpoint;
            //if both providers have to be available
            return _client.ExecuteAsync(request, token);
        }

        public async Task<S> SearchAsync(T request, CancellationToken token)
        {
            var restRequest = new RestRequest();
            restRequest.Resource = searchEndpoint;
            restRequest.AddHeader("Content-type", "application/json");
            restRequest.AddJsonBody(request);

            var response = await _client.PostAsync(restRequest, token);
            if (response.IsSuccessStatusCode)
                return JsonConvert.DeserializeObject<S>(response.Content);

            return null;
        }
    }
}
