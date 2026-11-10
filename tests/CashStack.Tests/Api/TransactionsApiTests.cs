using System.Net;
using CashStack.Api.Dtos;
using CashStack.Models;
using CashStack.Tests.Support;
using Xunit;

namespace CashStack.Tests.Api;

public class TransactionsApiTests : ApiTestBase
{
    private const string Url = "/api/v1/transactions";

    [Fact]
    public async Task Post_Valid_Returns201WithLocationAndCategoryName()
    {
        HttpResponseMessage response = await PostAsync(Url,
            new { type = "Expense", categoryId = TestData.Groceries, description = "Weekly shopping", amount = 42.50m });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        TransactionResponse created = await ReadAsync<TransactionResponse>(response);
        Assert.Equal("Groceries", created.CategoryName);
        Assert.EndsWith($"{Url}/{created.Id}", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task Post_MissingCategoryId_Returns400()
    {
        HttpResponseMessage response = await PostAsync(Url, new { type = "Expense", description = "x", amount = 1m });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Post_CategoryOfTheOtherType_Returns400WithOurMessage()
    {
        HttpResponseMessage response = await PostAsync(Url,
            new { type = "Expense", categoryId = TestData.Salary, description = "x", amount = 1m });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Contains("Salary", (await ReadProblemAsync(response)).Detail);
    }

    [Fact]
    public async Task GetById_Unknown_Returns404()
    {
        HttpResponseMessage response = await Client.GetAsync($"{Url}/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Summary_ContainsTotalsPerCategory()
    {
        await PostAsync(Url, new { type = "Income", categoryId = TestData.Salary, description = "Salary", amount = 2500m });
        await PostAsync(Url, new { type = "Expense", categoryId = TestData.Rent, description = "Rent", amount = 800m });

        SummaryResponse summary = await ReadAsync<SummaryResponse>(await Client.GetAsync($"{Url}/summary"));

        Assert.Equal(1700m, summary.Balance);
        Assert.Equal(["Salary", "Rent"], summary.Categories.Select(c => c.Name));
    }

    [Fact]
    public async Task EveryAddedTransaction_IsWrittenToTheAuditLog()
    {
        await PostAsync(Url, new { type = "Expense", categoryId = TestData.Groceries, description = "first", amount = 1m });
        await PostAsync(Url, new { type = "Expense", categoryId = TestData.Groceries, description = "second", amount = 2m });

        string[] lines = await File.ReadAllLinesAsync(Factory.AuditLogPath);

        Assert.Equal(2, lines.Length);
    }
}
