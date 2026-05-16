"use client";
import React from "react";
import { Navbar } from "@/components/layout/Navbar";
import { RouteGuard } from "@/components/auth/RouteGuard";
import { NotificationToast } from "@/components/NotificationToast";

export default function CitizenLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <RouteGuard requiredRole="citizen">
      <div className="min-h-screen">
        <Navbar />
        <NotificationToast />
        {children}
      </div>
    </RouteGuard>
  );
}
