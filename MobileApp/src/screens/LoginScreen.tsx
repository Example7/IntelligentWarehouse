import * as React from "react";
import { useState } from "react";
import {
  KeyboardAvoidingView,
  Platform,
  ScrollView,
  StyleSheet,
  Text,
  View,
} from "react-native";
import { TextInput as PaperTextInput } from "react-native-paper";

import {
  ActionButton,
  Card,
  ErrorBlock,
  SectionTitle,
  colors,
} from "../components/ui";
import { mobileApi } from "../lib/api";
import type { Session } from "../appTypes";

export function LoginScreen({
  defaultApiBaseUrl,
  onLogin,
}: {
  defaultApiBaseUrl: string;
  onLogin: (session: Session, apiBaseUrl: string) => Promise<void>;
}) {
  const [apiBaseUrl] = useState(defaultApiBaseUrl);
  const [mode, setMode] = useState<"login" | "register">("login");

  const [loginOrEmail, setLoginOrEmail] = useState("");
  const [password, setPassword] = useState("");

  const [registerName, setRegisterName] = useState("");
  const [registerLogin, setRegisterLogin] = useState("");
  const [registerEmail, setRegisterEmail] = useState("");
  const [registerPhone, setRegisterPhone] = useState("");
  const [registerAddress, setRegisterAddress] = useState("");
  const [registerPassword, setRegisterPassword] = useState("");
  const [registerConfirmPassword, setRegisterConfirmPassword] = useState("");

  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);

  async function submitLogin() {
    setBusy(true);
    setError(null);
    setSuccess(null);

    try {
      const response = await mobileApi.login(
        apiBaseUrl.trim(),
        loginOrEmail.trim(),
        password,
      );
      await onLogin(response, apiBaseUrl.trim());
    } catch (e) {
      setError(e instanceof Error ? e.message : "Nie udało się zalogować.");
    } finally {
      setBusy(false);
    }
  }

  async function submitRegister() {
    setBusy(true);
    setError(null);
    setSuccess(null);

    try {
      const response = await mobileApi.registerClient(apiBaseUrl.trim(), {
        name: registerName.trim(),
        login: registerLogin.trim(),
        email: registerEmail.trim(),
        password: registerPassword,
        confirmPassword: registerConfirmPassword,
        phone: registerPhone.trim() || null,
        address: registerAddress.trim() || null,
      });

      setSuccess(
        response.message || "Konto zostało utworzone. Możesz się zalogować.",
      );
      setMode("login");
      setLoginOrEmail(registerLogin.trim() || registerEmail.trim());
      setPassword("");
      setRegisterName("");
      setRegisterLogin("");
      setRegisterEmail("");
      setRegisterPhone("");
      setRegisterAddress("");
      setRegisterPassword("");
      setRegisterConfirmPassword("");
    } catch (e) {
      setError(
        e instanceof Error ? e.message : "Nie udało się utworzyć konta.",
      );
    } finally {
      setBusy(false);
    }
  }

  const canSubmitLogin =
    loginOrEmail.trim().length > 0 && password.length > 0 && !busy;
  const canSubmitRegister =
    registerName.trim().length > 0 &&
    registerLogin.trim().length > 0 &&
    registerEmail.trim().length > 0 &&
    registerPassword.length > 0 &&
    registerConfirmPassword.length > 0 &&
    !busy;

  return (
    <KeyboardAvoidingView
      style={{ flex: 1 }}
      behavior={Platform.OS === "ios" ? "padding" : undefined}
    >
      <ScrollView
        contentContainerStyle={[
          styles.scrollContent,
          styles.loginScrollContent,
        ]}
      >
        <View style={styles.loginStack}>
          <View style={styles.hero}>
            <Text style={styles.kicker}>Intelligent Warehouse</Text>
            <Text style={styles.title}>Aplikacja mobilna klienta</Text>
            <Text style={styles.subtitle}>
              Strefa klienta: zamówienia, rezerwacje, powiadomienia i profil.
            </Text>
          </View>

          <Card>
            <SectionTitle
              title={mode === "login" ? "Logowanie" : "Rejestracja klienta"}
            />
            {error ? <ErrorBlock message={error} /> : null}
            {success ? (
              <View style={styles.successBox}>
                <Text style={styles.successTitle}>Sukces</Text>
                <Text style={styles.successText}>{success}</Text>
              </View>
            ) : null}

            {mode === "login" ? (
              <>
                <Field label="Login lub e-mail">
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

                <View style={styles.actionsStack}>
                  <ActionButton
                    label={busy ? "Logowanie..." : "Zaloguj"}
                    onPress={submitLogin}
                    disabled={!canSubmitLogin}
                  />
                  <ActionButton
                    label="Załóż konto"
                    variant="ghost"
                    onPress={() => {
                      setMode("register");
                      setError(null);
                      setSuccess(null);
                    }}
                    disabled={busy}
                  />
                </View>
              </>
            ) : (
              <>
                <Field label="Nazwa klienta">
                  <PaperTextInput
                    mode="outlined"
                    dense
                    style={styles.input}
                    value={registerName}
                    onChangeText={setRegisterName}
                    autoCorrect={false}
                    placeholder="np. Firma XYZ"
                    textColor={colors.text}
                    outlineColor={colors.line}
                    activeOutlineColor={colors.accent}
                  />
                </Field>

                <Field label="Login">
                  <PaperTextInput
                    mode="outlined"
                    dense
                    style={styles.input}
                    value={registerLogin}
                    onChangeText={setRegisterLogin}
                    autoCapitalize="none"
                    autoCorrect={false}
                    placeholder="np. klient.xyz"
                    textColor={colors.text}
                    outlineColor={colors.line}
                    activeOutlineColor={colors.accent}
                  />
                </Field>

                <Field label="E-mail">
                  <PaperTextInput
                    mode="outlined"
                    dense
                    style={styles.input}
                    value={registerEmail}
                    onChangeText={setRegisterEmail}
                    autoCapitalize="none"
                    autoCorrect={false}
                    keyboardType="email-address"
                    placeholder="np. klient@firma.pl"
                    textColor={colors.text}
                    outlineColor={colors.line}
                    activeOutlineColor={colors.accent}
                  />
                </Field>

                <Field label="Telefon (opcjonalnie)">
                  <PaperTextInput
                    mode="outlined"
                    dense
                    style={styles.input}
                    value={registerPhone}
                    onChangeText={setRegisterPhone}
                    keyboardType="phone-pad"
                    placeholder="+48..."
                    textColor={colors.text}
                    outlineColor={colors.line}
                    activeOutlineColor={colors.accent}
                  />
                </Field>

                <Field label="Adres (opcjonalnie)">
                  <PaperTextInput
                    mode="outlined"
                    dense
                    style={styles.input}
                    value={registerAddress}
                    onChangeText={setRegisterAddress}
                    autoCorrect={false}
                    placeholder="Ulica, kod, miasto"
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
                    value={registerPassword}
                    onChangeText={setRegisterPassword}
                    secureTextEntry
                    autoCapitalize="none"
                    autoCorrect={false}
                    placeholder="Min. 8 znaków"
                    textColor={colors.text}
                    outlineColor={colors.line}
                    activeOutlineColor={colors.accent}
                  />
                </Field>

                <Field label="Potwierdź hasło">
                  <PaperTextInput
                    mode="outlined"
                    dense
                    style={styles.input}
                    value={registerConfirmPassword}
                    onChangeText={setRegisterConfirmPassword}
                    secureTextEntry
                    autoCapitalize="none"
                    autoCorrect={false}
                    placeholder="Powtórz hasło"
                    textColor={colors.text}
                    outlineColor={colors.line}
                    activeOutlineColor={colors.accent}
                  />
                </Field>

                <View style={styles.actionsStack}>
                  <ActionButton
                    label={busy ? "Tworzenie konta..." : "Utwórz konto"}
                    onPress={submitRegister}
                    disabled={!canSubmitRegister}
                  />
                  <ActionButton
                    label="Wróć do logowania"
                    variant="ghost"
                    onPress={() => {
                      setMode("login");
                      setError(null);
                    }}
                    disabled={busy}
                  />
                </View>
              </>
            )}
          </Card>
        </View>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

function Field({
  label,
  children,
}: React.PropsWithChildren<{ label: string }>) {
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
    padding: 16,
  },
  kicker: {
    color: colors.accent,
    fontSize: 11,
    fontWeight: "800",
    letterSpacing: 1.1,
    textTransform: "uppercase",
  },
  title: { color: colors.text, fontSize: 24, fontWeight: "900", marginTop: 6 },
  subtitle: { color: colors.muted, fontSize: 13, lineHeight: 19, marginTop: 8 },
  label: {
    color: colors.muted,
    fontSize: 12,
    fontWeight: "700",
    marginBottom: 6,
  },
  input: {
    backgroundColor: "rgba(22,35,61,.45)",
    borderRadius: 12,
    overflow: "hidden",
  },
  successBox: {
    marginBottom: 12,
    borderColor: "rgba(34,197,94,.4)",
    borderWidth: 1,
    borderRadius: 12,
    backgroundColor: "rgba(20,83,45,.30)",
    padding: 12,
  },
  successTitle: {
    color: "#86EFAC",
    fontWeight: "800",
  },
  successText: {
    color: "#DCFCE7",
    marginTop: 6,
  },
  actionsStack: {
    gap: 10,
  },
});
