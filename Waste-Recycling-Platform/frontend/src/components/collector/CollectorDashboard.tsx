"use client";
import React, { useState, useEffect } from "react";
import { LayoutDashboard, CheckCircle, MapPin, Search } from "lucide-react";
import { TaskCard } from "./TaskCard";
import { HistoryList } from "./HistoryList";
import { Modal, Button, Input, Badge } from "../ui";
import { collectorTaskApi, CollectionTask, CollectorStats } from "../../lib/api/collectorTaskApi";

export const CollectorDashboard: React.FC = () => {
  const [activeTab, setActiveTab] = useState<"tasks" | "history">("tasks");
  const [tasks, setTasks] = useState<CollectionTask[]>([]);
  const [stats, setStats] = useState<CollectorStats>({ totalAssigned: 0, totalOnTheWay: 0, totalCollected: 0, totalWeightKg: 0 });
  const [selectedTask, setSelectedTask] = useState<CollectionTask | null>(null);
  const [isModalOpen, setIsModalOpen] = useState(false);
  const [collectionImage, setCollectionImage] = useState<File | null>(null);
  const [confirmNote, setConfirmNote] = useState("");
  const [weightKg, setWeightKg] = useState("");
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchData();
  }, [activeTab]);

  const fetchData = async () => {
    try {
      setLoading(true);
      const [statsData, tasksData] = await Promise.all([
        collectorTaskApi.getStats(),
        collectorTaskApi.getTasks() // can filter server side or client side
      ]);
      setStats(statsData);
      setTasks(tasksData.filter(t => t.status !== "Collected"));
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateStatus = (task: CollectionTask) => {
    setSelectedTask(task);
    setIsModalOpen(true);
  };

  const closeModal = () => {
    setIsModalOpen(false);
    setSelectedTask(null);
    setCollectionImage(null);
    setConfirmNote("");
    setWeightKg("");
  };

  const handleSetOnTheWay = async (id: string) => {
    try {
      await collectorTaskApi.setOnTheWay(id);
      await fetchData(); // refresh data
      closeModal();
    } catch (err) {
      console.error(err);
      alert("Cập nhật trạng thái thất bại.");
    }
  };

  const handleCompleteCollection = async (id: string) => {
    if (!weightKg || Number.isNaN(Number(weightKg))) {
      alert("Vui lòng nhập khối lượng hợp lệ.");
      return;
    }

    try {
      const formData = new FormData();
      formData.append("WeightKg", weightKg);
      formData.append("Notes", confirmNote);
      if (collectionImage) {
        formData.append("Images", collectionImage);
      }

      await collectorTaskApi.completeTask(id, formData);
      await fetchData(); // refresh data
      closeModal();
    } catch (err) {
      console.error(err);
      alert("Hoàn thành nhiệm vụ thất bại.");
    }
  };

  const renderModalContent = () => {
    if (!selectedTask) return null;

    if (selectedTask.status === "Assigned") {
      return (
        <div className="space-y-4">
          <p className="text-gray-600">Bạn chuẩn bị bắt đầu thu gom cho nhiệm vụ <b>#{selectedTask.id.substring(0,8)}</b>.</p>
          <div className="bg-blue-50 p-4 rounded-md text-sm text-blue-700">
             Vui lòng đảm bảo bạn đã chuẩn bị phương tiện và dụng cụ cần thiết.
          </div>
          <div className="flex justify-end gap-2 mt-6">
            <Button variant="outline" onClick={closeModal}>Hủy</Button>
            <Button onClick={() => handleSetOnTheWay(selectedTask.id)}>Xác nhận Bắt đầu</Button>
          </div>
        </div>
      );
    }

    if (selectedTask.status === "OnTheWay") {
      return (
        <div className="space-y-4">
          <p className="text-gray-600">Hoàn thành thu gom cho nhiệm vụ <b>#{selectedTask.id.substring(0,8)}</b>.</p>
          
          <div>
             <label htmlFor="modal-weight" className="block text-sm font-medium text-gray-700 mb-1">Khối lượng (kg) *</label>
             <Input 
               id="modal-weight"
               type="number"
               value={weightKg} 
               onChange={(e) => setWeightKg(e.target.value)} 
               placeholder="ví dụ: 25" 
               required
             />
          </div>

          <div>
            <label htmlFor="file-upload" className="block text-sm font-medium text-gray-700 mb-1">Hình ảnh xác nhận thu gom</label>
            <div className="mt-1 flex justify-center px-6 pt-5 pb-6 border-2 border-gray-300 border-dashed rounded-md hover:border-emerald-500 transition-colors cursor-pointer bg-gray-50">
               <div className="space-y-1 text-center">
                 <svg className="mx-auto h-12 w-12 text-gray-400" stroke="currentColor" fill="none" viewBox="0 0 48 48" aria-hidden="true">
                   <path d="M28 8H12a4 4 0 00-4 4v20m32-12v8m0 0v8a4 4 0 01-4 4H12a4 4 0 01-4-4v-4m32-4l-3.172-3.172a4 4 0 00-5.656 0L28 28M8 32l9.172-9.172a4 4 0 015.656 0L28 28m0 0l4 4m4-24h8m-4-4v8m-12 4h.02" strokeWidth="2" strokeLinecap="round" strokeLinejoin="round" />
                 </svg>
                 <div className="flex text-sm text-gray-600">
                   <label htmlFor="file-upload" className="relative cursor-pointer bg-white rounded-md font-medium text-emerald-600 hover:text-emerald-500 focus-within:outline-none focus-within:ring-2 focus-within:ring-offset-2 focus-within:ring-emerald-500">
                     <span>Tải tệp lên</span>
                     <input id="file-upload" name="file-upload" type="file" className="sr-only" onChange={(e: React.ChangeEvent<HTMLInputElement>) => setCollectionImage(e.target.files?.[0] || null)} />
                   </label>
                   <p className="pl-1">hoặc kéo thả</p>
                 </div>
                 <p className="text-xs text-gray-500">PNG, JPG, GIF tối đa 10MB</p>
               </div>
            </div>
            {collectionImage && <p className="text-sm text-emerald-600 mt-2">Đã chọn: {collectionImage.name}</p>}
          </div>

          <div>
             <label htmlFor="modal-notes" className="block text-sm font-medium text-gray-700 mb-1">Ghi chú (Tùy chọn)</label>
             <Input 
               id="modal-notes"
               value={confirmNote} 
               onChange={(e) => setConfirmNote(e.target.value)} 
               placeholder="Nhập vấn đề phát sinh hoặc thông tin thêm..." 
             />
          </div>

          <div className="flex justify-end gap-2 mt-6">
            <Button variant="outline" onClick={closeModal}>Hủy</Button>
            <Button 
                onClick={() => handleCompleteCollection(selectedTask.id)}
                disabled={!collectionImage || !weightKg}
            >
                Hoàn thành nhiệm vụ
            </Button>
          </div>
        </div>
      );
    }

    if (selectedTask.status === "Collected") {
       return (
        <div className="space-y-4">
           <div className="bg-emerald-50 p-4 rounded-md flex items-start gap-3">
              <CheckCircle className="h-5 w-5 text-emerald-600 mt-0.5" />
              <div>
                 <h4 className="text-sm font-medium text-emerald-800">Đã hoàn thành nhiệm vụ</h4>
                 <p className="text-sm text-emerald-700 mt-1">
                    Nhiệm vụ này đã được thu gom và ghi nhận thành công.
                 </p>
              </div>
           </div>
           
           <div className="grid grid-cols-2 gap-4 text-sm">
             <div>
                <span className="text-gray-500 block">Loại rác</span>
                <span className="font-medium">{selectedTask.report.categoryName}</span>
             </div>
             <div>
                <span className="text-gray-500 block">Khối lượng</span>
                <span className="font-medium">{selectedTask.collectedWeightKg} kg</span>
             </div>
              <div>
                <span className="text-gray-500 block">Địa điểm</span>
                <span className="font-medium truncate">{selectedTask.report.address}</span>
             </div>
             <div>
                <span className="text-gray-500 block">Hoàn thành lúc</span>
                <span className="font-medium">{new Date(selectedTask.completedAt || "").toLocaleString()}</span>
             </div>
           </div>

           <div className="flex justify-end mt-6">
             <Button variant="outline" onClick={closeModal}>Đóng</Button>
           </div>
        </div>
       );
    }
    
    return null;
  };

  return (
    <div className="space-y-6">
        {/* Stats Overview */}
        <div className="grid grid-cols-1 md:grid-cols-4 gap-6">
            <div className="bg-white p-6 rounded-lg border border-gray-100 shadow-sm flex items-center justify-between">
                <div>
                   <p className="text-sm font-medium text-gray-500 uppercase">Nhiệm vụ mở</p>
                   <p className="text-3xl font-bold text-gray-900 mt-1">{stats.totalAssigned}</p>
                </div>
                <div className="p-3 bg-blue-50 rounded-full text-blue-600">
                   <LayoutDashboard className="h-6 w-6" />
                </div>
            </div>
            <div className="bg-white p-6 rounded-lg border border-gray-100 shadow-sm flex items-center justify-between">
                <div>
                   <p className="text-sm font-medium text-gray-500 uppercase">Đang đến nơi</p>
                   <p className="text-3xl font-bold text-purple-600 mt-1">{stats.totalOnTheWay}</p>
                </div>
                <div className="p-3 bg-purple-50 rounded-full text-purple-600">
                   <MapPin className="h-6 w-6" />
                </div>
            </div>
            <div className="bg-white p-6 rounded-lg border border-gray-100 shadow-sm flex items-center justify-between">
                <div>
                   <p className="text-sm font-medium text-gray-500 uppercase">Đã thu gom</p>
                   <p className="text-3xl font-bold text-emerald-600 mt-1">{stats.totalCollected}</p>
                </div>
                <div className="p-3 bg-emerald-50 rounded-full text-emerald-600">
                   <CheckCircle className="h-6 w-6" />
                </div>
            </div>
            <div className="bg-white p-6 rounded-lg border border-gray-100 shadow-sm flex items-center justify-between">
                <div>
                   <p className="text-sm font-medium text-gray-500 uppercase">Tổng khối lượng</p>
                   <p className="text-3xl font-bold text-emerald-600 mt-1">{stats.totalWeightKg} kg</p>
                </div>
                <div className="p-3 bg-emerald-50 rounded-full text-emerald-600">
                   <CheckCircle className="h-6 w-6" />
                </div>
            </div>
        </div>

        {/* Tabs & Content */}
        <div className="bg-white rounded-xl shadow-sm border border-gray-200 overflow-hidden min-h-[500px]">
            <div className="border-b border-gray-200 bg-gray-50 px-6 py-4 flex items-center justify-between">
               <div className="flex space-x-6">
                  <button 
                    onClick={() => setActiveTab("tasks")}
                    className={`pb-2 text-sm font-medium border-b-2 transition-colors ${activeTab === "tasks" ? "border-emerald-500 text-emerald-700" : "border-transparent text-gray-500 hover:text-gray-700"}`}
                  >
                    Nhiệm vụ hiện tại
                  </button>
                  <button 
                    onClick={() => setActiveTab("history")}
                    className={`pb-2 text-sm font-medium border-b-2 transition-colors ${activeTab === "history" ? "border-emerald-500 text-emerald-700" : "border-transparent text-gray-500 hover:text-gray-700"}`}
                  >
                    Lịch sử
                  </button>
               </div>
               
               {activeTab === "tasks" && (
                 <div className="relative">
                    <Search className="absolute left-3 top-1/2 -translate-y-1/2 h-4 w-4 text-gray-400" />
                    <input 
                      type="text" 
                      placeholder="Tìm kiếm nhiệm vụ..." 
                      className="pl-9 pr-4 py-2 border border-gray-300 rounded-md text-sm focus:ring-emerald-500 focus:border-emerald-500"
                    />
                 </div>
               )}
            </div>

            <div className="p-6">
               {activeTab === "tasks" ? (
                  loading ? (
                    <div className="py-12 text-center text-gray-500">Đang tải nhiệm vụ...</div>
                  ) : (
                    <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
                      {tasks.map(task => (
                        <TaskCard key={task.id} task={task} onUpdateStatus={handleUpdateStatus} />
                      ))}
                      {tasks.length === 0 && (
                          <div className="col-span-full py-12 text-center text-gray-500">
                             Hiện không có nhiệm vụ nào được giao.
                          </div>
                      )}
                    </div>
                  )
               ) : (
                  <HistoryList />
               )}
            </div>
        </div>

        {/* Action Modal */}
        <Modal 
          isOpen={isModalOpen}
          onClose={closeModal}
          title={selectedTask ? `Cập nhật nhiệm vụ #${selectedTask.id.substring(0,8)}` : "Cập nhật nhiệm vụ"}
        >
           {renderModalContent()}
        </Modal>
    </div>
  );
};