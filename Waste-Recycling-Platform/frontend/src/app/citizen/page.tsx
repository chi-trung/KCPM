"use client";
import React, { useEffect } from "react";
import { useRouter } from "next/navigation";

export default function CitizenPage() {
  const router = useRouter();

  useEffect(() => {
    // Chuyển hướng đến dashboard chính
    router.replace("/citizen/dashboard");
  }, [router]);

  return (
    <div className="min-h-screen flex items-center justify-center">
      <div className="text-center">
        <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-emerald-600 mx-auto mb-4"></div>
        <p className="text-gray-600">Đang chuyển hướng...</p>
      </div>
    </div>
  );
}
