import * as React from "react";
import { useState } from "react";
import { KeyboardAvoidingView, Platform, ScrollView, StyleSheet, Text, View } from "react-native";
import { TextInput as PaperTextInput } from "react-native-paper";

import { ActionButton, Card, ErrorBlock, SectionTitle, colors } from "../components/ui";
import { mobileApi } from "../lib/api";
import type { Session } from "../appTypes";

export function LoginScreen({
  defaultApiBaseUrl,
  onLogin
}: {
  defaultApiBaseUrl: string;
  onLogin: (session: Session, apiBaseUrl: string) => Promise<void>;
}) {
  const [apiBaseUrl] = useState(defaultApiBaseUrl);
  const [loginOrEmail, setLoginOrEmail] = useState("");
  const [password, setPassword] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);

  async function submit() {
    setBusy(true);
    setError(null);
    try {
      const response = await mobileApi.login(apiBaseUrl.trim(), loginOrEmail.trim(), password);
      await onLogin(response, apiBaseUrl.trim());
    } catch (e) {
      setError(e instanceof Error ? e.message : "Nie udało się zalogować.");
    } finally {
      setBusy(false);
    }
  }

  const canSubmit = loginOrEmail.trim().length > 0 && password.length > 0 && !busy;

  return (
    <KeyboardAvoidingView style={{ flex: 1 }} behavior={Platform.OS === "ios" ? "padding" : undefined}>
      <ScrollView contentContainerStyle={[styles.scrollContent, styles.loginScrollContent]}>
        <View style={styles.loginStack}>
          <View style={styles.hero}>
            <Text style={styles.kicker}>Intelligent Warehouse</Text>
            <Text style={styles.title}>Aplikacja mobilna klienta</Text>
            <Text style={styles.subtitle}>Strefa klienta: zamówienia, rezerwacje, powiadomienia i profil.</Text>
          </View>

          <Card>
            <SectionTitle title="Logowanie" />
            {error ? <ErrorBlock message={error} /> : null}

            <Field label="Login lub email">
              <PaperTextInput
                mode="outlined"
                dense
                style={styles.input}
                value={loginOrEmail}
                onChangeText={setLoginOrEmail}
                autoCapitalize="none"
                autoCorrect={false}
                placeholder="np. klient@demo.local"
                textColor={colors.text}
                outlineColor={colors.line}
                activeOutlineColor={colors.accent}
              />
            </Field>

            <Field label="Hasło">
              <PaperTextInput
                mode="outlined"
                dense
                style={styles.input}
                value={password}
                onChangeText={setPassword}
                secureTextEntry
                autoCapitalize="none"
                autoCorrect={false}
                placeholder="Hasło"
                textColor={colors.text}
                outlineColor={colors.line}
                activeOutlineColor={colors.accent}
              />
            </Field>

            <ActionButton label={busy ? "Logowanie..." : "Zaloguj"} onPress={submit} disabled={!canSubmit} />
            {__DEV__ ? <Text style={styles.devHint}>DEV API: {apiBaseUrl}</Text> : null}
          </Card>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

function Field({ label, children }: React.PropsWithChildren<{ label: string }>) {
  return (
    <View style={{ marginBottom: 12 }}>
      <Text style={styles.label}>{label}</Text>
      {children}
    </View>
  );
}

const styles = StyleSheet.create({
  scrollContent: { padding: 14, gap: 12 },
  loginScrollContent: { flexGrow: 1, justifyContent: "center" },
  loginStack: { width: "100%", maxWidth: 560, alignSelf: "center", gap: 12 },
  hero: {
    backgroundColor: "rgba(16,26,46,.88)",
    borderWidth: 1,
    borderColor: "rgba(56,189,248,.22)",
    borderRadius: 18,
    padding: 16
  },
  kicker: {
    color: colors.accent,
    fontSize: 11,
    fontWeight: "800",
    letterSpacing: 1.1,
    textTransform: "uppercase"
  },
  title: { color: colors.text, fontSize: 24, fontWeight: "900", marginTop: 6 },
  subtitle: { color: colors.muted, fontSize: 13, lineHeight: 19, marginTop: 8 },
  label: { color: colors.muted, fontSize: 12, fontWeight: "700", marginBottom: 6 },
  input: {
    backgroundColor: "rgba(22,35,61,.45)",
    borderRadius: 12,
    overflow: "hidden"
  },
  devHint: { color: colors.muted, fontSize: 11, marginTop: 8, opacity: 0.8 }
});
