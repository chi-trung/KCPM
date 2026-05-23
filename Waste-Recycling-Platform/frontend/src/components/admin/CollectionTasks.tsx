"use client";
import React, { useState, useEffect } from "react";
import { API_CONFIG } from "@/lib/api/config";
import { Search, MapPin, User, Clock, X, Weight, FileText } from "lucide-react";

interface Task {
  id: string;
  taskNumber: string;
  report: string;
  collector: string;
  location: string;
  status: string; // "Assigned", "OnTheWay", "Collected"
  createdAt: string;
  deadline: string;
  wasteQuantity: string;
  notes?: string;
  rawCollectorId?: string; // Dùng để kiểm tra xem đã assign chưa
}

interface Collector {
  id: string;
  name: string;
  phone: string;
  taskCount: number;
}

export const CollectionTasks: React.FC = () => {
  const [tasks, setTasks] = useState<Task[]>([]);
  const [collectors, setCollectors] = useState<Collector[]>([]);
  const [loading, setLoading] = useState(true);
  const [searchTerm, setSearchTerm] = useState("");
  const [filterStatus, setFilterStatus] = useState("all");
  
  // State cho Modal (Popup)
  const [selectedTask, setSelectedTask] = useState<Task | null>(null);
  const [selectedCollectorId, setSelectedCollectorId] = useState<string>("");
  const [assignLoading, setAssignLoading] = useState(false);

  // 1. GỌI API LẤY DANH SÁCH TASK
  const fetchTasks = async () => {
    try {
      setLoading(true);
      const token = localStorage.getItem("token") || "";

      // Gọi API C#
      const response = await fetch(`${API_CONFIG.BASE_URL}/enterprise/tasks`, {
        headers: {
          "Authorization": `Bearer ${token}`,
          "Accept": "*/*"
        }
      });

      if (!response.ok) throw new Error("Lỗi tải dữ liệu");

      // C# trả về mảng trực tiếp Ok(tasks) chứ không bọc trong "data"
      const apiData = await response.json(); 

      const mapStatus = (statusString: string) => {
        const s = statusString?.toLowerCase();
        if (s === "ontheway") return "on_way";
        if (s === "collected") return "collected";
        if (s === "assigned") return "assigned";
        return "pending"; // Nếu chưa gán hoặc trạng thái khác
      };

      const formattedTasks = apiData.map((item: any) => ({
        id: item.id,
        taskNumber: `#T-${item.id.substring(0, 6).toUpperCase()}`,
        report: item.reportId ? `#R-${item.reportId.substring(0, 6).toUpperCase()}` : "Không rõ",
        collector: item.collectorName || "Chưa phân công",
        location: item.report?.address || "Chưa cập nhật vị trí",
        status: !item.collectorId ? "pending" : mapStatus(item.status), // Nếu ko có collector -> Pending
        createdAt: item.assignedAt ? new Date(item.assignedAt).toLocaleString("vi-VN") : "Chưa rõ",
        deadline: item.completedAt ? new Date(item.completedAt).toLocaleString("vi-VN") : "Chưa hoàn thành",
        wasteQuantity: item.collectedWeightKg ? `${item.collectedWeightKg} kg` : "Chưa cập nhật",
        notes: item.notes || "Không có ghi chú",
        rawCollectorId: item.collectorId,
      }));

      setTasks(formattedTasks);
    } catch (error) {
      console.error("Lỗi fetch tasks:", error);
    } finally {
      setLoading(false);
    }
  };

  // 2. GỌI API LẤY DANH SÁCH COLLECTORS
  const fetchCollectors = async () => {
    try {
      const token = localStorage.getItem("token") || "";
      const response = await fetch(`${API_CONFIG.BASE_URL}/enterprise/tasks/collectors`, {
        headers: { "Authorization": `Bearer ${token}` }
      });
      if (response.ok) {
        const data = await response.json();
        setCollectors(data);
      }
    } catch (error) {
      console.error("Lỗi tải danh sách Collector", error);
    }
  };

  useEffect(() => {
    fetchTasks();
    fetchCollectors();
  }, []);

  // 3. XỬ LÝ PHÂN CÔNG (ASSIGN)
  const handleAssignCollector = async () => {
    if (!selectedCollectorId) {
      alert("Vui lòng chọn nhân viên thu gom!");
      return;
    }
    if (!selectedTask) return;

    try {
      setAssignLoading(true);
      const token = localStorage.getItem("token") || "";
      const url = `${API_CONFIG.BASE_URL}/enterprise/tasks/${selectedTask.id}/assign-collector`;

      const response = await fetch(url, {
        method: "PUT",
        headers: {
          "Authorization": `Bearer ${token}`,
          "Content-Type": "application/json"
        },
        body: JSON.stringify({ collectorId: selectedCollectorId })
      });

      if (response.ok) {
        alert("Phân công thành công!");
        setSelectedTask(null);
        fetchTasks(); // Load lại data mới
      } else {
        alert("Lỗi khi phân công. Mã lỗi: " + response.status);
      }
    } catch (error) {
      console.error(error);
      alert("Lỗi mạng khi phân công!");
    } finally {
      setAssignLoading(false);
    }
  };

  // Helper UI
  const getStatusColor = (status: string) => {
    switch (status) {
      case "pending": return "bg-yellow-100 text-yellow-700 border-yellow-300";
      case "assigned": return "bg-purple-100 text-purple-700 border-purple-300";
      case "on_way": return "bg-blue-100 text-blue-700 border-blue-300";
      case "collected": return "bg-green-100 text-green-700 border-green-300";
      default: return "bg-gray-100 text-gray-700 border-gray-300";
    }
  };

  const getStatusLabel = (status: string) => {
    switch (status) {
      case "pending": return "Chưa Phân Công";
      case "assigned": return "Đã Phân Công";
      case "on_way": return "Đang Trên Đường";
      case "collected": return "Đã Thu Gom";
      default: return status;
    }
  };

  const filteredTasks = tasks.filter((task) => {
    const matchesSearch =
      task.taskNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
      task.report.toLowerCase().includes(searchTerm.toLowerCase()) ||
      task.collector.toLowerCase().includes(searchTerm.toLowerCase()) ||
      task.location.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesStatus = filterStatus === "all" || task.status === filterStatus;
    return matchesSearch && matchesStatus;
  });

  return (
    <div className="space-y-6 relative">
      {/* Header */}
      <div>
        <h1 className="text-3xl font-bold text-gray-900">Quản Lý Thu Gom</h1>
        <p className="text-gray-600 mt-2">Theo dõi và phân công các task cho người thu gom</p>
      </div>

      {/* Search & Filter */}
      <div className="flex flex-col sm:flex-row gap-4">
        <div className="flex-1 relative">
          <Search size={20} className="absolute left-3 top-1/2 -translate-y-1/2 text-gray-400" />
          <input
            type="text"
            placeholder="Tìm theo mã task, báo cáo, nhân viên..."
            value={searchTerm}
            onChange={(e) => setSearchTerm(e.target.value)}
            className="w-full pl-10 pr-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#0AA468]"
          />
        </div>
        <select
          value={filterStatus}
          onChange={(e) => setFilterStatus(e.target.value)}
          className="px-4 py-2.5 border border-gray-300 rounded-lg focus:outline-none focus:ring-2 focus:ring-[#0AA468]"
        >
          <option value="all">Tất Cả Trạng Thái</option>
          <option value="pending">Chưa Phân Công</option>
          <option value="assigned">Đã Phân Công</option>
          <option value="on_way">Đang Trên Đường</option>
          <option value="collected">Đã Thu Gom</option>
        </select>
      </div>

      {/* Tasks Cards */}
      {loading ? (
        <div className="text-center py-12 text-blue-600 font-medium">Đang tải dữ liệu...</div>
      ) : (
        <div className="grid grid-cols-1 lg:grid-cols-2 gap-4">
          {filteredTasks.length > 0 ? (
            filteredTasks.map((task) => (
              <div
                key={task.id}
                className="bg-white rounded-xl border border-gray-200 p-6 hover:shadow-md transition-shadow flex flex-col"
              >
                <div className="flex items-start justify-between mb-4">
                  <div>
                    <p className="font-bold text-lg text-gray-900">{task.taskNumber}</p>
                    <p className="text-sm text-gray-600 font-medium flex items-center gap-1 mt-1">
                      <FileText size={14} className="text-gray-400" />
                      Báo cáo: <span className="text-[#0AA468] cursor-pointer hover:underline">{task.report}</span>
                    </p>
                  </div>
                  <span className={`inline-block px-3 py-1 rounded-full text-xs font-bold border ${getStatusColor(task.status)}`}>
                    {getStatusLabel(task.status)}
                  </span>
                </div>

                <div className="space-y-3 mb-4 grow">
                  <div className="flex items-center gap-3 text-gray-700">
                    <User size={16} className={task.rawCollectorId ? "text-blue-500" : "text-gray-400"} />
                    <span className={`text-sm font-medium ${!task.rawCollectorId && "text-red-500 italic"}`}>
                      {task.collector}
                    </span>
                  </div>
                  <div className="flex items-start gap-3 text-gray-700">
                    <MapPin size={16} className="text-[#0AA468] mt-0.5 shrink-0" />
                    <span className="text-sm line-clamp-2">{task.location}</span>
                  </div>
                  <div className="flex items-center gap-3 text-gray-700">
                    <Clock size={16} className="text-gray-400 shrink-0" />
                    <span className="text-sm">Giao lúc: {task.createdAt}</span>
                  </div>
                </div>

                <div className="bg-gray-50 rounded-lg p-3 mb-4 flex items-center gap-2 border border-gray-100">
                   <Weight size={18} className="text-gray-500" />
                  <p className="text-sm text-gray-700">
                    Lượng rác: <strong className="text-gray-900">{task.wasteQuantity}</strong>
                  </p>
                </div>

                <button 
                  onClick={() => {
                    setSelectedTask(task);
                    setSelectedCollectorId(""); // Reset dropdown
                  }}
                  className={`w-full py-2.5 font-bold rounded-lg transition-all ${
                    task.status === "pending" 
                    ? "bg-amber-500 hover:bg-amber-600 text-white" // Bật màu cam để nhắc việc phân công
                    : "bg-[#0AA468] hover:bg-[#088F5A] text-white"
                  }`}
                >
                  {task.status === "pending" ? "Phân Công Ngay" : "Xem Chi Tiết"}
                </button>
              </div>
            ))
          ) : (
            <div className="col-span-full text-center py-12">
              <p className="text-gray-500 font-medium">Không tìm thấy task nào</p>
            </div>
          )}
        </div>
      )}

      {/* MODAL CHI TIẾT & PHÂN CÔNG TASK */}
      {selectedTask && (
        <div className="fixed inset-0 z-50 flex items-center justify-center bg-black/50 p-4 sm:p-6">
          <div className="bg-white rounded-xl w-full max-w-lg max-h-[90vh] flex flex-col shadow-2xl overflow-hidden">
            <div className="flex items-center justify-between p-6 border-b border-gray-100 bg-white shrink-0">
              <div>
                <h2 className="text-xl font-bold text-gray-900">Chi Tiết Task {selectedTask.taskNumber}</h2>
                <p className="text-sm text-gray-500 mt-1">Thuộc báo cáo: <span className="text-[#0AA468] font-semibold">{selectedTask.report}</span></p>
              </div>
              <button onClick={() => setSelectedTask(null)} className="p-2 hover:bg-gray-100 rounded-full text-gray-500 transition-colors">
                <X size={24} />
              </button>
            </div>

            <div className="p-6 space-y-5 overflow-y-auto grow">
              <div className="flex justify-between items-center bg-gray-50 p-4 rounded-lg border border-gray-100">
                <span className="text-sm font-medium text-gray-600">Trạng thái hiện tại:</span>
                <span className={`px-3 py-1 rounded-full text-xs font-bold border ${getStatusColor(selectedTask.status)}`}>
                  {getStatusLabel(selectedTask.status)}
                </span>
              </div>

              {/* PHÂN CÔNG NHÂN VIÊN */}
              {selectedTask.status === "pending" ? (
                <div className="bg-amber-50 border border-amber-200 p-4 rounded-xl space-y-3">
                  <label className="block text-sm font-bold text-amber-900">Chọn nhân viên phân công:</label>
                  <select 
                    value={selectedCollectorId}
                    onChange={(e) => setSelectedCollectorId(e.target.value)}
                    className="w-full p-2.5 border border-amber-300 rounded-lg focus:ring-2 focus:ring-amber-500 outline-none"
                  >
                    <option value="">-- Chọn nhân viên thu gom --</option>
                    {collectors.map(c => (
                      <option key={c.id} value={c.id}>
                        {c.name} - {c.phone} (Đang có: {c.taskCount} task)
                      </option>
                    ))}
                  </select>
                </div>
              ) : (
                <div className="space-y-4">
                  <div>
                    <p className="text-sm text-gray-500 mb-1">Người thu gom phụ trách</p>
                    <div className="flex items-center gap-2">
                      <div className="w-8 h-8 bg-blue-100 text-blue-600 rounded-full flex items-center justify-center font-bold text-sm">
                        {selectedTask.collector.charAt(0)}
                      </div>
                      <p className="font-semibold text-gray-900">{selectedTask.collector}</p>
                    </div>
                  </div>
                </div>
              )}

              <div className="space-y-4">
                <div>
                  <p className="text-sm text-gray-500 mb-1">Địa điểm thu gom</p>
                  <div className="flex items-start gap-2">
                    <MapPin size={18} className="text-[#0AA468] mt-0.5 shrink-0" />
                    <p className="font-medium text-gray-900">{selectedTask.location}</p>
                  </div>
                </div>

                <div className="grid grid-cols-2 gap-4 border-t border-gray-100 pt-4">
                  <div>
                    <p className="text-sm text-gray-500 mb-1">Thời gian phân công</p>
                    <p className="text-sm font-medium text-gray-900">{selectedTask.createdAt}</p>
                  </div>
                  <div>
                    <p className="text-sm text-gray-500 mb-1">Khối lượng thu gom</p>
                    <p className="text-sm font-medium text-gray-900">{selectedTask.wasteQuantity}</p>
                  </div>
                </div>
              </div>

              {selectedTask.notes && (
                <div>
                  <p className="text-sm font-semibold text-gray-900 mb-2">Ghi chú từ nhân viên</p>
                  <p className="text-gray-700 bg-yellow-50 border border-yellow-100 p-4 rounded-lg text-sm italic">
                    {selectedTask.notes}
                  </p>
                </div>
              )}
            </div>

            <div className="p-4 border-t border-gray-100 bg-gray-50 flex justify-end gap-3 shrink-0">
              <button
                onClick={() => setSelectedTask(null)}
                className="px-6 py-2 bg-white border border-gray-300 hover:bg-gray-100 text-gray-700 font-bold rounded-lg transition-colors"
              >
                Đóng
              </button>
              {selectedTask.status === "pending" && (
                <button
                  onClick={handleAssignCollector}
                  disabled={assignLoading}
                  className="px-6 py-2 bg-amber-500 hover:bg-amber-600 text-white font-bold rounded-lg transition-colors disabled:opacity-50"
                >
                  {assignLoading ? "Đang xử lý..." : "Xác nhận phân công"}
                </button>
              )}
            </div>
          </div>
        </div>
      )}
    </div>
  );
};