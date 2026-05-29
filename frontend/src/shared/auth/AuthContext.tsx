/* eslint-disable react-refresh/only-export-components */
import {
  createContext,
  useCallback,
  useContext,
  useEffect,
  useRef,
  useState,
  type PropsWithChildren,
} from "react";
import { authApi } from "@/features/auth/api";
import type { LoginFormValues } from "@/features/auth/schema";
import type { UserDto } from "@/shared/api/dtos";
import {
  registerRefreshHandler,
  registerUnauthorizedHandler,
  setAccessToken,
} from "@/shared/api/client";

interface AuthContextValue {
  user: UserDto | null;
  accessToken: string | null;
  isAuthenticated: boolean;
  isBootstrapping: boolean;
  login: (values: LoginFormValues) => Promise<void>;
  logout: () => Promise<void>;
  refreshSession: () => Promise<string | null>;
}

const AuthContext = createContext<AuthContextValue | null>(null);

function redirectToLogin() {
  if (window.location.pathname !== "/login") {
    window.location.assign("/login");
  }
}

export function AuthProvider({ children }: PropsWithChildren) {
  const [user, setUser] = useState<UserDto | null>(null);
  const [accessTokenValue, setAccessTokenValue] = useState<string | null>(null);
  const [isBootstrapping, setIsBootstrapping] = useState(true);
  const bootstrappedRef = useRef(false);

  const applySession = useCallback((nextUser: UserDto | null, token: string | null) => {
    setUser(nextUser);
    setAccessTokenValue(token);
    setAccessToken(token);
  }, []);

  const clearSession = useCallback(() => {
    applySession(null, null);
  }, [applySession]);

  const refreshSession = useCallback(async () => {
    try {
      const result = await authApi.refresh();
      applySession(result.user, result.accessToken);
      return result.accessToken;
    } catch {
      clearSession();
      return null;
    }
  }, [applySession, clearSession]);

  const login = useCallback(async (values: LoginFormValues) => {
    const result = await authApi.login(values);
    applySession(result.user, result.accessToken);
  }, [applySession]);

  const logout = useCallback(async () => {
    try {
      await authApi.logout();
    } finally {
      clearSession();
      redirectToLogin();
    }
  }, [clearSession]);

  useEffect(() => {
    registerRefreshHandler(refreshSession);
    registerUnauthorizedHandler(() => {
      clearSession();
      redirectToLogin();
    });

    return () => {
      registerRefreshHandler(null);
      registerUnauthorizedHandler(null);
      setAccessToken(null);
    };
  }, [clearSession, refreshSession]);

  useEffect(() => {
    if (bootstrappedRef.current) {
      return;
    }

    bootstrappedRef.current = true;
    void refreshSession().finally(() => {
      setIsBootstrapping(false);
    });
  }, [refreshSession]);

  return (
    <AuthContext.Provider
      value={{
        user,
        accessToken: accessTokenValue,
        isAuthenticated: Boolean(user && accessTokenValue),
        isBootstrapping,
        login,
        logout,
        refreshSession,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

export function useAuthContext() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error("useAuth must be used within AuthProvider.");
  }

  return context;
}
