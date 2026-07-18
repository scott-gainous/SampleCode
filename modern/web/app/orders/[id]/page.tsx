import { notFound } from "next/navigation";
import { api, formatCurrency } from "@/lib/api";

// Migrates: legacy OrderDetail.aspx (unit: orders).
// Status updates go through POST /api/orders/{id}/status; the admin-only rule is
// enforced server-side in the API (legacy enforced it only in the UI).
export default async function OrderDetailPage({
  params,
}: {
  params: Promise<{ id: string }>;
}) {
  const { id } = await params;
  const orderId = Number(id);
  if (!Number.isInteger(orderId)) notFound();

  const order = await api.order(orderId).catch(() => null);
  if (!order) notFound();

  return (
    <>
      <h2>Order #{order.orderId}</h2>
      <table style={{ maxWidth: 480, marginBottom: 20 }}>
        <tbody>
          <tr>
            <th>Customer</th>
            <td>{order.companyName}</td>
          </tr>
          <tr>
            <th>Order date</th>
            <td>{order.orderDate.replace("T", " ").slice(0, 16)}</td>
          </tr>
          <tr>
            <th>Ship to</th>
            <td>
              {order.shipCity ?? ""}, {order.shipCountry ?? ""}
            </td>
          </tr>
          <tr>
            <th>Status</th>
            <td>
              <span className={`status status-${order.status}`}>{order.status}</span>
            </td>
          </tr>
        </tbody>
      </table>

      <h3>Items</h3>
      <table>
        <thead>
          <tr>
            <th>SKU</th>
            <th>Product</th>
            <th>Qty</th>
            <th>Unit price</th>
            <th>Line total</th>
          </tr>
        </thead>
        <tbody>
          {order.items.map((i) => (
            <tr key={i.orderItemId}>
              <td>{i.sku}</td>
              <td>{i.name}</td>
              <td>{i.quantity}</td>
              <td>{formatCurrency(i.unitPrice)}</td>
              <td>{formatCurrency(i.lineTotal)}</td>
            </tr>
          ))}
        </tbody>
      </table>
      <p>
        <strong>Order total: {formatCurrency(order.orderTotal)}</strong>
      </p>
    </>
  );
}
