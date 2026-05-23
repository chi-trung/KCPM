"use client";
import React, { useState } from "react";
import { 
  BookOpen, 
  AlertTriangle, 
  CheckCircle, 
  XCircle, 
  Info, 
  Calendar,
  FileText,
  Search,
  Filter,
  Leaf,
  Package,
  Zap
} from "lucide-react";

interface WasteGuide {
  id: string;
  name: string;
  category: "organic" | "recyclable" | "hazardous" | "other";
  description: string;
  examples: string[];
  disposalMethod: string;
  tips: string[];
  icon: React.ReactNode;
  color: string;
  bgColor: string;
}

const wasteGuides: WasteGuide[] = [
  {
    id: "organic",
    name: "Rác Hữu Cơ",
    category: "organic",
    description: "Rác sinh hoạt có thể phân hủy sinh học, dễ phân hủy trong môi trường tự nhiên.",
    examples: ["Thức ăn thừa", "Vỏ rau củ quả", "Bã trà, cà phê", "Lá cây", "Giấy ăn dính dầu mỡ"],
    disposalMethod: "Đóng gói trong túi kín, để riêng với các loại rác khác. Có thể dùng làm phân bón compost.",
    tips: [
      "Nên để rác hữu cơ trong thùng có nắp đậy để tránh mùi hôi",
      "Có thể ủ phân bón tại nhà nếu có điều kiện",
      "Trộn với đất để tăng tốc độ phân hủy"
    ],
    icon: <Leaf className="w-6 h-6" />,
    color: "text-green-600",
    bgColor: "bg-green-50 border-green-200"
  },
  {
    id: "recyclable",
    name: "Rác Tái Chế",
    category: "recyclable",
    description: "Các vật liệu có thể thu hồi, tái chế để sản xuất sản phẩm mới.",
    examples: ["Chai nhựa PET", "Giấy báo, giấy carton", "Lon nhôm, kim loại", "Chai thủy tinh", "Vỏ hộp sữa"],
    disposalMethod: "Rửa sạch, để khô, phân loại theo từng loại. Đập dẹp chai nhựa để tiết kiệm không gian.",
    tips: [
      "Trước khi bỏ, hãy rửa sạch để tránh mùi và vi khuẩn",
      "Bỏ nhãn mác trên chai lọ khi có thể",
      "Không tái chế giấy đã dính dầu mỡ hoặc chất lỏng"
    ],
    icon: <Package className="w-6 h-6" />,
    color: "text-blue-600",
    bgColor: "bg-blue-50 border-blue-200"
  },
  {
    id: "hazardous",
    name: "Rác Nguy Hai",
    category: "hazardous", 
    description: "Rác chứa chất độc hại, có thể gây nguy hiểm cho sức khỏe và môi trường.",
    examples: ["Pin cũ", "Thuốc hết hạn", "Bóng đèn huỳnh quang", "Sơn, hóa chất", "Dầu máy đã qua sử dụng"],
    disposalMethod: "Để trong bao bì nguyên vẹn, đánh dấu rõ ràng. Mang đến điểm tập kết chuyên dụng.",
    tips: [
      "Không bao giờ bỏ rác nguy hại cùng rác thông thường",
      "Đeo găng tay khi xử lý rác nguy hại",
      "Giữ nguyên bao bì sản phẩm để nhận biết dễ dàng"
    ],
    icon: <Zap className="w-6 h-6" />,
    color: "text-red-600",
    bgColor: "bg-red-50 border-red-200"
  },
  {
    id: "other",
    name: "Rác Khác",
    category: "other",
    description: "Các loại rác không thuộc các nhóm trên, khó tái chế hoặc xử lý đặc biệt.",
    examples: ["Khăn giấy đã dùng", "Túi nilon khó phân hủy", "Bàn chải đánh răng", "Đồ gốm sứ vỡ", "Túi hút chân không"],
    disposalMethod: "Đóng gói cẩn thận, đánh dấu nếu có vật sắc nhọn. Mang đến bãi rác thông thường.",
    tips: [
      "Gom các vật sắc nhọn vào hộp cứng để an toàn",
      "Giảm thiểu sử dụng đồ nhựa một lần",
      "Ưu tiên sản phẩm có bao bì tái chế được"
    ],
    icon: <XCircle className="w-6 h-6" />,
    color: "text-gray-600",
    bgColor: "bg-gray-50 border-gray-200"
  }
];

