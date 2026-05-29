import { useMutation } from "@tanstack/react-query";
import { useAuth } from "@/shared/auth/useAuth";

export function useLogin() {
  const auth = useAuth();

  return useMutation({
    mutationFn: auth.login,
  });
}

export function useLogout() {
  const auth = useAuth();

  return useMutation({
    mutationFn: async () => {
      await auth.logout();
    },
  });
}

export function useRefreshSession() {
  const auth = useAuth();

  return useMutation({
    mutationFn: auth.refreshSession,
  });
}
