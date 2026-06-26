import { Header } from "./layout/Header";
import { Footer } from "./layout/Footer";
import { Outlet } from "react-router-dom";
import { Toaster } from "sonner";

export const Layout = () => {
  return (
    <div className="flex h-screen flex-col">
      <Header />
      <main className="flex-1 overflow-hidden">
        <Toaster position="top-center" theme="system" />
        <Outlet />
      </main>
      <Footer />
    </div>
  );
};
