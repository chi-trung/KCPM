"use client";
import React from "react";
import { ReportForm } from "./ReportForm";

// 1. Nhập các component dùng chung vừa tạo
import { NotificationCenter, UserProfileMenu } from "@/components/shared";

export const CitizenDashboard: React.FC = () => {
  const handleReportSubmit = () => {
    // Xử lý sau khi báo cáo thành công
    console.log("Báo cáo đã được gửi thành công");
  };

  return (
    // Sử dụng flex-col và gap để tạo khoảng cách giữa Header và khung nội dung
    <div className="flex flex-col gap-6">

      {/* Phần nội dung chính - chỉ hiển thị ReportForm */}
      <div className="bg-white shadow-sm rounded-2xl border border-gray-100 p-6 md:p-8">
        <ReportForm onSubmit={handleReportSubmit} />
      </div>
    </div>
  );
};