"use client";

import React from "react";
import Link from "next/link";
import { Navbar } from "@/components/layout/Navbar";

export default function AuthLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <div className="flex flex-col min-h-screen w-full bg-gradient-to-b from-[#37474F] via-[#105E60] to-[#0AA468] overflow-hidden relative font-sans">
      {/* Background Shapes/Waves (abstract visualization of the design) */}
      <div className="absolute inset-0 overflow-hidden pointer-events-none z-0">
         {/* Mountain / cloud shapes - top left */}
        <div className="absolute top-0 left-0 w-full h-full pointer-events-none">
            {/* Left side abstract mountain shapes */}
            <svg
            className="absolute top-10 left-16 opacity-30"
            width="200"
            height="160"
            viewBox="0 0 200 160"
            fill="none"
            >
            <ellipse cx="70" cy="130" rx="70" ry="50" fill="rgba(100,180,160,0.5)" />
            <ellipse cx="140" cy="120" rx="60" ry="45" fill="rgba(80,160,150,0.4)" />
            <ellipse cx="100" cy="100" rx="45" ry="35" fill="rgba(120,200,175,0.4)" />
            </svg>

            {/* Right side cloud / mountain shapes */}
            <svg
            className="absolute top-6 right-10 opacity-30"
            width="260"
            height="180"
            viewBox="0 0 260 180"
            fill="none"
            >
            <ellipse cx="130" cy="150" rx="130" ry="70" fill="rgba(140,195,185,0.4)" />
            <ellipse cx="200" cy="130" rx="80" ry="55" fill="rgba(100,170,165,0.35)" />
            <ellipse cx="60" cy="120" rx="75" ry="55" fill="rgba(120,185,170,0.4)" />
            </svg>
        </div>

        {/* Bottom nature illustration */}
        <div className="absolute bottom-[-10%] md:bottom-[-20%] left-[-10%] w-[120%] pointer-events-none opacity-80">
             <svg
            viewBox="0 0 1440 320"
            xmlns="http://www.w3.org/2000/svg"
            className="w-full h-auto"
            preserveAspectRatio="none"
            >
            <path fill="#0f764a" fillOpacity="1" d="M0,192L48,197.3C96,203,192,213,288,229.3C384,245,480,267,576,250.7C672,235,768,181,864,181.3C960,181,1056,235,1152,234.7C1248,235,1344,181,1392,154.7L1440,128L1440,320L1392,320C1344,320,1248,320,1152,320C1056,320,960,320,864,320C768,320,672,320,576,320C480,320,384,320,288,320C192,320,96,320,48,320L0,320Z"></path>
            <path fill="#15612F" fillOpacity="0.6" d="M0,224L48,213.3C96,203,192,181,288,181.3C384,181,480,203,576,224C672,245,768,267,864,261.3C960,256,1056,224,1152,213.3C1248,203,1344,213,1392,218.7L1440,224L1440,320L1392,320C1344,320,1248,320,1152,320C1056,320,960,320,864,320C768,320,672,320,576,320C480,320,384,320,288,320C192,320,96,320,48,320L0,320Z"></path>
            </svg>
        </div>
      </div>

      {/* Main Content Grid */}
      <div className="container mx-auto px-6 z-10 flex-1 flex flex-col lg:flex-row items-center justify-center py-10 gap-16 lg:gap-20">
        {/* Left Side: Landing Content */}
        <div className="w-full lg:w-5/12 text-white flex flex-col justify-center items-start pl-0 lg:pl-10">
          <p className="text-sm font-bold tracking-widest uppercase mb-4 opacity-80">
            Nền tảng kết nối tái chế & thu gom rác
          </p>
          <h1 className="text-3xl md:text-3xl lg:text-4xl font-bold mb-6 uppercase tracking-wider leading-tight">
            Crowdsourced Waste Collection & Recycling
          </h1>
          <p className="text-sm leading-relaxed opacity-90 max-w-lg mb-8 font-light tracking-wide text-justify">
            Quản lý rác thải đô thị tại Việt Nam đang đối mặt với nhiều thách thức như lịch thu gom không ổn định, tỷ lệ phân loại rác tại nguồn thấp và sự phối hợp rời rạc giữa người dân, đơn vị thu gom và doanh nghiệp tái chế.
            <br /><br />
            Trong khi đó, quy định bắt buộc phân loại rác tại nguồn từ năm 2025 đặt ra nhu cầu cấp thiết về một nền tảng số hỗ trợ kết nối, điều phối và giám sát toàn bộ quy trình thu gom – tái chế theo khu vực một cách hiệu quả và minh bạch.
          </p>
          <Link 
            href="/"
            className="bg-white text-[#0AA468] px-8 py-3 rounded-full font-bold shadow-lg hover:shadow-xl hover:bg-gray-50 transition transform hover:-translate-y-0.5 text-xs uppercase tracking-wider inline-block"
          >
            Tìm Hiểu Thêm
          </Link>
        </div>

        {/* Right Side: Auth Form Container */}
        <div className="w-full lg:w-4/12 flex justify-center items-center relative">
          {/* Side text "WASTE RECYCLING PLATFORM" */}
          <div className="absolute -right-12 top-1/2 transform -translate-y-1/2 rotate-90 origin-center text-[10px] tracking-[0.5em] text-white font-bold opacity-70 hidden xl:block pointer-events-none whitespace-nowrap">
            WASTE RECYCLING PLATFORM
          </div>

          <div className="w-full max-w-[380px]">
             {children}
          </div>
        </div>
      </div>
    </div>
  );
}
