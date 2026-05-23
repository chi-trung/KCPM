"use client";

import React, { useEffect } from "react";
import { useRouter } from "next/navigation";
import { useAuth } from "@/contexts/AuthContext";
import type { UserRole } from "@/lib/api/auth";

interface RouteGuardProps {
  children: React.ReactNode;
  /** Role yêu cầu để truy cập trang này. Nếu không truyền thì chỉ cần đăng nhập. */
  requiredRole?: UserRole;
}

export function RouteGuard({ children, requiredRole }: RouteGuardProps) {
  const { user, isLoading, isAuthenticated } = useAuth();
  const router = useRouter();

  useEffect(() => {
    // Chờ AuthContext khởi tạo xong trước khi kiểm tra
    if (isLoading) return;

    // Chưa đăng nhập → về trang login
    if (!isAuthenticated) {
      router.replace("/login");
      return;
    }

    // Đã đăng nhập nhưng sai role → về trang chủ (không để lộ điểm cuối)
    if (requiredRole && user?.role?.toLowerCase() !== requiredRole.toLowerCase()) {
      router.replace("/");
      return;
    }
  }, [isLoading, isAuthenticated, user, requiredRole, router]);

  // Đang tải → hiển thị màn hình chờ trung lập
  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50">
        <div className="flex flex-col items-center gap-4">
          <div className="animate-spin rounded-full h-10 w-10 border-b-2 border-emerald-600" />
          <p className="text-sm text-gray-500">Đang xác thực...</p>
        </div>
      </div>
    );
  }

  // Chưa xác thực → không render gì (router.replace đang chạy)
  if (!isAuthenticated) return null;

  // Sai role → không render gì (router.replace đang chạy)
  if (requiredRole && user?.role?.toLowerCase() !== requiredRole.toLowerCase()) {
    return null;
  }

  return <>{children}</>;
}
