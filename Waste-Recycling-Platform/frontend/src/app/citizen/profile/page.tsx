"use client";

import React, { useState, useEffect } from "react";
import Link from "next/link";
import { useRouter } from "next/navigation";
import { profileApi, ProfileDto, UpdateProfileDto } from "@/lib/api/profileApi";
import { reportApi } from "@/lib/api/reportApi";
import { ApiError } from "@/lib/api/client";
import { 
  User, 
  Mail, 
  Phone, 
  Trophy, 
  Leaf, 
  Recycle,
  Award,
  TrendingUp,
  Calendar,
  Clock,
  Settings,
  HelpCircle,
  ChevronRight,
  Star,
  Target,
  Gift,
  BarChart3,
  History,
  Bell,
  MessageSquare
} from "lucide-react";

// Types for API responses
interface RewardData {
  totalPoints: number;
  nextLevelPoints?: number;
  level?: string;
  badge?: string;
}

interface RankingData {
  currentRanking: number;
  totalPeopleInArea: number;
  district: string;
}

interface ReportData {
  id: string;
  categoryName: string;
  status: "Pending" | "Accepted" | "Assigned" | "Collected";
  address: string;
  createdAt: string;
  imageCount: number;
}

const getLevelConfig = (level?: string) => {
  const configs: Record<string, { color: string; bgColor: string; borderColor: string }> = {
    "Bronze": { color: "text-amber-700", bgColor: "bg-amber-100", borderColor: "border-amber-300" },
    "Silver": { color: "text-gray-600", bgColor: "bg-gray-100", borderColor: "border-gray-300" },
    "Gold": { color: "text-yellow-600", bgColor: "bg-yellow-100", borderColor: "border-yellow-300" },
    "Hero": { color: "text-green-600", bgColor: "bg-green-100", borderColor: "border-green-300" }
  };
  return configs[level || "Bronze"];
};

const getStatusConfig = (status: string) => {
  const configs: Record<string, { color: string; bgColor: string }> = {
    "Pending": { color: "text-yellow-700", bgColor: "bg-yellow-100" },
    "Assigned": { color: "text-blue-700", bgColor: "bg-blue-100" },
    "Collected": { color: "text-green-700", bgColor: "bg-green-100" },
    "Accepted": { color: "text-purple-700", bgColor: "bg-purple-100" }
  };
  return configs[status] || configs["Pending"];
};

