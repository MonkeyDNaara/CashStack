using System.Net;
using System.Net.Http.Json;
using CashStack.Api.Dtos;
using CashStack.Models;
using CashStack.Tests.Support;
using Xunit;

namespace CashStack.Tests.Api;

public class CategoriesApiTests : ApiTestBase
{
    private const string Url = "/api/v1/categories";

    [Fact]
    public async Task GetAll_ReturnsSeededCategories_WithProtectedFlag()
    {
        List<CategoryResponse> all = await ReadAsync<List<CategoryResponse>>(await Client.GetAsync(Url));

        Assert.Equal(9, all.Count);
        Assert.Equal(2, all.Count(c => c.IsProtected));
    }

    [Fact]
    public async Task Post_DuplicateName_Returns409WithOurMessage()
    {
        HttpResponseMessage response = await PostAsync(Url, new { name = " rent ", type = "Expense" });

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        Assert.Equal("An Expense category named 'rent' already exists.", (await ReadProblemAsync(response)).Detail);
    }

    [Fact]
    public async Task CreateRenameDelete_Returns201_200_204_404()
    {
        HttpResponseMessage created = await PostAsync(Url, new { name = "Subscriptions", type = "Expense" });
        Assert.Equal(HttpStatusCode.Created, created.StatusCode);
        Guid id = (await ReadAsync<CategoryResponse>(created)).Id;

        HttpResponseMessage renamed = await Client.PutAsJsonAsync($"{Url}/{id}", new { name = "Streaming" }, Json);
        Assert.Equal(HttpStatusCode.OK, renamed.StatusCode);
        Assert.Equal("Streaming", (await ReadAsync<CategoryResponse>(renamed)).Name);

        Assert.Equal(HttpStatusCode.NoContent, (await Client.DeleteAsync($"{Url}/{id}")).StatusCode);
        Assert.Equal(HttpStatusCode.NotFound, (await Client.DeleteAsync($"{Url}/{id}")).StatusCode);
    }

    [Fact]
    public async Task Delete_ProtectedOther_Returns409()
    {
        HttpResponseMessage response = await Client.DeleteAsync($"{Url}/{DefaultCategories.OtherExpenseId}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Delete_CategoryInUse_Returns409()
    {
        await PostAsync("/api/v1/transactions",
            new { type = "Expense", categoryId = TestData.Rent, description = "Rent", amount = 800m });

        HttpResponseMessage response = await Client.DeleteAsync($"{Url}/{TestData.Rent}");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
