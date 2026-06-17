"use client";
import React, { useEffect, useState } from "react";
import { X, MapPin, Clock, AlignLeft, Lightbulb, Image as ImageIcon, Map } from "lucide-react";
import { API_CONFIG } from "@/lib/api/config";
import { reportApi } from "../../lib/api/reportApi";

interface ReportDetailModalProps {
  reportId: string;
  onClose: () => void;
}

const StatusBadge: React.FC<{ status: string }> = ({ status }) => {
  switch (status) {
    case "Pending":
      return <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-bold bg-amber-100 text-amber-800">Chờ Duyệt</span>;
    case "Accepted":
      return <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-bold bg-blue-100 text-blue-800">Đã Nhận</span>;
    case "Assigned":
      return <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-bold bg-purple-100 text-purple-800">Đang Đến Trạm</span>;
    case "Collected":
      return <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-bold bg-emerald-100 text-emerald-800">Hoàn Thành</span>;
    default:
      return <span className="inline-flex items-center px-3 py-1 rounded-full text-xs font-bold bg-gray-100 text-gray-800">{status}</span>;
  }
};

export const ReportDetailModal: React.FC<ReportDetailModalProps> = ({ reportId, onClose }) => {
  const [report, setReport] = useState<any>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);
  const [selectedImage, setSelectedImage] = useState<string | null>(null);

  useEffect(() => {
    const fetchReport = async () => {
      try {
        setIsLoading(true);
        const data = await reportApi.getReportById(reportId);
        setReport(data.report);
      } catch (err) {
        setError("Không thể tải thông tin báo cáo.");
      } finally {
        setIsLoading(false);
      }
    };
    fetchReport();
  }, [reportId]);

  return (
    <>
      <div className="fixed inset-0 z-50 flex items-center justify-center p-4 bg-gray-900/60 backdrop-blur-sm animate-in fade-in duration-200">
        <div className="bg-white rounded-3xl shadow-2xl w-full max-w-2xl max-h-[90vh] overflow-hidden flex flex-col animate-in zoom-in-95 duration-200">
          
          {/* Header */}
          <div className="flex items-center justify-between px-6 py-4 border-b border-gray-100 bg-gray-50/50">
            <div>
              <h3 className="text-lg font-bold text-gray-900">Chi Tiết Báo Cáo</h3>
              <p className="text-xs text-gray-500 font-mono mt-0.5">ID: {reportId.toUpperCase()}</p>
            </div>
            <button 
              onClick={onClose}
              className="p-2 text-gray-400 hover:text-gray-600 hover:bg-gray-100 rounded-full transition-colors"
            >
              <X size={20} />
            </button>
          </div>

          {/* Content */}
          <div className="flex-1 overflow-y-auto p-6">
            {isLoading ? (
              <div className="flex flex-col items-center justify-center py-20">
                <div className="w-10 h-10 border-4 border-emerald-500 border-t-transparent rounded-full animate-spin"></div>
                <p className="mt-4 text-sm text-gray-500">Đang tải dữ liệu...</p>
              </div>
            ) : error ? (
              <div className="text-center py-10">
                <div className="inline-flex items-center justify-center w-16 h-16 rounded-full bg-red-100 text-red-500 mb-4">
                  <X size={32} />
                </div>
                <p className="text-red-600 font-medium">{error}</p>
              </div>
            ) : report ? (
              <div className="space-y-8">
                
                {/* Top Info row */}
                <div className="flex flex-wrap items-center gap-4 pb-6 border-b border-gray-100">
                  <div className="flex-1 min-w-[200px]">
                    <p className="text-sm text-gray-500 mb-1">Trạng thái</p>
                    <StatusBadge status={report.status} />
                  </div>
                  <div className="flex-1 min-w-[200px]">
                    <p className="text-sm text-gray-500 mb-1">Loại Rác</p>
                    <p className="font-semibold text-gray-900">{report.categoryName || "Không xác định"}</p>
                  </div>
                  <div className="flex-1 min-w-[200px]">
                    <p className="text-sm text-gray-500 mb-1">Thời gian tạo</p>
                    <div className="flex items-center gap-1.5 font-medium text-gray-800">
                      <Clock size={16} className="text-emerald-500" />
                      {new Date(report.createdAt).toLocaleString("vi-VN")}
                    </div>
                  </div>
                </div>

                {/* Location */}
                <div className="bg-emerald-50/50 rounded-2xl p-5 border border-emerald-100">
                  <h4 className="text-sm font-bold text-emerald-800 flex items-center gap-2 mb-3">
                    <MapPin size={18} /> Vị Trí Báo Cáo
                  </h4>
                  <p className="text-sm text-gray-700 leading-relaxed font-medium mb-0">{report.address}</p>
                </div>

                {/* Description & Output */}
                <div className="grid md:grid-cols-2 gap-6">
                  <div>
                    <h4 className="text-sm font-bold text-gray-800 flex items-center gap-2 mb-3">
                      <AlignLeft size={18} className="text-blue-500" /> Ghi Chú
                    </h4>
                    <div className="bg-gray-50 rounded-2xl p-4 text-sm text-gray-700 min-h-[100px] whitespace-pre-wrap">
                      {report.description || <span className="text-gray-400 italic">Không có ghi chú thêm.</span>}
                    </div>
                  </div>

                  <div>
                    <h4 className="text-sm font-bold text-gray-800 flex items-center gap-2 mb-3">
                      <Lightbulb size={18} className="text-amber-500" /> Đề Xuất Phân Loại (AI)
                    </h4>
                    <div className="bg-amber-50 rounded-2xl p-4 text-sm text-amber-900 border border-amber-100 min-h-[100px]">
                      {report.aiSuggestion || <span className="text-amber-700/50 italic">AI chưa có đề xuất cho báo cáo này.</span>}
                    </div>
                  </div>
                </div>

                {/* Images */}
                <div>
                  <h4 className="text-sm font-bold text-gray-800 flex items-center gap-2 mb-4">
                    <ImageIcon size={18} className="text-indigo-500" /> Hình Ảnh Đính Kèm ({report.imageUrls?.length || 0})
                  </h4>
                  {report.imageUrls && report.imageUrls.length > 0 ? (
                    <div className="grid grid-cols-2 sm:grid-cols-3 gap-3">
                      {report.imageUrls.map((fileName: string, index: number) => {
                        const fileUrl = fileName.startsWith("http") ? fileName : (fileName.startsWith("/") ? `${API_CONFIG.SERVER_URL}${fileName}` : `${API_CONFIG.SERVER_URL}/uploads/${fileName}`);
                        
                        return (
                          <button 
                            type="button"
                            key={index}
                            onClick={() => setSelectedImage(fileUrl)} 
                            className="aspect-square bg-gray-100 rounded-xl overflow-hidden border border-gray-200 group relative flex items-center justify-center cursor-pointer hover:border-emerald-500 transition-colors"
                          >
                            <img 
                              src={fileUrl} 
                              alt={`Ảnh đính kèm ${index + 1}`} 
                              className="w-full h-full object-cover group-hover:scale-105 transition-transform duration-300"
                              onError={(e) => {
                                (e.target as HTMLImageElement).src = 'data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="400" height="400"><rect width="400" height="400" fill="%23f3f4f6"/><text x="50%" y="50%" font-family="sans-serif" font-size="16" fill="%239ca3af" text-anchor="middle" dominant-baseline="middle">Lỗi tải ảnh</text></svg>';
                              }}
                            />
                            <div className="absolute inset-0 bg-black/0 group-hover:bg-black/10 transition-colors flex items-center justify-center opacity-0 group-hover:opacity-100">
                              <ImageIcon size={24} className="text-white drop-shadow-md" />
                            </div>
                          </button>
                        );
                      })}
                    </div>
                  ) : (
                    <p className="text-sm text-gray-500 italic">Người dùng không đính kèm hình ảnh.</p>
                  )}
                </div>

              </div>
            ) : null}
          </div>
        </div>
      </div>

      {/* Full Screen Image Lightbox */}
      {selectedImage && (
        <div 
          role="presentation"
          className="fixed inset-0 z-[60] flex items-center justify-center bg-black/90 p-4 animate-in fade-in"
          onClick={() => setSelectedImage(null)}
        >
          <button 
            className="absolute top-6 right-6 text-white/70 hover:text-white bg-black/40 hover:bg-black/60 rounded-full p-2 transition-colors z-[70]"
            onClick={(e) => { e.stopPropagation(); setSelectedImage(null); }}
          >
            <X size={28} />
          </button>
          <img 
            src={selectedImage} 
            alt="Xem chi tiết ảnh" 
            className="max-w-full max-h-[90vh] object-contain rounded-lg shadow-2xl animate-in zoom-in-95" 
            onClick={(e) => e.stopPropagation()}
            onKeyDown={(e) => { if (e.key === 'Enter') e.stopPropagation(); }}
            role="presentation"
          />
        </div>
      )}
    </>
  );
};
