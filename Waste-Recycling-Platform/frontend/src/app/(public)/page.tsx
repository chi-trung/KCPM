"use client";
import React, { useState, useEffect } from "react";
import Link from "next/link";
import Image from "next/image";
import { API_CONFIG } from "@/lib/api/config";
import { 
  ArrowRight, 
  MapPin, 
  Recycle, 
  Trophy, 
  Users, 
  TreePine,
  Leaf
} from "lucide-react";

export default function Home() {
  // Khởi tạo state với các số bằng 0. Hoàn toàn không có data giả.
  const [stats, setStats] = useState({
    totalWaste: 0,
    totalPoints: 0,
    totalUsers: 0,
    treesSaved: 0
  });

  useEffect(() => {
    // Gọi API lấy dữ liệu thật từ Backend
    const fetchPublicStats = async () => {
      try {
        // GỌI API PUBLIC (Không cần Authorization Bearer)
        // TODO: Sửa lại đường dẫn này cho khớp với API C# của ông
        const response = await fetch(`${API_CONFIG.BASE_URL}/public/statistics`);
        
        if (response.ok) {
          const json = await response.json();
          const data = json.data || json;

          setStats({
            totalWaste: data.totalWasteKg || 0,
            totalPoints: data.totalPoints || 0,
            totalUsers: data.totalUsers || 0,
            treesSaved: data.treesSaved || 0 
          });
        } else {
          console.warn("API chưa sẵn sàng hoặc trả về lỗi. Giữ nguyên số 0.");
        }
      } catch (error) {
        console.error("Lỗi khi tải số liệu thống kê:", error);
      }
    };

    fetchPublicStats();
  }, []);

  const steps = [
    {
      id: 1,
      title: "Gửi Yêu Cầu",
      description: "Chụp ảnh rác, chọn loại và vị trí",
      icon: <MapPin className="w-8 h-8" />,
      color: "bg-blue-500"
    },
    {
      id: 2,
      title: "Thu Gom",
      description: "Đơn vị tái chế đến thu gom tại chỗ",
      icon: <Recycle className="w-8 h-8" />,
      color: "bg-emerald-500"
    },
    {
      id: 3,
      title: "Nhận Điểm",
      description: "Nhận điểm thưởng và đổi quà xanh",
      icon: <Trophy className="w-8 h-8" />,
      color: "bg-yellow-500"
    }
  ];

  const formatNumber = (num: number) => {
    return num.toLocaleString('vi-VN');
  };

  return (
    <div className="min-h-screen bg-gradient-to-br from-emerald-50 via-white to-blue-50">
      {/* Navigation provided by Layout */}

      {/* Hero Section */}
      <section className="relative overflow-hidden">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 pt-20 pb-32">
          <div className="text-center">
            <h1 className="text-5xl md:text-6xl font-bold text-gray-900 mb-6">
              <span className="text-emerald-600">Thu gom rác thông minh</span>
              <br />
              <span className="text-gray-800">Bảo vệ môi trường sống</span>
            </h1>
            <p className="text-xl text-gray-600 mb-8 max-w-3xl mx-auto">
              Nền tảng kết nối người dân, doanh nghiệp tái chế và dịch vụ thu gom rác. 
              Báo cáo rác chỉ trong 30 giây, nhận điểm thưởng và đổi lấy quà tặng xanh.
            </p>
            <div className="flex flex-col sm:flex-row gap-4 justify-center">
              <Link 
                href="/register"
                className="bg-emerald-600 text-white px-8 py-4 rounded-xl hover:bg-emerald-700 transition-colors font-semibold text-lg flex items-center justify-center gap-2 group"
              >
                Bắt đầu ngay
                <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
              </Link>
              <Link 
                href="/guide"
                className="bg-white text-gray-700 px-8 py-4 rounded-xl border border-gray-200 hover:bg-gray-50 transition-colors font-semibold text-lg flex items-center justify-center gap-2"
              >
                <Leaf className="w-5 h-5" />
                Tìm hiểu thêm
              </Link>
            </div>
          </div>
        </div>

        {/* Decorative Elements */}
        <div className="absolute top-20 left-10 w-20 h-20 bg-emerald-200 rounded-full opacity-20 blur-xl"></div>
        <div className="absolute top-40 right-10 w-32 h-32 bg-blue-200 rounded-full opacity-20 blur-xl"></div>
        <div className="absolute bottom-0 left-1/4 w-24 h-24 bg-yellow-200 rounded-full opacity-20 blur-xl"></div>
      </section>

      {/* Statistics Section */}
      <section className="py-20 bg-white">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-gray-900 mb-4">Tác động thực tế</h2>
            <p className="text-gray-600">Những con số biết nói từ cộng đồng của chúng tôi</p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-8">
            <div className="text-center">
              <div className="w-16 h-16 bg-emerald-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <Recycle className="w-8 h-8 text-emerald-600" />
              </div>
              <div className="text-3xl font-bold text-gray-900 mb-2 transition-all duration-1000">
                {formatNumber(stats.totalWaste)} kg
              </div>
              <div className="text-gray-600">Rác đã tái chế</div>
            </div>

            <div className="text-center">
              <div className="w-16 h-16 bg-yellow-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <Trophy className="w-8 h-8 text-yellow-600" />
              </div>
              <div className="text-3xl font-bold text-gray-900 mb-2 transition-all duration-1000">
                {formatNumber(stats.totalPoints)}
              </div>
              <div className="text-gray-600">Điểm thưởng đã phát</div>
            </div>

            <div className="text-center">
              <div className="w-16 h-16 bg-blue-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <Users className="w-8 h-8 text-blue-600" />
              </div>
              <div className="text-3xl font-bold text-gray-900 mb-2 transition-all duration-1000">
                {formatNumber(stats.totalUsers)}
              </div>
              <div className="text-gray-600">Người dùng tham gia</div>
            </div>

            <div className="text-center">
              <div className="w-16 h-16 bg-green-100 rounded-full flex items-center justify-center mx-auto mb-4">
                <TreePine className="w-8 h-8 text-green-600" />
              </div>
              <div className="text-3xl font-bold text-gray-900 mb-2 transition-all duration-1000">
                {formatNumber(stats.treesSaved)}
              </div>
              <div className="text-gray-600">Cây xanh đã cứu</div>
            </div>
          </div>
        </div>
      </section>

      {/* How It Works */}
      <section className="py-20 bg-gray-50">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="text-center mb-12">
            <h2 className="text-3xl font-bold text-gray-900 mb-4">Cách thức hoạt động</h2>
            <p className="text-gray-600">Đơn giản trong 3 bước, hiệu quả tức thì</p>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-3 gap-8">
            {steps.map((step, index) => (
              <div key={step.id} className="relative">
                <div className="bg-white rounded-2xl p-8 text-center shadow-sm border border-gray-100 hover:shadow-lg transition-shadow">
                  <div className={`w-16 h-16 ${step.color} rounded-full flex items-center justify-center text-white mx-auto mb-6`}>
                    {step.icon}
                  </div>
                  <div className="text-2xl font-bold text-gray-900 mb-3">{step.title}</div>
                  <div className="text-gray-600">{step.description}</div>
                </div>
                
                {/* Connector Line */}
                {index < steps.length - 1 && (
                  <div className="hidden md:block absolute top-1/2 -right-4 transform -translate-y-1/2 z-10">
                    <div className="w-8 h-0.5 bg-gray-300"></div>
                    <ArrowRight className="w-4 h-4 text-gray-400 absolute right-0 top-1/2 transform -translate-y-1/2" />
                  </div>
                )}
              </div>
            ))}
          </div>
        </div>
      </section>

      {/* CTA Section */}
      <section className="py-20 bg-emerald-600">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 text-center">
          <h2 className="text-3xl font-bold text-white mb-4">
            Sẵn sàng trở thành chiến thần xanh?
          </h2>
          <p className="text-emerald-100 text-lg mb-8 max-w-2xl mx-auto">
            Hàng ngàn người dân đã tham gia cuộc chiến chống rác thải. 
            Hãy cùng chúng tôi xây dựng một Việt Nam xanh hơn!
          </p>
          <Link 
            href="/register"
            className="bg-white text-emerald-600 px-8 py-4 rounded-xl hover:bg-gray-50 transition-colors font-semibold text-lg inline-flex items-center gap-2 group"
          >
            Tham gia ngay hôm nay
            <ArrowRight className="w-5 h-5 group-hover:translate-x-1 transition-transform" />
          </Link>
        </div>
      </section>

      {/* Footer */}
      <footer className="bg-gray-900 text-white py-12">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="grid grid-cols-1 md:grid-cols-4 gap-8">
            <div>
              <div className="flex items-center gap-3 mb-4">
                <Image
                  src="/logo/logo.png"
                  alt="CWCRP Logo"
                  width={100}
                  height={100}
                  className="rounded-lg"
                />
                <span className="font-bold text-xl">CWCRP</span>
              </div>
              <p className="text-gray-400">
                Nền tảng thu gom rác thông minh cho Việt Nam
              </p>
            </div>

            <div>
              <h3 className="font-semibold mb-4">Sản phẩm</h3>
              <ul className="space-y-2 text-gray-400">
                <li><Link href="/guide" className="hover:text-white">Hướng dẫn</Link></li>
                <li><Link href="/locations" className="hover:text-white">Tra cứu điểm</Link></li>
                <li><Link href="/leaderboard" className="hover:text-white">Bảng xếp hạng</Link></li>
              </ul>
            </div>

            <div>
              <h3 className="font-semibold mb-4">Hỗ trợ</h3>
              <ul className="space-y-2 text-gray-400">
                <li><Link href="/guide" className="hover:text-white">Cách dùng</Link></li>
                <li><Link href="/contact" className="hover:text-white">Liên hệ</Link></li>
                <li><Link href="/faq" className="hover:text-white">FAQ</Link></li>
              </ul>
            </div>

            <div>
              <h3 className="font-semibold mb-4">Về chúng tôi</h3>
              <ul className="space-y-2 text-gray-400">
                <li><Link href="/about" className="hover:text-white">Giới thiệu</Link></li>
                <li><Link href="/impact" className="hover:text-white">Tác động</Link></li>
                <li><Link href="/blog" className="hover:text-white">Blog</Link></li>
              </ul>
            </div>
          </div>

          <div className="border-t border-gray-800 mt-8 pt-8 text-center text-gray-400">
            <p>&copy; {new Date().getFullYear()} CWCRP. Tất cả quyền được bảo lưu.</p>
          </div>
        </div>
      </footer>
    </div>
  );
}