"use client";
import React, { useState, useEffect } from "react";
import { 
  Camera, MapPin, Upload, Trash2, AlertCircle, X, 
  Image as ImageIcon, Map, FileText, CheckCircle2, Recycle, Package, Leaf 
} from "lucide-react";
import { categoryApi, WasteCategory } from "../../lib/api/categoryApi";
import { reportApi } from "../../lib/api/reportApi";

interface ReportFormProps {
  onSubmit: () => void;
}

export const ReportForm: React.FC<ReportFormProps> = ({ onSubmit }) => {
  const [isSubmitting, setIsSubmitting] = useState(false);
  const [isGettingGPS, setIsGettingGPS] = useState(false);
  const [previews, setPreviews] = useState<string[]>([]);
  
  const [categories, setCategories] = useState<WasteCategory[]>([]);
  const [isLoadingCat, setIsLoadingCat] = useState(true);
  
  const [selectedCategoryId, setSelectedCategoryId] = useState<number | "">("");
  const [address, setAddress] = useState("");
  const [latitude, setLatitude] = useState<number | "">("");
  const [longitude, setLongitude] = useState<number | "">("");
  const [description, setDescription] = useState("");
  const [imageFiles, setImageFiles] = useState<File[]>([]);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    categoryApi.getAllCategories()
      .then(res => {
        setCategories(res.data || []);
        setIsLoadingCat(false);
      })
      .catch(err => {
        console.error("Failed to load categories", err);
        setCategories([]);
        setIsLoadingCat(false);
      });
  }, []);

  const reverseGeocode = async (lat: number, lon: number) => {
    try {
      const res = await fetch(`https://nominatim.openstreetmap.org/reverse?format=json&lat=${lat}&lon=${lon}&addressdetails=1&accept-language=vi-VN`);
      const data = await res.json();
      if (data && data.display_name) {
        return data.display_name;
      }
    } catch (error) {
      console.error("Reverse geocoding failed", error);
    }
    return `Vị trí GPS: ${lat}, ${lon}`;
  };

  const handleGPSLocation = () => {
    if ("geolocation" in navigator) {
      setIsGettingGPS(true);
      navigator.geolocation.getCurrentPosition(async (pos) => {
        setLatitude(pos.coords.latitude);
        setLongitude(pos.coords.longitude);
        const addressName = await reverseGeocode(pos.coords.latitude, pos.coords.longitude);
        setAddress(addressName);
        setIsGettingGPS(false);
      }, (err) => {
        alert("Không thể lấy GPS. Vui lòng kiểm tra quyền truy cập.");
        setIsGettingGPS(false);
      });
    } else {
      alert("Trình duyệt không hỗ trợ GPS.");
    }
  };

  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    if (e.target.files) {
      const filesArray = Array.from(e.target.files);
      setImageFiles(prev => [...prev, ...filesArray]);
      
      const newPreviews = filesArray.map(file => URL.createObjectURL(file));
      setPreviews(prev => [...prev, ...newPreviews]);
    }
  };

  const removeImage = (index: number) => {
    setImageFiles(prev => prev.filter((_, i) => i !== index));
    setPreviews(prev => prev.filter((_, i) => i !== index));
  };

  const geocodeAddress = async (addr: string) => {
    try {
      const res = await fetch(`https://nominatim.openstreetmap.org/search?format=json&q=${encodeURIComponent(addr)}`);
      const data = await res.json();
      if (data && data.length > 0) {
        return { lat: Number.parseFloat(data[0].lat), lon: Number.parseFloat(data[0].lon) };
      }
    } catch (error) {
      console.error("Geocoding failed", error);
    }
    return null;
  };

  // Helper để chọn Icon sinh động hơn dựa trên tên loại rác
  const getCategoryIcon = (name: string) => {
    const lowerName = name.toLowerCase();
    if (lowerName.includes("nhựa") || lowerName.includes("nilon")) return <Package size={24} />;
    if (lowerName.includes("hữu cơ") || lowerName.includes("thực phẩm")) return <Leaf size={24} />;
    if (lowerName.includes("giấy") || lowerName.includes("tái chế")) return <Recycle size={24} />;
    return <Trash2 size={24} />;
  };

  // LOGIC SUBMIT NGUYÊN BẢN CỦA BẠN (KHÔNG MẤT 1 DÒNG NÀO)
  const handleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);

    if (!selectedCategoryId || imageFiles.length === 0 || !address) {
      setError("Vui lòng điền đầy đủ thông tin địa chỉ, ảnh và loại rác.");
      return;
    }

    const token = typeof window !== "undefined" ? localStorage.getItem("token") : null;
    if (!token) {
      setError("Bạn chưa đăng nhập! Vui lòng nhấn nút 'ĐĂNG NHẬP' ở góc trên bên phải bằng tài khoản công dân (Citizen) để tạo báo cáo rác.");
      return;
    }

    setIsSubmitting(true);

    try {
      let finalLat = latitude;
      let finalLon = longitude;

      // Nếu user cung cấp địa chỉ nhưng chưa dùng GPS, thử geocode nó
      if (finalLat === "" || finalLon === "") {
        const coords = await geocodeAddress(address);
        if (coords) {
          finalLat = coords.lat;
          finalLon = coords.lon;
        } else {
          // Default fallback coordinates (Hanoi, Vietnam)
          finalLat = 21.0285;
          finalLon = 105.8048;
        }
      }

      const formData = new FormData();
      formData.append("WasteCategoryId", selectedCategoryId.toString());
      formData.append("Latitude", finalLat.toString());
      formData.append("Longitude", finalLon.toString());
      formData.append("Description", description);
      formData.append("Address", address);
      formData.append("AiSuggestion", "");
      
      // Nạp nhiều ảnh vào formData
      imageFiles.forEach(file => {
        formData.append("Images", file);
      });

      await reportApi.createWasteReport(formData);
      
      setIsSubmitting(false);
      onSubmit();
    } catch (err: any) {
      console.error(err);
      setError(err.message || "Đã xảy ra lỗi khi tạo báo cáo.");
      setIsSubmitting(false);
    }
  };

  return (
    <div className="max-w-4xl mx-auto space-y-6">
      
      {/* Tiêu đề trang */}
      <div className="mb-2">
        <h2 className="text-2xl font-bold text-gray-900">Tạo Báo Cáo Thu Gom</h2>
        <p className="text-gray-500 mt-1">Gửi hình ảnh và thông tin để đội thu gom xử lý nhanh chóng.</p>
      </div>

      {error && (
        <div className="p-4 bg-red-50 border border-red-200 text-red-700 rounded-xl flex items-start gap-3 shadow-sm">
          <AlertCircle size={20} className="mt-0.5 shrink-0" />
          <p className="text-sm font-medium">{error}</p>
        </div>
      )}

      <form onSubmit={handleSubmit} className="space-y-6">
        
        {/* KHỐI 1: TẢI ẢNH LÊN */}
        <div className="bg-white p-6 sm:p-8 rounded-2xl shadow-sm border border-gray-100">
          <div className="flex items-center gap-2 mb-4">
            <ImageIcon className="text-emerald-500" size={20} />
            <h3 className="text-lg font-bold text-gray-800">Hình ảnh hiện trường</h3>
          </div>
          
          <div className="relative group cursor-pointer">
            <input 
              type="file" 
              accept="image/*" 
              multiple
              className="absolute inset-0 w-full h-full opacity-0 cursor-pointer z-10" 
              onChange={handleFileChange}
              required={imageFiles.length === 0}
            />
            <div className="border-2 border-dashed border-emerald-300 rounded-2xl p-10 text-center bg-emerald-50/50 group-hover:bg-emerald-50 transition-all duration-300">
              <div className="w-16 h-16 bg-white text-emerald-600 rounded-full flex items-center justify-center mx-auto mb-4 shadow-sm group-hover:scale-110 transition-transform duration-300">
                <Camera size={32} />
              </div>
              <h4 className="text-base font-semibold text-emerald-800">Kéo thả hoặc bấm để tải ảnh lên</h4>
              <p className="text-sm text-emerald-600/70 mt-2">Hỗ trợ định dạng JPG, PNG • Tối đa 5MB / ảnh</p>
            </div>
          </div>

          {/* Lưới hiển thị ảnh đã chọn */}
          {previews.length > 0 && (
            <div className="grid grid-cols-2 sm:grid-cols-4 md:grid-cols-5 gap-4 mt-6">
              {previews.map((preview, index) => (
                <div key={index} className="relative aspect-square rounded-xl overflow-hidden border-2 border-gray-100 shadow-sm group">
                  <img src={preview} alt={`Preview ${index}`} className="w-full h-full object-cover transition-transform duration-500 group-hover:scale-110" />
                  <div className="absolute inset-0 bg-black/50 opacity-0 group-hover:opacity-100 transition-opacity duration-200 flex items-center justify-center">
                    <button 
                      type="button"
                      onClick={() => removeImage(index)}
                      className="bg-red-500 text-white p-2.5 rounded-full hover:bg-red-600 transition-all transform hover:scale-110 shadow-lg"
                      title="Xóa ảnh này"
                    >
                      <X size={18} />
                    </button>
                  </div>
                </div>
              ))}
            </div>
          )}
        </div>

        {/* KHỐI 2: THÔNG TIN CHI TIẾT */}
        <div className="bg-white p-6 sm:p-8 rounded-2xl shadow-sm border border-gray-100 space-y-8">
          
          {/* Vị trí */}
          <div className="space-y-3">
            <div className="flex items-center gap-2">
              <Map className="text-emerald-500" size={20} />
              <label htmlFor="report-address" className="text-lg font-bold text-gray-800">Vị trí thu gom</label>
            </div>
            
            <div className="flex flex-col sm:flex-row gap-3">
              <div className="relative flex-1">
                <div className="absolute inset-y-0 left-0 pl-4 flex items-center pointer-events-none">
                  <MapPin size={18} className="text-gray-400" />
                </div>
                <input 
                  id="report-address"
                  type="text" 
                  value={address}
                  onChange={(e) => setAddress(e.target.value)}
                  placeholder="Nhập địa chỉ cụ thể..." 
                  className="w-full pl-11 pr-4 py-3.5 bg-gray-50 border border-gray-200 rounded-xl focus:bg-white focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none transition-all text-gray-700"
                  required
                />
              </div>
              <button 
                type="button" 
                onClick={handleGPSLocation}
                disabled={isGettingGPS}
                className="bg-emerald-100 text-emerald-700 px-6 py-3.5 rounded-xl font-semibold hover:bg-emerald-200 hover:shadow-sm transition-all flex items-center justify-center gap-2 whitespace-nowrap disabled:opacity-70 disabled:cursor-not-allowed"
              >
                {isGettingGPS ? (
                  <div className="w-5 h-5 border-2 border-emerald-500/30 border-t-emerald-600 rounded-full animate-spin" />
                ) : (
                  <MapPin size={18} />
                )}
                {isGettingGPS ? "Đang định vị..." : "Lấy GPS"}
              </button>
            </div>
          </div>

          {/* Phân loại rác */}
          <div className="space-y-3">
            <div className="flex items-center gap-2">
              <Recycle className="text-emerald-500" size={20} />
              <span className="text-lg font-bold text-gray-800">Phân loại rác tại nguồn</span>
            </div>
            
            {isLoadingCat ? (
              <div className="flex items-center gap-2 text-gray-500 bg-gray-50 p-4 rounded-xl">
                <div className="w-5 h-5 border-2 border-gray-300 border-t-gray-600 rounded-full animate-spin" />
                Đang tải danh mục...
              </div>
            ) : (
              <div className="grid grid-cols-2 md:grid-cols-4 gap-4">
                {categories && categories.length > 0 ? categories.map((category) => {
                  const isSelected = selectedCategoryId === category.id;
                  return (
                    <label key={category.id} className="relative cursor-pointer group">
                      <input 
                        type="radio" 
                        name="wasteType" 
                        value={category.id} 
                        onChange={() => setSelectedCategoryId(category.id)}
                        checked={isSelected}
                        className="peer sr-only" 
                        required 
                      />
                      <div className={`p-4 h-full rounded-xl border-2 text-center transition-all duration-200 flex flex-col items-center justify-center gap-3 relative overflow-hidden
                        ${isSelected 
                          ? 'border-emerald-500 bg-emerald-50 text-emerald-800 shadow-sm' 
                          : 'border-gray-100 bg-gray-50 hover:border-emerald-200 hover:bg-white text-gray-500 hover:text-gray-700'
                        }`}
                      >
                        {isSelected && (
                          <div className="absolute top-2 right-2 text-emerald-500">
                            <CheckCircle2 size={16} />
                          </div>
                        )}
                        
                        <div className={isSelected ? 'text-emerald-500' : 'text-gray-400 group-hover:text-emerald-400 transition-colors'}>
                          {getCategoryIcon(category.name)}
                        </div>
                        <span className="text-sm font-semibold">{category.name}</span>
                      </div>
                    </label>
                  );
                }) : (
                  <div className="col-span-2 text-gray-500">Chưa có danh mục nào.</div>
                )}
              </div>
            )}
          </div>

          {/* Mô tả thêm */}
          <div className="space-y-3">
            <div className="flex items-center gap-2">
              <FileText className="text-emerald-500" size={20} />
              <label htmlFor="report-notes" className="text-lg font-bold text-gray-800">Ghi chú thêm (Tùy chọn)</label>
            </div>
            <textarea 
              id="report-notes"
              rows={3} 
              value={description}
              onChange={(e) => setDescription(e.target.value)}
              className="w-full bg-gray-50 border border-gray-200 rounded-xl p-4 focus:bg-white focus:ring-2 focus:ring-emerald-500 focus:border-transparent outline-none resize-none transition-all text-gray-700 placeholder:text-gray-400"
              placeholder="Nhập ghi chú chi tiết cho người thu gom (ví dụ: Gọi tôi trước 10 phút, rác để ở cổng phụ...)"
            ></textarea>
          </div>
        </div>

        {/* NÚT SUBMIT */}
        <div className="pt-2">
          <button 
            type="submit" 
            disabled={isSubmitting}
            className="w-full bg-emerald-600 hover:bg-emerald-700 active:bg-emerald-800 text-white font-bold py-4 px-8 rounded-xl shadow-lg shadow-emerald-600/20 transition-all duration-200 disabled:opacity-70 disabled:cursor-not-allowed flex items-center justify-center gap-3 text-lg group"
          >
            {isSubmitting ? (
               <span className="flex items-center gap-2">
                 <div className="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin" />
                 Đang Xử Lý...
               </span>
            ) : (
              <>
                <Upload size={22} className="group-hover:-translate-y-1 transition-transform duration-200" />
                Gửi Báo Cáo {imageFiles.length > 0 ? `(${imageFiles.length} ảnh)` : ''}
              </>
            )}
          </button>
        </div>
      </form>
    </div>
  );
};