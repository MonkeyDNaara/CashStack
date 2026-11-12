namespace CashStack.Demo;

// Seeded: how many transactions were added. Skipped: how many already existed in the plan's range.
public sealed record DemoSeedResult(bool Seeded, int Count, IReadOnlyList<string> CreatedCategories);
