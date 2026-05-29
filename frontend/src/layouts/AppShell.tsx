import { Outlet } from "react-router-dom";
import { Header } from "@/layouts/Header";
import { Sidebar } from "@/layouts/Sidebar";
import "@/layouts/AppShell.css";

export function AppShell() {
  return (
    <div className="app-shell">
      <Sidebar />
      <div className="app-shell__main">
        <Header />
        <main className="app-shell__content">
          <Outlet />
        </main>
      </div>
    </div>
  );
}
