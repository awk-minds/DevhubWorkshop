using System.Text.Json;
using Microsoft.Data.SqlClient;
using ShoppingCart.Models;
using Testcontainers.MsSql;
using Xunit;

namespace ShoppingCart.Tests.IntegrationTests;

public class RepositoryTest : IntegrationFixture
{
    [Fact]
    public async Task TestRepository()
    {
        var response = await Client.GetAsync("api/v1/Cart/products");
        response.EnsureSuccessStatusCode();

        var json = await response.Content.ReadAsStringAsync();

        var items = JsonSerializer.Deserialize<List<ShoppingItem>>(json, new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        });
        Assert.Equal(10, items?.Count);
    }
}