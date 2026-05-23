"use client";
import React, { useState, useEffect } from "react";
import {
  Map,
  Trash2,
  TrendingUp,
  Loader2,
  AlertCircle,
  FileText,
  Clock,
  CheckCircle2,
  PieChart as PieChartIcon,
  Activity
} from "lucide-react";
import {
  BarChart,
  Bar,
  XAxis,
  YAxis,
  CartesianGrid,
  Tooltip,
  ResponsiveContainer,
  PieChart,
  Pie,
  Cell,
  LineChart,
  Line,
} from "recharts";
import { API_CONFIG } from "@/lib/api/config";

const COLORS = ["#0AA468", "#F59E0B", "#3B82F6", "#8B5CF6", "#EF4444", "#10B981"];

// Types matching C# DTOs
interface WasteByAreaDto {
  area: string;
  count: number;
  weightKg: number;
}

interface WasteByTypeDto {
  type: string;
  count: number;
  weightKg: number;
  percentage: number;
}

interface MonthlyTrendDto {
  month: string;
  reportCount: number;
  weightKg: number;
}

interface ReportAnalyticsDto {
  totalReports: number;
  acceptedReports: number;
  pendingReports: number;
  rejectedReports: number;
  collectedReports: number;
  reportsByCategory: Record<string, number>;
  averageReportsPerDay: number;
  wasteByArea: WasteByAreaDto[];
  wasteByType: WasteByTypeDto[];
  monthlyTrends: MonthlyTrendDto[];
}

