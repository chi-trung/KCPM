import React, { useEffect, useState } from "react";
import { useRouter } from "next/navigation";
import { Table, Badge, Button } from "../ui";
import { collectorTaskApi, CollectionTask } from "../../lib/api/collectorTaskApi";

export const HistoryList: React.FC = () => {
  const router = useRouter();
  const [historyTasks, setHistoryTasks] = useState<CollectionTask[]>([]);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    fetchHistory();
  }, []);

  const fetchHistory = async () => {
    try {
      setLoading(true);
      const data = await collectorTaskApi.getTasks("Collected");
      setHistoryTasks(data);
    } catch (error) {
      console.error("Failed to fetch history:", error);
    } finally {
      setLoading(false);
    }
  };

  const tableData = historyTasks.map(task => ({
    id: task.id.substring(0, 8) + "...",
    taskId: task.id,
    type: task.report.categoryName || "Chưa phân loại",
    quantity: task.collectedWeightKg ? `${task.collectedWeightKg}kg` : "N/A",
    location: task.report.address,
    completedAt: task.completedAt ? new Date(task.completedAt).toLocaleString() : "N/A",
    status: task.status === "Collected" ? "Đã thu gom" : task.status
  }));

  return (
    <div className="bg-white rounded-lg shadow-sm border border-gray-100 overflow-hidden">
      <div className="p-4 border-b border-gray-100 bg-gray-50 flex items-center justify-between">
        <h3 className="font-semibold text-gray-800">Lịch sử thu gom</h3>
        <Badge variant="success">Tổng: {historyTasks.length}</Badge>
      </div>
      {loading ? (
        <div className="p-8 text-center text-gray-500">Đang tải lịch sử...</div>
      ) : (
        <Table 
          data={tableData}
          columns={[
            { label: "ID", key: "id", width: "10%" },
            { label: "Loại rác", key: "type", width: "15%" },
            { label: "Khối lượng", key: "quantity", width: "10%" },
            { label: "Địa điểm", key: "location", width: "25%" },
            { label: "Hoàn thành", key: "completedAt", width: "15%" },
            { 
              label: "Trạng thái", 
              key: "status",
              render: (val: string) => <Badge variant="success">{val}</Badge>
            },
            {
              label: "",
              key: "taskId",
              width: "10%",
              render: (val: string) => (
                 <Button variant="outline" size="sm" onClick={() => router.push(`/collector/tasks/${val}`)}>
                   Chi tiết
                 </Button>
              )
            }
          ]}
        />
      )}
    </div>
  );
};