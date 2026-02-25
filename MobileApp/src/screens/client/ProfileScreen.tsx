import * as React from "react";
import { useEffect, useState } from "react";
import { View } from "react-native";

import type { Session } from "../../appTypes";
import { mobileApi } from "../../lib/api";
import { formatDateTime } from "../../lib/format";
import { ActionButton, Card, DataRow, ErrorBlock, LoadingBlock, SectionTitle } from "../../components/ui";
import type { ClientProfileDto, CurrentUserDto } from "../../types";

export function ProfileScreen({
  apiBaseUrl,
  token,
  session
}: {
  apiBaseUrl: string;
  token: string;
  session: Session;
}) {
  const [profile, setProfile] = useState<ClientProfileDto | null>(null);
  const [me, setMe] = useState<CurrentUserDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setError(null);
    try {
      const [profileData, meData] = await Promise.all([mobileApi.getProfile(apiBaseUrl, token), mobileApi.me(apiBaseUrl, token)]);
      setProfile(profileData);
      setMe(meData);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Nie udało się pobrać profilu.");
    }
  }

  useEffect(() => {
    void load();
  }, [apiBaseUrl, token]);

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
        <DataRow label="Status" value={profile?.isActive ? "Aktywny" : "Nieaktywny"} />
        <DataRow label="Utworzono" value={formatDateTime(profile?.createdAtUtc)} />
      </Card>
      <Card>
        <SectionTitle title="JWT / auth me" />
        <DataRow label="Login" value={me?.login ?? session.login} />
        <DataRow label="Email" value={me?.email ?? session.email} />
        <DataRow label="Role" value={(me?.roles ?? session.roles).join(", ")} />
        <View style={{ marginTop: 10 }}>
          <ActionButton label="Odśwież profil" onPress={() => void load()} variant="ghost" />
        </View>
      </Card>
    </>
  );
}
