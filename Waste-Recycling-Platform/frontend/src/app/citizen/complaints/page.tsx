"use client";
import React, { useState, useEffect } from "react";
import Link from "next/link";
import { ArrowLeft, MessageSquare, Clock, CheckCircle2, AlertCircle, XCircle, ChevronDown, ChevronUp, Building2, Shield } from "lucide-react";
import { complaintApi, Complaint } from "@/lib/api/complaintApi";

export default function ComplaintsPage() {
  const [complaints, setComplaints] = useState<Complaint[]>([]);
  const [filter, setFilter] = useState<string>("all");
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [expandedId, setExpandedId] = useState<string | null>(null);
  const [escalatingId, setEscalatingId] = useState<string | null>(null);
  const [escalateReason, setEscalateReason] = useState("");
  const [escalating, setEscalating] = useState(false);

  useEffect(() => {
    loadComplaints();
  }, []);

  const loadComplaints = async () => {
    try {
      setLoading(true);
      const status = filter === "all" ? undefined : filter;
      const data = await complaintApi.getMyComplaints(1, 50, status);
      setComplaints(data.items);
    } catch (err) {
      setError("Không thể tải danh sách khiếu nại");
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleEscalate = async (complaintId: string) => {
    if (!escalateReason.trim()) {
      alert("Vui lòng nhập lý do chuyển lên Admin");
      return;
    }
    try {
      setEscalating(true);
      await complaintApi.escalateComplaint(complaintId, escalateReason);
      alert("Đã chuyển khiếu nại lên Admin");
      setEscalatingId(null);
      setEscalateReason("");
      loadComplaints();
    } catch (err: any) {
      console.error("Escalate error:", err);
      alert("Không thể chuyển khiếu nại: " + (err?.message || err?.response?.data?.message || "Unknown error"));
    } finally {
      setEscalating(false);
    }
  };

  useEffect(() => {
    loadComplaints();
  }, [filter]);

  const getStatusConfig = (status: string) => {
    switch (status) {
      case "Open":
        return { icon: AlertCircle, color: "text-amber-600", bg: "bg-amber-100", label: "Chờ xử lý" };
      case "InProgress":
        return { icon: Clock, color: "text-blue-600", bg: "bg-blue-100", label: "Đang xử lý" };
      case "Resolved":
        return { icon: CheckCircle2, color: "text-emerald-600", bg: "bg-emerald-100", label: "Đã giải quyết" };
      case "Rejected":
        return { icon: XCircle, color: "text-red-600", bg: "bg-red-100", label: "Bị từ chối" };
      case "Escalated":
        return { icon: Shield, color: "text-purple-600", bg: "bg-purple-100", label: "Chuyển Admin" };
      default:
        return { icon: AlertCircle, color: "text-gray-600", bg: "bg-gray-100", label: status };
    }
  };

  return (
    <div className="min-h-screen bg-gray-50/50 py-8">
      <div className="max-w-5xl mx-auto px-4 sm:px-6 lg:px-8 space-y-6">
        
        {/* Header */}
        <div className="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
          <div className="flex items-center gap-4">
            <Link href="/citizen/profile" className="p-2 hover:bg-white rounded-full transition-colors border border-transparent hover:border-gray-200 shadow-sm">
              <ArrowLeft size={24} className="text-gray-600" />
            </Link>
            <div>
              <h1 className="text-2xl sm:text-3xl font-bold text-gray-900 flex items-center gap-2">
                <MessageSquare className="text-emerald-500" /> Khiếu Nại & Hỗ Trợ
              </h1>
              <p className="text-sm text-gray-500 mt-1">Xem lại các khiếu nại và phản hồi của bạn</p>
            </div>
          </div>
          
          {/* Filter */}
          <div className="flex bg-white rounded-xl border border-gray-200 p-1 shadow-sm w-fit">
            <button 
              onClick={() => setFilter("all")}
              className={`px-4 py-2 rounded-lg text-sm font-semibold transition-colors ${filter === "all" ? "bg-gray-100 text-gray-900" : "text-gray-500 hover:text-gray-900"}`}
            >
              Tất cả
            </button>
            <button 
              onClick={() => setFilter("Open")}
              className={`px-4 py-2 rounded-lg text-sm font-semibold transition-colors ${filter === "Open" ? "bg-amber-100 text-amber-700" : "text-gray-500 hover:text-amber-600"}`}
            >
              Chờ xử lý
            </button>
            <button 
              onClick={() => setFilter("InProgress")}
              className={`px-4 py-2 rounded-lg text-sm font-semibold transition-colors ${filter === "InProgress" ? "bg-blue-100 text-blue-700" : "text-gray-500 hover:text-blue-600"}`}
            >
              Đang xử lý
            </button>
            <button 
              onClick={() => setFilter("Escalated")}
              className={`px-4 py-2 rounded-lg text-sm font-semibold transition-colors ${filter === "Escalated" ? "bg-red-100 text-red-700" : "text-gray-500 hover:text-red-600"}`}
            >
              Chuyển Admin
            </button>
            <button 
              onClick={() => setFilter("Resolved")}
              className={`px-4 py-2 rounded-lg text-sm font-semibold transition-colors ${filter === "Resolved" ? "bg-emerald-100 text-emerald-700" : "text-gray-500 hover:text-emerald-600"}`}
            >
              Đã giải quyết
            </button>
          </div>
        </div>

        {/* Info Banner */}
        <div className="bg-blue-50 border border-blue-200 rounded-xl p-4 flex items-start gap-3">
          <AlertCircle className="text-blue-500 shrink-0 mt-0.5" size={20} />
          <div>
            <p className="text-sm text-blue-800">
              <strong>Lưu ý:</strong> Để gửi khiếu nại mới, vui lòng vào trang 
              <Link href="/citizen/reports" className="underline font-semibold hover:text-blue-600"> Nhật ký thu gom</Link> 
              và nhấn nút &quot;Khiếu Nại&quot; trên báo cáo.
            </p>
          </div>
        </div>

        {/* Complaints List */}
        <div className="bg-white rounded-2xl shadow-sm border border-gray-100 overflow-hidden">
          {loading ? (
            <div className="p-8 text-center text-gray-500">
              <div className="animate-spin w-8 h-8 border-4 border-emerald-500 border-t-transparent rounded-full mx-auto mb-4"></div>
              Đang tải...
            </div>
          ) : error ? (
            <div className="p-8 text-center text-red-500">{error}</div>
          ) : complaints.length === 0 ? (
            <div className="p-8 text-center text-gray-500">
              <MessageSquare size={48} className="mx-auto text-gray-300 mb-3" />
              <p>Bạn chưa có khiếu nại nào.</p>
            </div>
          ) : (
            <div className="divide-y divide-gray-100">
              {complaints.map((complaint: Complaint) => {
                const statusConfig = getStatusConfig(complaint.status);
                const StatusIcon = statusConfig.icon;
                const isExpanded = expandedId === complaint.id;
                const hasResponse = complaint.enterpriseResponse || complaint.adminResponse;
                const hasEnterpriseReplied = complaint.status === "InProgress" || complaint.status === "Resolved" || complaint.status === "Escalated" || complaint.status === "Rejected";
                
                return (
                  <div key={complaint.id} className="p-6 hover:bg-gray-50/50 transition-colors">
                    <div className="flex items-start gap-4">
                      <div className={`p-3 rounded-xl ${statusConfig.bg} ${statusConfig.color}`}>
                        <StatusIcon size={24} />
                      </div>
                      <div className="flex-1 min-w-0">
                        <div className="flex items-center justify-between mb-2">
                          <span className="text-sm font-bold text-gray-900">
                            #{complaint.id.substring(0, 8).toUpperCase()}
                          </span>
                          <span className={`inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold ${statusConfig.bg} ${statusConfig.color}`}>
                            {statusConfig.label}
                          </span>
                        </div>
                        <p className="text-gray-800 mb-2">{complaint.content}</p>
                        <div className="flex items-center justify-between">
                          <div className="flex items-center gap-4 text-sm text-gray-500">
                            <span>Ngày gửi: {new Date(complaint.createdAt).toLocaleString('vi-VN')}</span>
                            {complaint.resolvedAt && (
                              <span className="text-emerald-600">
                                Giải quyết: {new Date(complaint.resolvedAt).toLocaleString('vi-VN')}
                              </span>
                            )}
                          </div>
                          {hasEnterpriseReplied && (
                            <button
                              onClick={() => setExpandedId(isExpanded ? null : complaint.id)}
                              className="flex items-center gap-1 text-sm text-emerald-600 hover:text-emerald-700 font-medium"
                            >
                              {isExpanded ? (
                                <>Thu gọn <ChevronUp size={16} /></>
                              ) : (
                                <>Xem phản hồi <ChevronDown size={16} /></>
                              )}
                            </button>
                          )}
                        </div>
                        
                        {/* Expanded Response Section */}
                        {isExpanded && hasEnterpriseReplied && (
                          <div className="mt-4 space-y-3">
                            {/* Enterprise Response */}
                            {complaint.enterpriseResponse ? (
                              <div className="bg-blue-50 border border-blue-200 rounded-xl p-4">
                                <div className="flex items-center gap-2 mb-2">
                                  <Building2 size={18} className="text-blue-600" />
                                  <h4 className="font-semibold text-blue-900">
                                    Phản hồi từ {complaint.enterpriseName || "Doanh nghiệp"}
                                  </h4>
                                  {complaint.enterpriseRespondedAt && (
                                    <span className="text-xs text-blue-600 ml-auto">
                                      {new Date(complaint.enterpriseRespondedAt).toLocaleString('vi-VN')}
                                    </span>
                                  )}
                                </div>
                                <p className="text-blue-800 text-sm leading-relaxed">
                                  {complaint.enterpriseResponse}
                                </p>
                              </div>
                            ) : (
                              <div className="bg-gray-50 border border-gray-200 rounded-xl p-4">
                                <div className="flex items-center gap-2 mb-2">
                                  <Building2 size={18} className="text-gray-600" />
                                  <h4 className="font-semibold text-gray-700">
                                    Phản hồi từ {complaint.enterpriseName || "Doanh nghiệp"}
                                  </h4>
                                </div>
                                <p className="text-gray-600 text-sm italic">
                                  Doanh nghiệp đã đóng khiếu nại mà không nhập phản hồi.
                                </p>
                              </div>
                            )}
                            
                            {/* Admin Response */}
                            {complaint.adminResponse && (
                              <div className="bg-purple-50 border border-purple-200 rounded-xl p-4">
                                <div className="flex items-center gap-2 mb-2">
                                  <Shield size={18} className="text-purple-600" />
                                  <h4 className="font-semibold text-purple-900">Phản hồi từ Admin</h4>
                                </div>
                                <p className="text-purple-800 text-sm leading-relaxed">
                                  {complaint.adminResponse}
                                </p>
                              </div>
                            )}
                            {(complaint.status === "InProgress" || complaint.status === "Resolved") && (
                              <div className="mt-4 pt-4 border-t border-gray-200">
                                {escalatingId === complaint.id ? (
                                  <div className="bg-amber-50 border border-amber-200 rounded-xl p-4">
                                    <h4 className="font-semibold text-amber-900 mb-2">Chuyển khiếu nại lên Admin</h4>
                                    <p className="text-amber-800 text-sm mb-3">
                                      Vui lòng nhập lý do bạn không đồng ý với cách giải quyết này.
                                    </p>
                                    <textarea
                                      className="w-full border border-amber-300 rounded-lg p-3 text-sm focus:ring-2 focus:ring-amber-500 focus:border-transparent"
                                      rows={3}
                                      placeholder="Nhập lý do chuyển lên Admin..."
                                      value={escalateReason}
                                      onChange={(e) => setEscalateReason(e.target.value)}
                                    />
                                    <div className="flex gap-2 mt-3">
                                      <button
                                        className="px-4 py-2 bg-amber-600 text-white rounded-lg text-sm hover:bg-amber-700 disabled:opacity-50"
                                        onClick={() => handleEscalate(complaint.id)}
                                        disabled={escalating || !escalateReason.trim()}
                                      >
                                        {escalating ? "Đang chuyển..." : "Xác nhận chuyển"}
                                      </button>
                                      <button
                                        className="px-4 py-2 bg-gray-300 text-gray-700 rounded-lg text-sm hover:bg-gray-400"
                                        onClick={() => { setEscalatingId(null); setEscalateReason(""); }}
                                      >
                                        Hủy
                                      </button>
                                    </div>
                                  </div>
                                ) : (
                                  <button
                                    className="flex items-center gap-2 text-amber-600 hover:text-amber-700 text-sm font-medium"
                                    onClick={() => setEscalatingId(complaint.id)}
                                  >
                                    <Shield size={16} />
                                    Không đồng ý? Chuyển lên Admin
                                  </button>
                                )}
                              </div>
                            )}
                          </div>
                        )}
                      </div>
                    </div>
                  </div>
                );
              })}
            </div>
          )}
        </div>
      </div>
    </div>
  );
}
