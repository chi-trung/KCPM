"use client";
import React, { useState, useEffect } from "react";
import { API_CONFIG } from "@/lib/api/config";
import Link from "next/link";
import { PlusCircle, FileText, Trophy, TrendingUp, Clock, CheckCircle, Camera, MapPin, Users, Award, Target, Crown, Medal } from "lucide-react";
import { useAuth } from "@/contexts/AuthContext"; // Dùng để nhận diện user

interface RecentReport {
  id: string;
  type: string;
  status: "pending" | "accepted" | "assigned" | "collected";
  date: string;
  points?: number;
}

interface ReportDto {
  id: string;
  categoryName: string;
  status: string;
  createdAt: string;
  address: string;
  imageCount: number;
}

interface Stats {
  currentPoints: number;
  completedReports: number;
  pendingReports: number;
  thisMonthReports: number;
}

interface TopLeader {
  id: string;
  name: string;
  points: number;
  level: string;
  rank: number;
}

export default function CitizenDashboardPage() {
  const { user } = useAuth(); // Lấy thông tin user đăng nhập
  
  const [stats, setStats] = useState<Stats>({
    currentPoints: 0,
    completedReports: 0,
    pendingReports: 0,
    thisMonthReports: 0
  });

  const [recentReports, setRecentReports] = useState<RecentReport[]>([]);
  const [loadingReports, setLoadingReports] = useState(true);

  const [topLeaders, setTopLeaders] = useState<TopLeader[]>([]);
  const [loadingLeaders, setLoadingLeaders] = useState(true);

  // Fetch citizen stats and reports from API
  useEffect(() => {
    const fetchDashboardData = async () => {
      try {
        setLoadingReports(true);
        
        // Get auth token from localStorage (stored by AuthContext)
        const token = localStorage.getItem('token');
        const headers: HeadersInit = token ? { 'Authorization': `Bearer ${token}` } : {};

        // Fetch rewards (current points)
        const rewardsRes = await fetch(`${API_CONFIG.BASE_URL}/citizens/rewards`, { headers });
        let currentPoints = 0;
        if (rewardsRes.ok) {
          const rewardsJson = await rewardsRes.json();
          currentPoints = rewardsJson.data?.totalPoints || 0;
        }

        // Fetch my reports
        const reportsRes = await fetch(`${API_CONFIG.BASE_URL}/reports/my-reports?page=1&pageSize=5`, { headers });
        let reports: ReportDto[] = [];
        if (reportsRes.ok) {
          const reportsJson = await reportsRes.json();
          reports = reportsJson.reports || [];
          
          // Calculate stats from reports
          const completed = reports.filter((r: ReportDto) => r.status.toLowerCase() === 'collected').length;
          const pending = reports.filter((r: ReportDto) => ['pending', 'accepted', 'assigned'].includes(r.status.toLowerCase())).length;
          
          // Calculate this month's reports
          const now = new Date();
          const thisMonth = reports.filter((r: ReportDto) => {
            const reportDate = new Date(r.createdAt);
            return reportDate.getMonth() === now.getMonth() && reportDate.getFullYear() === now.getFullYear();
          }).length;

          setStats({
            currentPoints,
            completedReports: completed,
            pendingReports: pending,
            thisMonthReports: thisMonth
          });

          // Format recent reports for display
          const formattedReports = reports.slice(0, 5).map((r: ReportDto) => ({
            id: r.id,
            type: r.categoryName || 'Không xác định',
            status: mapStatus(r.status),
            date: formatTimeAgo(r.createdAt),
            points: r.status === 'collected' ? 10 : undefined // Points awarded when collected
          }));
          setRecentReports(formattedReports);
        }
      } catch (error) {
        console.error("Error fetching dashboard data:", error);
      } finally {
        setLoadingReports(false);
      }
    };

    fetchDashboardData();
  }, []);

  // Helper to map API status to UI status
  const mapStatus = (apiStatus: string): "pending" | "accepted" | "assigned" | "collected" => {
    switch (apiStatus.toLowerCase()) {
      case 'pending': return 'pending';
      case 'accepted': return 'accepted';
      case 'assigned': return 'assigned';
      case 'collected': return 'collected';
      default: return 'pending';
    }
  };

  // Helper to format time ago
  const formatTimeAgo = (dateString: string): string => {
    const date = new Date(dateString);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffMins = Math.floor(diffMs / 60000);
    const diffHours = Math.floor(diffMs / 3600000);
    const diffDays = Math.floor(diffMs / 86400000);

    if (diffMins < 60) return `${diffMins} phút trước`;
    if (diffHours < 24) return `${diffHours} giờ trước`;
    if (diffDays === 1) return 'Hôm qua';
    if (diffDays < 7) return `${diffDays} ngày trước`;
    return date.toLocaleDateString('vi-VN');
  };

  // Fetch API Top 3 Bảng Xếp Hạng
  useEffect(() => {
    const fetchTopLeaders = async () => {
      try {
        const response = await fetch(`${API_CONFIG.BASE_URL}/citizens/rewards/leaderboard?page=1&pageSize=3`);
        if (response.ok) {
          const json = await response.json();
          const formatted = (json.data || []).map((item: any, index: number) => {
            let level = "Thành viên Đồng";
            if (item.totalPoints >= 2000) level = "Thành viên Bạch Kim";
            else if (item.totalPoints >= 1000) level = "Thành viên Vàng";
            else if (item.totalPoints >= 500) level = "Thành viên Bạc";

            return {
              id: item.citizenId,
              name: item.citizenName,
              points: item.totalPoints,
              level: level,
              rank: index + 1
            };
          });
          setTopLeaders(formatted);
        }
      } catch (error) {
        console.error("Lỗi lấy dữ liệu bảng xếp hạng:", error);
      } finally {
        setLoadingLeaders(false);
      }
    };

    fetchTopLeaders();
  }, []);

  const getStatusColor = (status: string) => {
    switch (status) {
      case "pending": return "bg-yellow-100 text-yellow-700";
      case "accepted": return "bg-blue-100 text-blue-700";
      case "assigned": return "bg-purple-100 text-purple-700";
      case "collected": return "bg-green-100 text-green-700";
      default: return "bg-gray-100 text-gray-700";
    }
  };

  const getStatusLabel = (status: string) => {
    switch (status) {
      case "pending": return "Đang chờ";
      case "accepted": return "Đã chấp nhận";
      case "assigned": return "Đã phân công";
      case "collected": return "Đã thu gom";
      default: return status;
    }
  };

  const formatNumber = (num: number) => {
    return num.toLocaleString('vi-VN');
  };

  // Helper để chọn màu cho Top 3
  const getRankStyle = (rank: number) => {
    switch (rank) {
      case 1: return { bg: "bg-yellow-50 border-yellow-200", icon: <Crown className="w-5 h-5 text-yellow-600" />, text: "text-yellow-600" };
      case 2: return { bg: "bg-slate-50 border-gray-200", icon: <Medal className="w-5 h-5 text-gray-400" />, text: "text-gray-600" };
      case 3: return { bg: "bg-amber-50 border-amber-200", icon: <Medal className="w-5 h-5 text-amber-600" />, text: "text-amber-600" };
      default: return { bg: "bg-gray-50 border-gray-100", icon: <Award className="w-5 h-5 text-gray-400" />, text: "text-gray-500" };
    }
  };

  return (
    <div className="min-h-screen bg-gray-50 pb-20">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 sticky top-0 z-30">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">Bảng Điều Khiển</h1>
              <p className="text-gray-600">Chào mừng trở lại! Hãy cùng bảo vệ môi trường</p>
            </div>
            <div className="flex items-center gap-2">
              <div className="w-2 h-2 bg-green-500 rounded-full animate-pulse"></div>
              <span className="text-sm text-gray-600">Online</span>
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6 space-y-6">
        {/* Stats Cards */}
        <div className="grid grid-cols-2 lg:grid-cols-4 gap-4">
          <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-4">
            <div className="flex items-center justify-between mb-2">
              <div className="w-10 h-10 bg-emerald-100 rounded-lg flex items-center justify-center">
                <Trophy className="w-5 h-5 text-emerald-600" />
              </div>
              <span className="text-xs text-gray-500">Hiện tại</span>
            </div>
            <div className="text-2xl font-bold text-gray-900">{formatNumber(stats.currentPoints)}</div>
            <div className="text-sm text-gray-600">Điểm thưởng</div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-4">
            <div className="flex items-center justify-between mb-2">
              <div className="w-10 h-10 bg-blue-100 rounded-lg flex items-center justify-center">
                <CheckCircle className="w-5 h-5 text-blue-600" />
              </div>
              <span className="text-xs text-gray-500">Tổng cộng</span>
            </div>
            <div className="text-2xl font-bold text-gray-900">{stats.completedReports}</div>
            <div className="text-sm text-gray-600">Đã hoàn thành</div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-4">
            <div className="flex items-center justify-between mb-2">
              <div className="w-10 h-10 bg-yellow-100 rounded-lg flex items-center justify-center">
                <Clock className="w-5 h-5 text-yellow-600" />
              </div>
              <span className="text-xs text-gray-500">Đang xử lý</span>
            </div>
            <div className="text-2xl font-bold text-gray-900">{stats.pendingReports}</div>
            <div className="text-sm text-gray-600">Báo cáo</div>
          </div>

          <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-4">
            <div className="flex items-center justify-between mb-2">
              <div className="w-10 h-10 bg-purple-100 rounded-lg flex items-center justify-center">
                <TrendingUp className="w-5 h-5 text-purple-600" />
              </div>
              <span className="text-xs text-gray-500">Tháng này</span>
            </div>
            <div className="text-2xl font-bold text-gray-900">{stats.thisMonthReports}</div>
            <div className="text-sm text-gray-600">Báo cáo mới</div>
          </div>
        </div>

        {/* Quick Actions */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <h2 className="text-lg font-semibold text-gray-900 mb-4">Thao tác nhanh</h2>
          <div className="grid grid-cols-2 lg:grid-cols-5 gap-4">
            <Link
              href="/citizen/create-report"
              className="flex flex-col items-center gap-3 p-4 rounded-lg border border-gray-200 hover:bg-emerald-50 hover:border-emerald-300 transition-colors group"
            >
              <div className="w-12 h-12 bg-emerald-100 rounded-lg flex items-center justify-center group-hover:bg-emerald-200 transition-colors">
                <Camera className="w-6 h-6 text-emerald-600" />
              </div>
              <span className="text-sm font-medium text-gray-700 text-center">Tạo báo cáo</span>
            </Link>

            <Link
              href="/citizen/reports"
              className="flex flex-col items-center gap-3 p-4 rounded-lg border border-gray-200 hover:bg-blue-50 hover:border-blue-300 transition-colors group"
            >
              <div className="w-12 h-12 bg-blue-100 rounded-lg flex items-center justify-center group-hover:bg-blue-200 transition-colors">
                <FileText className="w-6 h-6 text-blue-600" />
              </div>
              <span className="text-sm font-medium text-gray-700 text-center">Xem lịch sử</span>
            </Link>

            <Link
              href="/citizen/rewards"
              className="flex flex-col items-center gap-3 p-4 rounded-lg border border-gray-200 hover:bg-yellow-50 hover:border-yellow-300 transition-colors group"
            >
              <div className="w-12 h-12 bg-yellow-100 rounded-lg flex items-center justify-center group-hover:bg-yellow-200 transition-colors">
                <Trophy className="w-6 h-6 text-yellow-600" />
              </div>
              <span className="text-sm font-medium text-gray-700 text-center">Đổi thưởng</span>
            </Link>

            <Link
              href="/leaderboard"
              className="flex flex-col items-center gap-3 p-4 rounded-lg border border-gray-200 hover:bg-red-50 hover:border-red-300 transition-colors group"
            >
              <div className="w-12 h-12 bg-red-100 rounded-lg flex items-center justify-center group-hover:bg-red-200 transition-colors">
                <Crown className="w-6 h-6 text-red-600" />
              </div>
              <span className="text-sm font-medium text-gray-700 text-center">Bảng Xếp Hạng</span>
            </Link>

            <Link
              href="/locations"
              className="flex flex-col items-center gap-3 p-4 rounded-lg border border-gray-200 hover:bg-purple-50 hover:border-purple-300 transition-colors group"
            >
              <div className="w-12 h-12 bg-purple-100 rounded-lg flex items-center justify-center group-hover:bg-purple-200 transition-colors">
                <MapPin className="w-6 h-6 text-purple-600" />
              </div>
              <span className="text-sm font-medium text-gray-700 text-center">Tra cứu điểm</span>
            </Link>
          </div>
        </div>

        {/* Mini Leaderboard ĐÃ UPDATE LẤY API */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-4">
            <div className="flex items-center gap-2">
              <Crown className="w-5 h-5 text-yellow-600" />
              <h2 className="text-lg font-semibold text-gray-900">Bảng Xếp Hạng</h2>
            </div>
            <Link
              href="/leaderboard"
              className="text-emerald-600 hover:text-emerald-700 text-sm font-medium"
            >
              Xem tất cả →
            </Link>
          </div>

          <div className="space-y-3">
            {loadingLeaders ? (
              <div className="text-center py-6">
                <div className="inline-block animate-spin rounded-full h-6 w-6 border-b-2 border-emerald-600"></div>
              </div>
            ) : topLeaders.length > 0 ? (
              topLeaders.map((leader) => {
                const style = getRankStyle(leader.rank);
                const isMe = user?.id === leader.id; // Check xem có phải user hiện tại không
                return (
                  <div key={leader.id} className={`flex items-center justify-between p-3 rounded-lg border transition-colors ${style.bg} ${isMe ? 'ring-2 ring-emerald-500 ring-offset-1' : ''}`}>
                    <div className="flex items-center gap-3">
                      {style.icon}
                      <div>
                        <div className="flex items-center gap-2">
                          <p className="font-medium text-gray-900">{leader.name}</p>
                          {isMe && <span className="bg-emerald-500 text-white text-[10px] px-1.5 py-0.5 rounded uppercase font-bold">Bạn</span>}
                        </div>
                        <p className="text-xs text-gray-600">{leader.level}</p>
                      </div>
                    </div>
                    <span className={`font-bold ${style.text}`}>{formatNumber(leader.points)} đ</span>
                  </div>
                );
              })
            ) : (
              <div className="text-center text-gray-500 py-4 text-sm">Chưa có dữ liệu xếp hạng</div>
            )}
          </div>
        </div>

        {/* Recent Reports */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-4">
            <h2 className="text-lg font-semibold text-gray-900">Báo cáo gần đây</h2>
            <Link
              href="/citizen/reports"
              className="text-emerald-600 hover:text-emerald-700 text-sm font-medium"
            >
              Xem tất cả →
            </Link>
          </div>

          <div className="space-y-3">
            {loadingReports ? (
              <div className="text-center py-6">
                <div className="inline-block animate-spin rounded-full h-6 w-6 border-b-2 border-emerald-600"></div>
              </div>
            ) : recentReports.length > 0 ? (
              recentReports.map((report) => (
                <div key={report.id} className="flex items-center justify-between p-4 bg-gray-50 rounded-lg">
                  <div className="flex items-center gap-4">
                    <div className="w-10 h-10 bg-white rounded-lg flex items-center justify-center border border-gray-200">
                      <FileText className="w-5 h-5 text-gray-600" />
                    </div>
                    <div>
                      <p className="font-medium text-gray-900">{report.type}</p>
                      <p className="text-sm text-gray-600">{report.date}</p>
                    </div>
                  </div>
                  <div className="flex items-center gap-3">
                    {report.points && (
                      <span className="text-sm font-semibold text-emerald-600">+{report.points} điểm</span>
                    )}
                    <span className={`px-2 py-1 rounded-full text-xs font-medium ${getStatusColor(report.status)}`}>
                      {getStatusLabel(report.status)}
                    </span>
                  </div>
                </div>
              ))
            ) : (
              <div className="text-center text-gray-500 py-4 text-sm">Chưa có báo cáo nào</div>
            )}
          </div>
        </div>

        {/* Achievement Section */}
        <div className="bg-gradient-to-r from-emerald-50 to-blue-50 rounded-xl p-6 border border-emerald-200">
          <div className="flex items-center gap-3 mb-4">
            <Award className="w-6 h-6 text-emerald-600" />
            <h2 className="text-lg font-semibold text-gray-900">Thành tích của bạn</h2>
          </div>
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div className="bg-white rounded-lg p-4">
              <div className="flex items-center gap-2 mb-2">
                <Target className="w-4 h-4 text-emerald-600" />
                <span className="font-medium text-gray-900">Chiến thần xanh</span>
              </div>
              <p className="text-sm text-gray-600">Top 10 người dùng tích cực tháng này</p>
            </div>
            <div className="bg-white rounded-lg p-4">
              <div className="flex items-center gap-2 mb-2">
                <Users className="w-4 h-4 text-blue-600" />
                <span className="font-medium text-gray-900">Siêu phân loại</span>
              </div>
              <p className="text-sm text-gray-600">Phân loại đúng 95% báo cáo</p>
            </div>
            <div className="bg-white rounded-lg p-4">
              <div className="flex items-center gap-2 mb-2">
                <Trophy className="w-4 h-4 text-yellow-600" />
                <span className="font-medium text-gray-900">Thành viên vàng</span>
              </div>
              <p className="text-sm text-gray-600">Đạt 1000 điểm thưởng</p>
            </div>
          </div>
        </div>
      </div>

      {/* Floating Action Button */}
      <Link
        href="/citizen/create-report"
        className="fixed bottom-6 right-6 w-14 h-14 bg-emerald-600 text-white rounded-full shadow-lg hover:bg-emerald-700 transition-colors flex items-center justify-center group z-40"
      >
        <PlusCircle className="w-6 h-6 group-hover:rotate-90 transition-transform duration-300" />
      </Link>
    </div>
  );
}