import * as React from "react";
import { useEffect, useState } from "react";
import { StyleSheet, Text, View } from "react-native";
import { TextInput as PaperTextInput } from "react-native-paper";

import type { Session } from "../../appTypes";
import {
  ActionButton,
  Card,
  DataRow,
  ErrorBlock,
  LoadingBlock,
  SectionTitle,
  colors,
} from "../../components/ui";
import { mobileApi } from "../../lib/api";
import { formatDateTime } from "../../lib/format";
import type { ClientProfileDto, CurrentUserDto } from "../../types";

export function ProfileScreen({
  apiBaseUrl,
  token,
  session,
}: {
  apiBaseUrl: string;
  token: string;
  session: Session;
}) {
  const [profile, setProfile] = useState<ClientProfileDto | null>(null);
  const [me, setMe] = useState<CurrentUserDto | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmNewPassword, setConfirmNewPassword] = useState("");
  const [passwordBusy, setPasswordBusy] = useState(false);
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [passwordSuccess, setPasswordSuccess] = useState<string | null>(null);

  async function load() {
    setError(null);
    try {
      const [profileData, meData] = await Promise.all([
        mobileApi.getProfile(apiBaseUrl, token),
        mobileApi.me(apiBaseUrl, token),
      ]);
      setProfile(profileData);
      setMe(meData);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Nie udało się pobrać profilu.");
    }
  }

  useEffect(() => {
    void load();
  }, [apiBaseUrl, token]);

  async function submitPasswordChange() {
    setPasswordError(null);
    setPasswordSuccess(null);

    if (!currentPassword || !newPassword || !confirmNewPassword) {
      setPasswordError("Uzupełnij wszystkie pola hasła.");
      return;
    }

    if (newPassword.length < 8) {
      setPasswordError("Nowe hasło musi mieć co najmniej 8 znaków.");
      return;
    }

    if (newPassword !== confirmNewPassword) {
      setPasswordError("Nowe hasło i potwierdzenie muszą być takie same.");
      return;
    }

    if (newPassword === currentPassword) {
      setPasswordError("Nowe hasło musi być inne niż obecne.");
      return;
    }

    setPasswordBusy(true);
    try {
      const result = await mobileApi.changePassword(apiBaseUrl, token, {
        currentPassword,
        newPassword,
        confirmNewPassword,
      });
      setPasswordSuccess(result.message || "Hasło zostało zmienione.");
      setCurrentPassword("");
      setNewPassword("");
      setConfirmNewPassword("");
    } catch (e) {
      setPasswordError(
        e instanceof Error ? e.message : "Nie udało się zmienić hasła.",
      );
    } finally {
      setPasswordBusy(false);
    }
  }

  if (!profile && !error) return <LoadingBlock label="Pobieranie profilu..." />;
  if (!profile && error) return <ErrorBlock message={error} />;

  return (
    <>
      <Card>
        <SectionTitle title="Profil klienta" />
        <DataRow label="Nazwa" value={profile?.name} />
        <DataRow label="Email" value={profile?.email} />
        <DataRow label="Telefon" value={profile?.phone} />
        <DataRow label="Adres" value={profile?.address} />
        <DataRow
          label="Status"
          value={profile?.isActive ? "Aktywny" : "Nieaktywny"}
        />
        <DataRow
          label="Utworzono"
          value={formatDateTime(profile?.createdAtUtc)}
        />
      </Card>

      <Card>
        <SectionTitle
          title="Dane konta"
          subtitle="Dane logowania i uprawnienia konta mobilnego"
        />
        <DataRow label="Login" value={me?.login ?? session.login} />
        <DataRow label="Email logowania" value={me?.email ?? session.email} />
        <DataRow label="Rola / role" value={(me?.roles ?? session.roles).join(", ")} />
        <View style={{ marginTop: 10 }}>
          <ActionButton
            label="Odśwież profil"
            onPress={() => void load()}
            variant="ghost"
          />
        </View>
      </Card>

      <Card>
        <SectionTitle
          title="Bezpieczeństwo"
          subtitle="Zmiana hasła do konta"
        />

        {passwordError ? (
          <Text style={styles.errorText}>{passwordError}</Text>
        ) : null}
        {passwordSuccess ? (
          <Text style={styles.successText}>{passwordSuccess}</Text>
        ) : null}

        <PaperTextInput
          mode="outlined"
          dense
          value={currentPassword}
          onChangeText={setCurrentPassword}
          secureTextEntry
          autoCapitalize="none"
          autoCorrect={false}
          placeholder="Obecne hasło"
          textColor={colors.text}
          outlineColor={colors.line}
          activeOutlineColor={colors.accent}
          style={styles.input}
        />
        <PaperTextInput
          mode="outlined"
          dense
          value={newPassword}
          onChangeText={setNewPassword}
          secureTextEntry
          autoCapitalize="none"
          autoCorrect={false}
          placeholder="Nowe hasło (min. 8 znaków)"
          textColor={colors.text}
          outlineColor={colors.line}
          activeOutlineColor={colors.accent}
          style={styles.input}
        />
        <PaperTextInput
          mode="outlined"
          dense
          value={confirmNewPassword}
          onChangeText={setConfirmNewPassword}
          secureTextEntry
          autoCapitalize="none"
          autoCorrect={false}
          placeholder="Powtórz nowe hasło"
          textColor={colors.text}
          outlineColor={colors.line}
          activeOutlineColor={colors.accent}
          style={styles.input}
        />

        <View style={{ marginTop: 6 }}>
          <ActionButton
            label={passwordBusy ? "Zmienianie..." : "Zmień hasło"}
            onPress={() => void submitPasswordChange()}
            disabled={passwordBusy}
          />
        </View>
      </Card>
    </>
  );
}

const styles = StyleSheet.create({
  input: {
    backgroundColor: "rgba(22,35,61,.45)",
    borderRadius: 12,
    overflow: "hidden",
    marginBottom: 10,
  },
  errorText: {
    color: colors.danger,
    fontSize: 12,
    fontWeight: "700",
    marginBottom: 8,
  },
  successText: {
    color: colors.success,
    fontSize: 12,
    fontWeight: "700",
    marginBottom: 8,
  },
});
