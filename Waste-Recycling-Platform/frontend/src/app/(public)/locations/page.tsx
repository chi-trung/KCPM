"use client";
import React, { useState, useEffect } from "react";
import { Search, MapPin, Phone, Clock, Navigation, Star } from "lucide-react";

interface Location {
  id: string;
  name: string;
  address: string;
  province: string;
  district: string;
  type: 'recyclable' | 'organic' | 'hazardous' | 'general';
  distance?: string;
  rating: number;
  phone: string;
  hours: string;
  image?: string;
  coordinates?: {
    lat: number;
    lng: number;
  };
}

const mockLocations: Location[] = [
  {
    id: "1",
    name: "Công ty Tái chế Việt Green",
    address: "123 Nguyễn Văn Linh, Quận 7, TP.HCM",
    province: "Hồ Chí Minh",
    district: "Quận 7",
    type: "recyclable",
    distance: "2.5 km",
    rating: 4.5,
    phone: "(028) 1234-5678",
    hours: "07:00 - 18:00"
  },
  {
    id: "2", 
    name: "Trạm thu gom rác hữu cơ EcoLife",
    address: "456 Võ Văn Tần, Quận 3, TP.HCM",
    province: "Hồ Chí Minh",
    district: "Quận 3",
    type: "organic",
    distance: "1.2 km",
    rating: 4.2,
    phone: "(028) 2345-6789",
    hours: "06:00 - 20:00"
  },
  {
    id: "3",
    name: "Điểm thu gom rác nguy hại SafeZone",
    address: "789 Trần Hưng Đạo, Quận 1, TP.HCM", 
    province: "Hồ Chí Minh",
    district: "Quận 1",
    type: "hazardous",
    distance: "3.8 km",
    rating: 4.8,
    phone: "(028) 3456-7890",
    hours: "08:00 - 17:00"
  },
  {
    id: "4",
    name: "Tổng công ty Môi trường Xanh",
    address: "321 Lê Lợi, Quận 1, TP.HCM",
    province: "Hồ Chí Minh", 
    district: "Quận 1",
    type: "general",
    distance: "0.8 km",
    rating: 4.0,
    phone: "(028) 4567-8901",
    hours: "07:00 - 19:00"
  },
  {
    id: "5",
    name: "Trung tâm tái chế GreenTech",
    address: "654 Cộng Hòa, Quận Tân Bình, TP.HCM",
    province: "Hồ Chí Minh",
    district: "Tân Bình", 
    type: "recyclable",
    distance: "4.2 km",
    rating: 4.6,
    phone: "(028) 5678-9012",
    hours: "07:30 - 18:30"
  }
];

const wasteTypes = [
  { value: "all", label: "Tất cả", color: "bg-gray-100 text-gray-700" },
  { value: "recyclable", label: "Tái chế", color: "bg-blue-100 text-blue-700" },
  { value: "organic", label: "Hữu cơ", color: "bg-green-100 text-green-700" },
  { value: "hazardous", label: "Nguy hại", color: "bg-red-100 text-red-700" },
  { value: "general", label: "Thông thường", color: "bg-yellow-100 text-yellow-700" }
];

const provinces = [
  "Hồ Chí Minh",
  "Hà Nội", 
  "Đà Nẵng",
  "Cần Thơ",
  "Hải Phòng"
];

const districts = {
  "Hồ Chí Minh": ["Quận 1", "Quận 2", "Quận 3", "Quận 4", "Quận 5", "Quận 6", "Quận 7", "Quận 8", "Quận 9", "Quận 10", "Quận 11", "Quận 12", "Thủ Đức", "Bình Tân", "Bình Thạnh", "Gò Vấp", "Phú Nhuận", "Tân Bình", "Tân Phú"],
  "Hà Nội": ["Ba Đình", "Cầu Giấy", "Đống Đa", "Hai Bà Trưng", "Hoàn Kiếm", "Hoàng Mai", "Long Biên", "Thanh Xuân", "Sóc Sơn", "Đông Anh", "Gia Lâm", "Hà Đông"],
  "Đà Nẵng": ["Hải Châu", "Thanh Khê", "Sơn Trà", "Ngũ Hành Sơn", "Liên Chiểu", "Cẩm Lệ"],
  "Cần Thơ": ["Ninh Kiều", "Bình Thủy", "Cái Răng", "Ô Môn", "Thốt Nốt"],
  "Hải Phòng": ["Hồng Bàng", "Ngô Quyền", "Lê Chân", "Hải An", "Đồ Sơn", "Kiến An", "Dương Kinh"]
};

