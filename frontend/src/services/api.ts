// Services - Serviços para chamadas à API e lógica de negócios
// services/api.ts
import axios, { AxiosInstance, AxiosRequestConfig } from 'axios';

export const API_BASE_URL = 'http://localhost:5000/api';

class ApiService {
  private api: AxiosInstance;

  constructor() {
    this.api = axios.create({
      baseURL: API_BASE_URL,
      timeout: 10000,
      headers: {
        'Content-Type': 'application/json',
      }
    });

    this.setupInterceptors();
  }

  private setupInterceptors() {
    // Request interceptor - Add token
    this.api.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('accessToken');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => Promise.reject(error)
    );

    // Response interceptor - Handle token refresh
    this.api.interceptors.response.use(
      (response) => response,
      async (error) => {
        if (error.response?.status === 401) {
          const refreshToken = localStorage.getItem('refreshToken');
          if (refreshToken) {
            try {
              const response = await this.refreshToken(refreshToken);
              localStorage.setItem('accessToken', response.data.access_token);
              localStorage.setItem('refreshToken', response.data.refresh_token);
              
              // Retry original request
              error.config.headers.Authorization = `Bearer ${response.data.access_token}`;
              return this.api.request(error.config);
            } catch (refreshError) {
              this.logout();
              window.location.href = '/login';
            }
          }
        }
        return Promise.reject(error);
      }
    );
  }

  private async refreshToken(refreshToken: string) {
    return axios.post(`${API_BASE_URL}/Auth/refresh-token`, {
      refreshToken: refreshToken
    });
  }

  private logout() {
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  }

  getInstance() {
    return this.api;
  }
}

export const apiService = new ApiService();
export const api = apiService.getInstance();
