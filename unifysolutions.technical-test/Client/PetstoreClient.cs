using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Polly;
using Polly.Retry;
using unifysolutions.technical_test.Model;

namespace unifysolutions.technical_test.Client
{
    public class PetstoreClient : IDisposable
    {
        private readonly AsyncRetryPolicy _retryPolicy;
        private readonly HttpClient _client;
        private const string BaseUrl = "https://petstore.swagger.io";

        public PetstoreClient(HttpClient client)
        {
            _client = client;
            _client.BaseAddress = new Uri(BaseUrl);
            _retryPolicy = Policy
                .Handle<HttpRequestException>()
                .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
        }

        public async Task<IEnumerable<Pet>> GetAvailablePetsOrderByNameAsync(SortOrder order,
            CancellationToken cancellationToken = default)
        {
            var availablePets = await GetAvailablePetsAsync(cancellationToken);
            if (order == SortOrder.Descending)
            {
                return availablePets.OrderByDescending(pet => pet.Name);
            }
            else
            {
                return availablePets.OrderBy(pet => pet.Name);
            }
        }

        public async Task<IEnumerable<Pet>> GetAvailablePetsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                return await _retryPolicy.ExecuteAsync(async () => await GetPetsAsync("available", cancellationToken));
            } catch (HttpRequestException requestException)
            {
                Console.WriteLine("Could reach pet store /pet/findByStatus endpoint");
                throw;
            }
            
        }

        private async Task<IEnumerable<Pet>> GetPetsAsync(string status, CancellationToken cancellationToken = default)
        {
            var response = await _client.GetAsync($"/v2/pet/findByStatus?status={status}", cancellationToken);
            response.EnsureSuccessStatusCode();
            using (var responseStream = await response.Content.ReadAsStreamAsync(cancellationToken))
            {
                return await JsonSerializer.DeserializeAsync<IEnumerable<Pet>>(responseStream, cancellationToken: cancellationToken);
            }

        }

        public void Dispose()
        {
            _client.Dispose();
        }
    }
}
