"use client";
import React from "react";
import Link from "next/link";

export const HeroSection: React.FC = () => {
  return (
    <section className="relative w-full overflow-hidden hero-gradient" style={{ minHeight: "calc(100vh - 64px)" }}>
      {/* Background gradient overlay */}
      <div className="absolute inset-0 hero-gradient" />

      {/* Mountain / cloud shapes - top left */}
      <div className="absolute top-0 left-0 w-full h-full pointer-events-none">
        {/* Left side abstract mountain shapes */}
        <svg
          className="absolute top-10 left-16 opacity-50"
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
          className="absolute top-6 right-10 opacity-50"
          width="260"
          height="180"
          viewBox="0 0 260 180"
          fill="none"
        >
          <ellipse cx="130" cy="150" rx="130" ry="70" fill="rgba(140,195,185,0.4)" />
          <ellipse cx="200" cy="130" rx="80" ry="55" fill="rgba(100,170,165,0.35)" />
          <ellipse cx="60" cy="120" rx="75" ry="55" fill="rgba(120,185,170,0.4)" />
        </svg>

        {/* Additional distant mountains center-left */}
        <svg
          className="absolute top-16 left-1/3 opacity-30"
          width="180"
          height="130"
          viewBox="0 0 180 130"
          fill="none"
        >
          <ellipse cx="90" cy="110" rx="90" ry="55" fill="rgba(160,210,195,0.5)" />
          <ellipse cx="140" cy="100" rx="60" ry="40" fill="rgba(130,195,180,0.4)" />
        </svg>
      </div>

      {/* Hero Text - Center */}
      <div className="relative z-10 flex flex-col items-center justify-center pt-28 px-6 text-center">
        <p className="text-white/90 text-sm md:text-base tracking-widest uppercase mb-3 font-medium bg-emerald-800/40 px-4 py-1.5 rounded-full border border-emerald-500/30 backdrop-blur-sm">
          Crowdsourced Waste Collection & Recycling
        </p>
        <h1 className="text-white text-4xl md:text-5xl lg:text-5xl font-extrabold tracking-tight uppercase leading-tight mb-6" style={{ textShadow: "0 2px 20px rgba(0,0,0,0.15)" }}>
          KẾT NỐI TÁI CHẾ<br />VÌ MỘT CỘNG ĐỒNG XANH
        </h1>
        <p className="text-white/90 text-base md:text-lg mb-8 max-w-2xl font-light">
          Nền tảng kết nối trực tiếp người dân, doanh nghiệp tái chế và dịch vụ thu gom rác thải theo khu vực. Tham gia nền kinh tế tuần hoàn ngay hôm nay.
        </p>
        <Link
          href="/login"
          className="inline-block bg-emerald-500 hover:bg-emerald-400 text-white font-bold uppercase tracking-widest px-10 py-4 text-sm transition-all duration-300 hover:scale-105 hover:shadow-xl shadow-emerald-700/40 shadow-lg"
          style={{ letterSpacing: "0.15em" }}
        >
          Bắt Đầu Ngay
        </Link>

        {/* Scroll indicator */}
        <div className="mt-10 animate-bounce">
          <svg width="28" height="28" fill="none" stroke="white" strokeWidth="2" viewBox="0 0 24 24">
            <path strokeLinecap="round" strokeLinejoin="round" d="M19 9l-7 7-7-7" />
          </svg>
        </div>
      </div>

      {/* Bottom nature illustration */}
      <div className="absolute bottom-0 left-0 w-full pointer-events-none">
        <svg
          viewBox="0 0 1440 400"
          xmlns="http://www.w3.org/2000/svg"
          preserveAspectRatio="xMidYMax slice"
          className="w-full"
          style={{ display: "block" }}
        >
          {/* Ground layer */}
          <ellipse cx="720" cy="430" rx="900" ry="120" fill="rgba(20,100,60,0.35)" />

          {/* Large left bush / tree blob */}
          <ellipse cx="160" cy="370" rx="130" ry="100" fill="#1B7A3E" />
          <ellipse cx="100" cy="350" rx="90" ry="75" fill="#25A05B" />
          <ellipse cx="200" cy="340" rx="100" ry="80" fill="#1E9050" />

          {/* Left tall plant stem */}
          <rect x="155" y="340" width="10" height="60" fill="#15612F" />
          {/* Leaves left */}
          <ellipse cx="130" cy="310" rx="40" ry="25" fill="#27AE60" transform="rotate(-20 130 310)" />
          <ellipse cx="185" cy="300" rx="38" ry="22" fill="#2ECC71" transform="rotate(15 185 300)" />
          <ellipse cx="110" cy="290" rx="32" ry="18" fill="#16A34A" transform="rotate(-35 110 290)" />
          <ellipse cx="200" cy="280" rx="30" ry="17" fill="#22C55E" transform="rotate(25 200 280)" />

          {/* Flower - white */}
          <circle cx="162" cy="295" r="12" fill="white" opacity="0.9" />
          <circle cx="162" cy="295" r="6" fill="#FCD34D" />
          <ellipse cx="162" cy="282" rx="5" ry="8" fill="white" opacity="0.85" />
          <ellipse cx="162" cy="308" rx="5" ry="8" fill="white" opacity="0.85" />
          <ellipse cx="149" cy="295" rx="8" ry="5" fill="white" opacity="0.85" />
          <ellipse cx="175" cy="295" rx="8" ry="5" fill="white" opacity="0.85" />

          {/* Small plants left ground */}
          <ellipse cx="60" cy="390" rx="55" ry="40" fill="#1A6E38" />
          <ellipse cx="45" cy="375" rx="40" ry="30" fill="#229954" />

          {/* Center bottom plants */}
          <ellipse cx="370" cy="395" rx="80" ry="45" fill="#196F3D" />
          <ellipse cx="420" cy="380" rx="65" ry="40" fill="#239B56" />
          <ellipse cx="350" cy="380" rx="60" ry="38" fill="#1E8449" />

          {/* Right side plants */}
          <ellipse cx="1100" cy="385" rx="90" ry="55" fill="#1A7A3C" />
          <ellipse cx="1160" cy="370" rx="80" ry="50" fill="#24A158" />
          <ellipse cx="1060" cy="380" rx="70" ry="48" fill="#1F9048" />

          {/* More right foliage */}
          <ellipse cx="1300" cy="390" rx="110" ry="60" fill="#196033" />
          <ellipse cx="1380" cy="375" rx="90" ry="55" fill="#1E7A3E" />
          <ellipse cx="1260" cy="378" rx="80" ry="50" fill="#228B4A" />

          {/* Small accent plants right-center */}
          <ellipse cx="700" cy="400" rx="75" ry="42" fill="#1A6B38" />
          <ellipse cx="760" cy="390" rx="60" ry="38" fill="#21914E" />

          {/* Tall cactus-like plant right */}
          <rect x="890" y="320" width="14" height="80" fill="#166B34" />
          <ellipse cx="897" cy="320" rx="22" ry="18" fill="#1D8040" />
          <ellipse cx="875" cy="345" rx="18" ry="13" fill="#1A7538" transform="rotate(-20 875 345)" />
          <ellipse cx="920" cy="340" rx="18" ry="13" fill="#1A7538" transform="rotate(20 920 340)" />

          {/* Front ground cover */}
          <rect x="0" y="398" width="1440" height="10" fill="rgba(18,80,45,0.6)" />
        </svg>
      </div>

      {/* Right side content block */}
      <div className="absolute right-16 bottom-32 z-10 max-w-xs text-right hidden lg:block bg-emerald-900/30 p-5 rounded-xl backdrop-blur-sm border border-emerald-400/20">
        <p className="text-white/80 text-sm mb-2 italic">
          Đóng góp vào mạng lưới thu gom và tái chế rác thải thông minh.
        </p>
        <h2 className="text-white text-lg font-bold uppercase leading-snug mb-3">
          PHÂN LOẠI TẠI NGUỒN,<br />
          TẠO RA SỰ KHÁC BIỆT
        </h2>
        <div className="border-b-2 border-emerald-400 w-20 ml-auto" />
      </div>
    </section>
  );
};
