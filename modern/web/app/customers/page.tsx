import { api } from "@/lib/api";

// Migrates: legacy Customers.aspx (unit: customers).
// The edit form (CustomerEdit.aspx) migrates in the same unit — see migration/ledger.json.
export default async function CustomersPage({
  searchParams,
}: {
  searchParams: Promise<{ includeInactive?: string }>;
}) {
  const params = await searchParams;
  const includeInactive = params.includeInactive === "true";
  const customers = await api.customers(includeInactive);

  return (
    <>
      <h2>Customers</h2>
      <p>
        <a href={`/customers?includeInactive=${!includeInactive}`}>
          {includeInactive ? "Hide inactive" : "Include inactive"}
        </a>
      </p>
      <table>
        <thead>
          <tr>
            <th>Company</th>
            <th>Contact</th>
            <th>Email</th>
            <th>City</th>
            <th>Country</th>
            <th>Active</th>
            <th>Orders</th>
          </tr>
        </thead>
        <tbody>
          {customers.map((c) => (
            <tr key={c.customerId}>
              <td>{c.companyName}</td>
              <td>{c.contactName}</td>
              <td>{c.email}</td>
              <td>{c.city}</td>
              <td>{c.country}</td>
              <td>{c.isActive ? "Yes" : "No"}</td>
              <td>
                <a href={`/orders?customerId=${c.customerId}`}>{c.orderCount}</a>
              </td>
            </tr>
          ))}
        </tbody>
      </table>
    </>
  );
}
