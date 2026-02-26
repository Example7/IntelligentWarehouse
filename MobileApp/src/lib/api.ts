import type {
  ClientAttachmentDto,
  ChangePasswordRequestDto,
  ChangePasswordResponseDto,
  ClientDashboardDto,
  ClientCreateReservationRequestDto,
  ClientCreateReservationResponseDto,
  ClientNotificationDto,
  ClientOrderDetailsDto,
  ClientOrderListItemDto,
  ClientProductLookupDto,
  ClientProfileDto,
  ClientReservationDetailsDto,
  ClientReservationListItemDto,
  ClientWarehouseLookupDto,
  CurrentUserDto,
  LoginResponseDto,
  MobileNewsItemDto,
  MobilePageDetailsDto,
  MobilePageListItemDto,
} from "../types";

export class ApiError extends Error {
  status: number;
  payload?: unknown;

  constructor(message: string, status: number, payload?: unknown) {
    super(message);
    this.name = "ApiError";
    this.status = status;
    this.payload = payload;
  }
}

type RequestOptions = {
  method?: "GET" | "POST";
  token?: string;
  body?: unknown;
};

function normalizeBaseUrl(baseUrl: string): string {
  return baseUrl.trim().replace(/\/+$/, "");
}

async function request<T>(
  baseUrl: string,
  path: string,
  options: RequestOptions = {},
): Promise<T> {
  const headers: Record<string, string> = { Accept: "application/json" };

  if (options.body !== undefined) {
    headers["Content-Type"] = "application/json";
  }

  if (options.token) {
    headers.Authorization = `Bearer ${options.token}`;
  }

  const response = await fetch(`${normalizeBaseUrl(baseUrl)}${path}`, {
    method: options.method ?? "GET",
    headers,
    body: options.body !== undefined ? JSON.stringify(options.body) : undefined,
  });

  const contentType = response.headers.get("content-type") ?? "";
  let payload: unknown = null;

  if (contentType.includes("application/json")) {
    payload = await response.json();
  } else {
    payload = await response.text();
  }

  if (!response.ok) {
    const message =
      typeof payload === "string"
        ? payload
        : typeof payload === "object" &&
            payload &&
            "message" in (payload as Record<string, unknown>)
          ? String((payload as Record<string, unknown>).message)
          : `Błąd API (${response.status})`;
    throw new ApiError(message, response.status, payload);
  }

  return payload as T;
}

export const mobileApi = {
  login(baseUrl: string, loginOrEmail: string, password: string) {
    return request<LoginResponseDto>(baseUrl, "/api/auth/login", {
      method: "POST",
      body: { loginOrEmail, password },
    });
  },
  me(baseUrl: string, token: string) {
    return request<CurrentUserDto>(baseUrl, "/api/auth/me", { token });
  },
  changePassword(
    baseUrl: string,
    token: string,
    body: ChangePasswordRequestDto,
  ) {
    return request<ChangePasswordResponseDto>(baseUrl, "/api/auth/change-password", {
      method: "POST",
      token,
      body,
    });
  },
  getDashboard(baseUrl: string, token: string) {
    return request<ClientDashboardDto>(baseUrl, "/api/client/dashboard", {
      token,
    });
  },
  getOrders(baseUrl: string, token: string) {
    return request<ClientOrderListItemDto[]>(baseUrl, "/api/client/orders", {
      token,
    });
  },
  getOrderDetails(baseUrl: string, token: string, orderId: number) {
    return request<ClientOrderDetailsDto>(
      baseUrl,
      `/api/client/orders/${orderId}`,
      { token },
    );
  },
  getOrderAttachments(baseUrl: string, token: string, orderId: number) {
    return request<ClientAttachmentDto[]>(
      baseUrl,
      `/api/client/orders/${orderId}/attachments`,
      { token },
    );
  },
  getReservations(baseUrl: string, token: string) {
    return request<ClientReservationListItemDto[]>(
      baseUrl,
      "/api/client/reservations",
      { token },
    );
  },
  getReservationDetails(baseUrl: string, token: string, reservationId: number) {
    return request<ClientReservationDetailsDto>(
      baseUrl,
      `/api/client/reservations/${reservationId}`,
      { token },
    );
  },
  createReservation(
    baseUrl: string,
    token: string,
    body: ClientCreateReservationRequestDto,
  ) {
    return request<ClientCreateReservationResponseDto>(
      baseUrl,
      "/api/client/reservations",
      { method: "POST", token, body },
    );
  },
  getReservationWarehouses(baseUrl: string, token: string) {
    return request<ClientWarehouseLookupDto[]>(
      baseUrl,
      "/api/client/lookups/warehouses",
      { token },
    );
  },
  searchReservableProducts(
    baseUrl: string,
    token: string,
    q: string,
    take = 20,
    warehouseId?: number | null,
  ) {
    const warehousePart = warehouseId ? `&warehouseId=${warehouseId}` : "";
    return request<ClientProductLookupDto[]>(
      baseUrl,
      `/api/client/lookups/products?q=${encodeURIComponent(q)}&take=${take}${warehousePart}`,
      { token },
    );
  },
  getNotifications(baseUrl: string, token: string, take = 20) {
    return request<ClientNotificationDto[]>(
      baseUrl,
      `/api/client/notifications?take=${take}`,
      { token },
    );
  },
  getProfile(baseUrl: string, token: string) {
    return request<ClientProfileDto>(baseUrl, "/api/client/profile", { token });
  },
  getNews(baseUrl: string) {
    return request<MobileNewsItemDto[]>(baseUrl, "/api/mobile/content/news");
  },
  getPages(baseUrl: string) {
    return request<MobilePageListItemDto[]>(
      baseUrl,
      "/api/mobile/content/pages",
    );
  },
  getPage(baseUrl: string, slug: string) {
    return request<MobilePageDetailsDto>(
      baseUrl,
      `/api/mobile/content/pages/${encodeURIComponent(slug)}`,
    );
  },
};
