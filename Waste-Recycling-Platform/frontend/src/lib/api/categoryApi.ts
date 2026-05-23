import { apiClient } from "./client";

export interface WasteCategory {
  id: number;
  name: string;
  description: string;
}

export interface GetCategoriesResponse {
  message: string;
  data: WasteCategory[];
}

export const categoryApi = {
  /**
   * Fetches all available waste categories from the system.
   */
  getAllCategories: () => {
    return apiClient.get<GetCategoriesResponse>("/waste-categories");
  },
};
