import { Navigate } from "react-router-dom";
import { useAuth } from "@/features/auth";
import type { ReactNode } from "react";

interface RequirePermissionProps {
  children: ReactNode;
  permission: string;
  fallback?: ReactNode;
}

export function RequirePermission({
  children,
  permission,
  fallback,
}: RequirePermissionProps) {
  const { hasPermission, isAuthenticated, isLoading } = useAuth();

  if (isLoading) {
    return (
      <div className="flex items-center justify-center p-8">
        <div className="text-center">
          <div className="inline-block h-6 w-6 animate-spin rounded-full border-4 border-solid border-current border-r-transparent align-[-0.125em] motion-reduce:animate-[spin_1.5s_linear_infinite]"></div>
        </div>
      </div>
    );
  }

  if (!isAuthenticated) {
    return <Navigate to="/login" replace />;
  }

  if (!hasPermission(permission)) {
    if (fallback) {
      return <>{fallback}</>;
    }
    return (
      <div className="rounded-md bg-yellow-50 p-4 text-sm text-yellow-800">
        You don't have permission to access this resource.
      </div>
    );
  }

  return <>{children}</>;
}
