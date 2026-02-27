import * as React from "react";
import { useEffect, useState } from "react";
import { StyleSheet, Text, View } from "react-native";
import { TextInput as PaperTextInput } from "react-native-paper";

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
import type { ClientProfileDto } from "../../types";

export function ProfileScreen({
  apiBaseUrl,
  token,
}: {
  apiBaseUrl: string;
  token: string;
}) {
  const [profile, setProfile] = useState<ClientProfileDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  const [name, setName] = useState("");
  const [email, setEmail] = useState("");
  const [phone, setPhone] = useState("");
  const [address, setAddress] = useState("");

  const [profileBusy, setProfileBusy] = useState(false);
  const [profileError, setProfileError] = useState<string | null>(null);
  const [profileSuccess, setProfileSuccess] = useState<string | null>(null);

  const [currentPassword, setCurrentPassword] = useState("");
  const [newPassword, setNewPassword] = useState("");
  const [confirmNewPassword, setConfirmNewPassword] = useState("");
  const [passwordBusy, setPasswordBusy] = useState(false);
  const [passwordError, setPasswordError] = useState<string | null>(null);
  const [passwordSuccess, setPasswordSuccess] = useState<string | null>(null);

  async function load() {
    setError(null);
    try {
      const profileData = await mobileApi.getProfile(apiBaseUrl, token);
      setProfile(profileData);
      setName(profileData.name ?? "");
      setEmail(profileData.email ?? "");
      setPhone(profileData.phone ?? "");
      setAddress(profileData.address ?? "");
    } catch (e) {
      setError(
        e instanceof Error ? e.message : "Nie udało się pobrać profilu.",
      );
    }
  }

  useEffect(() => {
    void load();
  }, [apiBaseUrl, token]);

  async function submitProfileUpdate() {
    setProfileError(null);
    setProfileSuccess(null);

    if (!name.trim()) {
      setProfileError("Nazwa jest wymagana.");
      return;
    }

    if (!email.trim()) {
      setProfileError("Email jest wymagany.");
      return;
    }

    setProfileBusy(true);
    try {
      const updated = await mobileApi.updateProfile(apiBaseUrl, token, {
        name: name.trim(),
        email: email.trim(),
        phone: phone.trim() || null,
        address: address.trim() || null,
      });

      setProfile(updated);
      setName(updated.name ?? "");
      setEmail(updated.email ?? "");
      setPhone(updated.phone ?? "");
      setAddress(updated.address ?? "");
      setProfileSuccess("Dane profilu zostały zapisane.");
    } catch (e) {
      setProfileError(
        e instanceof Error ? e.message : "Nie udało się zapisać profilu.",
      );
    } finally {
      setProfileBusy(false);
    }
  }

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
        <SectionTitle
          title="Profil klienta"
          subtitle="Edycja danych kontaktowych"
        />

        {profileError ? (
          <Text style={styles.errorText}>{profileError}</Text>
        ) : null}
        {profileSuccess ? (
          <Text style={styles.successText}>{profileSuccess}</Text>
        ) : null}

        <Field label="Nazwa">
          <PaperTextInput
            mode="outlined"
            dense
            value={name}
            onChangeText={setName}
            placeholder="Nazwa klienta"
            textColor={colors.text}
            outlineColor={colors.line}
            activeOutlineColor={colors.accent}
            style={styles.input}
          />
        </Field>

        <Field label="Email">
          <PaperTextInput
            mode="outlined"
            dense
            value={email}
            onChangeText={setEmail}
            autoCapitalize="none"
            autoCorrect={false}
            keyboardType="email-address"
            placeholder="adres@email.com"
            textColor={colors.text}
            outlineColor={colors.line}
            activeOutlineColor={colors.accent}
            style={styles.input}
          />
        </Field>

        <Field label="Telefon">
          <PaperTextInput
            mode="outlined"
            dense
            value={phone}
            onChangeText={setPhone}
            keyboardType="phone-pad"
            placeholder="Telefon"
            textColor={colors.text}
            outlineColor={colors.line}
            activeOutlineColor={colors.accent}
            style={styles.input}
          />
        </Field>

        <Field label="Adres">
          <PaperTextInput
            mode="outlined"
            dense
            value={address}
            onChangeText={setAddress}
            placeholder="Adres"
            textColor={colors.text}
            outlineColor={colors.line}
            activeOutlineColor={colors.accent}
            style={styles.input}
          />
        </Field>

        <DataRow
          label="Utworzono"
          value={formatDateTime(profile?.createdAtUtc)}
        />

        <View style={styles.actionsRow}>
          <ActionButton
            label={profileBusy ? "Zapisywanie..." : "Zapisz dane"}
            onPress={() => void submitProfileUpdate()}
            disabled={profileBusy}
          />
          <ActionButton
            label="Odswież"
            variant="ghost"
            onPress={() => void load()}
            disabled={profileBusy}
          />
        </View>
      </Card>

      <Card>
        <SectionTitle title="Bezpieczeństwo" subtitle="Zmiana hasła do konta" />

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
            label={passwordBusy ? "Zmienianie..." : "Zmien hasło"}
            onPress={() => void submitPasswordChange()}
            disabled={passwordBusy}
          />
        </View>
      </Card>
    </>
  );
}

function Field({
  label,
  children,
}: React.PropsWithChildren<{ label: string }>) {
  return (
    <View style={styles.fieldWrap}>
      <Text style={styles.label}>{label}</Text>
      {children}
    </View>
  );
}

const styles = StyleSheet.create({
  label: {
    color: colors.muted,
    fontSize: 12,
    fontWeight: "700",
    marginBottom: 6,
  },
  fieldWrap: {
    marginBottom: 8,
  },
  input: {
    backgroundColor: "rgba(22,35,61,.45)",
    borderRadius: 12,
    overflow: "hidden",
    marginBottom: 6,
  },
  actionsRow: {
    gap: 10,
    marginTop: 12,
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
