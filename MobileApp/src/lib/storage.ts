import AsyncStorage from "@react-native-async-storage/async-storage";

export const STORAGE_KEYS = {
  session: "iw.mobile.session",
  apiBaseUrl: "iw.mobile.apiBaseUrl"
} as const;

export async function setStoredJson<T>(key: string, value: T): Promise<void> {
  await AsyncStorage.setItem(key, JSON.stringify(value));
}

export async function getStoredJson<T>(key: string): Promise<T | null> {
  const raw = await AsyncStorage.getItem(key);
  if (!raw) {
    return null;
  }

  try {
    return JSON.parse(raw) as T;
  } catch {
    return null;
  }
}

export async function clearSessionStorage(): Promise<void> {
  await AsyncStorage.removeItem(STORAGE_KEYS.session);
}
