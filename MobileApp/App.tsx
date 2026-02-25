import * as React from "react";
import { useEffect, useState } from "react";
import { Provider as PaperProvider } from "react-native-paper";
import { SafeAreaProvider } from "react-native-safe-area-context";

import { AppFrame } from "./src/components/AppFrame";
import { ClientShell } from "./src/components/ClientShell";
import { LoadingBlock } from "./src/components/ui";
import { DEFAULT_API_BASE_URL } from "./src/constants";
import type { Session } from "./src/appTypes";
import { LoginScreen } from "./src/screens/LoginScreen";
import { getStoredJson, setStoredJson, clearSessionStorage, STORAGE_KEYS } from "./src/lib/storage";
import { paperTheme } from "./src/theme/paperTheme";

export default function App() {
  const [booting, setBooting] = useState(true);
  const [session, setSession] = useState<Session | null>(null);
  const [apiBaseUrl, setApiBaseUrl] = useState(DEFAULT_API_BASE_URL);

  useEffect(() => {
    (async () => {
      const [savedSession, savedApiBaseUrl] = await Promise.all([
        getStoredJson<Session>(STORAGE_KEYS.session),
        getStoredJson<string>(STORAGE_KEYS.apiBaseUrl)
      ]);

      if (savedSession?.accessToken) {
        setSession(savedSession);
      }
      if (savedApiBaseUrl?.trim()) {
        setApiBaseUrl(savedApiBaseUrl);
      }

      setBooting(false);
    })();
  }, []);

  async function handleLogin(nextSession: Session, nextApiBaseUrl: string) {
    setSession(nextSession);
    setApiBaseUrl(nextApiBaseUrl);
    await Promise.all([
      setStoredJson(STORAGE_KEYS.session, nextSession),
      setStoredJson(STORAGE_KEYS.apiBaseUrl, nextApiBaseUrl)
    ]);
  }

  async function handleLogout() {
    setSession(null);
    await clearSessionStorage();
  }

  return (
    <SafeAreaProvider>
      <PaperProvider theme={paperTheme}>
        <AppFrame>
          {booting ? (
            <LoadingBlock label="Uruchamianie aplikacji..." />
          ) : session ? (
            <ClientShell session={session} apiBaseUrl={apiBaseUrl} onLogout={handleLogout} />
          ) : (
            <LoginScreen defaultApiBaseUrl={apiBaseUrl} onLogin={handleLogin} />
          )}
        </AppFrame>
      </PaperProvider>
    </SafeAreaProvider>
  );
}