export const WasteAnalytics: React.FC = () => {
  const [data, setData] = useState<ReportAnalyticsDto | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchAnalytics = async () => {
      try {
        setLoading(true);
        
        // Get token from localStorage
        const token = localStorage.getItem("token");
        if (!token) {
          throw new Error("Không tìm thấy token xác thực");
        }

        const response = await fetch(`${API_CONFIG.BASE_URL}/admin/analytics/reports`, {
          headers: {
            "Authorization": `Bearer ${token}`,
            "Content-Type": "application/json"
          }
        });

        if (!response.ok) {
          throw new Error(`HTTP error! status: ${response.status}`);
        }

        const json = await response.json();
        const apiData = json.data ? json.data : json;

        setData(apiData);
        setError(null);
      } catch (err) {
        console.error("API Error:", err);
        setError("Không thể tải dữ liệu thống kê. Vui lòng kiểm tra API.");
      } finally {
        setLoading(false);
      }
    };

    fetchAnalytics();
  }, []);

  if (loading) {
    return (
      <div className="flex flex-col items-center justify-center h-[70vh] animate-in fade-in duration-500">
        <div className="bg-white p-6 rounded-full shadow-sm mb-4">
          <Loader2 className="animate-spin text-emerald-600" size={32} />
        </div>
        <span className="text-gray-500 font-medium">Đang tổng hợp dữ liệu...</span>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex flex-col items-center justify-center h-[70vh] animate-in fade-in duration-500">
        <div className="bg-red-50 p-6 rounded-full mb-4">
          <AlertCircle className="text-red-500" size={32} />
        </div>
        <span className="text-red-600 font-medium text-lg">Lỗi tải dữ liệu</span>
        <span className="text-gray-500 mt-2">{error}</span>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="flex items-center justify-center h-[70vh]">
        <span className="text-gray-400 font-medium">Không có dữ liệu</span>
      </div>
    );
  }

  // Format data for charts
  const areaChartData = data.wasteByArea.map(item => ({
    name: item.area,
    reports: item.count,
    weight: item.weightKg,
  }));

  const typeChartData = data.wasteByType.map(item => ({
    name: item.type,
    reports: item.count,
    weight: item.weightKg,
    percentage: item.percentage,
  }));

  const trendChartData = data.monthlyTrends.map(item => ({
    month: item.month,
    reports: item.reportCount,
    weight: item.weightKg,
  }));

  return (
    <div className="space-y-6 animate-in fade-in duration-700">
      {/* Header - Đã xóa tiêu đề, chỉ giữ đồng hồ cập nhật nằm gọn bên phải */}
      <div className="flex justify-end mt-2">
        <div className="bg-white px-4 py-2 rounded-xl border border-gray-200 shadow-sm inline-block">
          <p className="text-sm text-gray-500 font-medium flex items-center gap-2">
            <Clock size={16} className="text-emerald-600"/> Cập nhật: {new Date().toLocaleTimeString("vi-VN")}
          </p>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-6">
        <div className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm hover:shadow-lg hover:-translate-y-1 transition-all duration-300 group">
          <div className="flex items-center justify-between mb-4">
            <p className="text-gray-500 text-sm font-bold uppercase tracking-wider">Tổng báo cáo</p>
            <div className="w-12 h-12 bg-blue-50 rounded-2xl flex items-center justify-center group-hover:bg-blue-500 transition-colors duration-300">
              <FileText size={24} className="text-blue-500 group-hover:text-white transition-colors duration-300" />
            </div>
          </div>
          <p className="text-4xl font-extrabold text-gray-900">{data.totalReports.toLocaleString()}</p>
        </div>

        <div className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm hover:shadow-lg hover:-translate-y-1 transition-all duration-300 group">
          <div className="flex items-center justify-between mb-4">
            <p className="text-gray-500 text-sm font-bold uppercase tracking-wider">Đã thu gom</p>
            <div className="w-12 h-12 bg-emerald-50 rounded-2xl flex items-center justify-center group-hover:bg-emerald-500 transition-colors duration-300">
              <CheckCircle2 size={24} className="text-emerald-500 group-hover:text-white transition-colors duration-300" />
            </div>
          </div>
          <p className="text-4xl font-extrabold text-gray-900">{data.collectedReports.toLocaleString()}</p>
        </div>

        <div className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm hover:shadow-lg hover:-translate-y-1 transition-all duration-300 group">
          <div className="flex items-center justify-between mb-4">
            <p className="text-gray-500 text-sm font-bold uppercase tracking-wider">Đang chờ</p>
            <div className="w-12 h-12 bg-amber-50 rounded-2xl flex items-center justify-center group-hover:bg-amber-500 transition-colors duration-300">
              <Clock size={24} className="text-amber-500 group-hover:text-white transition-colors duration-300" />
            </div>
          </div>
          <p className="text-4xl font-extrabold text-gray-900">{data.pendingReports.toLocaleString()}</p>
        </div>

        <div className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm hover:shadow-lg hover:-translate-y-1 transition-all duration-300 group">
          <div className="flex items-center justify-between mb-4">
            <p className="text-gray-500 text-sm font-bold uppercase tracking-wider">Trung bình / ngày</p>
            <div className="w-12 h-12 bg-purple-50 rounded-2xl flex items-center justify-center group-hover:bg-purple-500 transition-colors duration-300">
              <TrendingUp size={24} className="text-purple-500 group-hover:text-white transition-colors duration-300" />
            </div>
          </div>
          <p className="text-4xl font-extrabold text-gray-900">{data.averageReportsPerDay.toFixed(1)}</p>
        </div>
      </div>

      {/* Charts Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Waste by Area Chart */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6">
          <div className="mb-8 flex items-center gap-3">
            <div className="bg-gray-50 p-2 rounded-lg">
              <Map className="text-emerald-600" size={20} />
            </div>
            <div>
              <h2 className="text-lg font-bold text-gray-900">Thống Kê Theo Khu Vực</h2>
              <p className="text-sm text-gray-500 mt-0.5">Phân bố báo cáo rác theo quận/huyện</p>
            </div>
          </div>

          <div className="h-80 w-full">
            {areaChartData.length > 0 ? (
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={areaChartData} margin={{ top: 5, right: 30, left: 0, bottom: 5 }}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f3f4f6" />
                  <XAxis dataKey="name" stroke="#9ca3af" angle={-45} textAnchor="end" height={80} tick={{ fontSize: 12 }} />
                  <YAxis stroke="#9ca3af" tick={{ fontSize: 12 }} />
                  <Tooltip
                    cursor={{ fill: '#f9fafb' }}
                    contentStyle={{
                      borderRadius: "12px",
                      border: "none",
                      boxShadow: "0 10px 15px -3px rgb(0 0 0 / 0.1)",
                      backgroundColor: "#fff",
                      fontWeight: 600
                    }}
                  />
                  <Bar dataKey="reports" fill="#0AA468" name="Số báo cáo" radius={[4, 4, 0, 0]} />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-full flex items-center justify-center text-gray-400">Không có dữ liệu</div>
            )}
          </div>
        </div>

        {/* Waste by Type Chart */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 flex flex-col">
          <div className="mb-8 flex items-center gap-3">
            <div className="bg-gray-50 p-2 rounded-lg">
              <PieChartIcon className="text-emerald-600" size={20} />
            </div>
            <div>
              <h2 className="text-lg font-bold text-gray-900">Phân Loại Rác Thải</h2>
              <p className="text-sm text-gray-500 mt-0.5">Tỷ lệ rác thải theo vật liệu</p>
            </div>
          </div>

          <div className="h-64 w-full grow flex items-center justify-center">
            {typeChartData.length > 0 ? (
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={typeChartData}
                    cx="50%"
                    cy="50%"
                    innerRadius={70}
                    outerRadius={110}
                    paddingAngle={5}
                    dataKey="reports"
                    stroke="none"
                  >
                    {typeChartData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip
                    contentStyle={{
                      borderRadius: "12px",
                      border: "none",
                      boxShadow: "0 10px 15px -3px rgb(0 0 0 / 0.1)",
                      backgroundColor: "#fff",
                      fontWeight: 600
                    }}
                  />
                </PieChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-full flex items-center justify-center text-gray-400">Không có dữ liệu</div>
            )}
          </div>

          {/* Legend for Waste Types */}
          {typeChartData.length > 0 && (
            <div className="mt-6 pt-6 border-t border-gray-50 grid grid-cols-2 sm:grid-cols-3 gap-y-3 gap-x-2">
              {typeChartData.map((entry, index) => (
                <div key={entry.name} className="flex items-center gap-2">
                  <div
                    className="w-3 h-3 rounded-full shrink-0"
                    style={{ backgroundColor: COLORS[index % COLORS.length] }}
                  ></div>
                  <span className="text-sm text-gray-600 truncate" title={entry.name}>
                    {entry.name} <strong className="text-gray-900 ml-1">{entry.reports}</strong>
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Monthly Trends Chart */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 lg:col-span-2">
          <div className="mb-8 flex items-center gap-3">
            <div className="bg-gray-50 p-2 rounded-lg">
              <Activity className="text-emerald-600" size={20} />
            </div>
            <div>
              <h2 className="text-lg font-bold text-gray-900">Xu Hướng Thời Gian</h2>
              <p className="text-sm text-gray-500 mt-0.5">Biểu đồ lượng báo cáo rác theo từng tháng</p>
            </div>
          </div>

          <div className="h-80 w-full">
            {trendChartData.length > 0 ? (
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={trendChartData} margin={{ top: 5, right: 30, left: 0, bottom: 5 }}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#f3f4f6" />
                  <XAxis dataKey="month" stroke="#9ca3af" tick={{ fontSize: 12 }} />
                  <YAxis stroke="#9ca3af" tick={{ fontSize: 12 }} />
                  <Tooltip
                    contentStyle={{
                      borderRadius: "12px",
                      border: "none",
                      boxShadow: "0 10px 15px -3px rgb(0 0 0 / 0.1)",
                      backgroundColor: "#fff",
                      fontWeight: 600
                    }}
                  />
                  <Line
                    type="monotone"
                    dataKey="reports"
                    name="Số báo cáo"
                    stroke="#0AA468"
                    strokeWidth={4}
                    dot={{ r: 5, fill: "#fff", stroke: "#0AA468", strokeWidth: 3 }}
                    activeDot={{ r: 8 }}
                  />
                </LineChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-full flex items-center justify-center text-gray-400">Không có dữ liệu</div>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};