const regulations = [
  {
    title: "Quy định Phân loại Rác tại Nguồn 2025",
    content: "Theo Nghị định 08/2022/NĐ-CP, từ ngày 01/01/2025, tất cả các hộ gia đình, cơ sở sản xuất, kinh doanh, dịch vụ phải thực hiện phân loại rác tại nguồn theo quy định.",
    effectiveDate: "01/01/2025",
    penalty: "Phạt tiền từ 1.000.000 - 5.000.000 VNĐ nếu không thực hiện phân loại rác"
  },
  {
    title: "Tiêu chuẩn Phân loại",
    content: "Phân loại thành 3 nhóm chính: Rác hữu cơ (tối thiểu 50%), Rác tái chế (tối thiểu 30%), Rác khác (tối đa 20%).",
    effectiveDate: "01/01/2025",
    penalty: "Không tuân thủ tỷ lệ phân loại có thể bị phạt lên đến 10.000.000 VNĐ"
  },
  {
    title: "Trách nhiệm của Tổ chức/Cá nhân",
    content: "Chủ sở hữu nhà ở, chủ đầu tư dự án, chủ cơ sở sản xuất, kinh doanh, dịch vụ có trách nhiệm tổ chức thu gom, vận chuyển rác thải sinh hoạt đã phân loại tại nguồn.",
    effectiveDate: "01/01/2025",
    penalty: "Không thực hiện nghĩa vụ có thể bị phạt từ 5.000.000 - 20.000.000 VNĐ"
  }
];

