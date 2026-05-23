import { ApiError, apiClient } from "./client";
import { API_CONFIG } from "./config";

export interface TaskReport {
  id: string;
  description: string;
  address: string;
  latitude: number;
  longitude: number;
  status: string;
  categoryName: string | null;
  citizenName: string;
  citizenPhone: string | null;
  imageUrls: string[];
}

export interface CollectionTask {
  id: string;
  reportId: string;
  enterpriseId: string;
  collectorId: string;
  status: "Assigned" | "OnTheWay" | "Collected";
  collectedWeightKg: number | null;
  notes: string | null;
  assignedAt: string;
  completedAt: string | null;
  report: TaskReport;
  images?: string[];
  statusLogs?: { status: string; changedAt: string }[];
}

export interface CollectorStats {
  totalAssigned: number;
  totalOnTheWay: number;
  totalCollected: number;
  totalWeightKg: number;
}

export const collectorTaskApi = {
  /**
   * Fetches collection tasks for the current collector
   */
  getTasks: (status?: string) => {
    const query = status ? `?status=${status}` : "";
    return apiClient.get<CollectionTask[]>(`/collector/tasks${query}`);
  },

  /**
   * WRP-109: Fetch task detail by ID
   */
  getTaskById: (id: string) => {
    return apiClient.get<any>(`/collector/tasks/${id}`);
  },

  /**
   * Update task status to "OnTheWay"
   */
  setOnTheWay: (id: string) => {
    return apiClient.put<{ message: string; taskId: string }>(`/collector/tasks/${id}/on-the-way`, {});
  },

  /**
   * Complete task with "Collected" status, weight, notes and optional images
   */
  completeTask: async (id: string, formData: FormData) => {
    const token = typeof window !== "undefined" ? localStorage.getItem("token") : null;

    const headers: HeadersInit = {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    };

    const res = await fetch(`${API_CONFIG.BASE_URL}/collector/tasks/${id}/complete`, {
      method: "PUT",
      headers,
      body: formData,
    });

    let data: unknown;
    const contentType = res.headers.get("content-type") ?? "";
    if (contentType.includes("application/json")) {
      data = await res.json();
    } else {
      data = await res.text();
    }

    if (!res.ok) {
      const message =
        typeof data === "object" && data !== null && "message" in data
          ? String((data as { message: string }).message)
          : `HTTP ${res.status}`;
      throw new ApiError(res.status, message, data);
    }

    return data as { message: string; taskId: string };
  },

  /**
   * Fetches statistics for the dashboard
   */
  getStats: () => {
    return apiClient.get<CollectorStats>("/collector/tasks/stats");
  }
};
