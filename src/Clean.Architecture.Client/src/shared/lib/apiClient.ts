const API_BASE_URL = "http://localhost:5105/api";
const TOKEN_STORAGE_KEY = "auth_token";

export class ApiClient {
  private baseUrl: string;

  constructor(baseUrl: string = API_BASE_URL) {
    this.baseUrl = baseUrl;
  }

  private getToken(): string | null {
    if (typeof window === "undefined") {
      return null;
    }
    return localStorage.getItem(TOKEN_STORAGE_KEY);
  }

  private setToken(token: string | null): void {
    if (typeof window === "undefined") {
      return;
    }
    if (token) {
      localStorage.setItem(TOKEN_STORAGE_KEY, token);
    } else {
      localStorage.removeItem(TOKEN_STORAGE_KEY);
    }
  }

  private handleUnauthorized(): void {
    this.setToken(null);
    // Dispatch custom event for auth context to handle
    window.dispatchEvent(new CustomEvent("auth:unauthorized"));
  }

  protected async request<T>(
    endpoint: string,
    options?: RequestInit
  ): Promise<T> {
    const token = this.getToken();
    const headers: HeadersInit = {
      "Content-Type": "application/json",
      ...options?.headers,
    };

    if (token) {
      headers["Authorization"] = `Bearer ${token}`;
    }

    try {
      const response = await fetch(`${this.baseUrl}${endpoint}`, {
        ...options,
        headers,
      });

      // Handle 401 Unauthorized
      if (response.status === 401) {
        this.handleUnauthorized();
        throw new Error("Unauthorized: Please log in again");
      }

      if (!response.ok) {
        if (response.status === 404) {
          throw new Error(`Resource not found: ${endpoint}`);
        }
        if (response.status === 400) {
          const errorData = await response.json().catch(() => ({}));
          throw new Error(
            errorData.message ||
              errorData.detail ||
              `Bad request: ${response.statusText}`
          );
        }
        if (response.status >= 500) {
          throw new Error(
            `Server error: ${response.status} ${response.statusText}`
          );
        }
        throw new Error(`API Error: ${response.status} ${response.statusText}`);
      }

      const contentType = response.headers.get("content-type");
      if (contentType && contentType.includes("application/json")) {
        const data = await response.json();
        // Handle Result<T> wrapper from backend
        if (data && typeof data === "object" && "isSuccess" in data) {
          if (data.isSuccess) {
            return data.value ?? data;
          } else {
            throw new Error(data.errors?.[0]?.message || "Request failed");
          }
        }
        return data;
      } else {
        return {} as T;
      }
    } catch (error) {
      if (error instanceof TypeError && error.message.includes("fetch")) {
        throw new Error(
          "Network error: Unable to connect to the server. Please check if the API is running."
        );
      }
      throw error;
    }
  }

  // Expose token management for auth context
  static getToken(): string | null {
    if (typeof window === "undefined") {
      return null;
    }
    return localStorage.getItem(TOKEN_STORAGE_KEY);
  }

  static setToken(token: string | null): void {
    if (typeof window === "undefined") {
      return;
    }
    if (token) {
      localStorage.setItem(TOKEN_STORAGE_KEY, token);
    } else {
      localStorage.removeItem(TOKEN_STORAGE_KEY);
    }
  }
}
