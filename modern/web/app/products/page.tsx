import { api, formatCurrency } from "@/lib/api";

// Migrates: legacy Products.aspx (unit: products)
export default async function ProductsPage({
  searchParams,
}: {
  searchParams: Promise<{ includeDiscontinued?: string }>;
}) {
  const params = await searchParams;
  const includeDiscontinued = params.includeDiscontinued === "true";
  const products = await api.products(includeDiscontinued);

  return (
    <>
      <h2>Products</h2>
      <p>
        <a href={`/products?includeDiscontinued=${!includeDiscontinued}`}>
          {includeDiscontinued ? "Hide discontinued" : "Include discontinued"}
        </a>
      </p>
      <table>
        <thead>
          <tr>
            <th>SKU</th>
            <th>Name</th>
            <th>Unit price</th>
            <th>In stock</th>
            <th>Discontinued</th>
          </tr>
        </thead>
        <tbody>
          {products.map((p) => (
            <tr key={p.productId}>
              <td>{p.sku}</td>
              <td>{p.name}</td>
              <td>{formatCurrency(p.unitPrice)}</td>
              <td>{p.unitsInStock}</td>
              <td>{p.discontinued ? "Yes" : "No"}</td>
            </tr>
          ))}
        </tbody>
      </table>
    </>
  );
}
