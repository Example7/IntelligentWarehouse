import type { LoginResponseDto } from "./types";

export type Session = LoginResponseDto;

export type ClientTabKey =
  | "start"
  | "produkty"
  | "wz"
  | "rezerwacje"
  | "alerty"
  | "profil"
  | "cms";

export type ClientReservationCartItem = {
  productId: number;
  code: string;
  name: string;
  defaultUom?: string | null;
  quantity: number;
};
