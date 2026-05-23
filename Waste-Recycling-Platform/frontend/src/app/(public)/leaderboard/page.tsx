"use client";
import React, { useState, useMemo, useEffect } from "react";
import { Trophy, Users, MapPin, Calendar, Crown, ArrowLeft, Star } from "lucide-react";
import { API_CONFIG } from "@/lib/api/config";
import { useAuth } from "@/contexts/AuthContext";
import { useRouter } from "next/navigation"; // Import thêm useRouter để làm chức năng Quay Lại

interface LeaderboardEntry {
  id: string;
  name: string;
  avatar?: string;
  points: number;
  reports: number;
  rank: number;
  change: number;
  area: string;
  badges: string[];
  level: string;
}

interface AreaLeaderboard {
  area: string;
  totalPoints: number;
  totalReports: number;
  participants: number;
  rank: number;
  change: number;
}

const timeRanges = [
  { value: "month", label: "Tháng này" },
  { value: "quarter", label: "Quý này" },
  { value: "year", label: "Năm nay" },
  { value: "all", label: "Tất cả thời gian" }
];

const randomAvatars = ["👩‍💼", "👨‍🌾", "👩‍🎓", "👨‍💻", "👩‍🏫", "👨‍🔧", "👩‍⚕️"];

export default function LeaderboardPage() {
  const { user } = useAuth(); 
  const router = useRouter(); // Khởi tạo router
  const [activeTab, setActiveTab] = useState<"individual" | "area">("individual");
  const [timeRange, setTimeRange] = useState("all");
  const [searchTerm, setSearchTerm] = useState("");
  
  const [individualLeaders, setIndividualLeaders] = useState<LeaderboardEntry[]>([]);
  const [areaLeaders, setAreaLeaders] = useState<AreaLeaderboard[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const fetchIndividualLeaderboard = async () => {
      try {
        setLoading(true);
        const response = await fetch(`${API_CONFIG.BASE_URL}/citizens/rewards/leaderboard?page=1&pageSize=10`);
        if (response.ok) {
          const json = await response.json();
          const apiData = json.data || [];
          
          const formatted = apiData.map((item: any, index: number): LeaderboardEntry => {
            let level = "Thành viên Đồng";
            let badges = ["Mới tham gia"];
            if (item.totalPoints >= 2000) { level = "Thành viên Bạch Kim"; badges = ["Chiến thần xanh", "Thành viên VIP"]; }
            else if (item.totalPoints >= 1000) { level = "Thành viên Vàng"; badges = ["Chuyên gia", "Báo cáo nhanh"]; }
            else if (item.totalPoints >= 500) { level = "Thành viên Bạc"; badges = ["Tích cực"]; }

            return {
              id: item.citizenId,
              name: item.citizenName,
              points: item.totalPoints,
              reports: item.reportCount,
              rank: index + 1,
              change: 0,
              area: "Hồ Chí Minh",
              badges: badges,
              level: level,
              avatar: randomAvatars[index % randomAvatars.length]
            };
          });
          setIndividualLeaders(formatted);
        }
      } catch (error) {
        console.error("Lỗi fetch leaderboard:", error);
      } finally {
        setLoading(false);
      }
    };

    const fetchAreaLeaderboard = async () => {
      try {
        const response = await fetch(`${API_CONFIG.BASE_URL}/citizens/rewards/leaderboard/area?page=1&pageSize=10`);
        if (response.ok) {
          const json = await response.json();
          const apiData = json.data || [];
          const formatted = apiData.map((item: any, index: number) => ({
            area: item.area,
            totalPoints: item.totalPoints,
            totalReports: item.totalReports,
            participants: item.participants,
            rank: index + 1,
            change: 0
          }));
          setAreaLeaders(formatted);
        }
      } catch (error) {
        console.error("Lỗi fetch area leaderboard:", error);
      }
    };

    fetchIndividualLeaderboard();
    fetchAreaLeaderboard();
  }, []);

  const filteredIndividuals = useMemo(() => {
    return individualLeaders.filter(person =>
      person.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
      person.area.toLowerCase().includes(searchTerm.toLowerCase())
    );
  }, [individualLeaders, searchTerm]);

  const getPodiumStyle = (rank: number) => {
    switch (rank) {
      case 1:
        return {
          wrapper: "order-1 md:order-2 z-10 w-full md:w-5/12 transform md:-translate-y-6",
          card: "bg-gradient-to-b from-yellow-50 to-white border-2 border-yellow-300 shadow-[0_0_40px_rgba(234,179,8,0.3)]",
          circle: "bg-gradient-to-br from-yellow-300 to-yellow-500 shadow-lg shadow-yellow-500/50 text-white w-14 h-14 text-xl",
          avatarSize: "text-7xl",
          pointColor: "bg-clip-text text-transparent bg-gradient-to-r from-yellow-600 to-orange-500"
        };
      case 2:
        return {
          wrapper: "order-2 md:order-1 w-full md:w-4/12 mt-6 md:mt-0 opacity-95 hover:opacity-100",
          card: "bg-gradient-to-b from-slate-50 to-white border border-slate-300 shadow-[0_0_30px_rgba(148,163,184,0.2)]",
          circle: "bg-gradient-to-br from-slate-300 to-slate-500 shadow-lg shadow-slate-500/40 text-white w-12 h-12 text-lg",
          avatarSize: "text-6xl",
          pointColor: "bg-clip-text text-transparent bg-gradient-to-r from-slate-600 to-slate-800"
        };
      case 3:
        return {
          wrapper: "order-3 w-full md:w-4/12 mt-6 md:mt-0 opacity-95 hover:opacity-100",
          card: "bg-gradient-to-b from-orange-50 to-white border border-orange-300 shadow-[0_0_30px_rgba(251,146,60,0.2)]",
          circle: "bg-gradient-to-br from-orange-300 to-orange-500 shadow-lg shadow-orange-500/40 text-white w-12 h-12 text-lg",
          avatarSize: "text-6xl",
          pointColor: "bg-clip-text text-transparent bg-gradient-to-r from-orange-600 to-red-500"
        };
      default:
        return null;
    }
  };

  const formatNumber = (num: number) => {
    return num.toLocaleString('vi-VN');
  };

  return (
    <div className="min-h-screen bg-[#F8FAFC]">
      {/* Header - Có thêm thanh điều hướng nếu đã login */}
      <div className="bg-gradient-to-r from-[#065F46] via-[#047857] to-[#059669] text-white shadow-lg relative overflow-hidden">
        <div className="absolute top-0 right-0 w-64 h-64 bg-white opacity-5 rounded-full blur-3xl transform translate-x-1/2 -translate-y-1/2"></div>
        <div className="absolute bottom-0 left-0 w-48 h-48 bg-emerald-300 opacity-10 rounded-full blur-2xl transform -translate-x-1/2 translate-y-1/2"></div>
        
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6 relative z-10">
          
          {/* NÚT QUAY LẠI (Chỉ hiện khi đã đăng nhập) */}
          {user && (
            <button 
              onClick={() => router.back()} 
              className="flex items-center gap-2 text-emerald-100 hover:text-white font-semibold transition-colors mb-6 bg-white/10 w-fit px-4 py-2 rounded-lg backdrop-blur-sm"
            >
              <ArrowLeft size={18} /> Quay lại Bảng điều khiển
            </button>
          )}

          <div className="flex items-center gap-4 mb-3">
            <div className="p-3 bg-white/20 rounded-xl backdrop-blur-sm">
              <Trophy className="w-8 h-8 text-yellow-300" />
            </div>
            <div>
              <h1 className="text-3xl font-extrabold tracking-tight">Bảng Xếp Hạng</h1>
              <p className="text-emerald-100 mt-1 font-medium">Vinh danh các Chiến thần xanh bảo vệ môi trường</p>
            </div>
          </div>
        </div>
      </div>

      {/* Controls */}
      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 relative -mt-6 z-40">
        <div className="bg-white/80 backdrop-blur-md border border-white shadow-xl rounded-2xl p-4 flex flex-col md:flex-row gap-4 items-center justify-between">
          <div className="flex gap-2 p-1 bg-slate-100 rounded-xl">
            <button
              onClick={() => setActiveTab("individual")}
              className={`px-5 py-2.5 rounded-lg font-semibold transition-all duration-300 flex items-center gap-2 ${
                activeTab === "individual" 
                ? "bg-white text-emerald-600 shadow-sm transform scale-[1.02]" 
                : "text-slate-500 hover:text-slate-700 hover:bg-slate-200/50"
              }`}
            >
              <Users className="w-4 h-4" /> Cá nhân
            </button>
            <button
              onClick={() => setActiveTab("area")}
              className={`px-5 py-2.5 rounded-lg font-semibold transition-all duration-300 flex items-center gap-2 ${
                activeTab === "area" 
                ? "bg-white text-emerald-600 shadow-sm transform scale-[1.02]" 
                : "text-slate-500 hover:text-slate-700 hover:bg-slate-200/50"
              }`}
            >
              <MapPin className="w-4 h-4" /> Khu vực
            </button>
          </div>

          <div className="flex items-center gap-3 w-full md:w-auto">
            <div className="flex items-center gap-2 border border-slate-200 rounded-xl px-4 py-2.5 bg-white flex-1 md:flex-none shadow-sm focus-within:ring-2 focus-within:ring-emerald-500/20 focus-within:border-emerald-500 transition-all">
              <Calendar className="w-4 h-4 text-emerald-600" />
              <select
                value={timeRange}
                onChange={(e) => setTimeRange(e.target.value)}
                className="bg-transparent text-sm font-medium text-slate-700 focus:outline-none w-full cursor-pointer"
              >
                {timeRanges.map(range => (
                  <option key={range.value} value={range.value}>{range.label}</option>
                ))}
              </select>
            </div>

            {activeTab === "individual" && (
              <div className="relative flex-1 md:flex-none">
                <Users className="absolute left-3 top-1/2 transform -translate-y-1/2 text-emerald-600 w-4 h-4" />
                <input
                  type="text"
                  placeholder="Tìm kiếm nhanh..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full pl-10 pr-4 py-2.5 text-sm font-medium text-slate-700 border border-slate-200 rounded-xl focus:ring-2 focus:ring-emerald-500/20 focus:border-emerald-500 outline-none shadow-sm transition-all placeholder:text-slate-400"
                />
              </div>
            )}
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-12">
        {activeTab === "individual" ? (
          <div className="flex flex-col gap-8">
            {loading ? (
              <div className="text-center py-12">
                 <div className="animate-spin rounded-full h-12 w-12 border-b-2 border-emerald-600 mx-auto"></div>
                 <p className="mt-4 text-slate-500 font-medium">Đang cập nhật bảng vàng...</p>
              </div>
            ) : filteredIndividuals.length === 0 ? (
              <div className="text-center py-12 text-slate-500 font-medium bg-white rounded-2xl border border-dashed border-slate-300">Không tìm thấy dữ liệu.</div>
            ) : (
              <>
                <div className="flex flex-col md:flex-row justify-center items-end gap-6 pt-10 pb-8 px-4">
                  {filteredIndividuals.slice(0, 3).map((person) => {
                    const style = getPodiumStyle(person.rank);
                    if (!style) return null;
                    const isMe = user?.id === person.id; // CHECK: Người dùng hiện tại

                    return (
                      <div key={person.id} className={`${style.wrapper} transition-transform duration-500 ease-out`}>
                        <div className={`relative rounded-3xl p-6 md:p-8 flex flex-col items-center hover:-translate-y-2 transition-all duration-300 ease-out cursor-default ${style.card} ${isMe ? 'ring-4 ring-emerald-500 ring-offset-2' : ''}`}>
                          
                          {/* NHÃN ĐÁNH DẤU "BẠN" */}
                          {isMe && (
                            <div className="absolute top-4 right-4 bg-emerald-500 text-white text-xs font-bold px-3 py-1 rounded-full shadow-md z-20 animate-pulse">
                              BẠN
                            </div>
                          )}

                          {person.rank === 1 && (
                            <div className="absolute -top-12 left-1/2 transform -translate-x-1/2 text-yellow-400 animate-bounce">
                              <Crown className="w-8 h-8 drop-shadow-md" />
                            </div>
                          )}

                          <div className="absolute -top-6 left-1/2 transform -translate-x-1/2">
                            <div className={`rounded-full flex items-center justify-center font-bold border-4 border-white ${style.circle}`}>
                              #{person.rank}
                            </div>
                          </div>

                          <div className={`${style.avatarSize} mb-4 mt-4 drop-shadow-xl transform group-hover:scale-110 transition-transform`}>
                            {person.avatar}
                          </div>
                          
                          <h3 className="text-xl font-extrabold text-slate-800 text-center mb-2 line-clamp-1">{person.name}</h3>
                          
                          <div className="px-4 py-1.5 bg-emerald-50 text-emerald-700 border border-emerald-100 rounded-full text-xs font-bold tracking-wide uppercase mb-6 shadow-sm">
                            {person.level}
                          </div>

                          <div className="w-full space-y-3 mb-6 bg-white/60 rounded-2xl p-4 backdrop-blur-sm border border-white">
                            <div className="flex justify-between items-center">
                              <span className="text-slate-500 font-semibold text-sm">Điểm số</span>
                              <span className={`font-black text-xl ${style.pointColor}`}>
                                {formatNumber(person.points)}
                              </span>
                            </div>
                            <div className="h-px w-full bg-slate-200/60"></div>
                            <div className="flex justify-between items-center">
                              <span className="text-slate-500 font-semibold text-sm">Báo cáo</span>
                              <span className="font-bold text-slate-700">{formatNumber(person.reports)}</span>
                            </div>
                          </div>

                          <div className="flex flex-wrap gap-2 justify-center mt-auto">
                            {person.badges.map((badge, i) => (
                              <span key={i} className="px-3 py-1.5 bg-white shadow-sm rounded-full text-[11px] font-bold text-slate-600 border border-slate-100">
                                {badge}
                              </span>
                            ))}
                          </div>
                        </div>
                      </div>
                    );
                  })}
                </div>

                {/* Danh sách còn lại */}
                <div className="flex flex-col gap-3">
                  {filteredIndividuals.slice(3).map((person) => {
                    const isMe = user?.id === person.id;
                    return (
                      <div 
                        key={person.id} 
                        className={`group rounded-2xl p-4 sm:p-5 flex flex-col sm:flex-row items-center gap-5 border transition-all duration-300 ease-out cursor-default
                        ${isMe ? 'bg-emerald-50 border-emerald-400 shadow-md' : 'bg-white border-slate-200 hover:border-emerald-300 hover:shadow-lg hover:shadow-emerald-900/5'}`}
                      >
                        <div className={`flex-shrink-0 w-14 h-14 rounded-full border flex items-center justify-center font-black text-lg transition-colors
                          ${isMe ? 'bg-emerald-500 text-white border-emerald-600' : 'bg-slate-50 border-slate-200 text-slate-400 group-hover:bg-emerald-50 group-hover:text-emerald-600 group-hover:border-emerald-200'}`}>
                          #{person.rank}
                        </div>
                        
                        <div className="text-4xl hidden sm:block transform group-hover:scale-110 transition-transform duration-300">
                          {person.avatar}
                        </div>
                        
                        <div className="flex-1 text-center sm:text-left">
                          <div className="flex items-center justify-center sm:justify-start gap-2">
                            <h3 className={`font-bold text-lg transition-colors ${isMe ? 'text-emerald-800' : 'text-slate-800 group-hover:text-emerald-700'}`}>{person.name}</h3>
                            {isMe && <span className="bg-emerald-500 text-white text-[10px] px-2 py-0.5 rounded uppercase font-bold">Bạn</span>}
                          </div>
                          <p className="text-sm font-medium text-slate-500 flex justify-center sm:justify-start items-center gap-1.5 mt-1">
                            <MapPin className="w-3.5 h-3.5" /> {person.area}
                          </p>
                        </div>
                        
                        <div className="hidden md:flex items-center gap-2 px-4">
                          <span className={`px-3 py-1 rounded-lg text-xs font-bold uppercase tracking-wider ${isMe ? 'bg-emerald-100 text-emerald-700' : 'bg-slate-100 text-slate-600'}`}>
                            {person.level}
                          </span>
                        </div>

                        <div className="text-center sm:text-right mt-4 sm:mt-0 min-w-[120px]">
                          <div className={`text-xl font-black ${isMe ? 'text-emerald-700' : 'text-emerald-600'}`}>{formatNumber(person.points)}</div>
                          <div className="text-xs font-bold text-slate-400 uppercase tracking-wide mt-1">{formatNumber(person.reports)} báo cáo</div>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </>
            )}
          </div>
        ) : (
          /* Area Leaderboard */
          <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
            {areaLeaders.map((area) => (
              <div 
                key={area.area} 
                className="group bg-white rounded-2xl p-6 border border-slate-200 hover:border-emerald-300 hover:shadow-xl hover:shadow-emerald-900/5 transition-all duration-300 relative overflow-hidden cursor-default hover:-translate-y-1"
              >
                <div className="absolute -right-10 -top-10 w-32 h-32 bg-emerald-50 rounded-full opacity-50 group-hover:scale-150 transition-transform duration-500"></div>

                <div className="relative z-10 flex items-start gap-5">
                  <div className={`flex-shrink-0 w-12 h-12 rounded-xl flex items-center justify-center font-black text-lg shadow-sm
                    ${area.rank === 1 ? 'bg-gradient-to-br from-yellow-300 to-yellow-500 text-white shadow-yellow-500/30' : 
                      area.rank === 2 ? 'bg-gradient-to-br from-slate-300 to-slate-400 text-white shadow-slate-500/30' : 
                      area.rank === 3 ? 'bg-gradient-to-br from-orange-300 to-orange-400 text-white shadow-orange-500/30' : 
                      'bg-slate-100 text-slate-500'}`}
                  >
                    #{area.rank}
                  </div>
                  
                  <div className="flex-1">
                    <h3 className="text-lg font-bold text-slate-800 group-hover:text-emerald-700 transition-colors">{area.area}</h3>
                    <div className="flex flex-wrap items-center gap-4 mt-3">
                      <div className="flex items-center gap-1.5 bg-slate-50 px-2.5 py-1 rounded-md border border-slate-100">
                        <Users className="w-3.5 h-3.5 text-blue-500" /> 
                        <span className="text-xs font-bold text-slate-600">{formatNumber(area.participants)} người</span>
                      </div>
                      <div className="flex items-center gap-1.5 bg-slate-50 px-2.5 py-1 rounded-md border border-slate-100">
                        <Star className="w-3.5 h-3.5 text-yellow-500" /> 
                        <span className="text-xs font-bold text-slate-600">{formatNumber(area.totalReports)} báo cáo</span>
                      </div>
                    </div>
                  </div>

                  <div className="text-right">
                    <div className="text-2xl font-black bg-clip-text text-transparent bg-gradient-to-r from-emerald-600 to-teal-500">
                      {formatNumber(area.totalPoints)}
                    </div>
                    <div className="text-xs font-bold text-slate-400 uppercase tracking-wider mt-1">điểm</div>
                  </div>
                </div>
              </div>
            ))}
          </div>
        )}
      </div>
    </div>
  );
}