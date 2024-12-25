using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Mixvel.Contracts;
using Mixvel.Interfaces;
using RestSharp;
using System.Net;

namespace Mixvel.Services
{
    public class SearchService : ISearchService
    {
        private readonly ILogger<SearchService> _logger = null!;
        private readonly ProviderClient<ProviderOneSearchRequest, ProviderOneSearchResponse> _clientOne = null!;
        private readonly ProviderClient<ProviderTwoSearchRequest, ProviderTwoSearchResponse> _clientTwo = null!;
        private readonly IMemoryCache _cache = null!;
        private readonly IMapper _mapper = null!;

        public SearchService(ILogger<SearchService> logger,
                            IMapper mapper,
                            IMemoryCache cache)
        {
            _logger = logger;
            _clientOne = new ProviderClient<ProviderOneSearchRequest, ProviderOneSearchResponse>("http://provider-one/api/v1/");
            _clientTwo = new ProviderClient<ProviderTwoSearchRequest, ProviderTwoSearchResponse>("http://provider-two/api/v1/");
            _mapper = mapper;
            _cache = cache;
        }
        public async Task<bool> IsAvailableAsync(CancellationToken cancellationToken)
        {
            List<Task<RestResponse>> tasks = new List<Task<RestResponse>>();
            //if both providers have to be available
            tasks.Add(_clientOne.PingAsync(cancellationToken));
            tasks.Add(_clientTwo.PingAsync(cancellationToken));
            //if only one
            //var result = await _clientOne.PingAsync(cancellationToken);
            //if(result.StatusCode == HttpStatusCode.OK)
            //  return true;
            //var result = await _clientTwo.PingAsync(cancellationToken);
            //if(result.StatusCode == HttpStatusCode.OK)
            //  return true;

            //if both providers have to be available
            await Task.WhenAll(tasks);
            return tasks.All(x => x.Result.StatusCode == HttpStatusCode.OK);
            //if only one
            //return false;
        }

        public async Task<SearchResponse> SearchAsync(SearchRequest request, CancellationToken cancellationToken)
        {
            if (request.Filters.OnlyCached == true)
            {
                if (!_cache.TryGetValue(request, out SearchResponse result))
                {
                    result = await SearchInProvidersAsync(request, cancellationToken, true);
                }

                return result;
            }

            return await SearchInProvidersAsync(request, cancellationToken);
        }

        private async Task<SearchResponse> SearchInProvidersAsync(SearchRequest request, CancellationToken cancellationToken, bool saveToCache = false)
        {
            var result = new SearchResponse();
            var providerOneSearchRequest = _mapper.Map<ProviderOneSearchRequest>(request);
            var providerOneSearchResponse = await _clientOne.SearchAsync(providerOneSearchRequest, cancellationToken);
            //user check tickets by my expirience around 30 minutes, this value need configure in settings
            DateTime minExpirationTime = DateTime.Now.AddMinutes(30);

            if (providerOneSearchResponse != null)
            {
                var resultOneRoutes = _mapper.Map<Interfaces.Route[]>(providerOneSearchResponse.Routes);
                minExpirationTime = providerOneSearchResponse.Routes.Min(x => x.TimeLimit);
                result.Routes = result.Routes.Concat(resultOneRoutes).ToArray();
            }

            var providerTwoSearchRequest = _mapper.Map<ProviderTwoSearchRequest>(request);
            var providerTwoSearchResponse = await _clientTwo.SearchAsync(providerTwoSearchRequest, cancellationToken);

            if (providerTwoSearchResponse != null)
            {
                var resulTwoRoutes = _mapper.Map<Interfaces.Route[]>(providerTwoSearchResponse.Routes);
                result.Routes = result.Routes.Concat(resulTwoRoutes).ToArray();
                var minExpirationTimeFromProviderTwo = providerTwoSearchResponse.Routes.Min(x => x.TimeLimit);
                minExpirationTime = minExpirationTime > minExpirationTimeFromProviderTwo ? minExpirationTimeFromProviderTwo : minExpirationTime;
            }

            result.MaxPrice = result.Routes.Max(x => x.Price);
            result.MinPrice = result.Routes.Min(x => x.Price);
            result.MinMinutesRoute = result.Routes.Min(x => (int)(x.DestinationDateTime - x.OriginDateTime).TotalMinutes);
            result.MaxMinutesRoute = result.Routes.Max(x => (int)(x.DestinationDateTime - x.OriginDateTime).TotalMinutes);

            if(saveToCache)
            {
                _cache.Set(request, result, TimeSpan.FromTicks(minExpirationTime.Ticks));
            }

            return result;
        }
    }
}
