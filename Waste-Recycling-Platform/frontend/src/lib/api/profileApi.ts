import { apiClient } from "./client";

// ── Types ────────────────────────────────────────────────────────────────────

export interface ProfileDto {
  id: string;
  email: string;
  fullName: string;
  phone?: string;
  district?: string;
  ward?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt?: string;
}

export interface UpdateProfileDto {
  fullName: string;
  phone?: string;
  district?: string;
  ward?: string;
}

export interface ApiResponse<T> {
  message: string;
  data: T;
}

// ── Profile API calls ────────────────────────────────────────────────────────────

export const profileApi = {
  getProfile: () =>
    apiClient.get<ApiResponse<ProfileDto>>("/citizens/profile"),

  updateProfile: (data: UpdateProfileDto) =>
    apiClient.put<ApiResponse<ProfileDto>>("/citizens/profile", data),
};
