import { Platform } from "react-native";

export const DEFAULT_API_BASE_URL =
  process.env.EXPO_PUBLIC_API_BASE_URL ?? (Platform.OS === "android" ? "http://10.0.2.2:5095" : "http://localhost:5095");
