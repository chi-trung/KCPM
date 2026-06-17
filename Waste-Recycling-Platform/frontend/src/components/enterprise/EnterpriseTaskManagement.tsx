"use client";
import React, { useState, useEffect, useMemo } from "react";
import { API_CONFIG } from "@/lib/api/config";
import {
  Card,
  Button,
  Badge,
  Select,
  Modal,
} from "../ui";
import {
  enterpriseTaskApi,
  EnterpriseCollectionTask,
  EnterpriseCollector,
  EnterpriseTaskStats,
  TaskProgressResponse,
} from "../../lib/api/enterpriseTaskApi";
import { AlertCircle, MapPin, User, CheckCircle, Clock } from "lucide-react";
import { HubConnectionBuilder, LogLevel } from "@microsoft/signalr";
import { useAuth } from "@/contexts/AuthContext";

export const EnterpriseTaskManagement: React.FC = () => {
  const { token } = useAuth();
  const [tasks, setTasks] = useState<EnterpriseCollectionTask[]>([]);
  const [collectors, setCollectors] = useState<EnterpriseCollector[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [selectedTask, setSelectedTask] = useState<EnterpriseCollectionTask | null>(null);
  const [assignModalOpen, setAssignModalOpen] = useState(false);
  const [selectedCollector, setSelectedCollector] = useState<string>("");
  const [filterStatus, setFilterStatus] = useState<string>("all");
  const [showUnassignedOnly, setShowUnassignedOnly] = useState(false);
  const [taskStats, setTaskStats] = useState<EnterpriseTaskStats>({
    totalTasks: 0,
    totalUnassigned: 0,
    totalAssigned: 0,
    totalOnTheWay: 0,
    totalCollected: 0,
    totalWeightKg: 0,
  });

  // Progress tracking states
  const [progressModalOpen, setProgressModalOpen] = useState(false);
  const [progressData, setProgressData] = useState<TaskProgressResponse | null>(null);
  const [progressLoading, setProgressLoading] = useState(false);

  // Fetch tasks and collectors on component mount
  useEffect(() => {
    fetchData();
  }, [filterStatus, showUnassignedOnly]);

  const fetchData = async () => {
    setLoading(true);
    setError(null);
    try {
      const [tasksData, collectorsData, statsData] = await Promise.all([
        enterpriseTaskApi.getTasks(
          filterStatus !== "all" ? filterStatus : undefined,
          showUnassignedOnly
        ),
        enterpriseTaskApi.getAvailableCollectors(),
        enterpriseTaskApi.getStats(),
      ]);
      setTasks(tasksData);
      setCollectors(collectorsData);
      setTaskStats(statsData);
    } catch (err) {
      setError(err instanceof Error ? err.message : "Failed to fetch data");
      console.error(err);
    } finally {
      setLoading(false);
    }
  };

  // SignalR Real-time Updates Setup
  useEffect(() => {
    if (!token) return;
    
    const backendUrl = API_CONFIG.SERVER_URL;
    const newConnection = new HubConnectionBuilder()
      .withUrl(`${backendUrl}/hubs/task`, {
        accessTokenFactory: () => token
      })
      .configureLogging(LogLevel.Information)
      .withAutomaticReconnect()
      .build();

    newConnection.start()
      .then(() => {
        console.log("SignalR Connected");
        newConnection.on("TaskStatusUpdated", (taskId, status) => {
          console.log(`Task ${taskId} status changed to ${status}`);
          // Trigger a refresh when status changes
          fetchData();
        });
      })
      .catch((e) => console.log("SignalR Connection Error: ", e));

    return () => {
      newConnection.stop();
    };
  }, [token]);

  const handleAssignClick = (task: EnterpriseCollectionTask) => {
    setSelectedTask(task);
    setSelectedCollector("");
    setAssignModalOpen(true);
  };

  const handleAssignConfirm = async () => {
    if (!selectedTask || !selectedCollector) {
      alert("Please select a collector");
      return;
    }

    try {
      await enterpriseTaskApi.assignCollector(selectedTask.id, selectedCollector);
      alert("Collector assigned successfully!");
      setAssignModalOpen(false);
      setSelectedTask(null);
      setSelectedCollector("");
      await fetchData();
    } catch (err) {
      console.error(err);
      alert("Failed to assign collector");
    }
  };

  const getStatusColor = (status: string) => {
    const statusMap: Record<string, string> = {
      Assigned: "bg-blue-100 text-blue-800",
      OnTheWay: "bg-yellow-100 text-yellow-800",
      Collected: "bg-green-100 text-green-800",
    };
    return statusMap[status] || "bg-gray-100 text-gray-800";
  };

  const handleProgressClick = async (task: EnterpriseCollectionTask) => {
    setProgressModalOpen(true);
    setProgressData(null);
    setProgressLoading(true);
    try {
      const data = await enterpriseTaskApi.getTaskProgress(task.id);
      setProgressData(data);
    } catch (err) {
      console.error("Failed to fetch task progress:", err);
      alert("Failed to load task progress.");
      setProgressModalOpen(false);
    } finally {
      setProgressLoading(false);
    }
  };

  const unassignedCount = tasks.filter((t: EnterpriseCollectionTask) => !t.collectorId).length;

  const mapTasks = tasks.filter(
    (task) =>
      task.report.latitude !== null &&
      task.report.longitude !== null &&
      !Number.isNaN(task.report.latitude) &&
      !Number.isNaN(task.report.longitude)
  );

  const mapUrl = useMemo(() => {
    if (!mapTasks.length) return "";
    
    const lats = mapTasks.map(t => t.report.latitude);
    const lons = mapTasks.map(t => t.report.longitude);
    
    const padding = 0.005;
    const minLat = Math.min(...lats) - padding;
    const maxLat = Math.max(...lats) + padding;
    const minLon = Math.min(...lons) - padding;
    const maxLon = Math.max(...lons) + padding;
    
    const bbox = `${minLon},${minLat},${maxLon},${maxLat}`;
    
    return `https://www.openstreetmap.org/export/embed.html?bbox=${bbox}&layer=mapnik&marker=${lats[0]},${lons[0]}`;
  }, [mapTasks]);

  const mapLink = useMemo(() => {
    if (!mapTasks.length) return "https://www.openstreetmap.org";
    const first = mapTasks[0].report;
    return `https://www.openstreetmap.org/?mlat=${first.latitude}&mlon=${first.longitude}#map=13/${first.latitude}/${first.longitude}`;
  }, [mapTasks]);

  return (
    <div className="space-y-6">
      {/* Header */}
      <Card className="p-6">
        <h2 className="text-2xl font-bold text-gray-800 mb-4">
          Collector Assignment Management
        </h2>
        <p className="text-gray-600">
          {unassignedCount > 0 && (
            <span className="font-semibold">
              ⚠️ {unassignedCount} unassigned task(s)
            </span>
          )}
        </p>
      </Card>

      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card className="p-4 bg-white border border-gray-100 shadow-sm">
          <p className="text-sm text-gray-500 uppercase font-medium">Total Tasks</p>
          <p className="text-3xl font-bold text-gray-900 mt-3">{taskStats.totalTasks}</p>
        </Card>
        <Card className="p-4 bg-white border border-gray-100 shadow-sm">
          <p className="text-sm text-gray-500 uppercase font-medium">Unassigned</p>
          <p className="text-3xl font-bold text-red-600 mt-3">{taskStats.totalUnassigned}</p>
        </Card>
        <Card className="p-4 bg-white border border-gray-100 shadow-sm">
          <p className="text-sm text-gray-500 uppercase font-medium">On The Way</p>
          <p className="text-3xl font-bold text-yellow-700 mt-3">{taskStats.totalOnTheWay}</p>
        </Card>
        <Card className="p-4 bg-white border border-gray-100 shadow-sm">
          <p className="text-sm text-gray-500 uppercase font-medium">Collected</p>
          <p className="text-3xl font-bold text-emerald-600 mt-3">{taskStats.totalCollected}</p>
        </Card>
      </div>

      {mapTasks.length > 0 && (
        <Card className="p-4 bg-white border border-gray-100 shadow-sm">
          <div className="flex items-center justify-between mb-3">
            <div>
              <h3 className="text-lg font-semibold text-gray-900">Bản đồ vị trí thu gom</h3>
              <p className="text-sm text-gray-500 mt-1">
                Hiển thị {mapTasks.length} vị trí thu gom hiện tại. Nhấn vào bản đồ để xem chi tiết.
              </p>
            </div>
            <span className="text-xs uppercase tracking-wide text-gray-400">
              WRP-113
            </span>
          </div>
          <div className="block overflow-hidden rounded-xl border border-gray-200">
            <iframe
              src={mapUrl}
              className="w-full h-[320px] object-cover bg-gray-100"
              title="Enterprise collection task locations"
              style={{ border: 'none' }}
              loading="lazy"
            />
          </div>
          <p className="text-xs text-gray-500 mt-2 flex justify-between">
            <span>Bản đồ tương tác đa hướng (Kéo, Thả, Phóng to). Điểm đánh dấu là công việc đầu tiên.</span>
            <a href={mapLink} target="_blank" rel="noreferrer" className="text-blue-600 hover:text-blue-800 underline">
              Xem toàn màn hình
            </a>
          </p>
        </Card>
      )}

      {/* Error Message */}
      {error && (
        <div className="bg-red-50 border border-red-200 rounded-lg p-4 flex gap-3">
          <AlertCircle className="h-5 w-5 text-red-600 flex-shrink-0 mt-0.5" />
          <div>
            <p className="font-semibold text-red-800">Error</p>
            <p className="text-red-700 text-sm">{error}</p>
          </div>
        </div>
      )}

      {/* Filters */}
      <Card className="p-6">
        <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
          <div>
            <label htmlFor="ent-filter-status" className="block text-sm font-medium text-gray-700 mb-2">
              Filter by Status
            </label>
            <Select
              id="ent-filter-status"
              options={[
                { value: "all", label: "Tất cả trạng thái (All)" },
                { value: "Assigned", label: "Đã gán (Assigned)" },
                { value: "OnTheWay", label: "Trên đường (On the Way)" },
                { value: "Collected", label: "Hoàn thành (Collected)" },
              ]}
              value={filterStatus}
              onChange={(e: React.ChangeEvent<HTMLSelectElement>) => setFilterStatus(e.target.value)}
            />
          </div>

          <div className="flex items-end">
            <label className="flex items-center gap-2 cursor-pointer">
              <input
                type="checkbox"
                checked={showUnassignedOnly}
                onChange={(e: React.ChangeEvent<HTMLInputElement>) => setShowUnassignedOnly(e.target.checked)}
                className="w-4 h-4 rounded border-gray-300"
              />
              <span className="text-sm font-medium text-gray-700">
                Chỉ hiển thị chưa được gán (Unassigned Only)
              </span>
            </label>
          </div>

          <div className="flex items-end">
            <Button
              onClick={fetchData}
              disabled={loading}
              className="w-full bg-blue-600 hover:bg-blue-700 text-white"
            >
              {loading ? "Đang tải dữ liệu..." : "Tải lại dữ liệu (Refresh)"}
            </Button>
          </div>
        </div>
        <div className="mt-4 p-3 bg-blue-50 text-blue-800 text-sm rounded-lg border border-blue-100 flex items-center justify-between">
            <p>💡 <b>Lưu ý:</b> Nút <strong>Gán Nhiệm Vụ (Assign)</strong> được gắn ở từng công việc trong danh sách phía bên dưới.</p>
        </div>
      </Card>

      {/* Tasks List */}
      <Card className="overflow-hidden mt-4">
        <div className="border-b border-gray-100 px-6 py-4">
          <h4 className="font-semibold text-gray-900">Collection Tasks Directory</h4>
        </div>

        {tasks.length === 0 ? (
          <div className="px-6 py-10 text-center text-sm text-gray-500">
            <AlertCircle className="h-12 w-12 text-gray-400 mx-auto mb-2" />
            <p>No tasks found</p>
          </div>
        ) : (
          <div className="overflow-x-auto">
            <table className="min-w-full divide-y divide-gray-200">
              <thead className="bg-gray-50 text-left text-xs uppercase tracking-wide text-gray-500">
                <tr>
                  <th className="px-6 py-3">Task Details</th>
                  <th className="px-6 py-3">Location & Contact</th>
                  <th className="px-6 py-3">Collector</th>
                  <th className="px-6 py-3">Status & Data</th>
                  <th className="px-6 py-3">Actions</th>
                </tr>
              </thead>
              <tbody className="divide-y divide-gray-100 bg-white text-sm">
                {tasks.map((task: EnterpriseCollectionTask) => (
                  <tr key={task.id} className="hover:bg-gray-50 transition">
                    <td className="px-6 py-4 align-top">
                      <p className="font-semibold text-gray-900">Task {task.id.substring(0, 8)}</p>
                      <p className="text-xs text-gray-500 mt-1">Date: {new Date(task.assignedAt).toLocaleDateString("vi-VN")}</p>
                      {task.report.categoryName && (
                        <div className="mt-2">
                          <Badge className="bg-purple-100 text-purple-800">{task.report.categoryName}</Badge>
                        </div>
                      )}
                    </td>
                    <td className="px-6 py-4 align-top max-w-[280px]">
                      <div className="flex items-start gap-2 mb-2">
                        <MapPin className="h-4 w-4 text-gray-400 mt-0.5 flex-shrink-0" />
                        <div>
                          <p className="font-medium text-gray-900 break-words whitespace-normal">{task.report.address}</p>
                          <p className="text-xs text-gray-500">📍 {task.report.latitude.toFixed(4)}, {task.report.longitude.toFixed(4)}</p>
                        </div>
                      </div>
                      <div className="flex items-start gap-2">
                        <User className="h-4 w-4 text-gray-400 mt-0.5 flex-shrink-0" />
                        <div>
                          <p className="font-medium text-gray-900">{task.report.citizenName}</p>
                          {task.report.citizenPhone && <p className="text-xs text-gray-500">{task.report.citizenPhone}</p>}
                        </div>
                      </div>
                      {task.report.description && (
                        <p className="text-xs text-gray-600 mt-2 border-t border-gray-100 pt-2 break-words whitespace-normal">
                          Note: {task.report.description}
                        </p>
                      )}
                    </td>
                    <td className="px-6 py-4 align-top">
                      {task.collectorId ? (
                        <div>
                          <p className="font-medium text-blue-700 flex items-center gap-1">
                            <CheckCircle className="h-3 w-3" /> {task.collectorName}
                          </p>
                          {task.collectorPhone && <p className="text-xs text-gray-500">{task.collectorPhone}</p>}
                        </div>
                      ) : (
                        <Badge className="bg-red-100 text-red-800">Unassigned</Badge>
                      )}
                    </td>
                    <td className="px-6 py-4 align-top">
                      <div className="flex flex-col gap-2 items-start">
                        <Badge className={getStatusColor(task.status)}>
                          {task.status}
                        </Badge>
                        <p className="text-xs text-gray-500">
                          Updated: {new Date(task.latestStatusChangedAt ?? task.assignedAt).toLocaleString("vi-VN", { hour12: false })}
                        </p>
                        {task.status.toLowerCase() === "collected" && task.collectedWeightKg && (
                          <div className="text-xs text-green-700 font-medium bg-green-50 px-2 py-1 rounded inline-block border border-green-200">
                            Weight: {task.collectedWeightKg} kg
                            {task.notes && <span className="text-green-600 block break-words whitespace-normal">{task.notes}</span>}
                          </div>
                        )}
                      </div>
                    </td>
                    <td className="px-6 py-4 align-top">
                      <div className="flex flex-col gap-2">
                        {!task.collectorId && task.status.toLowerCase() === "assigned" && (
                          <Button
                            onClick={() => handleAssignClick(task)}
                            size="sm"
                            className="bg-emerald-600 hover:bg-emerald-700 text-white w-full"
                          >
                            Assign
                          </Button>
                        )}
                        <Button
                          onClick={() => handleProgressClick(task)}
                          size="sm"
                          variant="outline"
                          className="w-full flex items-center justify-center gap-1"
                        >
                          <Clock className="w-4 h-4" /> Progress
                        </Button>
                      </div>
                    </td>
                  </tr>
                ))}
              </tbody>
            </table>
          </div>
        )}
      </Card>

      {/* Assign Collector Modal */}
      <Modal
        isOpen={assignModalOpen}
        onClose={() => {
          setAssignModalOpen(false);
          setSelectedTask(null);
          setSelectedCollector("");
        }}
        title="Assign Collector to Task"
        onConfirm={handleAssignConfirm}
        confirmText="Assign"
      >
        <div className="space-y-4">
          {selectedTask && (
            <>
              <div className="bg-blue-50 border border-blue-200 rounded p-3">
                <p className="text-sm font-medium text-blue-900">Task Details</p>
                <p className="text-sm text-blue-800 mt-1">
                  Location: {selectedTask.report.address}
                </p>
                <p className="text-sm text-blue-800">
                  Citizen: {selectedTask.report.citizenName}
                </p>
              </div>

              <div>
                <label htmlFor="ent-select-collector" className="block text-sm font-medium text-gray-700 mb-2">
                  Select Collector
                </label>
                {collectors.length === 0 ? (
                  <p className="text-sm text-red-600">
                    No collectors available for this enterprise
                  </p>
                ) : (
                  <Select
                    id="ent-select-collector"
                    options={collectors.map((c) => ({
                      value: c.id,
                      label: `${c.name} (${c.taskCount} active task${c.taskCount !== 1 ? "s" : ""})`,
                    }))}
                    value={selectedCollector}
                    onChange={(e) => setSelectedCollector(e.target.value)}
                    placeholder="Choose a Collector..."
                  />
                )}
              </div>
            </>
          )}
        </div>
      </Modal>

      {/* Progress Modal */}
      <Modal
        isOpen={progressModalOpen}
        onClose={() => {
          setProgressModalOpen(false);
          setProgressData(null);
        }}
        title="Task Progress Timeline"
        confirmText="Close"
        onConfirm={() => setProgressModalOpen(false)}
      >
        <div className="p-2 max-h-[60vh] overflow-y-auto">
          {progressLoading ? (
            <div className="flex justify-center py-8">
              <span className="text-gray-500">Loading progress...</span>
            </div>
          ) : progressData ? (
            <div className="space-y-6">
              <div className="flex justify-between items-center border-b pb-3 border-gray-100">
                <span className="font-semibold text-gray-700">Task ID: {progressData.taskId.substring(0, 8)}</span>
                <Badge className={getStatusColor(progressData.currentStatus)}>
                  {progressData.currentStatus}
                </Badge>
              </div>
              
              <div className="relative border-l border-gray-200 ml-3 space-y-6">
                {progressData.timeline.map((event, index) => (
                  <div key={index} className="pl-6 relative">
                    <span 
                      className={`absolute -left-1.5 top-1 w-3 h-3 rounded-full border-2 border-white ${
                         event.status === 'Collected' ? 'bg-green-500' :
                         event.status === 'OnTheWay' ? 'bg-yellow-500' :
                         'bg-blue-500'
                      }`}
                    ></span>
                    <div className="flex flex-col gap-1">
                      <span className="text-sm font-semibold text-gray-900">{event.status}</span>
                      <span className="text-xs text-gray-500">
                        {new Date(event.timestamp).toLocaleString("vi-VN", { hour12: false })}
                      </span>
                      {event.details && (
                        <p className="text-sm text-gray-700 mt-1">{event.details}</p>
                      )}
                      {event.collectedWeightKg && (
                        <div className="mt-2 bg-green-50 border border-green-100 rounded p-2 text-sm text-green-800 inline-block">
                          <span className="font-semibold">Weight:</span> {event.collectedWeightKg} kg
                          {event.notes && <p className="mt-1 text-sm italic">{event.notes}</p>}
                        </div>
                      )}
                      {event.images && event.images.length > 0 && (
                        <div className="mt-3 flex gap-2 overflow-x-auto pb-2">
                          {event.images.map((img, i) => (
                            <a href={img} target="_blank" rel="noreferrer" key={i}>
                              <img src={img} alt={`Ảnh thu gom ${i + 1}`} className="h-20 w-20 object-cover rounded border border-gray-200" />
                            </a>
                          ))}
                        </div>
                      )}
                    </div>
                  </div>
                ))}
              </div>
            </div>
          ) : (
            <div className="text-center py-4 text-gray-500">No data found</div>
          )}
        </div>
      </Modal>
    </div>
  );
};
