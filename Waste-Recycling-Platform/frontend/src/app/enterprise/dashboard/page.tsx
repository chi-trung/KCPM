"use client";
import React from "react";
import { EnterpriseDashboard } from "@/components/enterprise/EnterpriseDashboard";

export default function EnterprisePage() {
  return (
    <div className="min-h-screen bg-gray-50">
      <main className="w-full px-4 py-8 sm:px-6 lg:px-8">
        <EnterpriseDashboard />
      </main>
    </div>
  );
}
