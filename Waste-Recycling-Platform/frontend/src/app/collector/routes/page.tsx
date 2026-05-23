"use client";
import React, { useEffect, useState } from "react";
import { collectorTaskApi, CollectionTask } from "@/lib/api/collectorTaskApi";
import { MapPin, Navigation, ExternalLink, Clock, Package } from "lucide-react";
import { Button, Badge } from "@/components/ui";

export default function RoutePage() {
  const [tasks, setTasks] = useState<CollectionTask[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchActiveTasks();
  }, []);

  const fetchActiveTasks = async () => {
    try {
      setLoading(true);
      const data = await collectorTaskApi.getTasks();
      // Lọc các nhiệm vụ chữa hoàn thành (Assigned, OnTheWay)
      const active = data.filter(t => t.status !== "Collected");
      
      // Sắp xếp đơn giản (VD: theo thời gian phân công, mock cho tuyến đường)
      active.sort((a, b) => new Date(a.assignedAt).getTime() - new Date(b.assignedAt).getTime());
      
      setTasks(active);
    } catch (err) {
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  const openGoogleMaps = (task: CollectionTask) => {
    // Nếu có tọa độ thì dùng tọa độ, nếu không dùng chuỗi địa chỉ
    const query = task.report.latitude && task.report.longitude 
        ? `${task.report.latitude},${task.report.longitude}` 
        : task.report.address;
    const url = `https://www.google.com/maps/search/?api=1&query=${encodeURIComponent(query)}`;
    window.open(url, "_blank");
  };

  return (
    <div className="max-w-5xl mx-auto px-4 py-8">
      <div className="mb-6 flex justify-between items-end">
        <div>
          <h1 className="text-2xl font-bold text-gray-900">Tuyến Đường Thu Gom</h1>
          <p className="text-gray-500 mt-1">Lộ trình được gợi ý để thu gom rác thải trong ngày.</p>
        </div>
        <div className="bg-emerald-50 px-4 py-2 rounded-lg text-emerald-800 font-medium">
          Tổng điểm đến: {tasks.length}
        </div>
      </div>

      {loading ? (
        <div className="text-center text-gray-500 py-12">Đang tải lịch trình...</div>
      ) : tasks.length === 0 ? (
        <div className="text-center py-20 bg-white border border-gray-200 rounded-lg shadow-sm">
          <Navigation className="mx-auto h-12 w-12 text-gray-300 mb-3" />
          <p className="text-gray-500">Bạn chưa có nhiệm vụ nào cần thu gom hôm nay.</p>
        </div>
      ) : (
        <div className="relative border-l-2 border-emerald-300 ml-4 md:ml-6 pl-6 space-y-8 pb-8">
          {tasks.map((task, index) => (
            <div key={task.id} className="relative bg-white rounded-lg p-5 shadow-sm border border-gray-100 hover:shadow-md transition-shadow">
              {/* Dot on the line */}
              <div className="absolute -left-[35px] top-6 w-6 h-6 rounded-full bg-emerald-500 border-4 border-white shadow flex items-center justify-center text-white text-xs font-bold">
                {index + 1}
              </div>

              <div className="flex flex-col md:flex-row md:justify-between md:items-start gap-4">
                <div className="space-y-3 flex-1">
                  <div className="flex items-center gap-3">
                    <h3 className="font-bold text-lg text-gray-800">Điểm Thu Gom #{index + 1}</h3>
                    <Badge variant={task.status === "OnTheWay" ? "info" : "warning"}>
                      {task.status === "OnTheWay" ? "Đang đến nơi" : "Đã phân công"}
                    </Badge>
                  </div>
                  
                  <div className="text-gray-600 flex items-start gap-2">
                    <MapPin className="h-5 w-5 text-gray-400 mt-0.5 flex-shrink-0" />
                    <span>{task.report.address}</span>
                  </div>

                  <div className="flex flex-wrap gap-4 text-sm bg-gray-50 p-3 rounded text-gray-600">
                     <span className="flex items-center gap-1.5"><Package className="w-4 h-4 text-gray-400" /> {task.report.categoryName}</span>
                     <span className="flex items-center gap-1.5"><Clock className="w-4 h-4 text-gray-400" /> {new Date(task.assignedAt).toLocaleTimeString("vi-VN", {hour: '2-digit', minute:'2-digit'})}</span>
                  </div>
                </div>

                <div className="flex flex-col gap-2 min-w-[200px]">
                   <Button onClick={() => openGoogleMaps(task)} className="w-full flex items-center justify-center bg-blue-600 hover:bg-blue-700">
                     <ExternalLink className="h-4 w-4 mr-2" />
                     Chỉ đường Google Maps
                   </Button>
                   <Button variant="outline" className="w-full" onClick={() => window.location.href = `/collector/tasks/${task.id}`}>
                     Chi tiết & Cập nhật
                   </Button>
                </div>
              </div>
            </div>
          ))}
        </div>
      )}
    </div>
  );
}