"use client";
import React from "react";
import Link from "next/link"; // Đã thêm Link để điều hướng
import { Award, Leaf, TrendingUp, Calendar, ChevronRight, Gift } from "lucide-react";

const mockLeaderboard = [
  { rank: 1, name: "Nguyễn Văn A", points: 2450, badge: "Thủ Lĩnh Xanh" },
  { rank: 2, name: "Trần Thị B", points: 1980, badge: "Hiệp Sĩ Môi Trường" },
  { rank: 3, name: "Lê Hoàng C", points: 1850, badge: "Chiến Binh Xanh" },
  { rank: 4, name: "Phạm D", points: 1720, badge: "" },
  { rank: 5, name: "Bạn (Nguyễn Dân)", points: 1240, badge: "", isMe: true },
];

const mockHistory = [
  { id: 1, date: "01/03/2026", reason: "Phân loại Nhựa đúng quy định", points: "+50" },
  { id: 2, date: "25/02/2026", reason: "Báo cáo thu gom lớn (>15kg)", points: "+100" },
  { id: 3, date: "20/02/2026", reason: "Duy trì hoạt động 4 tuần liên tiếp", points: "+200" },
];

export const RewardsTab: React.FC = () => {
  return (
    <div className="grid grid-cols-1 lg:grid-cols-3 gap-8 w-full">
      
      {/* CỘT 1: THÔNG TIN CÁ NHÂN & LỊCH SỬ */}
      <div className="col-span-1 lg:col-span-2 space-y-8">
        
        {/* Điểm số Card */}
        <div className="bg-gradient-to-br from-emerald-500 to-emerald-700 rounded-3xl p-8 text-white shadow-xl shadow-emerald-600/20 relative overflow-hidden transition-all hover:shadow-emerald-600/30">
          {/* Background Pattern */}
          <Leaf className="absolute -bottom-10 -right-10 w-64 h-64 text-emerald-800 opacity-20 transform -rotate-12" />
          
          <div className="relative z-10 flex flex-col md:flex-row justify-between items-start md:items-center gap-6">
            <div>
              <p className="text-emerald-50 font-medium tracking-wide text-sm mb-1 opacity-90">Tổng Điểm Hiện Tại</p>
              <div className="flex items-end gap-3">
                <h1 className="text-5xl md:text-6xl font-extrabold tracking-tight drop-shadow-sm">1,240</h1>
                <span className="text-xl font-medium text-emerald-100 pb-1.5 flex items-center gap-1">
                  <Award size={24} className="text-amber-300" /> GreenPoints
                </span>
              </div>
            </div>
            
            {/* NÚT ĐỔI QUÀ - Đã bọc bằng Link chuẩn bị cho trang sau này */}
            <Link 
              href="/citizen/rewards" // Đã trỏ đúng route
              className="bg-white text-emerald-700 hover:bg-emerald-50 hover:-translate-y-1 focus:ring-4 focus:ring-emerald-200 font-bold py-3.5 px-6 rounded-xl transition-all duration-200 shadow-md flex items-center gap-2 group"
            >
              <Gift size={20} className="group-hover:rotate-12 transition-transform" />
              Đổi Quà Ngay
            </Link>
          </div>
          
          {/* Thống kê nhanh */}
          <div className="relative z-10 mt-8 pt-6 border-t border-emerald-400/30 grid grid-cols-2 sm:grid-cols-3 gap-6">
            <div>
              <p className="text-xs text-emerald-100 uppercase tracking-wider mb-1">Hạng Hiện Tại</p>
              <p className="font-semibold text-lg flex items-center gap-2">
                Cư Dân Xanh
              </p>
            </div>
            <div>
              <p className="text-xs text-emerald-100 uppercase tracking-wider mb-1">Tổng Rác Đã Thu</p>
              <p className="font-semibold text-lg">45.5 kg</p>
            </div>
          </div>
        </div>

        {/* Lịch sử điểm nhanh */}
        <div>
          <h3 className="text-xl font-bold text-gray-900 mb-5 flex items-center gap-2">
            <TrendingUp size={24} className="text-emerald-500" />
            Lịch Sử Nhận Điểm
          </h3>
          <div className="bg-white rounded-2xl border border-gray-100 shadow-sm overflow-hidden divide-y divide-gray-50">
            {mockHistory.map((item) => (
              <div key={item.id} className="p-4 sm:p-5 flex items-center justify-between hover:bg-emerald-50/50 transition-colors group cursor-default">
                <div className="flex items-start gap-4">
                  <div className="bg-emerald-100 text-emerald-600 p-2.5 rounded-xl group-hover:bg-emerald-200 transition-colors">
                    <Calendar size={18} />
                  </div>
                  <div>
                    <h4 className="text-sm sm:text-base font-semibold text-gray-800">{item.reason}</h4>
                    <p className="text-xs sm:text-sm text-gray-500 mt-1">{item.date}</p>
                  </div>
                </div>
                <div className="text-emerald-600 font-bold text-lg bg-emerald-50 px-3 py-1.5 rounded-lg border border-emerald-100">
                  {item.points}
                </div>
              </div>
            ))}
            
            {/* NÚT XEM LỊCH SỬ - Đã bọc bằng Link */}
            <Link 
              href="/citizen/points-history" // Route dự kiến
              className="w-full py-4 text-sm text-emerald-600 font-bold hover:bg-emerald-50 transition-colors flex items-center justify-center gap-1 group"
            >
              Xem Toàn Bộ Lịch Sử 
              <ChevronRight size={18} className="group-hover:translate-x-1 transition-transform" />
            </Link>
          </div>
        </div>
      </div>

      {/* CỘT 2: BẢNG XẾP HẠNG */}
      <div className="col-span-1 border border-gray-100 bg-white rounded-3xl p-6 shadow-sm h-fit">
        <div className="flex items-center justify-between mb-6">
          <h3 className="text-lg font-bold text-gray-900 flex items-center gap-2">
            <Award size={24} className="text-amber-500" />
            Top Khu Vực
          </h3>
          <select className="text-sm bg-gray-50 border border-gray-200 rounded-lg text-gray-700 font-medium outline-none p-2 focus:ring-2 focus:ring-emerald-500 cursor-pointer">
            <option>Ba Đình</option>
            <option>Đống Đa</option>
            <option>Cầu Giấy</option>
          </select>
        </div>

        <div className="space-y-3">
          {mockLeaderboard.map((user) => (
            <div 
              key={user.rank} 
              className={`flex items-center justify-between p-3.5 rounded-xl transition-all duration-200
                ${user.isMe 
                  ? "bg-amber-50 border border-amber-200 shadow-sm transform scale-[1.02]" 
                  : "hover:bg-gray-50 border border-transparent hover:border-gray-100"}`}
            >
              <div className="flex items-center gap-3 sm:gap-4">
                {/* Huy hiệu thứ hạng */}
                <div className={`w-9 h-9 rounded-full flex justify-center items-center font-extrabold text-sm shadow-sm
                  ${user.rank === 1 ? "bg-gradient-to-br from-amber-300 to-amber-500 text-white" : 
                    user.rank === 2 ? "bg-gradient-to-br from-slate-200 to-slate-400 text-slate-800" : 
                    user.rank === 3 ? "bg-gradient-to-br from-orange-300 to-orange-500 text-white" : 
                    "bg-gray-100 text-gray-500"}
                `}>
                  #{user.rank}
                </div>
                
                {/* Tên & Badge */}
                <div>
                  <h4 className="text-sm font-bold text-gray-800 flex items-center gap-2">
                    {user.name}
                    {user.isMe && (
                      <span className="text-[10px] bg-amber-500 text-white px-2 py-0.5 rounded-full uppercase tracking-wider font-bold">
                        Bạn
                      </span>
                    )}
                  </h4>
                  {user.badge && (
                    <p className="text-[11px] text-emerald-600 font-semibold uppercase tracking-wide mt-0.5">
                      • {user.badge}
                    </p>
                  )}
                </div>
              </div>
              
              {/* Điểm */}
              <div className="text-right">
                <span className={`font-black text-sm ${user.isMe ? "text-amber-600" : "text-emerald-600"}`}>
                  {user.points}
                </span>
                <p className="text-[10px] text-gray-400 font-medium uppercase">pts</p>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
};