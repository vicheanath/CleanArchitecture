import { ApiClient } from "@/shared/lib/apiClient";
import type {
  AuthResponse,
  LoginRequest,
  RegisterRequest,
} from "../types/types";

class AuthApi extends ApiClient {
  async login(credentials: LoginRequest): Promise<AuthResponse> {
    const response = await this.request<{ value: AuthResponse }>(
      "/auth/login",
      {
        method: "POST",
        body: JSON.stringify(credentials),
      }
    );

    // Handle Result<T> wrapper
    if (response && "value" in response) {
      return response.value;
    }
    return response as AuthResponse;
  }

  async register(data: RegisterRequest): Promise<AuthResponse> {
    const response = await this.request<{ value: AuthResponse }>(
      "/auth/register",
      {
        method: "POST",
        body: JSON.stringify(data),
      }
    );

    // Handle Result<T> wrapper
    if (response && "value" in response) {
      return response.value;
    }
    return response as AuthResponse;
  }
}

export const authApi = new AuthApi();