export default function GuidePage() {
  const [selectedCategory, setSelectedCategory] = useState<string>("all");
  const [searchTerm, setSearchTerm] = useState("");
  const [activeTab, setActiveTab] = useState<"guide" | "regulations">("guide");

  const filteredGuides = wasteGuides.filter(guide => {
    const matchesCategory = selectedCategory === "all" || guide.category === selectedCategory;
    const matchesSearch = guide.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         guide.description.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         guide.examples.some(example => example.toLowerCase().includes(searchTerm.toLowerCase()));
    return matchesCategory && matchesSearch;
  });

  const categories = [
    { value: "all", label: "Tất cả", icon: <BookOpen className="w-4 h-4" /> },
    { value: "organic", label: "Hữu cơ", icon: <Leaf className="w-4 h-4" /> },
    { value: "recyclable", label: "Tái chế", icon: <Package className="w-4 h-4" /> },
    { value: "hazardous", label: "Nguy hại", icon: <Zap className="w-4 h-4" /> },
    { value: "other", label: "Khác", icon: <XCircle className="w-4 h-4" /> }
  ];

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
          <div className="flex items-center gap-3 mb-4">
            <BookOpen className="w-6 h-6 text-emerald-600" />
            <h1 className="text-2xl font-bold text-gray-900">Hướng dẫn phân loại rác</h1>
          </div>
          <p className="text-gray-600">
            Hướng dẫn chi tiết cách phân loại rác theo quy định 2025 và các mẹo hữu ích
          </p>
        </div>
      </div>

      {/* Tabs */}
      <div className="bg-white border-b border-gray-200 sticky top-0 z-40">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
          <div className="flex space-x-8">
            <button
              onClick={() => setActiveTab("guide")}
              className={`py-4 px-1 border-b-2 font-medium text-sm transition-colors ${
                activeTab === "guide"
                  ? "border-emerald-500 text-emerald-600"
                  : "border-transparent text-gray-500 hover:text-gray-700"
              }`}
            >
              Cẩm nang phân loại
            </button>
            <button
              onClick={() => setActiveTab("regulations")}
              className={`py-4 px-1 border-b-2 font-medium text-sm transition-colors ${
                activeTab === "regulations"
                  ? "border-emerald-500 text-emerald-600"
                  : "border-transparent text-gray-500 hover:text-gray-700"
              }`}
            >
              Quy định 2025
            </button>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
        {activeTab === "guide" ? (
          <>
            {/* Search and Filter */}
            <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6 mb-6">
              <div className="flex flex-col lg:flex-row gap-4">
                <div className="relative flex-1">
                  <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400 w-5 h-5" />
                  <input
                    type="text"
                    placeholder="Tìm kiếm loại rác, ví dụ: chai nhựa, giấy..."
                    value={searchTerm}
                    onChange={(e) => setSearchTerm(e.target.value)}
                    className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
                  />
                </div>
                <div className="flex items-center gap-2">
                  <Filter className="w-5 h-5 text-gray-400" />
                  <div className="flex gap-2">
                    {categories.map(category => (
                      <button
                        key={category.value}
                        onClick={() => setSelectedCategory(category.value)}
                        className={`px-4 py-2 rounded-lg border transition-colors flex items-center gap-2 ${
                          selectedCategory === category.value
                            ? "bg-emerald-600 text-white border-emerald-600"
                            : "bg-white text-gray-700 border-gray-300 hover:bg-gray-50"
                        }`}
                      >
                        {category.icon}
                        <span className="text-sm font-medium">{category.label}</span>
                      </button>
                    ))}
                  </div>
                </div>
              </div>
            </div>

            {/* Waste Guides */}
            <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
              {filteredGuides.map((guide) => (
                <div key={guide.id} className={`bg-white rounded-xl shadow-sm border ${guide.bgColor} p-6`}>
                  <div className="flex items-start gap-4 mb-4">
                    <div className={`w-12 h-12 rounded-lg flex items-center justify-center ${guide.bgColor}`}>
                      <div className={guide.color}>
                        {guide.icon}
                      </div>
                    </div>
                    <div className="flex-1">
                      <h3 className="text-xl font-bold text-gray-900 mb-2">{guide.name}</h3>
                      <p className="text-gray-600">{guide.description}</p>
                    </div>
                  </div>

                  {/* Examples */}
                  <div className="mb-4">
                    <h4 className="font-semibold text-gray-900 mb-2 flex items-center gap-2">
                      <CheckCircle className="w-4 h-4 text-emerald-600" />
                      Ví dụ:
                    </h4>
                    <div className="flex flex-wrap gap-2">
                      {guide.examples.map((example, index) => (
                        <span
                          key={index}
                          className="px-3 py-1 bg-white rounded-full text-sm text-gray-700 border border-gray-200"
                        >
                          {example}
                        </span>
                      ))}
                    </div>
                  </div>

                  {/* Disposal Method */}
                  <div className="mb-4">
                    <h4 className="font-semibold text-gray-900 mb-2 flex items-center gap-2">
                      <Info className="w-4 h-4 text-blue-600" />
                      Cách xử lý:
                    </h4>
                    <p className="text-gray-600 bg-white p-3 rounded-lg border border-gray-200">
                      {guide.disposalMethod}
                    </p>
                  </div>

                  {/* Tips */}
                  <div>
                    <h4 className="font-semibold text-gray-900 mb-2 flex items-center gap-2">
                      <AlertTriangle className="w-4 h-4 text-yellow-600" />
                      Mẹo hữu ích:
                    </h4>
                    <ul className="space-y-2">
                      {guide.tips.map((tip, index) => (
                        <li key={index} className="flex items-start gap-2 text-sm text-gray-600">
                          <div className="w-1.5 h-1.5 bg-emerald-600 rounded-full mt-2 flex-shrink-0"></div>
                          <span>{tip}</span>
                        </li>
                      ))}
                    </ul>
                  </div>
                </div>
              ))}
            </div>
          </>
        ) : (
          /* Regulations Tab */
          <div className="space-y-6">
            {regulations.map((regulation, index) => (
              <div key={index} className="bg-white rounded-xl shadow-sm border border-gray-200 p-6">
                <div className="flex items-start gap-4">
                  <div className="w-12 h-12 bg-red-50 rounded-lg flex items-center justify-center flex-shrink-0">
                    <FileText className="w-6 h-6 text-red-600" />
                  </div>
                  <div className="flex-1">
                    <h3 className="text-xl font-bold text-gray-900 mb-3">{regulation.title}</h3>
                    <p className="text-gray-600 mb-4">{regulation.content}</p>
                    
                    <div className="grid grid-cols-1 md:grid-cols-2 gap-4">
                      <div className="bg-emerald-50 rounded-lg p-4">
                        <div className="flex items-center gap-2 mb-2">
                          <Calendar className="w-4 h-4 text-emerald-600" />
                          <span className="font-semibold text-emerald-900">Hiệu lực từ:</span>
                        </div>
                        <p className="text-emerald-700">{regulation.effectiveDate}</p>
                      </div>
                      <div className="bg-red-50 rounded-lg p-4">
                        <div className="flex items-center gap-2 mb-2">
                          <AlertTriangle className="w-4 h-4 text-red-600" />
                          <span className="font-semibold text-red-900">Mức phạt:</span>
                        </div>
                        <p className="text-red-700">{regulation.penalty}</p>
                      </div>
                    </div>
                  </div>
                </div>
              </div>
            ))}

            {/* Important Notice */}
            <div className="bg-yellow-50 border border-yellow-200 rounded-xl p-6">
              <div className="flex items-start gap-4">
                <AlertTriangle className="w-6 h-6 text-yellow-600 flex-shrink-0 mt-1" />
                <div>
                  <h3 className="text-lg font-bold text-yellow-900 mb-2">Lưu ý quan trọng</h3>
                  <ul className="space-y-2 text-yellow-800">
                    <li>• Quy định áp dụng cho tất cả các hộ gia đình và cơ sở kinh doanh trên toàn quốc</li>
                    <li>• Cần có thùng rác riêng cho từng loại đã được dán nhãn rõ ràng</li>
                    <li>• Đơn vị thu gom có quyền từ chối thu gom nếu rác không được phân loại đúng quy định</li>
                    <li>• Nghiên cứu kỹ hướng dẫn của địa phương nơi bạn sinh sống</li>
                  </ul>
                </div>
              </div>
            </div>
          </div>
        )}
      </div>
    </div>
  );
}
