"use client";

import React, {
  createContext,
  useContext,
  useState,
  useEffect,
  useCallback,
} from "react";
import { useRouter } from "next/navigation";
import { authApi, type AuthUser, type UserRole } from "@/lib/api/auth";
import { ApiError } from "@/lib/api/client";

// ── Types ─────────────────────────────────────────────────────────────────────

interface AuthContextValue {
  user: AuthUser | null;
  token: string | null;
  isLoading: boolean;
  isAuthenticated: boolean;
  login: (email: string, password: string) => Promise<void>;
  register: (
    fullName: string,
    email: string,
    password: string,
    role: UserRole,
  ) => Promise<void>;
  logout: () => void;
}

// ── Context ───────────────────────────────────────────────────────────────────

const AuthContext = createContext<AuthContextValue | null>(null);

// ── Role → dashboard path mapping ─────────────────────────────────────────────

const ROLE_DASHBOARD: Record<UserRole, string> = {
  citizen: "/citizen",
  collector: "/collector",
  enterprise: "/enterprise",
  admin: "/admin",
};

// ── Provider ──────────────────────────────────────────────────────────────────

export function AuthProvider({ children }: { children: React.ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const router = useRouter();

  // Restore session from localStorage on mount
  useEffect(() => {
    const stored = localStorage.getItem("token");
    if (!stored) {
      setIsLoading(false);
      return;
    }

    setToken(stored);
    authApi
      .me()
      .then((u) => setUser(u))
      .catch(() => {
        localStorage.removeItem("token");
        setToken(null);
      })
      .finally(() => setIsLoading(false));
  }, []);

  const persist = useCallback((t: string, u: AuthUser) => {
    localStorage.setItem("token", t);
    setToken(t);
    setUser(u);
  }, []);

  const login = useCallback(
    async (email: string, password: string) => {
      const res = await authApi.login({ email, password });
      persist(res.token, res.user);
      const roleStr = res.user.role.toLowerCase() as UserRole;
      router.push(ROLE_DASHBOARD[roleStr] ?? "/");
    },
    [persist, router],
  );

  const register = useCallback(
    async (
      fullName: string,
      email: string,
      password: string,
      role: UserRole,
    ) => {
      await authApi.register({ fullName, email, password, role });
      // Redirect to login after successful registration
      // User should login with their new credentials
      setTimeout(() => router.push("/login"), 1500);
    },
    [router],
  );

  const logout = useCallback(() => {
    localStorage.removeItem("token");
    setToken(null);
    setUser(null);
    router.push("/login");
  }, [router]);

  return (
    <AuthContext.Provider
      value={{
        user,
        token,
        isLoading,
        isAuthenticated: !!user,
        login,
        register,
        logout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}

// ── Hook ──────────────────────────────────────────────────────────────────────

export function useAuth(): AuthContextValue {
  const ctx = useContext(AuthContext);
  if (!ctx) throw new Error("useAuth must be used inside <AuthProvider>");
  return ctx;
}

// Re-export ApiError so pages can do: import { ApiError } from "@/contexts/AuthContext"
export { ApiError };
