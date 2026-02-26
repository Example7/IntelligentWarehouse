import * as React from "react";
import { useEffect, useMemo, useState } from "react";
import { ScrollView, View } from "react-native";
import { Chip, TextInput as PaperTextInput } from "react-native-paper";

import { mobileApi } from "../../lib/api";
import { formatDateTime } from "../../lib/format";
import {
  ActionButton,
  Card,
  EmptyBlock,
  ErrorBlock,
  ListItem,
  LoadingBlock,
  Pill,
  SectionTitle,
  colors,
} from "../../components/ui";
import type { ClientNotificationDto } from "../../types";

function notificationTone(
  item: ClientNotificationDto,
): "neutral" | "good" | "warn" | "danger" {
  const sev = (item.severity || "").toUpperCase();
  if (sev.includes("GOOD") || sev.includes("SUCCESS")) return "good";
  if (sev.includes("DANGER") || sev.includes("CRIT")) return "danger";
  if (sev.includes("WARN")) return "warn";
  return item.isAcknowledged ? "neutral" : "warn";
}

export function NotificationsScreen({
  apiBaseUrl,
  token,
}: {
  apiBaseUrl: string;
  token: string;
}) {
  const LIST_PAGE_SIZE = 10;
  const [items, setItems] = useState<ClientNotificationDto[] | null>(null);
  const [error, setError] = useState<string | null>(null);
  const [visibleCount, setVisibleCount] = useState(LIST_PAGE_SIZE);
  const [searchTerm, setSearchTerm] = useState("");
  const [typeFilter, setTypeFilter] = useState<"all" | "Reservation" | "WZ">(
    "all",
  );

  async function load() {
    setError(null);
    setVisibleCount(LIST_PAGE_SIZE);
    try {
      setItems(await mobileApi.getNotifications(apiBaseUrl, token, 100));
    } catch (e) {
      setError(
        e instanceof Error ? e.message : "Nie udało się pobrać powiadomień.",
      );
    }
  }

  useEffect(() => {
    void load();
  }, [apiBaseUrl, token]);

  const filteredItems = useMemo(() => {
    const term = searchTerm.trim().toLowerCase();
    return (items ?? []).filter((item) => {
      if (typeFilter !== "all" && item.documentType !== typeFilter)
        return false;
      if (!term) return true;
      return (
        (item.title || "").toLowerCase().includes(term) ||
        (item.message || "").toLowerCase().includes(term) ||
        (item.documentNumber || "").toLowerCase().includes(term) ||
        (item.status || "").toLowerCase().includes(term) ||
        (item.warehouseName || "").toLowerCase().includes(term)
      );
    });
  }, [items, searchTerm, typeFilter]);

  useEffect(() => {
    setVisibleCount(LIST_PAGE_SIZE);
  }, [searchTerm, typeFilter, items]);

  return (
    <Card>
      <SectionTitle
        title="Powiadomienia klienta"
        subtitle="Statusy rezerwacji i dokumentów WZ."
      />

      <PaperTextInput
        mode="outlined"
        value={searchTerm}
        onChangeText={setSearchTerm}
        placeholder="Szukaj po numerze, statusie lub magazynie"
        style={{ backgroundColor: colors.cardAlt, marginBottom: 10 }}
        outlineStyle={{ borderRadius: 12, borderColor: colors.line }}
        textColor={colors.text}
        placeholderTextColor={colors.muted}
        theme={{
          colors: { primary: colors.accent, onSurfaceVariant: colors.muted },
        }}
        left={<PaperTextInput.Icon icon="magnify" color={colors.muted} />}
      />

      <ScrollView
        horizontal
        showsHorizontalScrollIndicator={false}
        style={{ marginBottom: 10 }}
      >
        <View style={{ flexDirection: "row", gap: 8, paddingBottom: 2 }}>
          <Chip
            compact
            selected={typeFilter === "all"}
            onPress={() => setTypeFilter("all")}
          >
            Wszystkie
          </Chip>
          <Chip
            compact
            selected={typeFilter === "Reservation"}
            onPress={() => setTypeFilter("Reservation")}
          >
            Rezerwacje
          </Chip>
          <Chip
            compact
            selected={typeFilter === "WZ"}
            onPress={() => setTypeFilter("WZ")}
          >
            WZ
          </Chip>
        </View>
      </ScrollView>

      {!items && !error ? (
        <LoadingBlock label="Pobieranie powiadomień..." />
      ) : error && !items ? (
        <ErrorBlock message={error} />
      ) : filteredItems.length === 0 ? (
        <EmptyBlock title="Brak powiadomień" />
      ) : (
        filteredItems
          .slice(0, visibleCount)
          .map((item) => (
            <ListItem
              key={item.notificationId}
              title={
                item.title ||
                `${item.productCode ?? ""} ${item.productName ?? ""}`.trim() ||
                "Powiadomienie"
              }
              subtitle={`${item.message} • ${item.warehouseName} • ${formatDateTime(item.createdAtUtc)}`}
              right={
                <Pill
                  label={item.status || item.severity || "Info"}
                  tone={notificationTone(item)}
                />
              }
            />
          ))
      )}

      {items && filteredItems.length > visibleCount ? (
        <View style={{ marginTop: 10 }}>
          <ActionButton
            label={`Pokaż więcej (${Math.min(visibleCount, filteredItems.length)}/${filteredItems.length})`}
            onPress={() =>
              setVisibleCount((prev) =>
                Math.min(prev + LIST_PAGE_SIZE, filteredItems.length),
              )
            }
            variant="secondary"
          />
        </View>
      ) : null}

      <View style={{ marginTop: 10 }}>
        <ActionButton
          label="Odśwież"
          onPress={() => void load()}
          variant="ghost"
        />
      </View>
    </Card>
  );
}
