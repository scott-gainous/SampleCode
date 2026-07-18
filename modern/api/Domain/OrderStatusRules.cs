namespace ContosoOrders.Api.Domain;

/// <summary>
/// Order status state machine, ported from legacy OrderDal.UpdateStatus
/// (legacy/ContosoOrders.Web/Code/Dal/OrderDal.cs). The legacy app enforced this
/// implicitly in the DAL and silently ignored invalid transitions; the modern API keeps
/// the same transition graph but reports invalid transitions explicitly (HTTP 409).
/// Covered by unit tests and by the parity suite (unit: orders).
/// </summary>
public static class OrderStatusRules
{
    public const string Pending = "Pending";
    public const string Shipped = "Shipped";
    public const string Cancelled = "Cancelled";

    public static readonly IReadOnlyList<string> All = [Pending, Shipped, Cancelled];

    public static bool IsValidStatus(string status) => All.Contains(status);

    /// <summary>Pending → Shipped|Cancelled; Shipped and Cancelled are terminal.</summary>
    public static bool CanTransition(string from, string to) =>
        from == Pending && (to == Shipped || to == Cancelled);
}
