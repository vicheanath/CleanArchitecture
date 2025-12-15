interface JwtPayload {
  userId: string;
  email: string;
  name: string;
  role?: string | string[];
  permission?: string | string[];
  exp: number;
  iat: number;
  [key: string]: unknown;
}

export function parseJwt(token: string): JwtPayload | null {
  try {
    const base64Url = token.split(".")[1];
    if (!base64Url) {
      return null;
    }
    const base64 = base64Url.replace(/-/g, "+").replace(/_/g, "/");
    const jsonPayload = decodeURIComponent(
      atob(base64)
        .split("")
        .map((c) => "%" + ("00" + c.charCodeAt(0).toString(16)).slice(-2))
        .join("")
    );
    return JSON.parse(jsonPayload) as JwtPayload;
  } catch (error) {
    console.error("Error parsing JWT:", error);
    return null;
  }
}

export function isTokenExpired(token: string): boolean {
  const payload = parseJwt(token);
  if (!payload || !payload.exp) {
    return true;
  }
  const expirationDate = new Date(payload.exp * 1000);
  return expirationDate < new Date();
}

export function getTokenExpiration(token: string): Date | null {
  const payload = parseJwt(token);
  if (!payload || !payload.exp) {
    return null;
  }
  return new Date(payload.exp * 1000);
}

export function extractPermissions(token: string): string[] {
  const payload = parseJwt(token);
  if (!payload) {
    return [];
  }

  const permissions: string[] = [];

  // Extract permissions from claims
  if (payload.permission) {
    if (Array.isArray(payload.permission)) {
      permissions.push(...payload.permission);
    } else {
      permissions.push(payload.permission);
    }
  }

  // Also check for permissions in other claim formats
  Object.keys(payload).forEach((key) => {
    if (key === "permission" || key === "permissions") {
      const value = payload[key];
      if (Array.isArray(value)) {
        permissions.push(...value);
      } else if (typeof value === "string") {
        permissions.push(value);
      }
    }
  });

  return [...new Set(permissions)]; // Remove duplicates
}

export function extractRoles(token: string): string[] {
  const payload = parseJwt(token);
  if (!payload) {
    return [];
  }

  const roles: string[] = [];

  if (payload.role) {
    if (Array.isArray(payload.role)) {
      roles.push(...payload.role);
    } else {
      roles.push(payload.role);
    }
  }

  // Also check for roles in other claim formats
  Object.keys(payload).forEach((key) => {
    if (
      key === "role" ||
      key === "roles" ||
      key === "http://schemas.microsoft.com/ws/2008/06/identity/claims/role"
    ) {
      const value = payload[key];
      if (Array.isArray(value)) {
        roles.push(...value);
      } else if (typeof value === "string") {
        roles.push(value);
      }
    }
  });

  return [...new Set(roles)]; // Remove duplicates
}
