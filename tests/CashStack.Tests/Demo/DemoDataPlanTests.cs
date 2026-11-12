using CashStack.Demo;
using CashStack.Models;
using Xunit;

namespace CashStack.Tests.Demo;

public class DemoDataPlanTests
{
    [Fact]
    public void EndOfMonth_ContainsThreeFullMonths()
    {
        // 14 bookings per month (13 recurring + one extra per month)
        IReadOnlyList<PlannedTransaction> plan = DemoDataPlan.Build(new DateOnly(2026, 12, 31));

        Assert.Equal(42, plan.Count);
        Assert.Equal(new DateOnly(2026, 10, 1), plan[0].Date);
        Assert.Equal(["October", "November", "December"],
            plan.Where(p => p.Category == "Salary").Select(p => p.Description.Replace("Salary ", "")));
    }

    [Fact]
    public void NeverContainsFutureDates()
    {
        var today = new DateOnly(2026, 11, 12);

        IReadOnlyList<PlannedTransaction> plan = DemoDataPlan.Build(today);

        Assert.All(plan, p => Assert.True(p.Date <= today));
        Assert.DoesNotContain(plan, p => p.Description == "Streaming" && p.Date.Month == 11 && p.Date.Day > 12);
        Assert.Contains(plan, p => p.Date == today && p.Category == "Subscriptions");
    }

    [Fact]
    public void SkipsDaysAMonthDoesNotHave()
    {
        // Two months before April 2026 is February (28 days): the groceries on the 30th are skipped
        IReadOnlyList<PlannedTransaction> plan = DemoDataPlan.Build(new DateOnly(2026, 4, 30));

        Assert.DoesNotContain(plan, p => p.Date.Month == 2 && p.Date.Day > 28);
        Assert.Equal(4, plan.Count(p => p.Date.Month == 2 && p.Category == "Groceries"));
    }

    [Fact]
    public void IsSortedByDate_AndUsesTheRightTypes()
    {
        IReadOnlyList<PlannedTransaction> plan = DemoDataPlan.Build(new DateOnly(2026, 12, 31));

        Assert.Equal(plan.OrderBy(p => p.Date).Select(p => p.Date), plan.Select(p => p.Date));
        Assert.All(plan.Where(p => p.Category is "Salary" or "Freelance"), p => Assert.Equal(TransactionType.Income, p.Type));
        Assert.Equal(3 * 2500m + 450m, plan.Where(p => p.Type == TransactionType.Income).Sum(p => p.Amount));
    }
}
