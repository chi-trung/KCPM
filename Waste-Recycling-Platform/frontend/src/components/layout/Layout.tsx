import React from "react";
import { Navbar } from "./Navbar";
import { Sidebar } from "./Sidebar";
import { Footer } from "./Footer";

interface LayoutProps {
  children: React.ReactNode;
  navbar?: React.ComponentProps<typeof Navbar>;
  sidebar?: React.ComponentProps<typeof Sidebar> | null;
  footer?: React.ComponentProps<typeof Footer> | null;
  showSidebar?: boolean;
}

export const Layout: React.FC<LayoutProps> = ({
  children,
  navbar = {
    transparent: false,
  },
  sidebar,
  footer = {
    companyName: "Waste Recycling Platform",
  },
  showSidebar = false,
}) => {
  const [sidebarOpen, setSidebarOpen] = React.useState(showSidebar);

  return (
    <div className="flex flex-col min-h-screen bg-white">
      {/* Navbar */}
      <Navbar {...navbar} />

      {/* Main Content */}
      <div className="flex flex-1 overflow-hidden">
        {/* Sidebar */}
        {sidebar && (
          <Sidebar
            {...sidebar}
            isOpen={sidebarOpen}
            onClose={() => setSidebarOpen(false)}
          />
        )}

        {/* Content Area */}
        <main className="flex-1 overflow-y-auto">
          <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
            {children}
          </div>
        </main>
      </div>

      {/* Footer */}
      {footer && <Footer {...footer} />}
    </div>
  );
};