export default function LocationsPage() {
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedType, setSelectedType] = useState("all");
  const [selectedProvince, setSelectedProvince] = useState("Hồ Chí Minh");
  const [selectedDistrict, setSelectedDistrict] = useState("");
  const [sortBy, setSortBy] = useState("distance");
  const [locations, setLocations] = useState<Location[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  // Filter locations from mock data
  useEffect(() => {
    const filterLocations = () => {
      try {
        setLoading(true);
        let filtered = mockLocations.filter(location => {
          const matchesSearch = location.name.toLowerCase().includes(searchTerm.toLowerCase()) ||
                               location.address.toLowerCase().includes(searchTerm.toLowerCase());
          const matchesType = selectedType === "all" || location.type === selectedType;
          const matchesProvince = location.province === selectedProvince;
          const matchesDistrict = !selectedDistrict || location.district === selectedDistrict;
          
          return matchesSearch && matchesType && matchesProvince && matchesDistrict;
        });

        // Sort by selected criteria
        filtered.sort((a, b) => {
          switch (sortBy) {
            case "distance":
              return (parseFloat(a.distance || "999") || 999) - (parseFloat(b.distance || "999") || 999);
            case "rating":
              return b.rating - a.rating;
            case "name":
              return a.name.localeCompare(b.name);
            default:
              return 0;
          }
        });

        setLocations(filtered);
        setError(null);
      } catch (err) {
        setError("Không thể tải danh sách địa điểm. Vui lòng thử lại sau.");
        console.error("Error filtering locations:", err);
      } finally {
        setLoading(false);
      }
    };

    filterLocations();
  }, [searchTerm, selectedType, selectedProvince, selectedDistrict, sortBy]);

  const getTypeColor = (type: string) => {
    const wasteType = wasteTypes.find(w => w.value === type);
    return wasteType ? wasteType.color : "bg-gray-100 text-gray-700";
  };

  const getTypeLabel = (type: string) => {
    const wasteType = wasteTypes.find(w => w.value === type);
    return wasteType ? wasteType.label : "Khác";
  };

  const renderStars = (rating: number) => {
    return Array.from({ length: 5 }, (_, i) => (
      <span key={i} className={i < Math.floor(rating) ? "text-yellow-400" : "text-gray-300"}>
        ★
      </span>
    ));
  };

  return (
    <div className="min-h-screen bg-gray-50">
      {/* Header */}
      <div className="bg-white border-b border-gray-200 sticky top-0 z-40">
        <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
          <div className="flex items-center justify-between">
            <div className="flex items-center gap-3">
              <MapPin className="w-6 h-6 text-emerald-600" />
              <h1 className="text-2xl font-bold text-gray-900">Tra cứu điểm thu gom</h1>
            </div>
            <div className="text-sm text-gray-600">
              Tìm thấy <span className="font-semibold text-emerald-600">{locations.length}</span> địa điểm
            </div>
          </div>
        </div>
      </div>

      <div className="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-6">
        {/* Search and Filters */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-6 mb-6">
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Search Bar */}
            <div>
              <div className="relative">
                <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 text-gray-400" size={20} />
                <input
                  type="text"
                  placeholder="Tìm kiếm tên địa điểm, địa chỉ..."
                  value={searchTerm}
                  onChange={(e) => setSearchTerm(e.target.value)}
                  className="w-full pl-10 pr-4 py-3 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500"
                />
              </div>
            </div>

            {/* Filters */}
            <div className="grid grid-cols-3 gap-3">
              <select
                value={selectedType}
                onChange={(e) => setSelectedType(e.target.value)}
                className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 text-sm"
              >
                {wasteTypes.map(type => (
                  <option key={type.value} value={type.value}>{type.label}</option>
                ))}
              </select>

              <select
                value={selectedProvince}
                onChange={(e) => {
                  setSelectedProvince(e.target.value);
                  setSelectedDistrict("");
                }}
                className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 text-sm"
              >
                {provinces.map(province => (
                  <option key={province} value={province}>{province}</option>
                ))}
              </select>

              <select
                value={selectedDistrict}
                onChange={(e) => setSelectedDistrict(e.target.value)}
                disabled={!selectedProvince}
                className="px-3 py-2 border border-gray-300 rounded-lg focus:ring-2 focus:ring-emerald-500 focus:border-emerald-500 disabled:bg-gray-100 text-sm"
              >
                <option value="">Tất cả quận/huyện</option>
                {selectedProvince && districts[selectedProvince as keyof typeof districts]?.map(district => (
                  <option key={district} value={district}>{district}</option>
                ))}
              </select>
            </div>
          </div>
        </div>

        {/* Loading State */}
        {loading && (
          <div className="flex items-center justify-center py-12">
            <div className="animate-spin rounded-full h-8 w-8 border-b-2 border-emerald-600"></div>
            <span className="ml-3 text-gray-600">Đang tải danh sách địa điểm...</span>
          </div>
        )}

        {/* Error State */}
        {error && (
          <div className="bg-red-50 border border-red-200 rounded-lg p-4 mb-6">
            <p className="text-red-700">{error}</p>
          </div>
        )}

        {/* Results */}
        {!loading && !error && (
          <div className="space-y-4">
            {locations.length === 0 ? (
              <div className="bg-white rounded-xl shadow-sm border border-gray-200 p-12 text-center">
                <MapPin className="w-12 h-12 text-gray-400 mx-auto mb-4" />
                <h3 className="text-lg font-semibold text-gray-900 mb-2">Không tìm thấy địa điểm</h3>
                <p className="text-gray-600">Thử thay đổi tiêu chí tìm kiếm để có kết quả tốt hơn</p>
              </div>
            ) : (
              locations.map((location) => (
                <div key={location.id} className="bg-white rounded-xl shadow-sm border border-gray-200 p-6 hover:shadow-md transition-shadow">
                  <div className="flex items-start justify-between">
                    <div className="flex-1">
                      <h3 className="font-semibold text-gray-900 mb-1">{location.name}</h3>
                      <p className="text-sm text-gray-600 mb-2">{location.address}</p>
                      
                      <div className="flex items-center gap-4 text-sm text-gray-600 mb-3">
                        <div className="flex items-center gap-1">
                          {renderStars(location.rating)}
                          <span className="ml-1">({location.rating})</span>
                        </div>
                        <div className="flex items-center gap-1">
                          <Phone size={14} />
                          <span>{location.phone}</span>
                        </div>
                        <div className="flex items-center gap-1">
                          <Clock size={14} />
                          <span>{location.hours}</span>
                        </div>
                      </div>

                      <div className="flex items-center gap-2">
                        <span className={`px-2 py-1 rounded-full text-xs font-medium ${getTypeColor(location.type)}`}>
                          {getTypeLabel(location.type)}
                        </span>
                        {location.distance && (
                          <span className="text-sm text-emerald-600 font-medium flex items-center gap-1">
                            <Navigation size={14} />
                            {location.distance}
                          </span>
                        )}
                      </div>
                    </div>

                    <div className="flex gap-2 ml-4">
                      <button className="px-4 py-2 bg-emerald-600 text-white rounded-lg hover:bg-emerald-700 transition-colors text-sm font-medium">
                        Chỉ đường
                      </button>
                      <button className="px-4 py-2 border border-gray-300 text-gray-700 rounded-lg hover:bg-gray-50 transition-colors text-sm font-medium">
                        Gọi điện
                      </button>
                    </div>
                  </div>
                </div>
              ))
            )}
          </div>
        )}
      </div>
    </div>
  );
}
