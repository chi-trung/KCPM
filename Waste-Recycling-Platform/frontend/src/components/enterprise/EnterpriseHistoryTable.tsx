import React, { useEffect, useState } from "react";
import { Badge, Card, Table } from "../ui";
import { enterpriseTaskApi, EnterpriseCollectionTask } from "../../lib/api/enterpriseTaskApi";

interface HistoryRow {
  id: string;
  category: string;
  location: string;
  collector: string;
  weight: string;
  statusUpdatedAt: string;
  completedAt: string;
  status: string;
}

export const EnterpriseHistoryTable: React.FC = () => {
  const [tasks, setTasks] = useState<EnterpriseCollectionTask[]>([]);
  const [loading, setLoading] = useState(true);
  const [error, setError] = useState<string | null>(null);

  useEffect(() => {
    const fetchHistory = async () => {
      setLoading(true);
      setError(null);
      try {
        const data = await enterpriseTaskApi.getTasks("Collected");
        setTasks(data);
      } catch (err) {
        setError(err instanceof Error ? err.message : "Failed to load completed task history.");
      } finally {
        setLoading(false);
      }
    };

    fetchHistory();
  }, []);

  const tableData: HistoryRow[] = tasks.map((task) => ({
    id: `${task.id.slice(0, 8)}...`,
    category: task.report.categoryName || "Unknown",
    location: task.report.address || "Unknown",
    collector: task.collectorName || "Unknown",
    weight: task.collectedWeightKg ? `${task.collectedWeightKg} kg` : "N/A",
    statusUpdatedAt: new Date(task.latestStatusChangedAt ?? task.assignedAt).toLocaleString("vi-VN", { hour12: false }),
    completedAt: task.completedAt ? new Date(task.completedAt).toLocaleString("vi-VN", { hour12: false }) : "N/A",
    status: task.status,
  }));

  return (
    <Card className="p-0 overflow-hidden">
      <div className="flex items-center justify-between border-b border-gray-100 bg-gray-50 px-6 py-4">
        <h3 className="text-lg font-semibold text-gray-900">Completed Task History</h3>
        <Badge variant="success">Total: {tasks.length}</Badge>
      </div>

      {loading && <div className="p-8 text-center text-gray-500">Loading history...</div>}
      {error && (
        <div className="m-6 rounded-lg border border-red-200 bg-red-50 p-3 text-sm text-red-700">{error}</div>
      )}

      {!loading && !error && (
        <Table
          data={tableData}
          columns={[
            { label: "Task", key: "id", width: "10%" },
            { label: "Category", key: "category", width: "15%" },
            { label: "Collector", key: "collector", width: "15%" },
            { label: "Weight", key: "weight", width: "12%" },
            { label: "Location", key: "location", width: "26%" },
            { label: "Status Updated At", key: "statusUpdatedAt", width: "16%" },
            { label: "Completed At", key: "completedAt", width: "12%" },
            {
              label: "Status",
              key: "status",
              width: "6%",
              render: (value: string) => <Badge variant="success">{value}</Badge>,
            },
          ]}
        />
      )}
    </Card>
  );
};
