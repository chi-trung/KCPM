import { apiClient } from "./client";

export interface EnterpriseRewardRule {
  id: string;
  wasteCategoryId: number;
  categoryName: string;
  pointsPerReport: number;
  bonusQuality: number;
  isActive: boolean;
}

export interface UpdateEnterpriseRewardRuleItem {
  wasteCategoryId: number;
  pointsPerReport: number;
  bonusQuality: number;
  isActive: boolean;
}

export const enterpriseRewardApi = {
  getRewardRules: () => {
    return apiClient.get<EnterpriseRewardRule[]>("/enterprise/reward-rules");
  },

  updateRewardRules: (rules: UpdateEnterpriseRewardRuleItem[]) => {
    return apiClient.put<{ message: string; updatedCount: number; updatedAt: string }>(
      "/enterprise/reward-rules",
      { rules }
    );
  },
};
