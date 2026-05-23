"use client";
import React, { useState, useEffect } from "react";
import { Search, MapPin, Eye, X, AlertCircle, FileText, User, MessageSquare, Image as ImageIcon, Clock, AlignLeft, Lightbulb } from "lucide-react";
import { ImageGallery } from "../shared/ImageGallery";
import { Portal } from "../shared/Portal";
import { API_CONFIG } from "@/lib/api/config";

// --- COMPONENT MODAL CHI TIẾT (XUẤT RA ĐỂ TRANG KHIẾU NẠI DÙNG CHUNG) ---
export const ReportDetailModal: React.FC<{ reportId: string; onClose: () => void }> = ({ reportId, onClose }) => {
  const [report, setReport] = useState<any>(null);
  const [loading, setLoading] = useState(true);
  const [galleryOpen, setGalleryOpen] = useState(false);
  const [galleryImages, setGalleryImages] = useState<string[]>([]);

  useEffect(() => {
    const fetchDetail = async () => {
      try {
        const token = localStorage.getItem("token") || "";
        const response = await fetch(`${API_CONFIG.BASE_URL}/reports/${reportId}`, {
          headers: { "Authorization": `Bearer ${token}`, "Accept": "*/*" }
        });

        if (!response.ok) {
          const errorData = await response.json().catch(() => ({}));
          throw new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
        }

        const json = await response.json();
        const detailedReport = json.report || json.data || json;
        
        const rawImages = detailedReport.imageUrls || detailedReport.images || (detailedReport.reportImages?.map((img: any) => img.imageUrl)) || [];
        const serverUrl = (API_CONFIG as any).SERVER_URL || API_CONFIG.BASE_URL.replace('/api', '');
        const formattedImages = rawImages.map((img: any) => {
           const fileName = typeof img === 'string' ? img.split('/').pop() : img.imageUrl?.split('/').pop();
           return `${serverUrl}/uploads/${fileName}`;
        });

        setReport({ ...detailedReport, formattedImages });
        setGalleryImages(formattedImages);
      } catch (err) { 
        console.error("[ReportDetailModal] Error:", err);
      } 
      finally { setLoading(false); }
    };
    fetchDetail();
  }, [reportId]);

  if (!reportId) return null;

  return (
    <Portal>
      <div className="fixed inset-0 z-[100] flex items-center justify-center p-4 bg-gray-900/60 backdrop-blur-sm animate-in fade-in duration-200">
        <div className="bg-white rounded-3xl shadow-2xl w-full max-w-2xl max-h-[90vh] overflow-hidden flex flex-col animate-in zoom-in-95 duration-200">
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 bg-gray-50/50">
            <h3 className="text-lg font-bold text-gray-900">Chi Tiết Báo Cáo Gốc</h3>
            <button onClick={onClose} className="p-2 text-gray-400 hover:text-gray-600 rounded-full"><X size={20} /></button>
          </div>

          <div className="flex-1 overflow-y-auto p-6">
            {loading ? (
              <div className="py-20 text-center"><div className="w-10 h-10 border-4 border-emerald-500 border-t-transparent rounded-full animate-spin mx-auto"></div></div>
            ) : report ? (
              <div className="space-y-6">
                <div className="bg-emerald-50/50 rounded-2xl p-5 border border-emerald-100">
                  <h4 className="text-sm font-bold text-emerald-800 flex items-center gap-2 mb-2"><MapPin size={18} /> Vị Trí</h4>
                  <p className="text-sm text-gray-700 font-medium">{report.address}</p>
                </div>
                <div className="grid grid-cols-2 gap-4">
                  <div className="bg-gray-50 p-4 rounded-xl">
                    <h4 className="text-xs font-bold text-gray-500 uppercase mb-2">Ghi chú</h4>
                    <p className="text-sm">{report.description || "Không có ghi chú."}</p>
                  </div>
                  <div className="bg-amber-50 p-4 rounded-xl">
                    <h4 className="text-xs font-bold text-amber-700 uppercase mb-2">AI Gợi ý</h4>
                    <p className="text-sm">{report.aiSuggestion || "Chưa có."}</p>
                  </div>
                </div>
                <div>
                  <h4 className="text-sm font-bold mb-3 flex items-center gap-2"><ImageIcon size={18} /> Hình ảnh</h4>
                  <div className="grid grid-cols-3 gap-2">
                    {report.formattedImages?.map((img: string, i: number) => (
                      <img key={i} src={img} onClick={() => setGalleryOpen(true)} className="aspect-square object-cover rounded-lg cursor-pointer hover:opacity-80 border" />
                    ))}
                  </div>
                </div>
              </div>
            ) : <p className="text-center py-10">Không tìm thấy dữ liệu.</p>}
          </div>
          <div className="p-4 border-t flex justify-end bg-gray-50/50"><button onClick={onClose} className="px-6 py-2 bg-white border rounded-xl font-bold text-sm">Đóng</button></div>
        </div>
      </div>
      <ImageGallery images={galleryImages} isOpen={galleryOpen} onClose={() => setGalleryOpen(false)} title="Ảnh Báo Cáo" />
    </Portal>
  );
};

