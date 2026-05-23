import React, { useState, useRef } from 'react';
import { UploadCloud, X, CheckCircle2, AlertCircle, Bot, RefreshCw } from 'lucide-react';

// Các loại rác trong hệ thống
const WASTE_CATEGORIES = [
  { id: 'organic', name: 'Rác hữu cơ', color: 'text-green-600', bg: 'bg-green-100', border: 'border-green-500' },
  { id: 'recyclable', name: 'Rác tái chế', color: 'text-blue-600', bg: 'bg-blue-100', border: 'border-blue-500' },
  { id: 'hazardous', name: 'Rác độc hại', color: 'text-red-600', bg: 'bg-red-100', border: 'border-red-500' },
  { id: 'general', name: 'Rác vô cơ khác', color: 'text-gray-600', bg: 'bg-gray-100', border: 'border-gray-500' },
];

export const WasteUpload: React.FC = () => {
  const [selectedImage, setSelectedImage] = useState<string | null>(null);
  const [file, setFile] = useState<File | null>(null);
  
  // States cho tính năng AI
  const [isAnalyzing, setIsAnalyzing] = useState(false);
  const [suggestedCategory, setSuggestedCategory] = useState<string | null>(null);
  const [confirmedCategory, setConfirmedCategory] = useState<string>('');
  const [description, setDescription] = useState('');

  const fileInputRef = useRef<HTMLInputElement>(null);

  // Xử lý khi người dùng chọn ảnh
  const handleImageChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const selected = e.target.files?.[0];
    if (selected) {
      setFile(selected);
      // Tạo URL tạm thời để preview ảnh ngay lập tức
      const imageUrl = URL.createObjectURL(selected);
      setSelectedImage(imageUrl);
      
      // Tự động kích hoạt AI "chạy bằng cơm" (Giả lập)
      simulateAIAnalysis();
    }
  };

  // Giả lập API gọi AI (Sau này mình sẽ thay bằng API .NET thật ở đây)
  const simulateAIAnalysis = () => {
    setIsAnalyzing(true);
    setSuggestedCategory(null);
    setConfirmedCategory('');

    setTimeout(() => {
      // Giả sử AI phân tích xong và đoán đây là Rác tái chế
      const fakeAiResult = 'recyclable'; 
      setSuggestedCategory(fakeAiResult);
      setConfirmedCategory(fakeAiResult); // Tự động điền kết quả AI đoán vào ô xác nhận
      setIsAnalyzing(false);
    }, 2000); // Quay vòng vòng 2 giây cho nó "nguy hiểm"
  };

  // Xóa ảnh để chọn lại
  const handleRemoveImage = () => {
    setSelectedImage(null);
    setFile(null);
    setSuggestedCategory(null);
    setConfirmedCategory('');
    if (fileInputRef.current) fileInputRef.current.value = '';
  };

  // Gửi form
  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    if (!confirmedCategory) {
      alert('Vui lòng xác nhận loại rác trước khi gửi!');
      return;
    }
    
    // Gói dữ liệu chuẩn bị gửi xuống Backend
    const submitData = {
      imageFile: file?.name, // Tạm in ra tên file, sau này sẽ gửi file thật lên Cloud
      category: confirmedCategory,
      description: description,
      isAiAssisted: !!suggestedCategory,
      didUserChangeAiSuggestion: suggestedCategory !== confirmedCategory
    };

    console.log('Dữ liệu chuẩn bị gửi đi:', submitData);
    alert(`Đã gửi báo cáo thành công!\nPhân loại: ${WASTE_CATEGORIES.find(c => c.id === confirmedCategory)?.name}`);
    
    // Reset form
    handleRemoveImage();
    setDescription('');
  };

  return (
    <div className="max-w-2xl mx-auto p-6 bg-white rounded-2xl shadow-sm border border-gray-100 animate-in fade-in duration-500">
      <h2 className="text-2xl font-bold text-gray-800 mb-6 flex items-center gap-2">
        Báo cáo rác thải <Bot className="text-blue-500" size={28}/>
      </h2>

      <form onSubmit={handleSubmit} className="space-y-6">
        
        {/* KHU VỰC UPLOAD ẢNH */}
        {!selectedImage ? (
          <div 
            onClick={() => fileInputRef.current?.click()}
            className="border-2 border-dashed border-gray-300 rounded-xl p-10 text-center cursor-pointer hover:bg-gray-50 hover:border-blue-400 transition-all group"
          >
            <div className="mx-auto w-16 h-16 bg-blue-50 text-blue-500 rounded-full flex items-center justify-center mb-4 group-hover:scale-110 transition-transform">
              <UploadCloud size={32} />
            </div>
            <h3 className="text-lg font-medium text-gray-700 mb-1">Nhấn để tải ảnh lên</h3>
            <p className="text-sm text-gray-400">Hỗ trợ JPG, PNG (Tối đa 5MB)</p>
            <input 
              type="file" 
              ref={fileInputRef} 
              onChange={handleImageChange} 
              accept="image/*" 
              className="hidden" 
            />
          </div>
        ) : (
          <div className="relative rounded-xl overflow-hidden border border-gray-200 bg-gray-50">
            <img src={selectedImage} alt="Preview" className="w-full h-64 object-contain" />
            <button 
              type="button" 
              onClick={handleRemoveImage}
              className="absolute top-3 right-3 p-2 bg-white/80 hover:bg-white text-gray-700 rounded-full shadow-sm transition-all"
            >
              <X size={20} />
            </button>
          </div>
        )}

        {/* KHU VỰC AI PHÂN TÍCH & XÁC NHẬN */}
        {selectedImage && (
          <div className="bg-blue-50/50 p-5 rounded-xl border border-blue-100 space-y-4">
            
            {/* Trạng thái AI đang load */}
            {isAnalyzing && (
              <div className="flex items-center gap-3 text-blue-600 font-medium">
                <RefreshCw className="animate-spin" size={20} />
                <span>Trợ lý AI đang phân tích hình ảnh...</span>
              </div>
            )}

            {/* Trạng thái AI phân tích xong */}
            {!isAnalyzing && suggestedCategory && (
              <div className="animate-in slide-in-from-top-4 duration-300">
                <div className="flex items-start gap-3 mb-4">
                  <div className="mt-1"><Bot className="text-blue-600" size={24}/></div>
                  <div>
                    <h4 className="font-semibold text-gray-800">AI Gợi ý phân loại:</h4>
                    <p className="text-sm text-gray-600">Dựa vào hình ảnh, hệ thống dự đoán đây là:</p>
                    
                    {/* Hiển thị kết quả của AI */}
                    <div className="mt-2 inline-flex items-center gap-2 px-3 py-1.5 bg-white border border-blue-200 text-blue-700 rounded-lg shadow-sm font-medium">
                      <CheckCircle2 size={18} className="text-blue-500"/>
                      {WASTE_CATEGORIES.find(c => c.id === suggestedCategory)?.name}
                    </div>
                  </div>
                </div>

                {/* Phần người dùng xác nhận lại */}
                <div className="bg-white p-4 rounded-lg border border-gray-200">
                  <label className="block text-sm font-medium text-gray-700 mb-2 flex items-center gap-2">
                    <AlertCircle size={16} className="text-amber-500"/> 
                    Bạn có đồng ý với AI không? (Có thể chọn lại)
                  </label>
                  <div className="grid grid-cols-2 sm:grid-cols-4 gap-2">
                    {WASTE_CATEGORIES.map(cat => (
                      <div 
                        key={cat.id}
                        onClick={() => setConfirmedCategory(cat.id)}
                        className={`cursor-pointer text-center py-2 px-3 rounded-lg border text-sm font-medium transition-all ${
                          confirmedCategory === cat.id 
                            ? `${cat.bg} ${cat.border} ${cat.color} shadow-sm ring-1 ring-current` 
                            : 'border-gray-200 text-gray-600 hover:bg-gray-50'
                        }`}
                      >
                        {cat.name}
                      </div>
                    ))}
                  </div>
                </div>
              </div>
            )}
          </div>
        )}

        {/* KHU VỰC GHI CHÚ */}
        <div>
          <label className="block text-sm font-medium text-gray-700 mb-2">Ghi chú thêm (Không bắt buộc)</label>
          <textarea 
            rows={3} 
            className="w-full border border-gray-300 rounded-xl p-3 focus:ring-2 focus:ring-blue-500 outline-none resize-none"
            placeholder="Ví dụ: Rác nằm ở góc đường..."
            value={description}
            onChange={(e) => setDescription(e.target.value)}
          ></textarea>
        </div>

        {/* NÚT GỬI */}
        <button 
          type="submit" 
          disabled={!selectedImage || isAnalyzing || !confirmedCategory}
          className="w-full py-3 bg-blue-600 hover:bg-blue-700 text-white font-semibold rounded-xl disabled:bg-gray-300 disabled:cursor-not-allowed transition-colors"
        >
          {isAnalyzing ? 'Vui lòng đợi AI...' : 'Gửi Báo Cáo Rác Thải'}
        </button>
      </form>
    </div>
  );
};