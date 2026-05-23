import React, { useMemo } from "react";
import { Card } from "../ui";
import { EnterpriseTaskStats } from "../../lib/api/enterpriseTaskApi";
import { EnterpriseRequest } from "./types";

interface ReportsAnalyticsProps {
  requests: EnterpriseRequest[];
  taskStats: EnterpriseTaskStats | null;
}

interface BucketItem {
  key: string;
  count: number;
}

const toBuckets = (items: string[]): BucketItem[] => {
  const counter = new Map<string, number>();

  items.forEach((item) => {
    const normalized = item.trim() || "Unknown";
    counter.set(normalized, (counter.get(normalized) ?? 0) + 1);
  });

  return Array.from(counter.entries())
    .map(([key, count]) => ({ key, count }))
    .sort((a, b) => b.count - a.count)
    .slice(0, 6);
};

export const ReportsAnalytics: React.FC<ReportsAnalyticsProps> = ({ requests, taskStats }) => {
  const requestsByType = useMemo(() => {
    return toBuckets(requests.map((request) => request.type));
  }, [requests]);

  const requestsByArea = useMemo(() => {
    return toBuckets(
      requests.map((request) => {
        const [firstArea] = request.location.split(",");
        return firstArea ?? "Unknown";
      })
    );
  }, [requests]);

  const maxTypeCount = Math.max(...requestsByType.map((item) => item.count), 1);
  const maxAreaCount = Math.max(...requestsByArea.map((item) => item.count), 1);

  return (
    <div className="space-y-6">
      <div className="grid grid-cols-1 gap-4 md:grid-cols-3">
        <Card className="p-5">
          <p className="text-sm text-gray-500">Total Tasks</p>
          <p className="mt-1 text-2xl font-bold text-gray-900">{taskStats?.totalTasks ?? 0}</p>
        </Card>
        <Card className="p-5">
          <p className="text-sm text-gray-500">On The Way</p>
          <p className="mt-1 text-2xl font-bold text-amber-700">{taskStats?.totalOnTheWay ?? 0}</p>
        </Card>
        <Card className="p-5">
          <p className="text-sm text-gray-500">Collected Weight (kg)</p>
          <p className="mt-1 text-2xl font-bold text-emerald-700">{taskStats?.totalWeightKg ?? 0}</p>
        </Card>
      </div>

      <div className="grid grid-cols-1 gap-6 xl:grid-cols-2">
        <Card className="p-6">
          <h3 className="text-lg font-semibold text-gray-900">Requests by Waste Type</h3>
          <p className="mt-1 text-sm text-gray-500">Top categories from incoming requests</p>

          <div className="mt-5 space-y-3">
            {requestsByType.length === 0 ? (
              <p className="text-sm text-gray-500">No data available.</p>
            ) : (
              requestsByType.map((item) => (
                <div key={item.key}>
                  <div className="mb-1 flex items-center justify-between text-sm">
                    <span className="font-medium text-gray-700">{item.key}</span>
                    <span className="text-gray-500">{item.count}</span>
                  </div>
                  <div className="h-2 rounded-full bg-gray-100">
                    <div
                      className="h-2 rounded-full bg-emerald-500"
                      style={{ width: `${(item.count / maxTypeCount) * 100}%` }}
                    />
                  </div>
                </div>
              ))
            )}
          </div>
        </Card>

        <Card className="p-6">
          <h3 className="text-lg font-semibold text-gray-900">Requests by Area</h3>
          <p className="mt-1 text-sm text-gray-500">Distribution based on report addresses</p>

          <div className="mt-5 space-y-3">
            {requestsByArea.length === 0 ? (
              <p className="text-sm text-gray-500">No data available.</p>
            ) : (
              requestsByArea.map((item) => (
                <div key={item.key}>
                  <div className="mb-1 flex items-center justify-between text-sm">
                    <span className="font-medium text-gray-700">{item.key}</span>
                    <span className="text-gray-500">{item.count}</span>
                  </div>
                  <div className="h-2 rounded-full bg-gray-100">
                    <div
                      className="h-2 rounded-full bg-sky-500"
                      style={{ width: `${(item.count / maxAreaCount) * 100}%` }}
                    />
                  </div>
                </div>
              ))
            )}
          </div>
        </Card>
      </div>
    </div>
  );
};
