import axios, { AxiosError, type InternalAxiosRequestConfig } from "axios";
import type { ApiErrorPayload } from "@/shared/api/dtos";

declare module "axios" {
  export interface InternalAxiosRequestConfig {
    _retry?: boolean;
  }
}

const baseURL = import.meta.env.VITE_API_BASE_URL ?? "/api";

let accessToken: string | null = null;
let refreshHandler: (() => Promise<string | null>) | null = null;
let unauthorizedHandler: (() => void) | null = null;
let pendingRefresh: Promise<string | null> | null = null;

function isRefreshRequest(config?: InternalAxiosRequestConfig) {
  return config?.url?.includes("/auth/refresh") ?? false;
}

function applyAccessToken(config: InternalAxiosRequestConfig) {
  if (accessToken) {
    config.headers.set("Authorization", `Bearer ${accessToken}`);
  }

  return config;
}

export const authClient = axios.create({
  baseURL,
  withCredentials: true,
});

export const apiClient = axios.create({
  baseURL,
  withCredentials: true,
});

apiClient.interceptors.request.use(applyAccessToken);

apiClient.interceptors.response.use(
  (response) => response,
  async (error: AxiosError<ApiErrorPayload>) => {
    const responseStatus = error.response?.status;
    const originalRequest = error.config;

    if (
      responseStatus !== 401 ||
      !originalRequest ||
      originalRequest._retry ||
      isRefreshRequest(originalRequest)
    ) {
      return Promise.reject(error);
    }

    originalRequest._retry = true;

    try {
      if (!refreshHandler) {
        throw error;
      }

      pendingRefresh ??= refreshHandler().finally(() => {
        pendingRefresh = null;
      });
      const nextToken = await pendingRefresh;

      if (!nextToken) {
        throw error;
      }

      originalRequest.headers.set("Authorization", `Bearer ${nextToken}`);
      return apiClient(originalRequest);
    } catch {
      unauthorizedHandler?.();
      return Promise.reject(error);
    }
  },
);

export function setAccessToken(token: string | null) {
  accessToken = token;
}

export function registerRefreshHandler(handler: (() => Promise<string | null>) | null) {
  refreshHandler = handler;
}

export function registerUnauthorizedHandler(handler: (() => void) | null) {
  unauthorizedHandler = handler;
}

export function toApiError(error: unknown) {
  if (axios.isAxiosError<ApiErrorPayload>(error)) {
    const err = error.response?.data?.error;
    return {
      status: error.response?.status ?? null,
      code: err?.code ?? null,
      message: err?.message ?? "Yêu cầu thất bại.",
      details: err?.details ?? null,
    };
  }

  return {
    status: null,
    code: null,
    message: error instanceof Error ? error.message : "Yêu cầu thất bại.",
    details: null,
  };
}
