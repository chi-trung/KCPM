"use client";
import React, { useState, useEffect } from "react";
import Link from "next/link";
import { ArrowLeft, Award, Gift, Ticket, Heart, Search, ShoppingBag, Loader2 } from "lucide-react";
import { citizenRewardApi, CitizenRewardData } from "@/lib/api/citizenRewardApi";

// Mock rewards data (will be replaced with API when available)
const mockRewards = [
  { id: 1, title: "Voucher Highlands Coffee 50K", points: 500, category: "voucher", image: "☕", color: "bg-orange-100 text-orange-600" },
  { id: 2, title: "Túi rác sinh học phân hủy hoàn toàn (Cuộn)", points: 250, category: "physical", image: "♻️", color: "bg-emerald-100 text-emerald-600" },
  { id: 3, title: "Đóng góp Quỹ Trồng Rừng Tương Lai", points: 1000, category: "charity", image: "🌳", color: "bg-green-100 text-green-600" },
  { id: 4, title: "Voucher CGV Cinema 1 vé 2D", points: 800, category: "voucher", image: "🎟️", color: "bg-red-100 text-red-600" },
  { id: 5, title: "Bình nước giữ nhiệt Eco-friendly", points: 1200, category: "physical", image: "🥤", color: "bg-blue-100 text-blue-600" },
  { id: 6, title: "Mã giảm giá 20% Shopee Xtra", points: 300, category: "voucher", image: "🛍️", color: "bg-orange-100 text-orange-600" },
];

export default function RewardsStorePage() {
  const [filter, setFilter] = useState("all");
  const [rewardData, setRewardData] = useState<CitizenRewardData | null>(null);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    loadRewardData();
  }, []);

  const loadRewardData = async () => {
    try {
      setLoading(true);
      const data = await citizenRewardApi.getRewards();
      setRewardData(data);
    } catch (err) {
      setError("Không thể tải dữ liệu điểm thưởng");
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const userPoints = rewardData?.totalPoints || 0;

  const filteredRewards = filter === "all" 
    ? mockRewards 
    : mockRewards.filter(r => r.category === filter);

  return (
    <div className="min-h-screen bg-gray-50/50 py-8">
      <div className="max-w-6xl mx-auto px-4 sm:px-6 lg:px-8 space-y-8">
        
        {/* Nút quay lại & Tiêu đề */}
        <div className="flex items-center gap-4">
          <Link href="/citizen" className="p-2 hover:bg-white rounded-full transition-colors border border-transparent hover:border-gray-200 shadow-sm">
            <ArrowLeft size={24} className="text-gray-600" />
          </Link>
          <div>
            <h1 className="text-2xl sm:text-3xl font-bold text-gray-900 flex items-center gap-2">
              <Gift className="text-emerald-500" /> Cửa Hàng Đổi Quà
            </h1>
            <p className="text-sm text-gray-500 mt-1">Sử dụng GreenPoints của bạn để đổi lấy những phần quà hấp dẫn</p>
          </div>
        </div>

        {/* Banner Số điểm hiện có */}
        <div className="bg-gradient-to-r from-emerald-600 to-teal-600 rounded-3xl p-6 sm:p-8 text-white shadow-lg flex flex-col sm:flex-row items-center justify-between gap-6 relative overflow-hidden">
          <div className="relative z-10 flex items-center gap-4">
            <div className="bg-white/20 p-4 rounded-2xl backdrop-blur-sm">
              <Award size={40} className="text-amber-300" />
            </div>
            <div>
              <p className="text-emerald-100 font-medium tracking-wider text-sm uppercase">Điểm khả dụng của bạn</p>
              <h2 className="text-4xl font-black mt-1">{userPoints.toLocaleString()} <span className="text-xl font-medium opacity-80">pts</span></h2>
            </div>
          </div>
          <div className="relative z-10 w-full sm:w-auto">
            <div className="bg-white/10 backdrop-blur-md border border-white/20 rounded-xl p-2 flex items-center gap-2">
              <Search size={20} className="text-emerald-100 ml-2" />
              <input 
                type="text" 
                placeholder="Tìm kiếm quà tặng..." 
                className="bg-transparent border-none text-white placeholder-emerald-100/70 focus:ring-0 outline-none w-full sm:w-48"
              />
            </div>
          </div>
        </div>

        {/* Bộ lọc Danh mục */}
        <div className="flex gap-3 overflow-x-auto pb-2 hide-scrollbar">
          {[
            { id: "all", label: "Tất cả", icon: ShoppingBag },
            { id: "voucher", label: "Voucher & Giảm giá", icon: Ticket },
            { id: "physical", label: "Quà Hiện vật", icon: Gift },
            { id: "charity", label: "Quyên góp", icon: Heart },
          ].map(cat => (
            <button
              key={cat.id}
              onClick={() => setFilter(cat.id)}
              className={`flex items-center gap-2 px-5 py-2.5 rounded-xl font-semibold whitespace-nowrap transition-all
                ${filter === cat.id 
                  ? "bg-emerald-600 text-white shadow-md" 
                  : "bg-white text-gray-600 border border-gray-200 hover:border-emerald-300 hover:bg-emerald-50"}`}
            >
              <cat.icon size={18} />
              {cat.label}
            </button>
          ))}
        </div>

        {/* Lưới Quà Tặng */}
        <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6">
          {filteredRewards.map(reward => (
            <div key={reward.id} className="bg-white rounded-2xl p-5 border border-gray-100 shadow-sm hover:shadow-xl hover:-translate-y-1 transition-all duration-300 flex flex-col">
              <div className={`w-16 h-16 rounded-2xl flex items-center justify-center text-3xl mb-4 shadow-inner ${reward.color}`}>
                {reward.image}
              </div>
              <h3 className="font-bold text-gray-800 text-lg leading-tight mb-2 line-clamp-2 flex-1">
                {reward.title}
              </h3>
              <div className="flex items-end justify-between mt-4 pt-4 border-t border-gray-50">
                <div>
                  <p className="text-xs text-gray-500 font-medium mb-0.5">Giá đổi</p>
                  <p className="font-bold text-emerald-600 flex items-center gap-1">
                    {reward.points} <span className="text-xs font-normal">pts</span>
                  </p>
                </div>
                <button 
                  disabled={userPoints < reward.points}
                  className={`px-4 py-2 rounded-lg font-bold text-sm transition-colors ${
                    userPoints >= reward.points
                      ? "bg-emerald-100 text-emerald-700 hover:bg-emerald-600 hover:text-white"
                      : "bg-gray-100 text-gray-400 cursor-not-allowed"
                  }`}
                >
                  Đổi Quà
                </button>
              </div>
            </div>
          ))}
        </div>
      </div>
    </div>
  );
}