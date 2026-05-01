import "../styles/globals.css";
import type { Metadata } from "next";
import { AuthProvider } from "@/context/AuthContext";
import { WishlistProvider } from "@/context/WishlistContext";
import Header from "@/components/common/Header";
import Footer from "@/components/common/Footer";

export const metadata: Metadata = {
  title: "Eyewear Showcase",
  description: "Browse eyewear products with customizations and images.",
};

export default function RootLayout({ children }: { children: React.ReactNode }) {
  return (
    <html lang="en">
      <body>
        <AuthProvider>
          <WishlistProvider>
            <div className="siteShell">
              <Header />
              <div className="siteContent">{children}</div>
              <Footer />
            </div>
          </WishlistProvider>
        </AuthProvider>
      </body>
    </html>
  );
}
