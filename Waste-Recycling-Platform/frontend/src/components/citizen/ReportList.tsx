"use client";
import React, { useState, useEffect } from "react";
import { Clock, CheckCircle2, Truck, AlertCircle, MessageSquare, MapPin } from "lucide-react";
import { reportApi } from "../../lib/api/reportApi";
import { useAuth } from "@/contexts/AuthContext";
import { useToast } from "@/components/ui/Toast";
import { ReportDetailModal } from "./ReportDetailModal";

type ReportStatus = "Pending" | "Accepted" | "Assigned" | "Collected";

const StatusBadge: React.FC<{ status: string }> = ({ status }) => {
  switch (status) {
    case "Pending":
      return <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-amber-100 text-amber-800"><Clock size={14} /> Chờ Duyệt</span>;
    case "Accepted":
      return <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-blue-100 text-blue-800"><CheckCircle2 size={14} /> Đã Nhận</span>;
    case "Assigned":
      return <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-purple-100 text-purple-800"><Truck size={14} /> Đang Đến Trạm</span>;
    case "Collected":
      return <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-emerald-100 text-emerald-800"><CheckCircle2 size={14} /> Hoàn Thành</span>;
    default:
      return <span className="inline-flex items-center gap-1.5 px-3 py-1 rounded-full text-xs font-semibold bg-gray-100 text-gray-800">{status}</span>;
  }
};

