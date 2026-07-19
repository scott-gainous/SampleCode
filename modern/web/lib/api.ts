// Typed client for ContosoOrders.Api. Server components call these functions;
// keep the types in sync with modern/api/Domain/Models.cs.

const API_BASE_URL = process.env.API_BASE_URL ?? "http://localhost:5080";

export interface DashboardSummary {
  customerCount: number;
  productCount: number;
  openOrderCount: number;
  revenueToDate: number;
}

export interface CustomerSummary {
  customerId: number;
  companyName: string;
  contactName: string;
  email: string;
  phone: string | null;
  city: string | null;
  country: string | null;
  isActive: boolean;
  createdUtc: string;
  orderCount: number;
}

export interface Product {
  productId: number;
  sku: string;
  name: string;
  unitPrice: number;
  unitsInStock: number;
  discontinued: boolean;
}

export interface OrderSummary {
  orderId: number;
  customerId: number;
  companyName: string;
  orderDate: string;
  status: OrderStatus;
  shipCity: string | null;
  shipCountry: string | null;
  orderTotal: number;
}

export interface OrderItem {
  orderItemId: number;
  productId: number;
  sku: string;
  name: string;
  quantity: number;
  unitPrice: number;
  lineTotal: number;
}

export interface OrderDetail extends Omit<OrderSummary, "orderTotal"> {
  orderTotal: number;
  items: OrderItem[];
}

export type OrderStatus = "Pending" | "Shipped" | "Cancelled";

async function get<T>(path: string): Promise<T> {
  const res = await fetch(`${API_BASE_URL}${path}`, { cache: "no-store" });
  if (!res.ok) {
    throw new Error(`API ${path} failed: ${res.status} ${res.statusText}`);
  }
  return res.json() as Promise<T>;
}

export const api = {
  dashboard: () => get<DashboardSummary>("/api/dashboard"),
  customers: (includeInactive = false) =>
    get<CustomerSummary[]>(`/api/customers?includeInactive=${includeInactive}`),
  products: (includeDiscontinued = false) =>
    get<Product[]>(`/api/products?includeDiscontinued=${includeDiscontinued}`),
  orders: (filters: { customerId?: number; status?: OrderStatus } = {}) => {
    const params = new URLSearchParams();
    if (filters.customerId) params.set("customerId", String(filters.customerId));
    if (filters.status) params.set("status", filters.status);
    const qs = params.toString();
    return get<OrderSummary[]>(`/api/orders${qs ? `?${qs}` : ""}`);
  },
  order: (id: number) => get<OrderDetail>(`/api/orders/${id}`),
};

export function formatCurrency(value: number): string {
  // Matches the legacy app's server-side {0:C} formatting (en-US in production).
  return new Intl.NumberFormat("en-US", { style: "currency", currency: "USD" }).format(value);
}
