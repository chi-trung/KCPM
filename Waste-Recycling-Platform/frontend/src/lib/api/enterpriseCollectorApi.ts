import { apiClient } from "./client";

export interface EnterpriseCollectorDto {
  id: string;
  name: string;
  email: string;
  phone: string | null;
  isAvailable: boolean;
  createdAt: string;
  taskCount: number;
}

export interface CreateEnterpriseCollectorPayload {
  fullName: string;
  email: string;
  phone?: string;
  temporaryPassword: string;
  isAvailable: boolean;
}

export interface UpdateEnterpriseCollectorPayload {
  fullName: string;
  email: string;
  phone?: string;
  temporaryPassword?: string;
  isAvailable: boolean;
}

export const enterpriseCollectorApi = {
  getCollectors: () => {
    return apiClient.get<EnterpriseCollectorDto[]>("/enterprise/collectors");
  },

  createCollector: (payload: CreateEnterpriseCollectorPayload) => {
    return apiClient.post<{ message: string; collector: EnterpriseCollectorDto }>(
      "/enterprise/collectors",
      payload
    );
  },

  updateCollector: (collectorId: string, payload: UpdateEnterpriseCollectorPayload) => {
    return apiClient.put<{ message: string; collector: EnterpriseCollectorDto }>(
      `/enterprise/collectors/${collectorId}`,
      payload
    );
  },

  deleteCollector: (collectorId: string) => {
    return apiClient.delete<{ message: string }>(`/enterprise/collectors/${collectorId}`);
  },
};
