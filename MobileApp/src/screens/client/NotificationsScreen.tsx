import * as React from "react";
import { useEffect, useState } from "react";
import { View } from "react-native";

import { mobileApi } from "../../lib/api";
import { formatDateTime } from "../../lib/format";
import { ActionButton, Card, EmptyBlock, ErrorBlock, ListItem, LoadingBlock, Pill, SectionTitle } from "../../components/ui";
import type { ClientNotificationDto } from "../../types";

export function NotificationsScreen({ apiBaseUrl, token }: { apiBaseUrl: string; token: string }) {
  const [items, setItems] = useState<ClientNotificationDto[] | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setError(null);
    try {
      setItems(await mobileApi.getNotifications(apiBaseUrl, token, 30));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Nie udało się pobrać alertów.");
    }
  }

  useEffect(() => {
    void load();
  }, [apiBaseUrl, token]);

  return (
    <Card>
      <SectionTitle title="Alerty / powiadomienia" />
      {!items && !error ? (
        <LoadingBlock label="Pobieranie alertów..." />
      ) : error && !items ? (
        <ErrorBlock message={error} />
      ) : items && items.length === 0 ? (
        <EmptyBlock title="Brak alertów" />
      ) : (
        items?.map((item) => (
          <ListItem
            key={item.notificationId}
            title={`${item.productCode} — ${item.productName}`}
            subtitle={`${item.message} • ${item.warehouseName} • ${formatDateTime(item.createdAtUtc)}`}
            right={<Pill label={item.severity || "Info"} tone={item.isAcknowledged ? "neutral" : "warn"} />}
          />
        ))
      )}
      <View style={{ marginTop: 10 }}>
        <ActionButton label="Odśwież" onPress={() => void load()} variant="ghost" />
      </View>
    </Card>
  );
}
