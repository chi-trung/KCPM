"use client";
import React from "react";
import { RouteGuard } from "@/components/auth/RouteGuard";

export default function EnterpriseLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <RouteGuard requiredRole="enterprise">
      <div className="min-h-screen">{children}</div>
    </RouteGuard>
  );
}
