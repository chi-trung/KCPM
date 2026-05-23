import { apiClient } from './client';

export interface CitizenRewardData {
  totalPoints: number;
  level: string;
  badge: string;
  nextLevelPoints: number;
}

export interface RewardHistoryItem {
  id: string;
  points: number;
  type: string;
  reason: string;
  referenceId?: string;
  referenceType?: string;
  createdAt: string;
}

export interface RewardHistoryResponse {
  items: RewardHistoryItem[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export const citizenRewardApi = {
  // Get total points and level info
  getRewards: async (): Promise<CitizenRewardData> => {
    const response = await apiClient.get<{ data: CitizenRewardData }>('/citizens/rewards');
    return response.data;
  },

  // Get reward points history
  getRewardHistory: async (page = 1, pageSize = 10): Promise<RewardHistoryResponse> => {
    const response = await apiClient.get<{ data: RewardHistoryResponse }>(
      `/citizens/rewards/history?page=${page}&pageSize=${pageSize}`
    );
    return response.data;
  },
};
