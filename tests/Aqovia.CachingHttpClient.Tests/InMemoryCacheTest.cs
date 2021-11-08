using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;
using Aqovia.CachingHttpClient.Tests.Abstraction;
using Aqovia.CachingHttpClient.AspNetCoreApi;
using CacheCow.Client.Headers;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;
using Xunit;

namespace Aqovia.CachingHttpClient.Tests
{
    public class InMemoryCacheTest
    {
        private readonly System.Net.Http.HttpClient _cachedHttpClient = CachingHelpers.GetClient();

        [Fact]
        public async Task ShouldLoadFromCacheOnSecondRequest()
        {
            var server = new WebHostBuilder()
                .UseStartup<Startup>()
                .UseKestrel(options => { options.Listen(IPAddress.Loopback, 5002); })
                .Build();
            await server.StartAsync();

            _cachedHttpClient.BaseAddress = new Uri("http://localhost:5002");
            var response = await _cachedHttpClient.GetAsync("profile");
            var responseText = await response.Content.ReadAsStringAsync();
            var profiles = JsonConvert.DeserializeObject<IList<Profile>>(responseText);
            Assert.NotNull(profiles);
            Assert.NotEmpty(profiles);

            var secondResponse = await _cachedHttpClient.GetAsync("profile");
            var isCachedResponse = secondResponse.Headers.GetCacheCowHeader().RetrievedFromCache;
            var secondResponseText = await secondResponse.Content.ReadAsStringAsync();
            var secondProfiles = JsonConvert.DeserializeObject<IList<Profile>>(secondResponseText);
            Assert.True(isCachedResponse);
            Assert.NotNull(secondProfiles);
            Assert.NotEmpty(secondProfiles);

            await server.StopAsync();
        }
    }
}
