import { useAuthContext } from "@/shared/auth/AuthContext";

export function useAuth() {
  return useAuthContext();
}
