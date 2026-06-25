import { Header } from "./layout/Header";
import { Footer } from "./layout/Footer";
import { Outlet } from "react-router-dom";

export const Layout = () => {
  return (
    <div className="flex h-screen flex-col">
      <Header />
      <main className="flex-1 overflow-hidden">
        <Outlet />
      </main>
      <Footer />
    </div>
  );
};
