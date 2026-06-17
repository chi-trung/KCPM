"use client";

import React, { useState, useEffect } from "react";
import { useRouter } from "next/navigation";
import { User, Lock, Bell, Shield, Save, Camera, Mail, Phone, MapPin, Settings } from "lucide-react";
import { profileApi, ProfileDto, UpdateProfileDto } from "@/lib/api/profileApi";
import { ApiError } from "@/lib/api/client";

type Tab = "profile" | "security" | "notifications";

export default function SettingsPage() {
  const router = useRouter();
  const [activeTab, setActiveTab] = useState<Tab>("profile");
  const [profile, setProfile] = useState<ProfileDto | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);
  const [updateLoading, setUpdateLoading] = useState(false);
  const [updateForm, setUpdateForm] = useState<UpdateProfileDto>({
    fullName: "",
    phone: "",
    district: "",
    ward: "",
  });

  useEffect(() => {
    if (activeTab === "profile") {
      loadProfile();
    }
  }, [activeTab]);

  const loadProfile = async () => {
    try {
      setLoading(true);
      setError(null);
      const response = await profileApi.getProfile();
      setProfile(response.data);
      setUpdateForm({
        fullName: response.data.fullName,
        phone: response.data.phone || "",
        district: response.data.district || "",
        ward: response.data.ward || "",
      });
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message);
      } else {
        setError("Failed to load profile");
      }
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateProfile = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!updateForm.fullName.trim()) {
      setError("Full name is required");
      return;
    }

    try {
      setUpdateLoading(true);
      setError(null);
      setSuccessMessage(null);
      
      const response = await profileApi.updateProfile(updateForm);
      setProfile(response.data);
      setSuccessMessage("Profile updated successfully!");
      
      setTimeout(() => setSuccessMessage(null), 3000);
    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message);
      } else {
        setError("Failed to update profile");
      }
    } finally {
      setUpdateLoading(false);
    }
  };

  const handleInputChange = (field: keyof UpdateProfileDto, value: string) => {
    setUpdateForm(prev => ({ ...prev, [field]: value }));
    setError(null);
  };

  return (
    <div className="min-h-screen bg-gray-50/50 py-10">
      <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8">
        
        {/* Header */}
        <div className="mb-8">
          <h1 className="text-3xl font-bold text-gray-900">Cài Đặt Tài Khoản</h1>
          <p className="text-gray-500 mt-2">Quản lý thông tin cá nhân và bảo mật của bạn</p>
        </div>

        {successMessage && (
          <div className="bg-green-50 border border-green-200 text-green-700 px-4 py-3 rounded-lg mb-6">
            {successMessage}
          </div>
        )}

        {error && (
          <div className="bg-red-50 border border-red-200 text-red-700 px-4 py-3 rounded-lg mb-6">
            {error}
          </div>
        )}

        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden flex flex-col md:flex-row min-h-[600px]">
          
          {/* Sidebar Navigation */}
          <div className="w-full md:w-64 bg-gray-50/50 border-r border-gray-100 p-6">
            <nav className="space-y-2">
              <button
                onClick={() => setActiveTab("profile")}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl font-medium transition-all ${
                  activeTab === "profile"
                    ? "bg-emerald-100 text-emerald-800 shadow-sm"
                    : "text-gray-600 hover:bg-gray-100 hover:text-gray-900"
                }`}
              >
                <User size={18} className={activeTab === "profile" ? "text-emerald-600" : "text-gray-400"} />
                Hồ sơ cá nhân
              </button>
              
              <button
                onClick={() => setActiveTab("security")}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl font-medium transition-all ${
                  activeTab === "security"
                    ? "bg-emerald-100 text-emerald-800 shadow-sm"
                    : "text-gray-600 hover:bg-gray-100 hover:text-gray-900"
                }`}
              >
                <Shield size={18} className={activeTab === "security" ? "text-emerald-600" : "text-gray-400"} />
                Bảo mật & Mật khẩu
              </button>

              <button
                onClick={() => setActiveTab("notifications")}
                className={`w-full flex items-center gap-3 px-4 py-3 rounded-xl font-medium transition-all ${
                  activeTab === "notifications"
                    ? "bg-emerald-100 text-emerald-800 shadow-sm"
                    : "text-gray-600 hover:bg-gray-100 hover:text-gray-900"
                }`}
              >
                <Bell size={18} className={activeTab === "notifications" ? "text-emerald-600" : "text-gray-400"} />
                Cài đặt thông báo
              </button>
            </nav>
          </div>

          {/* Nội dung chính */}
          <div className="flex-1 p-6 md:p-10">
            
            {/* TAB 1: HỒ SƠ CÁ NHÂN */}
            {activeTab === "profile" && (
              <div className="max-w-2xl animate-in fade-in slide-in-from-bottom-4 duration-500">
                <h2 className="text-xl font-bold text-gray-800 mb-6 flex items-center gap-2">
                  <User size={24} className="text-emerald-500" />
                  Thông tin cơ bản
                </h2>
                
                {/* Avatar Section */}
                <div className="flex items-center gap-6 mb-8">
                  <div className="relative">
                    <div className="w-24 h-24 bg-gradient-to-br from-emerald-400 to-emerald-600 rounded-full flex items-center justify-center text-white text-3xl font-bold shadow-md">
                      {profile?.fullName?.charAt(0) || "U"}
                    </div>
                    <button className="absolute bottom-0 right-0 bg-white p-2 rounded-full shadow-md border border-gray-100 hover:bg-gray-50 text-emerald-600 transition-colors">
                      <Camera size={16} />
                    </button>
                  </div>
                  <div>
                    <h3 className="text-lg font-bold text-gray-900">Ảnh đại diện</h3>
                    <p className="text-sm text-gray-500 mt-1">Nên dùng ảnh vuông, định dạng JPG, PNG.</p>
                  </div>
                </div>

                {loading ? (
                  <div className="flex items-center justify-center py-8">
                    <div className="text-gray-500">Loading...</div>
                  </div>
                ) : (
                  <form onSubmit={handleUpdateProfile} className="space-y-5">
                    <div className="space-y-2">
                      <label htmlFor="settings-fullname" className="text-sm font-semibold text-gray-700">Họ và Tên *</label>
                      <input 
                        id="settings-fullname"
                        type="text" 
                        value={updateForm.fullName}
                        onChange={(e) => handleInputChange('fullName', e.target.value)}
                        className="w-full bg-gray-50 border border-gray-200 rounded-xl p-3 focus:bg-white focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition-all text-gray-800"
                        required
                      />
                    </div>

                    <div className="space-y-2">
                      <label htmlFor="settings-phone" className="text-sm font-semibold text-gray-700">Số điện thoại</label>
                      <input 
                        id="settings-phone"
                        type="tel" 
                        value={updateForm.phone}
                        onChange={(e) => handleInputChange('phone', e.target.value)}
                        className="w-full bg-gray-50 border border-gray-200 rounded-xl p-3 focus:bg-white focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition-all text-gray-800"
                      />
                    </div>

                    <div className="space-y-2">
                      <label htmlFor="settings-email" className="text-sm font-semibold text-gray-700">Địa chỉ Email</label>
                      <input 
                        id="settings-email"
                        type="email" 
                        value={profile?.email || ""}
                        disabled
                        className="w-full bg-gray-100 border border-gray-200 rounded-xl p-3 text-gray-500 cursor-not-allowed"
                      />
                      <p className="text-xs text-gray-500">Email không thể thay đổi sau khi đăng ký.</p>
                    </div>

                    <div className="grid grid-cols-1 md:grid-cols-2 gap-5">
                      <div className="space-y-2">
                        <label htmlFor="settings-district" className="text-sm font-semibold text-gray-700">Quận/Huyện</label>
                        <input 
                          id="settings-district"
                          type="text" 
                          value={updateForm.district}
                          onChange={(e) => handleInputChange('district', e.target.value)}
                          className="w-full bg-gray-50 border border-gray-200 rounded-xl p-3 focus:bg-white focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition-all text-gray-800"
                        />
                      </div>
                      <div className="space-y-2">
                        <label htmlFor="settings-ward" className="text-sm font-semibold text-gray-700">Phường/Xã</label>
                        <input 
                          id="settings-ward"
                          type="text" 
                          value={updateForm.ward}
                          onChange={(e) => handleInputChange('ward', e.target.value)}
                          className="w-full bg-gray-50 border border-gray-200 rounded-xl p-3 focus:bg-white focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition-all text-gray-800"
                        />
                      </div>
                    </div>

                    <div className="pt-4 border-t border-gray-100">
                      <button 
                        type="submit" 
                        disabled={updateLoading}
                        className="bg-emerald-600 hover:bg-emerald-700 text-white font-semibold py-3 px-6 rounded-xl shadow-md shadow-emerald-500/20 transition-all flex items-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                      >
                        <Save size={18} />
                        {updateLoading ? 'Đang lưu...' : 'Lưu Thay Đổi'}
                      </button>
                    </div>
                  </form>
                )}
              </div>
            )}

            {/* TAB 2: BẢO MẬT */}
            {activeTab === "security" && (
              <div className="max-w-2xl animate-in fade-in slide-in-from-bottom-4 duration-500">
                 <h2 className="text-xl font-bold text-gray-800 mb-6 flex items-center gap-2">
                  <Lock size={24} className="text-emerald-500" />
                  Đổi mật khẩu
                </h2>

                <form className="space-y-5" onSubmit={(e) => e.preventDefault()}>
                  <div className="space-y-2">
                    <label htmlFor="settings-current-password" className="text-sm font-semibold text-gray-700">Mật khẩu hiện tại</label>
                    <input 
                      id="settings-current-password"
                      type="password" 
                      placeholder="••••••••"
                      className="w-full bg-gray-50 border border-gray-200 rounded-xl p-3 focus:bg-white focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition-all text-gray-800"
                    />
                  </div>
                  
                  <div className="space-y-2">
                    <label htmlFor="settings-new-password" className="text-sm font-semibold text-gray-700">Mật khẩu mới</label>
                    <input 
                      id="settings-new-password"
                      type="password" 
                      placeholder="••••••••"
                      className="w-full bg-gray-50 border border-gray-200 rounded-xl p-3 focus:bg-white focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition-all text-gray-800"
                    />
                  </div>

                  <div className="space-y-2">
                    <label htmlFor="settings-confirm-password" className="text-sm font-semibold text-gray-700">Xác nhận mật khẩu mới</label>
                    <input 
                      id="settings-confirm-password"
                      type="password" 
                      placeholder="••••••••"
                      className="w-full bg-gray-50 border border-gray-200 rounded-xl p-3 focus:bg-white focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition-all text-gray-800"
                    />
                  </div>

                  <div className="pt-4 border-t border-gray-100">
                    <button type="button" className="bg-emerald-600 hover:bg-emerald-700 text-white font-semibold py-3 px-6 rounded-xl shadow-md shadow-emerald-500/20 transition-all flex items-center gap-2">
                      <Save size={18} />
                      Cập nhật mật khẩu
                    </button>
                  </div>
                </form>
              </div>
            )}

            {/* TAB 3: THÔNG BÁO */}
            {activeTab === "notifications" && (
              <div className="max-w-2xl animate-in fade-in slide-in-from-bottom-4 duration-500">
                 <h2 className="text-xl font-bold text-gray-800 mb-6 flex items-center gap-2">
                  <Bell size={24} className="text-emerald-500" />
                  Cài đặt thông báo
                </h2>

                <div className="space-y-6">
                  <div className="bg-gray-50 rounded-xl p-6">
                    <h3 className="font-semibold text-gray-800 mb-4">Thông báo Email</h3>
                    <div className="space-y-3">
                      <label className="flex items-center justify-between cursor-pointer">
                        <span>
                          <span className="font-medium text-gray-700 block">Báo cáo mới được tiếp nhận</span>
                          <span className="text-sm text-gray-500 block">Nhận email khi báo cáo rác được tiếp nhận</span>
                        </span>
                        <input type="checkbox" defaultChecked aria-label="Báo cáo mới được tiếp nhận" className="w-5 h-5 text-emerald-600 rounded focus:ring-emerald-500" />
                      </label>
                      
                      <label className="flex items-center justify-between cursor-pointer">
                        <span>
                          <span className="font-medium text-gray-700 block">Báo cáo đã được thu gom</span>
                          <span className="text-sm text-gray-500 block">Nhận email khi rác đã được thu gom thành công</span>
                        </span>
                        <input type="checkbox" defaultChecked aria-label="Báo cáo đã được thu gom" className="w-5 h-5 text-emerald-600 rounded focus:ring-emerald-500" />
                      </label>
                      
                      <label className="flex items-center justify-between cursor-pointer">
                        <span>
                          <span className="font-medium text-gray-700 block">Cập nhật điểm thưởng</span>
                          <span className="text-sm text-gray-500 block">Nhận email khi có thay đổi điểm thưởng</span>
                        </span>
                        <input type="checkbox" aria-label="Cập nhật điểm thưởng" className="w-5 h-5 text-emerald-600 rounded focus:ring-emerald-500" />
                      </label>
                    </div>
                  </div>

                  <div className="bg-gray-50 rounded-xl p-6">
                    <h3 className="font-semibold text-gray-800 mb-4">Thông báo Push</h3>
                    <div className="space-y-3">
                      <label className="flex items-center justify-between cursor-pointer">
                        <span>
                          <span className="font-medium text-gray-700 block">Collector đang đến</span>
                          <span className="text-sm text-gray-500 block">Nhận thông báo khi collector sắp đến địa điểm</span>
                        </span>
                        <input type="checkbox" defaultChecked aria-label="Collector đang đến" className="w-5 h-5 text-emerald-600 rounded focus:ring-emerald-500" />
                      </label>
                      
                      <label className="flex items-center justify-between cursor-pointer">
                        <span>
                          <span className="font-medium text-gray-700 block">Khuyến mãi & Ưu đãi</span>
                          <span className="text-sm text-gray-500 block">Nhận thông báo về các chương trình khuyến mãi</span>
                        </span>
                        <input type="checkbox" aria-label="Khuyến mãi và Ưu đãi" className="w-5 h-5 text-emerald-600 rounded focus:ring-emerald-500" />
                      </label>
                    </div>
                  </div>

                  <div className="pt-4 border-t border-gray-100">
                    <button type="button" className="bg-emerald-600 hover:bg-emerald-700 text-white font-semibold py-3 px-6 rounded-xl shadow-md shadow-emerald-500/20 transition-all flex items-center gap-2">
                      <Save size={18} />
                      Lưu Cài Đặt
                    </button>
                  </div>
                </div>
              </div>
            )}
            
          </div>
        </div>
      </div>
    </div>
  );
}