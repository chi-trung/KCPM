import React from "react";
import { useRouter } from "next/navigation";
import { Badge, Button } from "../ui";
import { MapPin, Clock } from "lucide-react";
import { CollectionTask } from "../../lib/api/collectorTaskApi";

interface TaskCardProps {
  task: CollectionTask;
  onUpdateStatus?: (task: CollectionTask) => void;
}

const statusColors: Record<string, "warning" | "info" | "success" | "default"> = {
  Assigned: "warning",
  OnTheWay: "info",
  Collected: "success",
};

export const TaskCard: React.FC<TaskCardProps> = ({ task, onUpdateStatus }) => {
  const router = useRouter();

  const getStatusLabel = (status: string) => {
    switch (status) {
      case "Assigned": return "Đã giao";
      case "OnTheWay": return "Đang đến nơi";
      case "Collected": return "Đã thu gom";
      default: return status.replace(/_/g, " ");
    }
  };

  return (
    <div className="bg-white rounded-lg border border-gray-200 p-4 hover:shadow-md transition-shadow">
      <div className="flex justify-between items-start mb-3">
        <div>
          <h3 className="font-semibold text-gray-800">{task.report.categoryName || "Chưa phân loại"}</h3>
          <p className="text-sm text-gray-500">Từ: {task.report.citizenName}</p>
        </div>
        <Badge variant={statusColors[task.status] || "default"}>
          {getStatusLabel(task.status)}
        </Badge>
      </div>

      <div className="space-y-2 mb-4">
        <div className="flex items-center text-sm text-gray-600">
          <MapPin className="h-4 w-4 mr-2 text-gray-400" />
          {task.report.address}
        </div>
        <div className="flex items-center text-sm text-gray-600">
          <Clock className="h-4 w-4 mr-2 text-gray-400" />
          Ngày giao: {new Date(task.assignedAt).toLocaleString()}
        </div>
      </div>

      <Button onClick={() => router.push(`/collector/tasks/${task.id}`)} className="w-full">
        Xem chi tiết & Cập nhật
      </Button>
    </div>
  );
};