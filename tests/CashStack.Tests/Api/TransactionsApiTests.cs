using System.Net;
using CashStack.Api.Dtos;
using CashStack.Models;
using CashStack.Tests.Support;
using Microsoft.AspNetCore.Mvc;
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
    public async Task Post_PastDate_IsListedOnThatDay()
    {
        HttpResponseMessage response = await PostAsync(Url, new
        {
            type = "Expense", categoryId = TestData.Groceries, description = "September shopping", amount = 30m,
            date = "2026-09-15"
        });

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        TransactionResponse created = await ReadAsync<TransactionResponse>(response);
        Assert.Equal(new DateOnly(2026, 9, 15), DateOnly.FromDateTime(created.Timestamp.DateTime));

        List<TransactionResponse> september = await ReadAsync<List<TransactionResponse>>(
            await Client.GetAsync($"{Url}?from=2026-09-15&to=2026-09-15"));
        Assert.Equal(created.Id, Assert.Single(september).Id);
    }

    [Fact]
    public async Task Post_FutureDate_Returns400WithFieldErrorForDate()
    {
        HttpResponseMessage response = await PostAsync(Url, new
        {
            type = "Expense", categoryId = TestData.Groceries, description = "x", amount = 1m, date = "2099-01-01"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        ValidationProblemDetails problem = await ReadAsync<ValidationProblemDetails>(response);
        Assert.Contains("future", Assert.Single(problem.Errors["date"]));
    }

    [Fact]
    public async Task Post_InvalidDateFormat_Returns400()
    {
        HttpResponseMessage response = await PostAsync(Url, new
        {
            type = "Expense", categoryId = TestData.Groceries, description = "x", amount = 1m, date = "15.09.2026"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
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
