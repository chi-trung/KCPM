"use client";
import React, { useState, useEffect } from "react";
import { API_CONFIG } from "@/lib/api/config";
import { Search, AlertCircle, CheckCircle, XCircle, MessageSquare, ShieldAlert } from "lucide-react";
import { ConfirmationModal, useConfirmation } from "../shared/ConfirmationModal";
import { ToastContainer, useToast } from "../shared/Toast";
// Import thêm modal chi tiết báo cáo để xem nhanh
import { ReportDetailModal } from "./ReportsManagement"; 

export const DisputesManagement: React.FC = () => {
  const [searchTerm, setSearchTerm] = useState("");
  const [filterStatus, setFilterStatus] = useState("all");
  const [disputes, setDisputes] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [actionLoading, setActionLoading] = useState(false);

  // State để xem nhanh báo cáo gốc
  const [viewingReportId, setViewingReportId] = useState<string | null>(null);

  const modal = useConfirmation();
  const toast = useToast();

  const fetchDisputes = async () => {
    try {
      setLoading(true);
      const token = localStorage.getItem("token") || "";
      let url = `${API_CONFIG.BASE_URL}/admin/complaints?page=1&pageSize=100`;
      
      if (searchTerm.trim() !== "") url += `&searchTerm=${encodeURIComponent(searchTerm)}`;
      if (filterStatus !== "all") url += `&status=${filterStatus}`;

      const response = await fetch(url, {
        headers: { "Authorization": `Bearer ${token}`, "Accept": "*/*" }
      });

      if (!response.ok) throw new Error("Lỗi khi tải dữ liệu");
      const json = await response.json();
      const apiData = json.data || [];

      const mapStatus = (statusNum: any) => {
        const s = statusNum?.toString().toLowerCase();
        if (s === "1" || s === "resolved") return "resolved";
        if (s === "2" || s === "rejected") return "rejected";
        return "pending";
      };

      const formattedDisputes = apiData.map((item: any) => ({
        id: item.id,
        number: `#C-${item.id.substring(0, 6).toUpperCase()}`,
        citizen: item.citizenName || item.citizen?.fullName || "Người dùng ẩn danh",
        reportId: item.reportId, // Giữ ID gốc để gọi modal
        reportNumber: item.reportId ? `#R-${item.reportId.substring(0, 6).toUpperCase()}` : "Không rõ",
        status: mapStatus(item.status),
        createdAt: new Date(item.createdAt).toLocaleString("vi-VN"),
        content: item.content || "Không có nội dung chi tiết",
        adminResponse: item.adminResponse || "",
        // Lịch sử phản hồi
        enterpriseResponse: item.enterpriseResponse || null,
        enterpriseRespondedAt: item.enterpriseRespondedAt ? new Date(item.enterpriseRespondedAt).toLocaleString("vi-VN") : null,
        escalationReason: item.escalationReason || null,
      }));

      setDisputes(formattedDisputes);
    } catch (error) {
      console.error("Lỗi fetch disputes:", error);
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const delayDebounceFn = setTimeout(() => { fetchDisputes(); }, 500);
    return () => clearTimeout(delayDebounceFn);
  }, [searchTerm, filterStatus]);

  const handleAction = async (id: string, action: "resolve" | "reject") => {
    const actionTitle = action === "resolve" ? "Giải Quyết Khiếu Nại" : "Từ Chối Khiếu Nại";
    
    const adminResponse = await modal.prompt({
      title: actionTitle,
      message: `Nhập phản hồi của bạn cho khiếu nại này:`,
      placeholder: "Nhập nội dung phản hồi...",
      confirmText: action === "resolve" ? "Xác Nhận Giải Quyết" : "Từ Chối",
      cancelText: "Hủy",
    });

    if (adminResponse === null) return;

    try {
      setActionLoading(true);
      const token = localStorage.getItem("token") || "";
      const url = `${API_CONFIG.BASE_URL}/admin/complaints/${id}/${action}`;

      const response = await fetch(url, {
        method: "POST",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json"
        },
        body: JSON.stringify({ adminResponse })
      });

      if (response.ok) {
        toast.success(`Đã xử lý khiếu nại thành công!`);
        fetchDisputes();
      } else {
        toast.error("Thao tác thất bại.");
      }
    } catch (error) {
      toast.error("Lỗi kết nối!");
    } finally {
      setActionLoading(false);
    }
  };

  const getStatusStyle = (status: string) => {
    switch (status) {
      case "pending": return "bg-amber-100 text-amber-700 border-amber-200";
      case "resolved": return "bg-emerald-100 text-emerald-700 border-emerald-200";
      case "rejected": return "bg-red-100 text-red-700 border-red-200";
      default: return "bg-gray-100 text-gray-700 border-gray-200";
    }
  };

  return (
    <div className="space-y-6 animate-in fade-in duration-500 pt-2">
      {/* Search & Filter Bar (Giữ nguyên của ông) */}
      <div className="bg-white rounded-2xl shadow-sm border border-gray-100 p-6 flex flex-col sm:flex-row gap-4 items-end">
          <div className="flex-1 w-full relative">
            <label htmlFor="dispute-search" className="block text-sm font-semibold text-gray-700 mb-2">Tìm kiếm khiếu nại</label>
            <div className="relative">
              <Search size={18} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
              <input
                id="dispute-search"
                type="text"
                placeholder="Nhập mã khiếu nại, tên người dân..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="w-full pl-10 pr-4 py-2.5 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-emerald-500 transition-all text-sm"
              />
            </div>
          </div>
          <div className="w-full sm:w-64">
            <label htmlFor="dispute-filter-status" className="block text-sm font-semibold text-gray-700 mb-2">Trạng thái</label>
            <select
              id="dispute-filter-status"
              value={filterStatus}
              onChange={(e) => setFilterStatus(e.target.value)}
              className="w-full px-4 py-2.5 border border-gray-200 rounded-xl focus:outline-none focus:ring-2 focus:ring-emerald-500 appearance-none bg-white cursor-pointer transition-all text-sm font-medium text-gray-700"
            >
              <option value="all">Tất Cả Trạng Thái</option>
              <option value="pending">Chờ Xử Lý</option>
              <option value="resolved">Đã Giải Quyết</option>
              <option value="rejected">Bị Từ Chối</option>
            </select>
          </div>
      </div>

      {/* Disputes List */}
      {loading ? (
        <div className="p-16 flex flex-col items-center justify-center bg-white rounded-2xl border border-gray-100">
          <div className="inline-block animate-spin rounded-full h-8 w-8 border-4 border-gray-100 border-t-emerald-600"></div>
          <p className="mt-4 text-gray-500 font-medium text-sm">Đang đồng bộ dữ liệu...</p>
        </div>
      ) : (
        <div className="space-y-4">
          {disputes.map((dispute) => (
            <div key={dispute.id} className="bg-white rounded-2xl border border-gray-100 p-6 shadow-sm hover:shadow-md transition-shadow group">
              <div className="flex items-start justify-between mb-5">
                <div>
                  <p className="font-bold text-lg text-gray-900">{dispute.number}</p>
                  <p className="text-sm text-gray-500 mt-0.5">
                    Báo cáo gốc:{' '}
                    <button 
                      type="button"
                      onClick={() => setViewingReportId(dispute.reportId)}
                      className="font-bold text-emerald-600 hover:text-emerald-700 underline"
                    >
                      {dispute.reportNumber}
                    </button>
                  </p>
                </div>
                <div className={`px-3 py-1.5 rounded-full text-xs font-bold border ${getStatusStyle(dispute.status)}`}>
                  {dispute.status === "pending" ? "CHỜ XỬ LÝ" : dispute.status === "resolved" ? "ĐÃ GIẢI QUYẾT" : "BỊ TỪ CHỐI"}
                </div>
              </div>

              <div className="grid grid-cols-2 gap-4 mb-5 bg-gray-50/50 p-4 rounded-xl border border-gray-100">
                <div>
                  <p className="text-[10px] font-bold text-gray-400 uppercase tracking-wider mb-1">Người gửi</p>
                  <p className="font-bold text-gray-800 text-sm">{dispute.citizen}</p>
                </div>
                <div>
                  <p className="text-[10px] font-bold text-gray-400 uppercase tracking-wider mb-1">Thời gian</p>
                  <p className="font-bold text-gray-800 text-sm">{dispute.createdAt}</p>
                </div>
              </div>

              <div className="bg-amber-50/50 rounded-xl p-4 border border-amber-100 mb-5">
                <p className="text-xs font-bold text-amber-900 mb-1">Nội dung khiếu nại:</p>
                <p className="text-sm text-amber-800 leading-relaxed italic">"{dispute.content}"</p>
              </div>

              {/* Lịch sử phản hồi */}
              {(dispute.enterpriseResponse || dispute.escalationReason) && (
                <div className="space-y-3 mb-5">
                  {/* Phản hồi từ doanh nghiệp */}
                  {dispute.enterpriseResponse && (
                    <div className="bg-blue-50/50 rounded-xl p-4 border border-blue-100">
                      <div className="flex items-center gap-2 mb-2">
                        <MessageSquare size={16} className="text-blue-600" />
                        <p className="text-xs font-bold text-blue-900">Phản hồi từ Doanh nghiệp:</p>
                      </div>
                      <p className="text-sm text-blue-800 leading-relaxed italic">"{dispute.enterpriseResponse}"</p>
                      {dispute.enterpriseRespondedAt && (
                        <p className="text-xs text-blue-600 mt-2">Phản hồi lúc: {dispute.enterpriseRespondedAt}</p>
                      )}
                    </div>
                  )}
                  
                  {/* Lý do escalate */}
                  {dispute.escalationReason && (
                    <div className="bg-purple-50/50 rounded-xl p-4 border border-purple-100">
                      <div className="flex items-center gap-2 mb-2">
                        <ShieldAlert size={16} className="text-purple-600" />
                        <p className="text-xs font-bold text-purple-900">Lý do chuyển lên Admin:</p>
                      </div>
                      <p className="text-sm text-purple-800 leading-relaxed italic">"{dispute.escalationReason}"</p>
                    </div>
                  )}
                </div>
              )}

              {dispute.status === "pending" ? (
                <div className="flex gap-3 pt-5 border-t border-gray-100">
                  <button onClick={() => handleAction(dispute.id, "reject")} disabled={actionLoading} className="flex-1 py-2.5 bg-white border border-red-200 text-red-600 hover:bg-red-50 font-bold rounded-xl transition-all shadow-sm">TỪ CHỐI</button>
                  <button onClick={() => handleAction(dispute.id, "resolve")} disabled={actionLoading} className="flex-1 py-2.5 bg-emerald-600 hover:bg-emerald-700 text-white font-bold rounded-xl transition-all shadow-emerald-200 shadow-md">ĐỒNG Ý & GIẢI QUYẾT</button>
                </div>
              ) : (
                <div className="pt-5 border-t border-gray-100">
                  <div className={`rounded-xl p-4 border ${dispute.status === "resolved" ? "bg-emerald-50 border-emerald-200" : "bg-red-50 border-red-200"}`}>
                    <p className={`text-xs font-bold mb-1 ${dispute.status === "resolved" ? "text-emerald-900" : "text-red-900"}`}>Phản hồi của Admin:</p>
                    <p className="text-sm leading-relaxed">{dispute.adminResponse || "Không có phản hồi."}</p>
                  </div>
                </div>
              )}
            </div>
          ))}
        </div>
      )}

      {/* Modal xem nhanh báo cáo gốc */}
      {viewingReportId && (
        <ReportDetailModal 
          reportId={viewingReportId} 
          onClose={() => setViewingReportId(null)} 
        />
      )}

      <ConfirmationModal isOpen={modal.isOpen} config={modal.config} onConfirm={modal.onConfirm} onCancel={modal.onCancel} isLoading={actionLoading} />
      <ToastContainer toasts={toast.toasts} onRemove={toast.removeToast} />
    </div>
  );
};