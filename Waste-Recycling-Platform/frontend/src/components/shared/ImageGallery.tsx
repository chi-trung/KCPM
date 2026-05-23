import React, { useState, useEffect } from 'react';
import { X, ChevronLeft, ChevronRight } from 'lucide-react';
import { Portal } from './Portal';

interface ImageGalleryProps {
  images: string[];
  isOpen: boolean;
  onClose: () => void;
  title?: string;
}

export const ImageGallery: React.FC<ImageGalleryProps> = ({ 
  images, 
  isOpen, 
  onClose, 
  title = "Hình ảnh" 
}) => {
  const [currentIndex, setCurrentIndex] = useState(0);

  // Reset về ảnh đầu tiên mỗi khi mở lại modal
  useEffect(() => {
    if (isOpen) {
      setCurrentIndex(0);
      // Khóa cuộn chuột nền khi mở ảnh
      document.body.style.overflow = 'hidden';
    } else {
      document.body.style.overflow = 'unset';
    }
    return () => { document.body.style.overflow = 'unset'; };
  }, [isOpen]);

  // Lắng nghe phím ESC để đóng
  useEffect(() => {
    const handleKeyDown = (e: KeyboardEvent) => {
      if (e.key === 'Escape') onClose();
      if (e.key === 'ArrowRight') nextImage(e as any);
      if (e.key === 'ArrowLeft') prevImage(e as any);
    };
    if (isOpen) window.addEventListener('keydown', handleKeyDown);
    return () => window.removeEventListener('keydown', handleKeyDown);
  }, [isOpen, currentIndex, images.length]);

  if (!isOpen || !images || images.length === 0) return null;

  const nextImage = (e: React.MouseEvent) => {
    e?.stopPropagation();
    setCurrentIndex((prev) => (prev === images.length - 1 ? 0 : prev + 1));
  };

  const prevImage = (e: React.MouseEvent) => {
    e?.stopPropagation();
    setCurrentIndex((prev) => (prev === 0 ? images.length - 1 : prev - 1));
  };

  return (
    <Portal>
      <div className="fixed inset-0 z-[9999] flex items-center justify-center bg-slate-900/95 backdrop-blur-md animate-in fade-in duration-200">
        
        {/* Header: Tiêu đề & Nút đóng */}
        <div className="absolute top-0 inset-x-0 p-6 flex justify-between items-center bg-gradient-to-b from-black/60 to-transparent z-10">
          <h3 className="text-white font-bold text-lg tracking-wide">{title}</h3>
          <button 
            onClick={onClose} 
            className="p-2.5 bg-white/10 hover:bg-white/20 text-white rounded-full backdrop-blur-sm transition-all hover:scale-110"
            title="Đóng (ESC)"
          >
            <X size={24} />
          </button>
        </div>

        {/* Vùng hiển thị ảnh chính */}
        <div 
          className="relative w-full max-w-6xl h-full max-h-[85vh] p-4 flex items-center justify-center" 
          onClick={onClose} /* Click ra ngoài viền ảnh để đóng */
        >
          <img
            src={images[currentIndex]}
            alt={`Gallery image ${currentIndex + 1}`}
            className="max-w-full max-h-full object-contain rounded-xl shadow-2xl animate-in zoom-in-95 duration-300"
            onClick={(e) => e.stopPropagation()} // Click vào trong ảnh thì không bị đóng
            onError={(e) => {
              (e.target as HTMLImageElement).src = "data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg' width='100' height='100'%3E%3Crect fill='%231e293b' width='100' height='100'/%3E%3Ctext x='50%' y='50%' text-anchor='middle' dy='.3em' fill='%2394a3b8' font-size='10'%3ELỗi tải ảnh%3C/text%3E%3C/svg%3E";
            }}
          />

          {/* Mũi tên điều hướng (Chỉ hiện khi có >= 2 ảnh) */}
          {images.length > 1 && (
            <>
              <button
                onClick={prevImage}
                className="absolute left-6 md:left-12 p-4 bg-black/40 hover:bg-white/20 text-white rounded-full backdrop-blur-md transition-all transform hover:scale-110 border border-white/10"
                title="Ảnh trước (Mũi tên trái)"
              >
                <ChevronLeft size={32} />
              </button>
              <button
                onClick={nextImage}
                className="absolute right-6 md:right-12 p-4 bg-black/40 hover:bg-white/20 text-white rounded-full backdrop-blur-md transition-all transform hover:scale-110 border border-white/10"
                title="Ảnh tiếp (Mũi tên phải)"
              >
                <ChevronRight size={32} />
              </button>
            </>
          )}
        </div>

        {/* Bộ đếm số thứ tự ảnh */}
        {images.length > 1 && (
          <div className="absolute bottom-8 left-1/2 -translate-x-1/2 px-5 py-2.5 bg-black/50 border border-white/10 text-white text-sm font-bold tracking-widest rounded-full backdrop-blur-md shadow-lg">
            {currentIndex + 1} / {images.length}
          </div>
        )}
      </div>
    </Portal>
  );
};