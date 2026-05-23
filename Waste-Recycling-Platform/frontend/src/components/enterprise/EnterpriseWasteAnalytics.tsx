"use client";
import React, { useState, useEffect } from "react";
import {
  BarChart3,
  MapPin,
  Trash2,
  TrendingUp,
  Loader2,
  AlertCircle,
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

export const EnterpriseWasteAnalytics: React.FC = () => {
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

        const response = await fetch(`${API_CONFIG.BASE_URL}/enterprise/analytics/reports`, {
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
      <div className="flex items-center justify-center h-64">
        <Loader2 className="animate-spin text-blue-500 mr-2" />
        <span className="text-gray-600">Đang tải dữ liệu thống kê...</span>
      </div>
    );
  }

  if (error) {
    return (
      <div className="flex items-center justify-center h-64">
        <AlertCircle className="text-red-500 mr-2" />
        <span className="text-red-600">{error}</span>
      </div>
    );
  }

  if (!data) {
    return (
      <div className="flex items-center justify-center h-64">
        <span className="text-gray-400">Không có dữ liệu</span>
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
    <div className="space-y-6">
      {/* Header */}
      <div className="flex flex-col sm:flex-row sm:items-center sm:justify-between gap-4">
        <div>
          <h1 className="text-3xl font-bold text-gray-900">Thống Kê Rác Thải</h1>
          <p className="text-gray-600 mt-2">
            Phân tích dữ liệu rác thải theo khu vực, loại và xu hướng thời gian cho doanh nghiệp
          </p>
        </div>
        <div className="text-right">
          <p className="text-sm text-gray-600 font-medium">
            Cập nhật: {new Date().toLocaleTimeString("vi-VN")}
          </p>
        </div>
      </div>

      {/* Summary Cards */}
      <div className="grid grid-cols-1 sm:grid-cols-2 lg:grid-cols-4 gap-4">
        <div className="bg-white rounded-xl border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-4">
            <div className="w-12 h-12 bg-blue-500 rounded-lg flex items-center justify-center">
              <Trash2 size={24} className="text-white" />
            </div>
          </div>
          <p className="text-gray-600 text-sm mb-2 font-medium">Tổng báo cáo</p>
          <p className="text-3xl font-bold text-gray-900">{data.totalReports.toLocaleString()}</p>
        </div>

        <div className="bg-white rounded-xl border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-4">
            <div className="w-12 h-12 bg-green-500 rounded-lg flex items-center justify-center">
              <BarChart3 size={24} className="text-white" />
            </div>
          </div>
          <p className="text-gray-600 text-sm mb-2 font-medium">Đã chấp nhận</p>
          <p className="text-3xl font-bold text-gray-900">{data.acceptedReports.toLocaleString()}</p>
        </div>

        <div className="bg-white rounded-xl border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-4">
            <div className="w-12 h-12 bg-yellow-500 rounded-lg flex items-center justify-center">
              <MapPin size={24} className="text-white" />
            </div>
          </div>
          <p className="text-gray-600 text-sm mb-2 font-medium">Đã thu gom</p>
          <p className="text-3xl font-bold text-gray-900">{data.collectedReports.toLocaleString()}</p>
        </div>

        <div className="bg-white rounded-xl border border-gray-200 p-6">
          <div className="flex items-center justify-between mb-4">
            <div className="w-12 h-12 bg-purple-500 rounded-lg flex items-center justify-center">
              <TrendingUp size={24} className="text-white" />
            </div>
          </div>
          <p className="text-gray-600 text-sm mb-2 font-medium">Tỷ lệ thu gom</p>
          <p className="text-3xl font-bold text-gray-900">
            {data.totalReports > 0 ? ((data.collectedReports / data.totalReports) * 100).toFixed(1) : 0}%
          </p>
        </div>
      </div>

      {/* Charts Grid */}
      <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
        {/* Waste by Area Chart */}
        <div className="bg-white rounded-xl border border-gray-200 p-6">
          <div className="mb-6">
            <h2 className="text-lg font-bold text-gray-900">📍 Thống Kê Rác Theo Khu Vực</h2>
            <p className="text-sm text-gray-600 mt-1">Phân bố báo cáo rác theo quận/huyện</p>
          </div>

          <div className="h-80 w-full">
            {areaChartData.length > 0 ? (
              <ResponsiveContainer width="100%" height="100%">
                <BarChart data={areaChartData} margin={{ top: 5, right: 30, left: 0, bottom: 5 }}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E5E7EB" />
                  <XAxis dataKey="name" stroke="#6B7280" angle={-45} textAnchor="end" height={80} />
                  <YAxis stroke="#6B7280" />
                  <Tooltip
                    contentStyle={{
                      borderRadius: "8px",
                      border: "none",
                      boxShadow: "0 4px 6px -1px rgb(0 0 0 / 0.1)",
                      backgroundColor: "#fff",
                    }}
                  />
                  <Bar dataKey="reports" fill="#0AA468" name="Số báo cáo" />
                </BarChart>
              </ResponsiveContainer>
            ) : (
              <div className="h-full flex items-center justify-center text-gray-400">Không có dữ liệu</div>
            )}
          </div>
        </div>

        {/* Waste by Type Chart */}
        <div className="bg-white rounded-xl border border-gray-200 p-6">
          <div className="mb-6">
            <h2 className="text-lg font-bold text-gray-900">♻️ Thống Kê Rác Theo Loại</h2>
            <p className="text-sm text-gray-600 mt-1">Phân loại rác thải theo loại vật liệu</p>
          </div>

          <div className="h-80 w-full">
            {typeChartData.length > 0 ? (
              <ResponsiveContainer width="100%" height="100%">
                <PieChart>
                  <Pie
                    data={typeChartData}
                    cx="50%"
                    cy="50%"
                    innerRadius={60}
                    outerRadius={100}
                    paddingAngle={5}
                    dataKey="reports"
                  >
                    {typeChartData.map((entry, index) => (
                      <Cell key={`cell-${index}`} fill={COLORS[index % COLORS.length]} />
                    ))}
                  </Pie>
                  <Tooltip
                    contentStyle={{
                      borderRadius: "8px",
                      border: "none",
                      boxShadow: "0 4px 6px -1px rgb(0 0 0 / 0.1)",
                      backgroundColor: "#fff",
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
            <div className="mt-6 space-y-2">
              {typeChartData.map((entry, index) => (
                <div key={entry.name} className="flex items-center gap-3">
                  <div
                    className="w-3 h-3 rounded-full shrink-0"
                    style={{ backgroundColor: COLORS[index % COLORS.length] }}
                  ></div>
                  <span className="text-sm text-gray-700">
                    {entry.name}: <strong className="text-gray-900">{entry.reports} báo cáo</strong>
                  </span>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* Monthly Trends Chart */}
        <div className="bg-white rounded-xl border border-gray-200 p-6 lg:col-span-2">
          <div className="mb-6">
            <h2 className="text-lg font-bold text-gray-900">📈 Xu Hướng Thời Gian</h2>
            <p className="text-sm text-gray-600 mt-1">Biểu đồ báo cáo rác theo tháng</p>
          </div>

          <div className="h-80 w-full">
            {trendChartData.length > 0 ? (
              <ResponsiveContainer width="100%" height="100%">
                <LineChart data={trendChartData} margin={{ top: 5, right: 30, left: 0, bottom: 5 }}>
                  <CartesianGrid strokeDasharray="3 3" vertical={false} stroke="#E5E7EB" />
                  <XAxis dataKey="month" stroke="#6B7280" />
                  <YAxis stroke="#6B7280" />
                  <Tooltip
                    contentStyle={{
                      borderRadius: "8px",
                      border: "none",
                      boxShadow: "0 4px 6px -1px rgb(0 0 0 / 0.1)",
                      backgroundColor: "#fff",
                    }}
                  />
                  <Line
                    type="monotone"
                    dataKey="reports"
                    name="Số báo cáo"
                    stroke="#0AA468"
                    strokeWidth={3}
                    dot={{ r: 4, fill: "#0AA468" }}
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
