import Constants from "expo-constants";
import { Platform } from "react-native";

const API_PORT = "5095";

function extractHost(value: string | undefined): string | null {
  if (!value) {
    return null;
  }

  const host = value.split(":")[0]?.trim();
  return host ? host : null;
}

function resolveExpoHost(): string | null {
  const candidates = [
    extractHost((Constants.expoConfig as { hostUri?: string } | null)?.hostUri),
    extractHost(
      (
        Constants as {
          manifest2?: { extra?: { expoGo?: { debuggerHost?: string } } };
        }
      ).manifest2?.extra?.expoGo?.debuggerHost,
    ),
    extractHost(
      (Constants as { manifest?: { debuggerHost?: string } }).manifest
        ?.debuggerHost,
    ),
  ];

  return candidates.find((host) => Boolean(host)) ?? null;
}

function resolveDefaultApiBaseUrl(): string {
  const envValue = process.env.EXPO_PUBLIC_API_BASE_URL?.trim();
  if (envValue && envValue.toLowerCase() !== "auto") {
    return envValue;
  }

  const expoHost = resolveExpoHost();
  if (expoHost && expoHost !== "localhost" && expoHost !== "127.0.0.1") {
    return `http://${expoHost}:${API_PORT}`;
  }

  if (Platform.OS === "android") {
    return `http://10.0.2.2:${API_PORT}`;
  }

  return `http://localhost:${API_PORT}`;
}

export const DEFAULT_API_BASE_URL = resolveDefaultApiBaseUrl();
