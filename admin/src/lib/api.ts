const API_URL = process.env.NEXT_PUBLIC_API_URL || "http://localhost:5000";

class ApiClient {
  private token: string | null = null;

  constructor() {
    if (typeof window !== "undefined") {
      this.token = localStorage.getItem("token");
    }
  }

  setToken(token: string) {
    this.token = token;
    if (typeof window !== "undefined") {
      localStorage.setItem("token", token);
    }
  }

  clearToken() {
    this.token = null;
    if (typeof window !== "undefined") {
      localStorage.removeItem("token");
    }
  }

  isAuthenticated(): boolean {
    return !!this.token;
  }

  private async request<T>(path: string, options: RequestInit = {}): Promise<T> {
    const headers: HeadersInit = {
      "Content-Type": "application/json",
      ...(this.token ? { Authorization: `Bearer ${this.token}` } : {}),
    };

    const response = await fetch(`${API_URL}${path}`, {
      ...options,
      headers: { ...headers, ...options.headers },
    });

    if (response.status === 401) {
      this.clearToken();
      if (typeof window !== "undefined") {
        window.location.href = "/login";
      }
      throw new Error("Unauthorized");
    }

    if (!response.ok) {
      const error = await response.text();
      throw new Error(error || `HTTP ${response.status}`);
    }

    if (response.status === 204) return {} as T;
    return response.json();
  }

  // Auth
  async login(email: string, password: string): Promise<{ token: string }> {
    const result = await this.request<{ token: string }>("/api/auth/login", {
      method: "POST",
      body: JSON.stringify({ email, password }),
    });
    this.setToken(result.token);
    return result;
  }

  // Dashboard
  async getDashboardStats() {
    return this.request<import("@/types").DashboardStats>("/api/dashboard/stats");
  }

  // Users
  async getUsers(page = 1, pageSize = 20) {
    return this.request<import("@/types").PaginatedResponse<import("@/types").User>>(
      `/api/users?page=${page}&pageSize=${pageSize}`
    );
  }

  async getUser(id: string) {
    return this.request<import("@/types").User>(`/api/users/${id}`);
  }

  // Jobs
  async getJobs(page = 1, pageSize = 20) {
    return this.request<import("@/types").PaginatedResponse<import("@/types").Job>>(
      `/api/jobs?page=${page}&pageSize=${pageSize}`
    );
  }

  async moderateJob(id: string, data: { isActive: boolean; isSpam: boolean; isFeatured: boolean }) {
    return this.request(`/api/jobs/${id}/moderate`, {
      method: "PUT",
      body: JSON.stringify(data),
    });
  }

  async deleteJob(id: string) {
    return this.request(`/api/jobs/${id}`, { method: "DELETE" });
  }

  // Channels
  async getChannels() {
    return this.request<import("@/types").Channel[]>("/api/channels");
  }

  async addChannel(data: { telegramId: number; title: string; username?: string }) {
    return this.request("/api/channels", {
      method: "POST",
      body: JSON.stringify(data),
    });
  }

  async toggleChannel(id: string) {
    return this.request(`/api/channels/${id}/toggle`, { method: "PUT" });
  }

  async deleteChannel(id: string) {
    return this.request(`/api/channels/${id}`, { method: "DELETE" });
  }
}

export const api = new ApiClient();
