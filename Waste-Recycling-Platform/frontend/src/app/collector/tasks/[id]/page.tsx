"use client";
import React, { useEffect, useState, useRef } from "react";
import { useParams, useRouter } from "next/navigation";
import { collectorTaskApi } from "@/lib/api/collectorTaskApi";
import { API_CONFIG } from "@/lib/api/config";
import { Button, Input, Badge } from "@/components/ui";
import { MapPin, User, ArrowLeft, Image as ImageIcon, CheckCircle, Clock, UploadCloud, X } from "lucide-react";

export default function TaskDetailPage() {
  const params = useParams();
  const router = useRouter();
  const [task, setTask] = useState<any>(null);
  const [weightKg, setWeightKg] = useState("");
  const [notes, setNotes] = useState("");
  const [images, setImages] = useState<File[]>([]);
  const [isDragging, setIsDragging] = useState(false);
  const [selectedImage, setSelectedImage] = useState<string | null>(null);
  const fileInputRef = useRef<HTMLInputElement>(null);

  useEffect(() => {
    if (params.id) {
      collectorTaskApi.getTaskById(params.id as string)
        .then(res => setTask(res))
        .catch(() => alert("Không thể tải thông tin nhiệm vụ"));
    }
  }, [params.id]);

  if (!task) return <div className="p-8 text-center text-gray-500 mt-20">Đang tải thông tin nhiệm vụ...</div>;

  const handleStartPickup = async () => {
    try {
      await collectorTaskApi.setOnTheWay(task.id);
      window.location.reload(); 
    } catch (err: any) {
      console.error(err);
      alert(`Không thể cập nhật trạng thái: ${err.message || JSON.stringify(err.data)}`);
    }
  };

  const handleComplete = async () => {
    if (!weightKg) return alert("Vui lòng nhập khối lượng (kg)");
    if (images.length === 0) return alert("Vui lòng tải lên ít nhất một hình ảnh xác minh");
    
    try {
      const formData = new FormData();
      formData.append("WeightKg", weightKg);
      formData.append("Notes", notes);
      images.forEach(img => formData.append("Images", img));

      await collectorTaskApi.completeTask(task.id, formData);
      window.location.reload();
    } catch (err: any) {
      alert(`Không thể hoàn thành nhiệm vụ: ${err.message || JSON.stringify(err.data)}`);
    }
  };

  const handleDragOver = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(true);
  };

  const handleDragLeave = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
  };

  const handleDrop = (e: React.DragEvent) => {
    e.preventDefault();
    setIsDragging(false);
    if (e.dataTransfer.files && e.dataTransfer.files.length > 0) {
      const newFiles = Array.from(e.dataTransfer.files).filter(f => f.type.startsWith('image/'));
      setImages(prev => [...prev, ...newFiles]);
    }
  };

  const handleFileSelect = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files && e.target.files.length > 0) {
      const newFiles = Array.from(e.target.files).filter(f => f.type.startsWith('image/'));
      setImages(prev => [...prev, ...newFiles]);
    }
    // Cần reset giá trị input nếu muốn upload tiếp một luồng file cũ
    if (fileInputRef.current) {
        fileInputRef.current.value = '';
    }
  };

  const removeImage = (index: number) => {
    setImages(prev => prev.filter((_, i) => i !== index));
  };

  return (
    <div className="min-h-screen bg-gray-50 py-8 px-4">
      <div className="max-w-4xl mx-auto">
        <Button variant="outline" onClick={() => router.back()} className="mb-4 text-emerald-600 hover:text-emerald-700">
          <ArrowLeft className="h-4 w-4 mr-2" /> Quay lại
        </Button>

        {/* WRP-109: Task Details Section */}
        <div className="bg-white shadow-sm rounded-lg p-6 mb-6 border border-gray-200">
          <div className="flex justify-between items-start mb-6 border-b pb-4">
            <div>
              <h1 className="text-2xl font-bold text-gray-900">{task.report.categoryName || "Rác thải không xác định"}</h1>
              <p className="text-gray-500 mt-1 text-sm">Mã nhiệm vụ: {task.id}</p>
            </div>
            <Badge variant={task.status === "Collected" || task.status?.toLowerCase() === "collected" ? "success" : task.status === "OnTheWay" ? "info" : "warning"}>
              {task.status === "Collected" || task.status?.toLowerCase() === "collected" ? "Đã thu gom" : 
               task.status === "OnTheWay" || task.status?.toLowerCase() === "ontheway" ? "Đang di chuyển" : 
               task.status === "Assigned" || task.status?.toLowerCase() === "assigned" ? "Đã phân công" : 
               task.status ? task.status.replaceAll("_", " ") : ""}
            </Badge>
          </div>

          <div className="grid md:grid-cols-2 gap-8">
            <div>
              <h3 className="font-semibold text-lg mb-3 text-gray-800 border-b pb-2">Thông tin Người dân</h3>
              <div className="space-y-3 text-gray-700 text-sm">
                <p className="flex items-center"><User className="h-4 w-4 mr-2 text-gray-400"/> {task.report.citizenName}</p>
                <p className="flex items-start"><MapPin className="h-4 w-4 mr-2 text-gray-400 mt-1 flex-shrink-0"/> {task.report.address}</p>
                {task.report.citizenPhone && <p className="flex items-center text-blue-600">📞 {task.report.citizenPhone}</p>}
                
                {task.report.description && (
                  <div className="mt-4 bg-yellow-50 p-3 rounded text-yellow-800 border border-yellow-200">
                    <b>Ghi chú người gửi:</b> {task.report.description}
                  </div>
                )}
              </div>
            </div>

            <div>
               <h3 className="font-semibold text-lg mb-3 text-gray-800 border-b pb-2">Lịch trình</h3>
               <div className="space-y-3 flex flex-col text-sm text-gray-600">
                 <div className="flex items-center">
                    <Clock className="w-4 h-4 mr-2 text-gray-400" /> Được chỉ định lúc: {new Date(task.assignedAt).toLocaleString('vi-VN')}
                 </div>
                 {task.completedAt && (
                   <div className="flex items-center">
                     <CheckCircle className="w-4 h-4 mr-2 text-emerald-500" /> Hoàn thành lúc: {new Date(task.completedAt).toLocaleString('vi-VN')}
                   </div>
                 )}
               </div>
            </div>
          </div>
        </div>

        {/* Report Images Section */}
        {task.report?.imageUrls && task.report.imageUrls.length > 0 && (
          <div className="bg-white shadow-sm rounded-lg p-6 mb-6 border border-gray-200">
            <h3 className="font-semibold text-lg mb-4 text-gray-800 border-b pb-2 flex items-center gap-2">
              <ImageIcon className="h-5 w-5 text-indigo-500" /> 
              Hình ảnh từ người dân ({task.report.imageUrls.length})
            </h3>
            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3">
              {task.report.imageUrls.map((fileName: string, index: number) => {
                const fileUrl = fileName.startsWith("http") ? fileName : (fileName.startsWith("/") ? `${API_CONFIG.SERVER_URL}${fileName}` : `${API_CONFIG.SERVER_URL}/uploads/${fileName}`);
                return (
                  <button 
                    type="button"
                    key={index}
                    onClick={() => setSelectedImage(fileUrl)}
                    className="aspect-square bg-gray-100 rounded-lg overflow-hidden border border-gray-200 cursor-pointer hover:border-emerald-500 transition-colors group relative"
                  >
                    <img 
                      src={fileUrl} 
                      alt={`Báo cáo #${index + 1}`}
                      className="w-full h-full object-cover group-hover:scale-105 transition-transform"
                      onError={(e) => {
                        (e.target as HTMLImageElement).src = 'data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100"><rect width="100" height="100" fill="%23f3f4f6"/><text x="50%" y="50%" font-family="sans-serif" font-size="12" fill="%239ca3af" text-anchor="middle" dominant-baseline="middle">Lỗi</text></svg>';
                      }}
                    />
                    <div className="absolute inset-0 bg-black/0 group-hover:bg-black/20 transition-colors flex items-center justify-center">
                      <ImageIcon className="text-white opacity-0 group-hover:opacity-100 transition-opacity drop-shadow-lg" size={24} />
                    </div>
                  </button>
                );
              })}
            </div>
          </div>
        )}

        {/* Collector Images Section */}
        {task.status === "Collected" && task.images && task.images.length > 0 && (
          <div className="bg-white shadow-sm rounded-lg p-6 mb-6 border border-emerald-200">
            <h3 className="font-semibold text-lg mb-4 text-emerald-800 border-b border-emerald-100 pb-2 flex items-center gap-2">
              <CheckCircle className="h-5 w-5 text-emerald-500" /> 
              Hình ảnh đã thu gom ({task.images.length})
            </h3>
            <div className="grid grid-cols-2 sm:grid-cols-3 md:grid-cols-4 gap-3">
              {task.images.map((imgObj: any, index: number) => {
                // Determine whether task.images contains strings or objects (with ImageUrl)
                const fileName = typeof imgObj === 'string' ? imgObj : (imgObj.imageUrl || imgObj.ImageUrl || '');
                const fileUrl = fileName.startsWith("http") ? fileName : (fileName.startsWith("/") ? `${API_CONFIG.SERVER_URL}${fileName}` : `${API_CONFIG.SERVER_URL}/uploads/${fileName}`);
                return (
                  <button 
                    type="button"
                    key={index}
                    onClick={() => setSelectedImage(fileUrl)}
                    className="aspect-square bg-emerald-50 rounded-lg overflow-hidden border border-emerald-200 cursor-pointer hover:border-emerald-500 transition-colors group relative"
                  >
                    <img 
                      src={fileUrl} 
                      alt={`Thu gom #${index + 1}`}
                      className="w-full h-full object-cover group-hover:scale-105 transition-transform"
                      onError={(e) => {
                        (e.target as HTMLImageElement).src = 'data:image/svg+xml;utf8,<svg xmlns="http://www.w3.org/2000/svg" width="100" height="100"><rect width="100" height="100" fill="%23f3f4f6"/><text x="50%" y="50%" font-family="sans-serif" font-size="12" fill="%239ca3af" text-anchor="middle" dominant-baseline="middle">Lỗi</text></svg>';
                      }}
                    />
                    <div className="absolute inset-0 bg-black/0 group-hover:bg-black/20 transition-colors flex items-center justify-center">
                      <ImageIcon className="text-white opacity-0 group-hover:opacity-100 transition-opacity drop-shadow-lg" size={24} />
                    </div>
                  </button>
                );
              })}
            </div>
          </div>
        )}

        {/* Update Task Status Section */}
        <div className="bg-white shadow-sm rounded-lg p-6 mb-6 border border-gray-200">
          <h3 className="font-semibold text-lg mb-4 text-emerald-900 border-b pb-2">Cập nhật tiến độ nhiệm vụ</h3>
          
          {task.status === "Assigned" && (
            <div className="space-y-4 max-w-md">
              <p className="text-sm text-gray-500">Bạn đã được phân công thu gom rác thải này. Vui lòng cập nhật trạng thái khi bạn bắt đầu di chuyển.</p>
              <Button onClick={handleStartPickup} className="w-full">
                Bắt đầu di chuyển (Đang đến nơi)
              </Button>
            </div>
          )}

          {task.status === "OnTheWay" && (
            <div className="space-y-4 max-w-md">
              <p className="text-sm text-gray-500">Vui lòng nhập khối lượng và hình ảnh xác minh để hoàn thành nhiệm vụ.</p>
              <div>
                <label htmlFor="task-weight" className="block text-sm font-medium text-gray-700 mb-1">Khối lượng (kg) *</label>
                <Input id="task-weight" type="number" min="0" step="0.1" value={weightKg} onChange={e => setWeightKg(e.target.value)} required />
              </div>
              <div>
                <label htmlFor="task-images" className="block text-sm font-medium text-gray-700 mb-2">Hình ảnh xác minh * ({images.length} ảnh)</label>
                
                {/* Khu vực kéo thả */}
                <button 
                  type="button"
                  onDragOver={handleDragOver}
                  onDragLeave={handleDragLeave}
                  onDrop={handleDrop}
                  onClick={() => fileInputRef.current?.click()}
                  className={`relative w-full flex flex-col items-center justify-center p-6 border-2 border-dashed rounded-xl cursor-pointer transition-colors ${
                    isDragging ? 'border-emerald-500 bg-emerald-50' : 'border-gray-300 bg-gray-50 hover:bg-gray-100'
                  }`}
                >
                  <input 
                    id="task-images"
                    type="file" 
                    multiple 
                    accept="image/*"
                    ref={fileInputRef}
                    onChange={handleFileSelect} 
                    className="hidden" 
                  />
                  <UploadCloud className={`w-10 h-10 mb-3 ${isDragging ? 'text-emerald-500' : 'text-gray-400'}`} />
                  <span className="text-sm font-medium text-gray-700">Kéo thả ảnh vào đây để tải lên</span>
                  <span className="text-xs text-gray-500 mt-1">hoặc nhấn để chọn file (.jpg, .png, .gif) - Hỗ trợ tải nhiều ảnh</span>
                </button>

                {/* Danh sách ảnh đã chọn */}
                {images.length > 0 && (
                  <div className="grid grid-cols-2 md:grid-cols-3 gap-3 mt-4">
                    {images.map((img, idx) => (
                      <div key={idx} className="relative group aspect-square rounded-lg overflow-hidden border border-gray-200">
                        <img 
                          src={URL.createObjectURL(img)} 
                          alt={`preview-${idx}`} 
                          className="w-full h-full object-cover"
                        />
                        <button
                          type="button"
                          onClick={(e) => { e.stopPropagation(); removeImage(idx); }}
                          className="absolute top-1 right-1 bg-red-500 text-white rounded-full p-1 opacity-0 group-hover:opacity-100 transition-opacity"
                        >
                          <X className="w-4 h-4" />
                        </button>
                      </div>
                    ))}
                  </div>
                )}
              </div>
              <div>
                <label htmlFor="task-notes" className="block text-sm font-medium text-gray-700 mb-1">Ghi chú thêm</label>
                <Input id="task-notes" value={notes} onChange={e => setNotes(e.target.value)} />
              </div>
              <Button onClick={handleComplete} className="w-full" disabled={!weightKg || images.length === 0}>
                Hoàn thành thu gom (Đã thu gom)
              </Button>
            </div>
          )}

          {task.status === "Collected" && (
             <div className="space-y-4 max-w-md bg-emerald-50 p-4 rounded-lg border border-emerald-100">
               <div className="flex items-center text-emerald-700 font-medium">
                  <CheckCircle className="w-5 h-5 mr-2 text-emerald-500" />
                  Bạn đã hoàn thành nhiệm vụ này.
               </div>
               <p className="text-sm text-emerald-600">Khối lượng thu gom: {task.collectedWeightKg} kg</p>
               {task.notes && <p className="text-sm text-emerald-600">Ghi chú: {task.notes}</p>}
             </div>
          )}
        </div>
      </div>

      {/* Full Screen Image Lightbox */}
      {selectedImage && (
        <div 
          role="dialog"
          aria-modal="true"
          className="fixed inset-0 z-[60] flex items-center justify-center bg-black/90 p-4"
          onClick={() => setSelectedImage(null)}
          onKeyDown={(e) => { if (e.key === 'Escape') setSelectedImage(null); }}
        >
          <button 
            className="absolute top-6 right-6 text-white/70 hover:text-white bg-black/40 hover:bg-black/60 rounded-full p-2 transition-colors"
            onClick={(e) => { e.stopPropagation(); setSelectedImage(null); }}
          >
            <svg className="w-6 h-6" fill="none" stroke="currentColor" viewBox="0 0 24 24">
              <path strokeLinecap="round" strokeLinejoin="round" strokeWidth={2} d="M6 18L18 6M6 6l12 12" />
            </svg>
          </button>
          <img 
            src={selectedImage} 
            alt="Ảnh phóng to" 
            className="max-w-full max-h-[90vh] object-contain rounded-lg shadow-2xl"
            onClick={(e) => e.stopPropagation()} 
          />
        </div>
      )}
    </div>
  );
}