export const ReportList: React.FC = () => {
  const [feedbackOpen, setFeedbackOpen] = useState<string | null>(null);
  const [selectedReportId, setSelectedReportId] = useState<string | null>(null);
  const [feedbackTexts, setFeedbackTexts] = useState<Record<string,string>>({});
  const [reports, setReports] = useState<any[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const { token } = useAuth();
  const { addToast } = useToast();

  useEffect(() => {
    loadReports();
  }, []);

  const loadReports = async () => {
    setIsLoading(true);
    setError(null);
    try {
      const res = await reportApi.getMyReports();
      setReports(res.reports);
    } catch (err: any) {
      console.error(err);
      setError("Không thể tải dữ liệu lịch sử. Vui lòng thử lại.");
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex justify-between items-end mb-6">
        <div>
          <h2 className="text-2xl font-bold text-gray-800">Nhật Ký Thu Gom</h2>
          <p className="text-gray-500 mt-1">Theo dõi trạng thái các yêu cầu thu gom rác của bạn.</p>
        </div>
        <button 
          onClick={loadReports}
          className="text-sm font-medium text-emerald-600 hover:text-emerald-700 bg-emerald-50 px-3 py-1.5 rounded-lg transition-colors"
        >
          Làm mới
        </button>
      </div>

      {isLoading ? (
        <div className="text-center py-10 text-gray-500">
          <div className="w-8 h-8 border-4 border-emerald-500 border-t-transparent rounded-full animate-spin mx-auto mb-4"></div>
          Đang tải lịch sử báo cáo...
        </div>
      ) : error ? (
        <div className="p-4 bg-red-50 text-red-600 rounded-xl text-center border border-red-100">
          {error}
        </div>
      ) : reports.length === 0 ? (
        <div className="text-center py-10 text-gray-500 bg-gray-50 rounded-2xl border border-gray-100">
          <Clock size={32} className="mx-auto text-gray-400 mb-3" />
          <p>Bạn chưa có báo cáo thu gom nào.</p>
        </div>
      ) : (
        <div className="grid gap-4">
          {reports.map((report) => (
            <div 
              key={report.id} 
              onClick={() => setSelectedReportId(report.id)}
              className="bg-white border border-gray-100 p-5 rounded-2xl shadow-sm hover:shadow-md transition-all cursor-pointer hover:border-emerald-200 group"
            >
              <div className="flex flex-col md:flex-row md:items-center justify-between gap-4">
                <div className="flex-1 space-y-3">
                  <div className="flex items-center justify-between md:justify-start gap-4">
                    <span className="text-sm font-bold text-gray-900 bg-gray-100 px-2 py-1 rounded group-hover:bg-emerald-50 group-hover:text-emerald-700 transition-colors">
                      {report.id.substring(0, 8).toUpperCase()}
                    </span>
                    <StatusBadge status={report.status} />
                  </div>
                  
                  <div className="grid grid-cols-2 lg:grid-cols-4 gap-4 text-sm">
                    <div>
                      <span className="block text-gray-500 text-xs mb-1">Thời gian tạo</span>
                      <span className="font-medium text-gray-800">{new Date(report.createdAt).toLocaleString("vi-VN")}</span>
                    </div>
                    <div>
                      <span className="block text-gray-500 text-xs mb-1">Phân loại</span>
                      <span className="font-medium text-gray-800">{report.categoryName}</span>
                    </div>
                    <div>
                      <span className="block text-gray-500 text-xs mb-1">Đính kèm</span>
                      <span className="font-medium text-gray-800">{report.imageCount} Ảnh</span>
                    </div>
                    <div className="flex items-center gap-1.5 text-gray-600">
                      <MapPin size={16} className="text-emerald-500 shrink-0" />
                      <span className="truncate" title={report.address}>{report.address}</span>
                    </div>
                  </div>
                </div>

                <div className="flex shrink-0 gap-3 border-t md:border-t-0 pt-4 md:pt-0 border-gray-100 md:border-l md:pl-6">
                  {report.status !== "Pending" && (
                    <button
                      onClick={(e) => {
                        e.stopPropagation();
                        setFeedbackOpen(feedbackOpen === report.id ? null : report.id);
                      }}
                      className="flex items-center gap-2 text-sm font-medium text-gray-600 hover:text-red-600 transition-colors px-3 py-2 rounded-lg hover:bg-red-50 z-10"
                      title="Gửi Khiếu Nại / Phản Hồi"
                    >
                      <AlertCircle size={18} /> Khiếu Nại
                    </button>
                  )}
                </div>
              </div>

              {/* Feedback Form Expandable */}
              {feedbackOpen === report.id && (
                <div 
                  className="mt-4 pt-4 border-t border-gray-100 bg-gray-50/50 rounded-xl p-4 animate-in fade-in slide-in-from-top-2"
                  onClick={(e) => e.stopPropagation()}
                >
                  <h4 className="text-sm font-bold text-gray-800 flex items-center gap-2 mb-3">
                    <MessageSquare size={16} className="text-red-500" /> Gửi Phản Hồi
                  </h4>
                  <textarea 
                    className="w-full text-sm rounded-lg border-gray-200 border p-3 focus:ring-2 focus:ring-red-500 outline-none resize-none bg-white"
                    rows={2}
                    placeholder="Ví dụ: Người thu gom đến trễ hơn cam kết, thái độ không tốt..."
                    value={feedbackTexts[report.id] ?? ''}
                    onChange={(e) => setFeedbackTexts(prev => ({ ...prev, [report.id]: e.target.value }))}
                  ></textarea>
                  <div className="flex justify-end gap-2 mt-3">
                    <button onClick={() => setFeedbackOpen(null)} className="px-4 py-2 text-sm font-medium text-gray-600 hover:bg-gray-200 rounded-lg transition-colors">Hủy</button>
                    <button
                      onClick={async () => {
                        const content = feedbackTexts[report.id] ?? '';
                        if (!content || content.trim().length === 0) {
                          addToast('Nội dung khiếu nại/ phản hồi không được để trống', 'error');
                          return;
                        }
                        if (!token) {
                          addToast('Bạn cần đăng nhập để gửi khiếu nại', 'error');
                          return;
                        }
                        try {
                          const res = await fetch(`${process.env.NEXT_PUBLIC_API_URL || 'http://localhost:8080/api'}/complaints`, {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json', Authorization: `Bearer ${token}` },
                            body: JSON.stringify({ content, reportId: report.id }),
                          });
                          if (!res.ok) {
                            const body = await res.json().catch(() => ({}));
                            addToast(body?.message || 'Gửi khiếu nại thất bại', 'error');
                            return;
                          }
                          addToast('Gửi khiếu nại thành công', 'success');
                          setFeedbackOpen(null);
                          // optionally refresh reports or complaints
                          loadReports();
                        } catch (e) {
                          console.error(e);
                          addToast('Lỗi khi gửi khiếu nại', 'error');
                        }
                      }}
                      className="px-4 py-2 text-sm font-medium text-white bg-red-600 hover:bg-red-700 rounded-lg transition-colors shadow-sm shadow-red-500/30"
                    >
                      Gửi Phản Hồi
                    </button>
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Render Modal if a report is selected */}
      {selectedReportId && (
        <ReportDetailModal 
          reportId={selectedReportId} 
          onClose={() => setSelectedReportId(null)} 
        />
      )}
    </div>
  );
};
