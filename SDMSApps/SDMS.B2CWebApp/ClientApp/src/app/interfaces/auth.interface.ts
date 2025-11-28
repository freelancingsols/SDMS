/**
 * Authentication-related interfaces and types
 */

export interface UserInfo {
  userId: string;
  email: string;
  displayName?: string;
  externalProvider?: string;
  profilePictureUrl?: string;
  lastLoginDate?: string;
  roles?: string[];
}

export interface LoginRequest {
  email: string;
  password: string;
  provider?: 'auth0' | 'google';
  idToken?: string;
  code?: string;
}

export interface RegisterRequest {
  email: string;
  password: string;
  displayName?: string;
}

export interface LoginResponse {
  userId: string;
  email: string;
  displayName?: string;
  externalProvider?: string;
  success: boolean;
  message: string;
}

export interface RegisterResponse {
  userId: string;
  email: string;
  displayName?: string;
  message: string;
}

export type AuthenticationProvider = 'auth0' | 'google';