export default function ProfilePage() {
  const router = useRouter();
  const [profile, setProfile] = useState<ProfileDto | null>(null);
  const [rewardData, setRewardData] = useState<RewardData | null>(null);
  const [rankingData, setRankingData] = useState<RankingData | null>(null);
  const [activeReports, setActiveReports] = useState<ReportData[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [isEditing, setIsEditing] = useState(false);
  const [updateForm, setUpdateForm] = useState<UpdateProfileDto>({
    fullName: "",
    phone: "",
    district: "",
    ward: "",
  });
  const [updateLoading, setUpdateLoading] = useState(false);
  const [successMessage, setSuccessMessage] = useState<string | null>(null);

  useEffect(() => {
    loadAllData();
  }, []);

  const loadAllData = async () => {
    try {
      setLoading(true);
      setError(null);

      // Load profile data
      const profileResponse = await profileApi.getProfile();
      setProfile(profileResponse.data);
      setUpdateForm({
        fullName: profileResponse.data.fullName,
        phone: profileResponse.data.phone || "",
        district: profileResponse.data.district || "",
        ward: profileResponse.data.ward || "",
      });

      // Load reward data (using existing rewards API)
      try {
        const rewardsResponse = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/citizens/rewards`, {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`,
            'Content-Type': 'application/json',
          },
        });
        
        if (rewardsResponse.ok) {
          const rewardsData = await rewardsResponse.json();
          setRewardData({
            totalPoints: rewardsData.data?.totalPoints || 0,
            level: rewardsData.data?.level || "Bronze",
            badge: rewardsData.data?.badge || "Người mới bắt đầu",
            nextLevelPoints: rewardsData.data?.nextLevelPoints || 1000,
          });
        }
      } catch (rewardError) {
        console.warn("Failed to load reward data:", rewardError);
        // Set default values
        setRewardData({
          totalPoints: 0,
          level: "Bronze",
          badge: "Người mới bắt đầu",
          nextLevelPoints: 1000,
        });
      }

      // Load ranking data
      try {
        const rankingResponse = await fetch(`${process.env.NEXT_PUBLIC_API_URL}/citizens/rewards/leaderboard`, {
          headers: {
            'Authorization': `Bearer ${localStorage.getItem('token')}`,
            'Content-Type': 'application/json',
          },
        });
        
        if (rankingResponse.ok) {
          const rankingResult = await rankingResponse.json();
          // Find current user's ranking
          const currentUserData = rankingResult.data?.find((item: any) => item.citizenId === profile?.id);
          if (currentUserData) {
            setRankingData({
              currentRanking: currentUserData.rank,
              totalPeopleInArea: rankingResult.pagination?.total || 100,
              district: profile?.district || "Chưa xác định",
            });
          }
        }
      } catch (rankingError) {
        console.warn("Failed to load ranking data:", rankingError);
      }

      // Load active reports
      try {
        const reportsResponse = await reportApi.getMyReports(1, 5);
        const activeReportsOnly = reportsResponse.reports.filter(
          (report: ReportData) => report.status === "Pending" || report.status === "Assigned"
        );
        setActiveReports(activeReportsOnly);
      } catch (reportsError) {
        console.warn("Failed to load reports:", reportsError);
      }

    } catch (err) {
      if (err instanceof ApiError) {
        setError(err.message);
      } else {
        setError("Failed to load profile data");
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
      setIsEditing(false);
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
  };

  // Navigation handlers
  const navigateTo = (path: string) => {
    router.push(path);
  };

  if (loading) {
    return (
      <div className="min-h-screen bg-gray-50 p-4">
        <div className="max-w-6xl mx-auto">
          {/* Header Skeleton */}
          <div className="bg-white rounded-xl shadow-sm p-6 mb-6">
            <div className="flex items-center gap-4">
              <div className="w-20 h-20 bg-gray-200 rounded-full animate-pulse"></div>
              <div className="flex-1">
                <div className="h-6 bg-gray-200 rounded w-1/3 mb-2 animate-pulse"></div>
                <div className="h-4 bg-gray-200 rounded w-1/2 animate-pulse"></div>
              </div>
            </div>
          </div>
          
          {/* Cards Skeleton */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-6 mb-6">
            {[1, 2, 3].map(i => (
              <div key={i} className="bg-white rounded-xl shadow-sm p-6">
                <div className="h-4 bg-gray-200 rounded w-1/2 mb-4 animate-pulse"></div>
                <div className="h-8 bg-gray-200 rounded w-3/4 animate-pulse"></div>
              </div>
            ))}
          </div>
        </div>
      </div>
    );
  }

  if (error && !profile) {
    return (
      <div className="min-h-screen bg-gray-50 flex items-center justify-center">
        <div className="bg-red-50 border border-red-200 text-red-700 px-6 py-4 rounded-lg">
          {error}
        </div>
      </div>
    );
  }

  const levelConfig = getLevelConfig(rewardData?.level);

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header Section - 1/3 màn hình */}
      <div className="bg-gradient-to-br from-green-50 to-emerald-100 border-b border-green-200">
        <div className="max-w-6xl mx-auto px-4 py-8">
          <div className="flex flex-col md:flex-row items-center gap-6">
            {/* Avatar & Info */}
            <div className="flex items-center gap-4">
              <div className={`w-24 h-24 rounded-full border-4 ${levelConfig.borderColor} ${levelConfig.bgColor} flex items-center justify-center`}>
                <User size={48} className={levelConfig.color} />
              </div>
              <div>
                <h1 className="text-2xl font-bold text-gray-900">{profile?.fullName || "User Name"}</h1>
                <div className="flex items-center gap-2 mt-1">
                  <Mail size={16} className="text-gray-500" />
                  <span className="text-gray-600">{profile?.email || "user@email.com"}</span>
                </div>
              </div>
            </div>

            {/* Level & Badge */}
            <div className="flex-1 text-center md:text-right">
              <div className={`inline-flex items-center gap-2 px-4 py-2 ${levelConfig.bgColor} ${levelConfig.color} rounded-full font-semibold mb-2`}>
                <Trophy size={20} />
                {rewardData?.level || "Bronze"} - {rewardData?.badge || "Người mới bắt đầu"}
              </div>
              
              {/* Progress Bar */}
              <div className="w-full max-w-xs mx-auto md:ml-auto">
                <div className="flex justify-between text-sm text-gray-600 mb-1">
                  <span>{rewardData?.totalPoints || 0} điểm</span>
                  <span>{rewardData?.nextLevelPoints || 1000} điểm</span>
                </div>
                <div className="w-full bg-gray-200 rounded-full h-3">
                  <div 
                    className="bg-gradient-to-r from-green-400 to-emerald-500 h-3 rounded-full transition-all duration-500"
                    style={{ width: `${Math.min(((rewardData?.totalPoints || 0) / (rewardData?.nextLevelPoints || 1000)) * 100, 100)}%` }}
                  ></div>
                </div>
                <p className="text-xs text-gray-500 mt-1">
                  Cần thêm {Math.max((rewardData?.nextLevelPoints || 1000) - (rewardData?.totalPoints || 0), 0)} điểm để lên cấp
                </p>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-6xl mx-auto px-4 py-6">
        {successMessage && (
          <div className="bg-green-50 border border-green-200 text-green-700 px-4 py-3 rounded-lg mb-6">
            {successMessage}
          </div>
        )}

        <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
          {/* Main Content - 2/3 */}
          <div className="lg:col-span-2 space-y-6">
            {/* Impact Cards */}
            <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
              {/* Points Card */}
              <div className="bg-white rounded-xl shadow-sm p-6 border-l-4 border-green-500">
                <div className="flex items-center justify-between mb-2">
                  <Award className="text-green-600" size={24} />
                  <span className="text-xs text-gray-500 bg-green-100 px-2 py-1 rounded">Active</span>
                </div>
                <h3 className="text-2xl font-bold text-gray-900">{(rewardData?.totalPoints || 0).toLocaleString()}</h3>
                <p className="text-sm text-gray-600">Điểm hiện có</p>
                <button 
                  onClick={() => navigateTo('/citizen/rewards')}
                  className="mt-3 text-sm text-green-600 hover:text-green-700 font-medium flex items-center gap-1"
                >
                  <Gift size={16} />
                  Đổi quà
                  <ChevronRight size={16} />
                </button>
              </div>

              {/* Reports Card */}
              <div className="bg-white rounded-xl shadow-sm p-6 border-l-4 border-blue-500">
                <div className="flex items-center justify-between mb-2">
                  <Recycle className="text-blue-600" size={24} />
                  <Leaf className="text-blue-400" size={16} />
                </div>
                <h3 className="text-2xl font-bold text-gray-900">{activeReports.length}</h3>
                <p className="text-sm text-gray-600">Báo cáo đang hoạt động</p>
                <div className="mt-2 text-xs text-blue-600">
                  <Clock size={12} className="inline mr-1" />
                  {activeReports.length} yêu cầu
                </div>
              </div>

              {/* Profile Completion Card */}
              <div className="bg-white rounded-xl shadow-sm p-6 border-l-4 border-purple-500">
                <div className="flex items-center justify-between mb-2">
                  <BarChart3 className="text-purple-600" size={24} />
                  <Target className="text-purple-400" size={16} />
                </div>
                <h3 className="text-2xl font-bold text-gray-900">
                  {profile?.phone && profile?.district && profile?.ward ? "100%" : "75%"}
                </h3>
                <p className="text-sm text-gray-600">Hồ sơ hoàn chỉnh</p>
                <div className="mt-2 text-xs text-purple-600">
                  <Star size={12} className="inline mr-1" />
                  {profile?.phone && profile?.district && profile?.ward ? "Đầy đủ" : "Cần cập nhật"}
                </div>
              </div>
            </div>

            {/* Regional Ranking */}
            {rankingData && (
              <div className="bg-white rounded-xl shadow-sm p-6">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-lg font-semibold text-gray-900">Xếp hạng khu vực</h3>
                  <Trophy className="text-yellow-500" size={24} />
                </div>
                <div className="flex items-center justify-between">
                  <div>
                    <div className="flex items-baseline gap-2">
                      <span className="text-3xl font-bold text-gray-900">#{rankingData.currentRanking}</span>
                      <span className="text-gray-500">/ {rankingData.totalPeopleInArea.toLocaleString()}</span>
                    </div>
                    <p className="text-sm text-gray-600 mt-1">tại {rankingData.district}</p>
                  </div>
                  <button 
                    onClick={() => navigateTo('/leaderboard')}
                    className="bg-green-600 text-white px-4 py-2 rounded-lg hover:bg-green-700 transition-colors flex items-center gap-2"
                  >
                    <BarChart3 size={16} />
                    Xem bảng xếp hạng
                  </button>
                </div>
              </div>
            )}

            {/* Active Requests */}
            {activeReports.length > 0 && (
              <div className="bg-white rounded-xl shadow-sm p-6">
                <div className="flex items-center justify-between mb-4">
                  <h3 className="text-lg font-semibold text-gray-900">Yêu cầu đang hoạt động</h3>
                  <Clock className="text-gray-400" size={20} />
                </div>
                <div className="space-y-3">
                  {activeReports.slice(0, 3).map(report => {
                    const statusConfig = getStatusConfig(report.status);
                    return (
                      <div key={report.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                        <div className="flex items-center gap-3">
                          <Recycle size={20} className="text-gray-600" />
                          <div>
                            <p className="font-medium text-gray-900">Rác {report.categoryName}</p>
                            <p className="text-xs text-gray-500">{new Date(report.createdAt).toLocaleString('vi-VN')}</p>
                          </div>
                        </div>
                        <span className={`px-2 py-1 text-xs rounded-full ${statusConfig.bgColor} ${statusConfig.color}`}>
                          {report.status}
                        </span>
                      </div>
                    );
                  })}
                </div>
                {activeReports.length > 3 && (
                  <button 
                    onClick={() => navigateTo('/citizen/reports')}
                    className="mt-3 text-sm text-blue-600 hover:text-blue-700 font-medium"
                  >
                    Xem tất cả ({activeReports.length} yêu cầu)
                  </button>
                )}
              </div>
            )}
          </div>

          {/* Sidebar - 1/3 */}
          <div className="space-y-6">
            {/* Quick Actions */}
            <div className="bg-white rounded-xl shadow-sm p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Quản lý tài khoản</h3>
              <div className="space-y-2">
                <button 
                  onClick={() => navigateTo('/citizen/reports')}
                  className="w-full flex items-center gap-3 p-3 hover:bg-gray-50 rounded-lg transition-colors text-left"
                >
                  <History size={20} className="text-gray-400" />
                  <div className="flex-1">
                    <p className="font-medium text-gray-900">Lịch sử báo cáo</p>
                    <p className="text-xs text-gray-500">Xem lại các báo cáo đã gửi</p>
                  </div>
                  <ChevronRight size={16} className="text-gray-400" />
                </button>

                <button 
                  onClick={() => navigateTo('/citizen/points-history')}
                  className="w-full flex items-center gap-3 p-3 hover:bg-gray-50 rounded-lg transition-colors text-left"
                >
                  <Award size={20} className="text-gray-400" />
                  <div className="flex-1">
                    <p className="font-medium text-gray-900">Lịch sử điểm thưởng</p>
                    <p className="text-xs text-gray-500">Chi tiết các lần cộng/trừ điểm</p>
                  </div>
                  <ChevronRight size={16} className="text-gray-400" />
                </button>

                <button 
                  onClick={() => navigateTo('/settings')}
                  className="w-full flex items-center gap-3 p-3 hover:bg-gray-50 rounded-lg transition-colors text-left"
                >
                  <Settings size={20} className="text-gray-400" />
                  <div className="flex-1">
                    <p className="font-medium text-gray-900">Cài đặt tài khoản</p>
                    <p className="text-xs text-gray-500">Quản lý thông tin cá nhân & bảo mật</p>
                  </div>
                  <ChevronRight size={16} className="text-gray-400" />
                </button>

                <button 
                  disabled
                  className="w-full flex items-center gap-3 p-3 hover:bg-gray-50 rounded-lg transition-colors text-left opacity-50 cursor-not-allowed"
                >
                  <Bell size={20} className="text-gray-400" />
                  <div className="flex-1">
                    <p className="font-medium text-gray-900">Cài đặt thông báo</p>
                    <p className="text-xs text-gray-500">Sắp ra mắt</p>
                  </div>
                  <span className="text-xs bg-gray-200 text-gray-600 px-2 py-1 rounded">Coming soon</span>
                </button>

                <button 
                  onClick={() => navigateTo('/citizen/complaints')}
                  className="w-full flex items-center gap-3 p-3 hover:bg-gray-50 rounded-lg transition-colors text-left"
                >
                  <MessageSquare size={20} className="text-gray-400" />
                  <div className="flex-1">
                    <p className="font-medium text-gray-900">Hỗ trợ & Khiếu nại</p>
                    <p className="text-xs text-gray-500">Xem khiếu nại đã gửi</p>
                  </div>
                  <ChevronRight size={16} className="text-gray-400" />
                </button>
              </div>
            </div>

            {/* Account Summary */}
            <div className="bg-white rounded-xl shadow-sm p-6">
              <h3 className="text-lg font-semibold text-gray-900 mb-4">Tóm tắt tài khoản</h3>
              <div className="space-y-3">
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-500">Trạng thái</span>
                  <span className={`inline-flex px-2 py-1 text-xs rounded-full ${
                    profile?.isActive 
                      ? 'bg-green-100 text-green-800' 
                      : 'bg-red-100 text-red-800'
                  }`}>
                    {profile?.isActive ? 'Hoạt động' : 'Không hoạt động'}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-500">Ngày tham gia</span>
                  <span className="text-sm font-medium text-gray-900">
                    {profile?.createdAt ? new Date(profile.createdAt).toLocaleDateString('vi-VN') : "N/A"}
                  </span>
                </div>
                <div className="flex justify-between items-center">
                  <span className="text-sm text-gray-500">Cập nhật lần cuối</span>
                  <span className="text-sm font-medium text-gray-900">
                    {profile?.updatedAt ? new Date(profile.updatedAt).toLocaleDateString('vi-VN') : "Chưa cập nhật"}
                  </span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
}