// --- COMPONENT CHÍNH (QUẢN LÝ DANH SÁCH) ---
export const ReportsManagement: React.FC = () => {
  const [reports, setReports] = useState<any[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedReportId, setSelectedReportId] = useState<string | null>(null);
  const [userRole, setUserRole] = useState<string | null>(null);

  // Get user role from JWT
  useEffect(() => {
    const token = localStorage.getItem("token");
    if (token) {
      try {
        const decoded = JSON.parse(atob(token.split('.')[1]));
        setUserRole(decoded['http://schemas.microsoft.com/ws/2008/06/identity/claims/role'] || decoded.role);
      } catch (err) {
        console.error("Failed to decode token:", err);
      }
    }
  }, []);

  const fetchReports = async () => {
    try {
      setLoading(true);
      const token = localStorage.getItem("token") || ""; 
      
      // Auto-detect endpoint based on role
      const endpoint = userRole === "Citizen" 
        ? `${API_CONFIG.BASE_URL}/reports/my-reports?page=1&pageSize=100`
        : `${API_CONFIG.BASE_URL}/reports/all?page=1&pageSize=100`;

      const response = await fetch(endpoint, {
        headers: { "Authorization": `Bearer ${token}`, "Accept": "*/*" }
      });

      if (!response.ok) {
        const errorData = await response.json().catch(() => ({}));
        throw new Error(errorData.message || `HTTP ${response.status}: ${response.statusText}`);
      }

      const json = await response.json();
      setReports(json.reports || []);
    } catch (error) { 
      console.error("[ReportsManagement] Error:", error);
    } 
    finally { setLoading(false); }
  };

  useEffect(() => { 
    if (userRole) {
      fetchReports(); 
    }
  }, [userRole]);

  return (
    <div className="space-y-6 pt-2">
      <div className="bg-white rounded-2xl shadow-sm border p-6 flex gap-4">
        <div className="relative flex-1">
          <Search size={18} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
          <input type="text" placeholder="Tìm kiếm báo cáo..." value={searchTerm} onChange={(e) => setSearchTerm(e.target.value)} className="w-full pl-10 pr-4 py-2.5 border rounded-xl focus:ring-2 focus:ring-emerald-500 text-sm" />
        </div>
      </div>

      {loading ? (
        <div className="flex justify-center items-center py-20">
          <div className="w-10 h-10 border-4 border-emerald-500 border-t-transparent rounded-full animate-spin"></div>
        </div>
      ) : reports.length === 0 ? (
        <div className="text-center py-20 bg-white rounded-2xl shadow-sm border">
          <FileText size={40} className="mx-auto text-gray-300 mb-3" />
          <p className="text-gray-500 font-medium">Chưa có báo cáo nào</p>
        </div>
      ) : (
        <div className="bg-white rounded-2xl shadow-sm border overflow-hidden">
          <table className="w-full text-left">
            <thead className="bg-gray-50 border-b">
              <tr>
                <th className="px-6 py-4 text-sm font-semibold text-gray-500">Mã Báo Cáo</th>
                <th className="px-6 py-4 text-sm font-semibold text-gray-500">Người Dùng</th>
                <th className="px-6 py-4 text-center text-sm font-semibold text-gray-500">Thao tác</th>
              </tr>
            </thead>
            <tbody className="divide-y">
              {reports.map((r) => (
                <tr key={r.id} className="hover:bg-gray-50">
                  <td className="px-6 py-4 font-bold text-gray-900">#R-{r.id.substring(0, 6).toUpperCase()}</td>
                  <td className="px-6 py-4 text-sm">{r.citizenName || "N/A"}</td>
                  <td className="px-6 py-4 text-center">
                    <button onClick={() => setSelectedReportId(r.id)} className="p-2 bg-white border rounded-lg hover:bg-emerald-50 text-gray-500 hover:text-emerald-600"><Eye size={18} /></button>
                  </td>
                </tr>
              ))}
            </tbody>
          </table>
        </div>
      )}

      {/* Dùng chung Modal chi tiết đã tách ở trên */}
      {selectedReportId && <ReportDetailModal reportId={selectedReportId} onClose={() => setSelectedReportId(null)} />}
    </div>
  );
};