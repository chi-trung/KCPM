"use client";
import React, { useEffect } from "react";
import { useRouter } from "next/navigation";

export default function EnterprisePage() {
  const router = useRouter();

  useEffect(() => {
    // Chuyển hướng đến dashboard chính
    router.replace("/enterprise/dashboard");
  }, [router]);

  return (
    <div className="min-h-screen flex items-center justify-center">
      <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-emerald-600"></div>
    </div>
  );
}
