"use client";
import React from "react";
import { Navbar } from "@/components/layout/Navbar";

export default function SettingsLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="min-h-screen">
      <Navbar />
      {children}
    </div>
  );
}
