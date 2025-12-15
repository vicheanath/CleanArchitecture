import { useState, useEffect, useCallback, type ReactNode } from "react";
import { useNavigate } from "react-router-dom";
import { AuthContext, type AuthContextType } from "./AuthContext";
import { authApi } from "../api/authApi";
import { ApiClient } from "@/shared/lib/apiClient";
import {
  parseJwt,
  isTokenExpired,
  extractPermissions,
  extractRoles,
} from "@/shared/lib/jwt";
import type { User, AuthResponse } from "../types/types";

interface AuthProviderProps {
  children: ReactNode;
}

export function AuthProvider({ children }: AuthProviderProps) {
  const [user, setUser] = useState<User | null>(null);
  const [token, setToken] = useState<string | null>(null);
  const [permissions, setPermissions] = useState<string[]>([]);
  const [roles, setRoles] = useState<string[]>([]);
  const [isLoading, setIsLoading] = useState(true);
  const navigate = useNavigate();

  // Initialize auth state from localStorage
  useEffect(() => {
    const storedToken = ApiClient.getToken();
    if (storedToken && !isTokenExpired(storedToken)) {
      const payload = parseJwt(storedToken);
      if (payload) {
        const extractedPermissions = extractPermissions(storedToken);
        const extractedRoles = extractRoles(storedToken);

        setToken(storedToken);
        setPermissions(extractedPermissions);
        setRoles(extractedRoles);
        setUser({
          userId:
            payload.userId ||
            payload[
              "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"
            ] ||
            "",
          email:
            payload.email ||
            payload[
              "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress"
            ] ||
            "",
          firstName: payload.firstName || "",
          lastName: payload.lastName || "",
          roles: extractedRoles,
          permissions: extractedPermissions,
        });
      }
    } else if (storedToken && isTokenExpired(storedToken)) {
      // Clear expired token
      ApiClient.setToken(null);
    }
    setIsLoading(false);
  }, []);

  // Listen for unauthorized events
  useEffect(() => {
    const handleUnauthorized = () => {
      setUser(null);
      setToken(null);
      setPermissions([]);
      setRoles([]);
      navigate("/login");
    };

    window.addEventListener("auth:unauthorized", handleUnauthorized);
    return () => {
      window.removeEventListener("auth:unauthorized", handleUnauthorized);
    };
  }, [navigate]);

  const updateAuthState = useCallback((authResponse: AuthResponse) => {
    const { token: newToken, ...userData } = authResponse;

    // Extract permissions and roles from token
    const extractedPermissions = extractPermissions(newToken);
    const extractedRoles = extractRoles(newToken);

    // Store token
    ApiClient.setToken(newToken);
    setToken(newToken);
    setPermissions(extractedPermissions);
    setRoles(extractedRoles);
    setUser({
      userId: userData.userId,
      email: userData.email,
      firstName: userData.firstName,
      lastName: userData.lastName,
      roles: extractedRoles.length > 0 ? extractedRoles : userData.roles,
      permissions:
        extractedPermissions.length > 0
          ? extractedPermissions
          : userData.permissions,
    });
  }, []);

  const login = useCallback(
    async (email: string, password: string) => {
      try {
        const response = await authApi.login({ email, password });
        updateAuthState(response);
        navigate("/");
      } catch (error) {
        throw error;
      }
    },
    [navigate, updateAuthState]
  );

  const register = useCallback(
    async (
      email: string,
      password: string,
      firstName: string,
      lastName: string
    ) => {
      try {
        const response = await authApi.register({
          email,
          password,
          firstName,
          lastName,
        });
        updateAuthState(response);
        navigate("/");
      } catch (error) {
        throw error;
      }
    },
    [navigate, updateAuthState]
  );

  const logout = useCallback(() => {
    ApiClient.setToken(null);
    setUser(null);
    setToken(null);
    setPermissions([]);
    setRoles([]);
    navigate("/login");
  }, [navigate]);

  const hasPermission = useCallback(
    (permission: string): boolean => {
      return permissions.includes(permission);
    },
    [permissions]
  );

  const hasRole = useCallback(
    (role: string): boolean => {
      return roles.includes(role);
    },
    [roles]
  );

  const value: AuthContextType = {
    user,
    token,
    permissions,
    roles,
    isAuthenticated: !!user && !!token,
    isLoading,
    login,
    register,
    logout,
    hasPermission,
    hasRole,
  };

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}
