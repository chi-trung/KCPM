"use client";
import React from "react";
import { EnterpriseDashboard } from "@/components/enterprise/EnterpriseDashboard";

export default function EnterpriseReportsPage() {
  return (
    <div className="min-h-screen bg-gray-50">
      <main className="mx-auto w-full max-w-7xl px-4 py-8 sm:px-6 lg:px-8">
        <EnterpriseDashboard initialTab="analytics" />
      </main>
    </div>
  );
}
