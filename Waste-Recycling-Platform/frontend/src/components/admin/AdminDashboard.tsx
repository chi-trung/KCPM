"use client";
import React, { useState } from "react";
import Image from "next/image";
import {
  Users,
  FileText,
  Truck,
  AlertCircle,
  LogOut,
  Menu,
  X,
  BarChart3,
  Building2,
  ChevronRight
} from "lucide-react";
import { useAuth } from "@/contexts/AuthContext";
import { useRouter } from "next/navigation";
import { WasteAnalytics } from "./WasteAnalytics";
import { UserManagement } from "./UserManagement";
import { ReportsManagement } from "./ReportsManagement";
import { CollectionTasks } from "./CollectionTasks";
import { DisputesManagement } from "./DisputesManagement";
import { EnterpriseManagement } from "./EnterpriseManagement";
import { useSignalR } from "@/hooks/useSignalR";

type Tab = "analytics" | "users" | "reports" | "tasks" | "disputes" | "enterprises";

export const AdminDashboard: React.FC = () => {
  const { user, logout } = useAuth();
  const router = useRouter();
  const [activeTab, setActiveTab] = useState<Tab>("analytics");
  const [sidebarOpen, setSidebarOpen] = useState(true);
  const [taskStatusUpdates, setTaskStatusUpdates] = useState<Record<string, string>>({});
  const token = typeof window !== 'undefined' ? localStorage.getItem('token') : null;

  useSignalR({
    enabled: true,
    token,
    onTaskStatusUpdated: (taskId, status) => {
      setTaskStatusUpdates((prev) => ({
        ...prev,
        [taskId]: status,
      }));
    },
    onError: (error) => {
      console.error("[AdminDashboard] SignalR Error:", error);
    },
  });

  const tabs = [
    { id: "analytics" as Tab, label: "Thống Kê Rác", icon: BarChart3 },
    { id: "reports" as Tab, label: "Quản Lý Báo Cáo", icon: FileText },
    { id: "disputes" as Tab, label: "Quản Lý Khiếu Nại", icon: AlertCircle },
    { id: "enterprises" as Tab, label: "Quản Lý Doanh Nghiệp", icon: Building2 },
    { id: "users" as Tab, label: "Quản Lý Người Dùng", icon: Users },
  ];

  const handleLogout = () => {
    logout();
    router.push("/");
  };

  const renderContent = () => {
    switch (activeTab) {
      case "analytics": return <WasteAnalytics />;
      case "reports": return <ReportsManagement />;
      case "disputes": return <DisputesManagement />;
      case "enterprises": return <EnterpriseManagement />;
      case "users": return <UserManagement />;
      default: return <WasteAnalytics />;
    }
  };

  return (
    <div className="flex h-screen bg-[#F8FAFC] overflow-hidden font-sans">
      {/* Sidebar */}
      <aside
        className={`${
          sidebarOpen ? "w-72" : "w-20"
        } bg-white border-r border-slate-200 flex flex-col transition-all duration-300 ease-in-out hidden md:flex z-30 shadow-[4px_0_24px_rgba(0,0,0,0.02)]`}
      >
        {/* Branding */}
        <div className="p-8 mb-4">
          <div className="flex items-center gap-3 overflow-hidden">
            <div className="min-w-[40px] h-10 relative">
              <Image
                src="/logo/logo.jpg"
                alt="CWCRP Logo"
                fill
                className="rounded-xl object-contain"
              />
            </div>
            {sidebarOpen && (
              <div className="animate-in fade-in slide-in-from-left-4 duration-500">
                <h1 className="text-xl font-black text-slate-800 tracking-tight leading-none">CWCRP</h1>
                <p className="text-[10px] font-bold text-emerald-600 uppercase tracking-widest mt-1">Admin Portal</p>
              </div>
            )}
          </div>
        </div>

        {/* Navigation */}
        <nav className="flex-1 px-4 space-y-1.5 overflow-y-auto custom-scrollbar">
          {tabs.map((tab) => {
            const Icon = tab.icon;
            const isActive = activeTab === tab.id;
            return (
              <button
                key={tab.id}
                onClick={() => setActiveTab(tab.id)}
                className={`
                  w-full flex items-center gap-3.5 px-4 py-3.5 rounded-2xl text-sm font-bold transition-all duration-200 group relative
                  ${
                    isActive
                      ? "bg-emerald-50 text-emerald-700 shadow-sm"
                      : "text-slate-500 hover:bg-slate-50 hover:text-slate-700"
                  }
                `}
              >
                {isActive && (
                  <div className="absolute left-0 w-1.5 h-6 bg-emerald-500 rounded-r-full animate-in slide-in-from-left-2" />
                )}
                
                <Icon 
                  size={20} 
                  className={`transition-transform duration-200 group-hover:scale-110 ${isActive ? "text-emerald-600" : "text-slate-400"}`} 
                />
                
                {sidebarOpen && (
                  <span className="animate-in fade-in duration-300 flex-1 text-left">
                    {tab.label}
                  </span>
                )}

                {sidebarOpen && isActive && (
                  <ChevronRight size={14} className="text-emerald-400 animate-in fade-in" />
                )}
              </button>
            );
          })}
        </nav>

        {/* User Profile - Bottom */}
        <div className="p-4 mt-auto border-t border-slate-100">
          <div className={`
            flex items-center gap-3 p-3 rounded-2xl bg-slate-50 border border-slate-100 transition-all
            ${sidebarOpen ? "px-3" : "px-0 justify-center bg-transparent border-none"}
          `}>
            <div className="w-10 h-10 rounded-xl bg-gradient-to-br from-emerald-500 to-teal-600 flex items-center justify-center text-white font-black text-sm shadow-md shadow-emerald-100 shrink-0">
              {user?.fullName?.charAt(0).toUpperCase() || "A"}
            </div>
            
            {sidebarOpen && (
              <div className="flex-1 min-w-0 animate-in fade-in duration-300">
                <p className="text-[13px] font-black text-slate-800 truncate">
                  {user?.fullName || "Administrator"}
                </p>
                <p className="text-[11px] font-medium text-slate-400 truncate">{user?.email}</p>
              </div>
            )}
          </div>

          <button
            onClick={handleLogout}
            className={`
              mt-3 w-full flex items-center gap-3.5 px-4 py-3.5 rounded-2xl text-xs font-black transition-all
              text-rose-500 hover:bg-rose-50 group
              ${sidebarOpen ? "" : "justify-center px-0"}
            `}
          >
            <LogOut size={18} className="group-hover:-translate-x-1 transition-transform" />
            {sidebarOpen && <span className="uppercase tracking-widest">Đăng Xuất</span>}
          </button>
        </div>
      </aside>

      {/* Main Viewport */}
      <main className="flex-1 flex flex-col min-w-0 relative">
        
        {/* Header đã xóa bỏ ở đây để không hiện dòng tiêu đề và thông báo phèn */}

        {/* Mobile Header (Simplified) */}
        <div className="md:hidden bg-white border-b border-slate-200 p-5 flex items-center justify-between sticky top-0 z-40 shadow-sm">
          <button
            onClick={() => setSidebarOpen(!sidebarOpen)}
            className="p-2 text-slate-600 bg-slate-50 rounded-xl"
          >
            {sidebarOpen ? <X size={22} /> : <Menu size={22} />}
          </button>
          <span className="font-black text-slate-800 tracking-tight uppercase text-sm">Dashboard</span>
          <button onClick={handleLogout} className="p-2 text-rose-500 bg-rose-50 rounded-xl">
            <LogOut size={20} />
          </button>
        </div>

        {/* Content Container - Đẩy padding lên để sát mép trên một cách hợp lý */}
        <div className="flex-1 overflow-y-auto px-6 py-6 lg:px-10 lg:py-8 max-w-[1600px] w-full mx-auto animate-in fade-in slide-in-from-bottom-4 duration-700">
           {renderContent()}
        </div>

      </main>
    </div>
  );
};