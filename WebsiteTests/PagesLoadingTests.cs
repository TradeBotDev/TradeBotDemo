using Microsoft.AspNetCore.Mvc.Testing;
using System;
using System.Threading.Tasks;
using Xunit;

namespace WebsiteTests
{
    public class PagesLoadingTests : IClassFixture<WebApplicationFactory<Website.Startup>>
    {
        private readonly WebApplicationFactory<Website.Startup> _factory;

        public PagesLoadingTests(WebApplicationFactory<Website.Startup> factory) => _factory = factory;

        [Theory]
        [InlineData("/")]
        [InlineData("/Account")]
        [InlineData("/Authorization/Login")]
        [InlineData("/Authorization/Register")]
        public async Task Get_EndpointsReturnSuccessAndCorrectContentType(string url)
        {
            var client = _factory.CreateClient();
            var response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            Assert.Equal("text/html; charset=utf-8", response.Content.Headers.ContentType.ToString());
        }
    }
}
