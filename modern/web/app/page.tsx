import { api, formatCurrency } from "@/lib/api";

// Migrates: legacy Default.aspx (unit: dashboard)
export default async function DashboardPage() {
  const summary = await api.dashboard();
  return (
    <>
      <h2>Dashboard</h2>
      <div className="tiles">
        <div className="tile">
          <span className="big">{summary.customerCount}</span>
          Active customers
        </div>
        <div className="tile">
          <span className="big">{summary.productCount}</span>
          Products
        </div>
        <div className="tile">
          <span className="big">{summary.openOrderCount}</span>
          Open orders
        </div>
        <div className="tile">
          <span className="big">{formatCurrency(summary.revenueToDate)}</span>
          Revenue to date
        </div>
      </div>
    </>
  );
}
