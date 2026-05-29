export interface ApiEnvelope<T> {
  data: T;
  meta?: PagedMeta;
}

export interface PagedMeta {
  page: number;
  pageSize: number;
  total: number;
}

export interface ApiErrorDetail {
  field: string;
  message: string;
}

export interface ApiErrorPayload {
  error?: {
    code?: string;
    message?: string;
    details?: ApiErrorDetail[];
  };
}

export interface UserDto {
  id: string;
  username: string;
  fullName: string;
  email: string | null;
  departmentId: string | null;
  departmentCode: string | null;
  roles: string[];
}

export interface DepartmentLookupDto {
  id: string;
  code: string;
  name: string;
}

export interface AuthResponseDto {
  accessToken: string;
  accessExpiresAt: string;
  user: UserDto;
}

export interface LoginRequestDto {
  username: string;
  password: string;
}
