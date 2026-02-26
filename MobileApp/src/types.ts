export type NullableDateString = string | null;

export interface LoginResponseDto {
  accessToken: string;
  expiresAtUtc: string;
  userId: number;
  login: string;
  email: string;
  roles: string[];
}

export interface CurrentUserDto {
  userId: number;
  login: string;
  email?: string | null;
  roles: string[];
}

export interface ChangePasswordRequestDto {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface ChangePasswordResponseDto {
  message: string;
}

export interface ClientOrderListItemDto {
  orderId: number;
  number: string;
  status: string;
  issuedAtUtc: string;
  postedAtUtc: NullableDateString;
  warehouseName: string;
  itemsCount: number;
  totalQuantity: number;
}

export interface ClientReservationListItemDto {
  reservationId: number;
  number: string;
  status: string;
  createdAtUtc: string;
  expiresAtUtc: NullableDateString;
  warehouseName: string;
  itemsCount: number;
  totalQuantity: number;
}

export interface ClientDashboardDto {
  activeOrdersCount: number;
  postedOrdersCount: number;
  openReservationsCount: number;
  recentOrders: ClientOrderListItemDto[];
  recentReservations: ClientReservationListItemDto[];
}

export interface ClientOrderItemDto {
  itemId: number;
  lineNo: number;
  productId: number;
  productCode: string;
  productName: string;
  quantity: number;
  locationId?: number | null;
  locationCode?: string | null;
}

export interface ClientOrderDetailsDto {
  orderId: number;
  number: string;
  status: string;
  issuedAtUtc: string;
  postedAtUtc: NullableDateString;
  warehouseName: string;
  note?: string | null;
  totalQuantity: number;
  items: ClientOrderItemDto[];
}

export interface ClientReservationItemDto {
  itemId: number;
  lineNo: number;
  productId: number;
  productCode: string;
  productName: string;
  quantity: number;
  locationId?: number | null;
  locationCode?: string | null;
}

export interface ClientReservationDetailsDto {
  reservationId: number;
  number: string;
  status: string;
  createdAtUtc: string;
  expiresAtUtc: NullableDateString;
  warehouseName: string;
  note?: string | null;
  totalQuantity: number;
  items: ClientReservationItemDto[];
}

export interface ClientWarehouseLookupDto {
  warehouseId: number;
  name: string;
}

export interface ClientProductLookupDto {
  productId: number;
  code: string;
  name: string;
  defaultUom?: string | null;
  availableQuantity?: number | null;
}

export interface ClientCreateReservationItemRequestDto {
  productId: number;
  locationId?: number | null;
  quantity: number;
}

export interface ClientCreateReservationRequestDto {
  warehouseId: number;
  expiresAtUtc?: string | null;
  note?: string | null;
  items: ClientCreateReservationItemRequestDto[];
}

export interface ClientCreateReservationResponseDto {
  reservationId: number;
  number: string;
  status: string;
  createdAtUtc: string;
  expiresAtUtc?: string | null;
  autoActivationAttempted?: boolean;
  autoActivationSucceeded?: boolean;
  autoActivationMessage?: string | null;
}

export interface ClientNotificationDto {
  notificationId: number;
  severity: string;
  message: string;
  title: string;
  status?: string | null;
  documentType?: string | null;
  documentNumber?: string | null;
  createdAtUtc: string;
  isAcknowledged: boolean;
  productCode?: string | null;
  productName?: string | null;
  warehouseName: string;
}

export interface ClientProfileDto {
  customerId: number;
  name: string;
  email?: string | null;
  phone?: string | null;
  address?: string | null;
  isActive: boolean;
  createdAtUtc: string;
}

export interface ClientAttachmentDto {
  attachmentId: number;
  documentType: string;
  documentId: number;
  fileName: string;
  contentType: string;
  filePath: string;
  uploadedAtUtc: string;
}

export interface MobileNewsItemDto {
  id: number;
  linkTitle: string;
  title: string;
  content: string;
  position: number;
}

export interface MobilePageListItemDto {
  id: number;
  slug: string;
  title: string;
  content: string;
  position: number;
}

export interface MobilePageDetailsDto {
  id: number;
  slug: string;
  title: string;
  content: string;
  position: number;
}
