using ContosoOrders.Api.Domain;
using Xunit;

namespace ContosoOrders.Api.Tests;

/// <summary>
/// Locks in the status state machine ported from legacy OrderDal.UpdateStatus.
/// If these fail, behavior has drifted from the legacy app — check the parity suite too.
/// </summary>
public class OrderStatusRulesTests
{
    [Theory]
    [InlineData("Pending", "Shipped", true)]
    [InlineData("Pending", "Cancelled", true)]
    [InlineData("Shipped", "Cancelled", false)]
    [InlineData("Shipped", "Pending", false)]
    [InlineData("Cancelled", "Shipped", false)]
    [InlineData("Cancelled", "Pending", false)]
    [InlineData("Pending", "Pending", false)]
    public void CanTransition_matches_legacy_state_machine(string from, string to, bool expected)
    {
        Assert.Equal(expected, OrderStatusRules.CanTransition(from, to));
    }

    [Theory]
    [InlineData("Pending", true)]
    [InlineData("Shipped", true)]
    [InlineData("Cancelled", true)]
    [InlineData("pending", false)]  // legacy comparisons are case-sensitive
    [InlineData("Open", false)]
    [InlineData("", false)]
    public void IsValidStatus_accepts_only_legacy_status_strings(string status, bool expected)
    {
        Assert.Equal(expected, OrderStatusRules.IsValidStatus(status));
    }
}
