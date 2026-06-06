"use client";
import React, { useState } from "react";
import Link from "next/link";
import Image from "next/image";
import { usePathname } from "next/navigation";
import { useAuth } from "@/contexts/AuthContext";
import { Menu, X, LogIn, UserPlus } from "lucide-react";
import { NotificationCenter, UserProfileMenu } from "@/components/shared";

interface NavbarProps {
  transparent?: boolean;
}

export const Navbar: React.FC<NavbarProps> = ({
  transparent = false,
}) => {
  const [isMobileMenuOpen, setIsMobileMenuOpen] = useState(false);
  const pathname = usePathname();
  const { isAuthenticated, user } = useAuth();

  // Luôn về trang chủ public khi click logo/CWCRP
  const getLogoHref = () => {
    return "/";
  };

  // Lấy trang chủ theo vai trò
  const getHomeHref = () => {
    if (!isAuthenticated) return "/";
    
    switch (user?.role?.toLowerCase()) {
      case 'citizen': return '/citizen/dashboard';
      case 'collector': return '/collector/dashboard';
      case 'enterprise': return '/enterprise/dashboard';
      case 'admin': return '/admin/dashboard';
      default: return '/';
    }
  };

  // Menu items theo vai trò
  const getMenuItems = () => {
    const homeHref = getHomeHref();
    
    // Guest Menu - Tập trung vào giới thiệu và điều hướng
    const guestMenu = [
      { label: "Trang Chủ", href: "/" },
      { label: "Hướng Dẫn", href: "/guide" },
      { label: "Tra Cứu Điểm", href: "/locations" },
      { label: "Bảng Xếp Hạng", href: "/leaderboard" },
    ];

    const citizenMenu = [
      { label: "Bảng Điều Khiển", href: "/citizen/dashboard" },
      { label: "Tạo Báo Cáo", href: "/citizen/create-report" },
      { label: "Quản Lý Báo Cáo", href: "/citizen/reports" },
      { label: "Điểm Thưởng", href: "/citizen/rewards" },
    ];

    const collectorMenu = [
      { label: "Bảng Điều Khiển", href: "/collector/dashboard" },
      { label: "Tuyến Đường", href: "/collector/routes" },
    ];

    const enterpriseMenu = [
      { label: "Bảng Điều Khiển", href: "/enterprise/dashboard" },
      { label: "Quản Lý Điểm", href: "/enterprise/points" },
      { label: "Báo Cáo", href: "/enterprise/reports" },
    ];

    const adminMenu = [
      { label: "Bảng Điều Khiển", href: "/admin/dashboard" },
      { label: "Quản Lý Người Dùng", href: "/admin/users" },
      { label: "Quản Lý Hệ Thống", href: "/admin/system" },
    ];

    if (!isAuthenticated) return guestMenu;
    
    switch (user?.role?.toLowerCase()) {
      case 'citizen': return citizenMenu;
      case 'collector': return collectorMenu;
      case 'enterprise': return enterpriseMenu;
      case 'admin': return adminMenu;
      default: return guestMenu;
    }
  };

  const menuItems = getMenuItems();
  const homeHref = getHomeHref();

  // Danh sách các trang cho phép hiển thị Navbar này
  const publicPaths = ["/", "/about", "/features", "/contact", "/guide", "/locations", "/leaderboard", "/settings", "/login", "/register"];
  const protectedPaths = ["/dashboard", "/reports", "/rewards", "/citizen", "/collector", "/enterprise", "/admin"];
  const authPaths = ["/reset-password"];
  
  // Kiểm tra xem pathname hiện tại có nằm trong danh sách không
  const shouldShowNavbar = publicPaths.includes(pathname) || 
                           pathname?.startsWith("/reset-password") ||
                           protectedPaths.some(path => pathname?.startsWith(path));

  // Ẩn navbar trên các trang không nằm trong danh sách
  if (!shouldShowNavbar) {
    return null; 
  }

  const isActive = (href: string) => {
    if (href === "/") return pathname === "/";
    return pathname?.startsWith(href);
  };

  return (
    <nav
      className="z-50 bg-white/80 backdrop-blur-md border-b border-gray-100 sticky top-0"
    >
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
        <div className="flex items-center justify-between h-16 sm:h-20">
          {/* Logo */}
          <Link
            href={getLogoHref()}
            className="flex items-center gap-3 group flex-shrink-0"
          >
            <Image
              src="/logo/logo.jpg"
              alt="CWCRP Logo"
              width={100}
              height={100}
              className="rounded-lg"
            />
            <span
              className={`font-bold text-lg hidden sm:inline transition-colors ${
                transparent
                  ? "text-white group-hover:text-[#0AA468]"
                  : "text-gray-900 group-hover:text-[#0AA468]"
              }`}
            >
              CWCRP
            </span>
          </Link>

          {/* Desktop Nav Links */}
          <div className="hidden md:flex items-center gap-8">
            {menuItems.map((item) => (
              <Link
                key={item.label}
                href={item.href}
                className={`text-sm font-semibold tracking-wide transition-colors ${
                  isActive(item.href)
                    ? "text-[#0AA468]"
                    : transparent
                    ? "text-white/80 hover:text-[#0AA468]"
                    : "text-gray-600 hover:text-[#0AA468]"
                }`}
              >
                {item.label}
              </Link>
            ))}
          </div>

          {/* Auth/Profile Section - Desktop */}
          <div className="hidden md:flex items-center gap-4">
            {isAuthenticated ? (
              <div className="flex items-center gap-2 pl-4 border-l border-gray-200">
                <NotificationCenter />
                <UserProfileMenu />
              </div>
            ) : (
              <>
                <Link
                  href="/login"
                  className={`flex items-center gap-2 px-4 py-2 rounded-lg font-semibold text-sm transition-colors ${
                    pathname === "/login"
                      ? "text-[#0AA468]"
                      : transparent
                      ? "text-white/80 hover:text-white"
                      : "text-gray-600 hover:text-[#0AA468]"
                  }`}
                >
                  <LogIn size={16} />
                  <span>Đăng Nhập</span>
                </Link>
                <Link
                  href="/register"
                  className={`px-4 py-2 rounded-lg font-semibold text-sm transition-all ${
                    pathname === "/register"
                      ? "bg-[#0AA468] text-white"
                      : transparent
                      ? "bg-white/20 text-white hover:bg-white/30"
                      : "bg-[#0AA468] text-white hover:bg-[#088F5A]"
                  }`}
                >
                  <span className="flex items-center gap-2">
                    <UserPlus size={16} />
                    <span>Đăng Ký</span>
                  </span>
                </Link>
              </>
            )}
          </div>

          {/* Mobile Menu Toggle & Icons */}
          <div className="md:hidden flex items-center gap-2">
            {isAuthenticated ? (
              <div className="flex items-center gap-1">
                <NotificationCenter />
                <UserProfileMenu />
              </div>
            ) : (
              <button
                onClick={() => setIsMobileMenuOpen(!isMobileMenuOpen)}
                className={`p-2 rounded-lg transition-colors ${
                  transparent
                    ? "text-white hover:bg-white/10"
                    : "text-gray-600 hover:bg-gray-100"
                }`}
                aria-label="Toggle menu"
              >
                {isMobileMenuOpen ? <X size={24} /> : <Menu size={24} />}
              </button>
            )}
          </div>
        </div>

        {/* Mobile Menu Dropdown */}
        {isMobileMenuOpen && (
          <div className={`md:hidden py-4 border-t ${transparent ? "border-white/10" : "border-gray-100"}`}>
            {menuItems.map((item) => (
              <Link
                key={item.label}
                href={item.href}
                className={`block px-4 py-2 text-sm font-semibold transition-colors ${
                  isActive(item.href)
                    ? "text-[#0AA468]"
                    : transparent
                    ? "text-white/80 hover:text-[#0AA468]"
                    : "text-gray-600 hover:text-[#0AA468]"
                }`}
                onClick={() => setIsMobileMenuOpen(false)}
              >
                {item.label}
              </Link>
            ))}
            
            {!isAuthenticated && (
              <div className="mt-2 border-t border-gray-100 pt-2">
                <Link
                  href="/login"
                  className={`flex items-center gap-2 px-4 py-2 rounded-lg font-semibold text-sm transition-colors ${
                    pathname === "/login"
                      ? "text-[#0AA468]"
                      : transparent
                      ? "text-white/80 hover:text-white"
                      : "text-gray-600 hover:text-[#0AA468]"
                  }`}
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  <LogIn size={16} />
                  <span>Đăng Nhập</span>
                </Link>
                <Link
                  href="/register"
                  className={`flex items-center gap-2 px-4 py-2 mt-2 rounded-lg font-semibold text-sm transition-all ${
                    pathname === "/register"
                      ? "bg-[#0AA468] text-white"
                      : transparent
                      ? "bg-white/20 text-white hover:bg-white/30"
                      : "bg-[#0AA468] text-white hover:bg-[#088F5A]"
                  }`}
                  onClick={() => setIsMobileMenuOpen(false)}
                >
                  <UserPlus size={16} />
                  <span>Đăng Ký</span>
                </Link>
              </div>
            )}
          </div>
        )}
      </div>
    </nav>
  );
};