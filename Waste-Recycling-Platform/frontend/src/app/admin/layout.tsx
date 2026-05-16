"use client";
import React from "react";
import { RouteGuard } from "@/components/auth/RouteGuard";

// Admin không cần Navbar - có sidebar riêng trong AdminDashboard component
export default function AdminLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <RouteGuard requiredRole="admin">
      {children}
    </RouteGuard>
  );
}
