import { authClient } from "@/shared/api/client";
import type { ApiEnvelope, AuthResponseDto, LoginRequestDto } from "@/shared/api/dtos";

async function unwrapAuthRequest(promise: Promise<{ data: ApiEnvelope<AuthResponseDto> }>) {
  const response = await promise;
  return response.data.data;
}

export const authApi = {
  login(payload: LoginRequestDto) {
    return unwrapAuthRequest(
      authClient.post<ApiEnvelope<AuthResponseDto>>("/auth/login", payload),
    );
  },
  refresh() {
    return unwrapAuthRequest(
      authClient.post<ApiEnvelope<AuthResponseDto>>("/auth/refresh", {}),
    );
  },
  async logout() {
    await authClient.post("/auth/logout", {});
  },
};
