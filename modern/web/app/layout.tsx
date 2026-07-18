import type { Metadata } from "next";
import Link from "next/link";
import "./globals.css";

export const metadata: Metadata = {
  title: "Contoso Orders",
  description: "Modern replacement for the legacy Contoso Orders WebForms app",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>
        <header className="header">
          <h1>Contoso Orders</h1>
          <nav>
            <Link href="/">Dashboard</Link>
            <Link href="/customers">Customers</Link>
            <Link href="/orders">Orders</Link>
            <Link href="/products">Products</Link>
          </nav>
        </header>
        <main className="content">{children}</main>
      </body>
    </html>
  );
}
