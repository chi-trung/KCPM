"use client";
import React from "react";
import { ReportForm } from "@/components/citizen/ReportForm";
import { useRouter } from "next/navigation";

export default function CitizenCreateReportPage() {
  const router = useRouter();

  const handleSuccess = () => {
    router.push("/citizen/reports");
  };

  return (
    <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
      <div className="max-w-3xl mx-auto">
        <h1 className="text-2xl font-bold text-gray-900 mb-6">Tạo Báo Cáo Rác Mới</h1>
        <div className="bg-white shadow-sm rounded-2xl border border-gray-100 p-6 md:p-8">
          <ReportForm onSubmit={handleSuccess} />
        </div>
      </div>
    </div>
  );
}
