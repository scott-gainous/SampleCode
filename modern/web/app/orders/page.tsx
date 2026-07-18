import { api, type OrderStatus } from "@/lib/api";
import { formatCurrency } from "@/lib/api";

const STATUSES: OrderStatus[] = ["Pending", "Shipped", "Cancelled"];

// Migrates: legacy Orders.aspx (unit: orders)
export default async function OrdersPage({
  searchParams,
}: {
  searchParams: Promise<{ customerId?: string; status?: string }>;
}) {
  const params = await searchParams;
  const customerId = params.customerId ? Number(params.customerId) : undefined;
  const status = STATUSES.includes(params.status as OrderStatus)
    ? (params.status as OrderStatus)
    : undefined;
  const orders = await api.orders({ customerId, status });

  return (
    <>
      <h2>Orders</h2>
      <p>
        Status:{" "}
        <a href="/orders">(all)</a>{" "}
        {STATUSES.map((s) => (
          <a key={s} href={`/orders?status=${s}`} style={{ marginRight: 10 }}>
            {s}
          </a>
        ))}
      </p>
      <table>
        <thead>
          <tr>
            <th>Order #</th>
            <th>Customer</th>
            <th>Date</th>
            <th>Status</th>
            <th>Total</th>
          </tr>
        </thead>
        <tbody>
          {orders.map((o) => (
            <tr key={o.orderId}>
              <td>
                <a href={`/orders/${o.orderId}`}>#{o.orderId}</a>
              </td>
              <td>{o.companyName}</td>
              <td>{o.orderDate.slice(0, 10)}</td>
              <td>
                <span className={`status status-${o.status}`}>{o.status}</span>
              </td>
              <td>{formatCurrency(o.orderTotal)}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </>
  );
}
