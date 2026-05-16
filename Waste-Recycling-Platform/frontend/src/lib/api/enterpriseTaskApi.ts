import { ApiError, apiClient } from "./client";

export interface EnterpriseTaskReport {
  id: string;
  description: string;
  address: string;
  latitude: number;
  longitude: number;
  status: string;
  categoryName: string | null;
  citizenName: string;
  citizenPhone: string | null;
}

export interface EnterpriseCollectionTask {
  id: string;
  reportId: string;
  enterpriseId: string;
  collectorId: string | null;
  collectorName: string | null;
  collectorPhone: string | null;
  status: "Assigned" | "OnTheWay" | "Collected";
  collectedWeightKg: number | null;
  notes: string | null;
  assignedAt: string;
  completedAt: string | null;
  latestStatusChangedAt: string | null;
  report: EnterpriseTaskReport;
}

export interface EnterpriseCollector {
  id: string;
  name: string;
  email: string;
  phone: string | null;
  isAvailable: boolean;
  createdAt: string;
  taskCount: number;
}

export interface EnterpriseTaskStats {
  totalTasks: number;
  totalUnassigned: number;
  totalAssigned: number;
  totalOnTheWay: number;
  totalCollected: number;
  totalWeightKg: number;
}

export interface EnterpriseWasteCategory {
  id: number;
  name: string;
}

export interface EnterpriseProfile {
  id: string;
  companyName: string;
  serviceArea: string | null;
  capacityKgPerDay: number | null;
  status?: "Pending" | "Verified" | "Rejected"; // Status for approval workflow
  rejectionReason?: string | null; // Reason if rejected
}

export interface TaskTimelineEvent {
  status: string;
  timestamp: string;
  details?: string | null;
  collectedWeightKg?: number | null;
  notes?: string | null;
  images?: string[];
}

export interface TaskProgressResponse {
  taskId: string;
  currentStatus: string;
  timeline: TaskTimelineEvent[];
}

export const enterpriseTaskApi = {
  /**
   * Fetches collection tasks for the current enterprise
   */
  getTasks: (status?: string, unassignedOnly?: boolean) => {
    const params = new URLSearchParams();
    if (status) params.append("status", status);
    if (unassignedOnly) params.append("unassigned", "true");
    const query = params.toString() ? `?${params.toString()}` : "";
    return apiClient.get<EnterpriseCollectionTask[]>(`/enterprise/tasks${query}`);
  },

  /**
   * Assign a collector to a task
   */
  assignCollector: (taskId: string, collectorId: string) => {
    return apiClient.put<{ message: string; taskId: string; collectorId: string }>(
      `/enterprise/tasks/${taskId}/assign-collector`,
      { collectorId }
    );
  },

  /**
   * Get available collectors for this enterprise
   */
  getAvailableCollectors: () => {
    return apiClient.get<EnterpriseCollector[]>(`/enterprise/tasks/collectors`);
  },

  /**
   * Get statistics for enterprise tasks
   */
  getStats: () => {
    return apiClient.get<EnterpriseTaskStats>(`/enterprise/tasks/stats`);
  },

  /**
   * Get enterprise profile and accepted waste types
   */
  getProfile: () => {
    return apiClient.get<{
      id: string;
      companyName: string;
      serviceArea: string | null;
      capacityKgPerDay: number | null;
      status?: "Pending" | "Verified" | "Rejected";
      rejectionReason?: string | null;
      acceptedWasteTypes: Array<{ wasteCategoryId: number; categoryName: string }>;
    }>(`/enterprise/tasks/profile`);
  },

  /**
   * Update enterprise profile fields
   */
  updateProfile: (payload: { serviceArea: string; capacityKgPerDay: number | null }) => {
    return apiClient.put<{ message: string }>(`/enterprise/tasks/profile`, payload);
  },

  /**
   * Get available waste categories and currently accepted category IDs
   */
  getWasteTypes: () => {
    return apiClient.get<{
      allCategories: Array<{ id: number; name: string }>;
      acceptedIds: number[];
    }>(`/enterprise/tasks/waste-types`);
  },

  /**
   * Update the list of accepted waste categories for this enterprise
   */
  updateWasteTypes: (payload: { wasteCategoryIds: number[] }) => {
    return apiClient.put<{ message: string }>(`/enterprise/tasks/waste-types`, payload);
  },

  /**
   * Get progress/timeline of a specific task
   */
  getTaskProgress: (taskId: string) => {
    return apiClient.get<TaskProgressResponse>(`/enterprise/tasks/${taskId}/progress`);
  },
};
