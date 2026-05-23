import { ApiError, apiClient } from "./client";
import { API_CONFIG } from "./config";

export const reportApi = {
  /**
   * Creates a new waste report for a citizen.
   * @param formData The form data containing Latitude, Longitude, Description, Address, WasteCategoryId, and Images.
   */
  createWasteReport: async (formData: FormData) => {
    const token =
      typeof window !== "undefined" ? localStorage.getItem("token") : null;

    const headers: HeadersInit = {
      ...(token ? { Authorization: `Bearer ${token}` } : {}),
    };

    const res = await fetch(`${API_CONFIG.BASE_URL}/reports/create`, {
      method: "POST",
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

    return data;
  },

  /**
   * Fetches the waste reports created by the currently logged-in citizen.
   */
  getMyReports: (page = 1, pageSize = 10) => {
    return apiClient.get<{
      message: string;
      pagination: { page: number; pageSize: number; total: number; totalPages: number };
      reports: Array<{
        id: string;
        citizenName: string;
        categoryName: string;
        status: "Pending" | "Accepted" | "Assigned" | "Collected";
        address: string;
        createdAt: string;
        imageCount: number;
      }>;
    }>(`/reports/my-reports?page=${page}&pageSize=${pageSize}`);
  },

  /**
   * Fetches the detailed information of a specific waste report by ID.
   * @param id The unique identifier of the report.
   */
  getReportById: (id: string) => {
    return apiClient.get<{
      message: string;
      report: any;
    }>(`/reports/${id}`);
  },

  /**
   * Accept a waste report and create a collection task.
   * @param reportId The ID of the report to accept.
   */
  acceptReport: (reportId: string) => {
    return apiClient.post<{
      message: string;
      reportId: string;
      collectionTaskId: string;
      reportStatus: string;
    }>(`/reports/${reportId}/accept`, {});
  },

  /**
   * Reject a waste report with optional reason.
   * @param reportId The ID of the report to reject.
   * @param reason Optional reason for rejection.
   */
  rejectReport: (reportId: string, reason?: string) => {
    return apiClient.post<{
      message: string;
      reportId: string;
      reportStatus: string;
      rejectionReason?: string;
    }>(`/reports/${reportId}/reject`, { reason });
  },

  /**
   * Fetches available reports for the current enterprise with optional filtering.
   * @param page Page number for pagination
   * @param pageSize Number of items per page
   * @param status Optional status filter ("Pending", "Accepted", etc.)
   */
  getEnterpriseAvailableReports: (page = 1, pageSize = 10, status?: string) => {
    const params = new URLSearchParams();
    params.append("page", page.toString());
    params.append("pageSize", pageSize.toString());
    if (status) params.append("status", status);
    const query = `?${params.toString()}`;
    return apiClient.get<{
      message: string;
      pagination: { page: number; pageSize: number; total: number; totalPages: number };
      reports: Array<{
        id: string;
        citizenName: string;
        categoryName: string;
        status: string;
        address: string;
        createdAt: string;
        imageCount: number;
      }>;
    }>(`/reports/enterprise/available${query}`);
  },
};
