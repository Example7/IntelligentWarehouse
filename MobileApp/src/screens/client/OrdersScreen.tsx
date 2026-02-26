import * as React from "react";
import { useEffect, useState } from "react";
import { ScrollView, StyleSheet, Text, View } from "react-native";
import { Modal, Portal } from "react-native-paper";

import { mobileApi } from "../../lib/api";
import {
  formatDateTime,
  formatNumber,
  statusLabel,
  statusTone,
} from "../../lib/format";
import {
  ActionButton,
  Card,
  DataRow,
  EmptyBlock,
  ErrorBlock,
  InlineRow,
  ListItem,
  LoadingBlock,
  Pill,
  colors,
} from "../../components/ui";
import type {
  ClientOrderDetailsDto,
  ClientOrderListItemDto,
} from "../../types";

export function OrdersScreen({
  apiBaseUrl,
  token,
  openRequest,
  onOpenRequestHandled,
}: {
  apiBaseUrl: string;
  token: string;
  openRequest?: { orderId: number; nonce: number } | null;
  onOpenRequestHandled?: () => void;
}) {
  const LIST_PAGE_SIZE = 10;
  const [items, setItems] = useState<ClientOrderListItemDto[] | null>(null);
  const [details, setDetails] = useState<ClientOrderDetailsDto | null>(null);
  const [selectedOrderId, setSelectedOrderId] = useState<number | null>(null);
  const [detailsLoading, setDetailsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [visibleCount, setVisibleCount] = useState(LIST_PAGE_SIZE);

  async function loadList() {
    setError(null);
    setVisibleCount(LIST_PAGE_SIZE);
    try {
      setItems(await mobileApi.getOrders(apiBaseUrl, token));
    } catch (e) {
      setError(
        e instanceof Error ? e.message : "Nie udało się pobrać zamówień.",
      );
    }
  }

  async function showDetails(id: number) {
    setError(null);
    setSelectedOrderId(id);
    setDetailsLoading(true);
    try {
      setDetails(await mobileApi.getOrderDetails(apiBaseUrl, token, id));
    } catch (e) {
      setError(
        e instanceof Error ? e.message : "Nie udało się pobrać szczegółów WZ.",
      );
      setSelectedOrderId(null);
      setDetails(null);
    } finally {
      setDetailsLoading(false);
    }
  }

  function closeDetails() {
    setSelectedOrderId(null);
    setDetails(null);
  }

  useEffect(() => {
    void loadList();
  }, [apiBaseUrl, token]);

  useEffect(() => {
    if (!openRequest?.orderId) return;
    void showDetails(openRequest.orderId);
    onOpenRequestHandled?.();
  }, [openRequest?.nonce]);

  return (
    <>
      <Card>
        <Text style={styles.sectionTitle}>Dokumenty WZ klienta</Text>
        {!items && !error ? (
          <LoadingBlock label="Pobieranie WZ..." />
        ) : error && !items ? (
          <ErrorBlock message={error} />
        ) : items && items.length === 0 ? (
          <EmptyBlock title="Brak dokumentów WZ" />
        ) : (
          items?.slice(0, visibleCount).map((item) => (
            <ListItem
              key={item.orderId}
              title={item.number}
              subtitle={`${item.warehouseName} • ${formatDateTime(item.issuedAtUtc)} • ${item.itemsCount} poz.`}
              right={
                <View style={{ alignItems: "flex-end" }}>
                  <Pill
                    label={statusLabel(item.status, "wz")}
                    tone={statusTone(item.status)}
                  />
                  <Text style={styles.metricText}>
                    {formatNumber(item.totalQuantity)}
                  </Text>
                  {selectedOrderId === item.orderId ? (
                    <Text style={styles.openHint}>Otwarte</Text>
                  ) : null}
                </View>
              }
              onPress={() => void showDetails(item.orderId)}
            />
          ))
        )}
        {items && items.length > visibleCount ? (
          <View style={{ marginTop: 10 }}>
            <ActionButton
              label={`Pokaż więcej (${Math.min(visibleCount, items.length)}/${items.length})`}
              onPress={() =>
                setVisibleCount((prev) =>
                  Math.min(prev + LIST_PAGE_SIZE, items.length),
                )
              }
              variant="secondary"
            />
          </View>
        ) : null}
        <View style={{ marginTop: 10 }}>
          <ActionButton
            label="Odśwież listę"
            onPress={() => void loadList()}
            variant="ghost"
          />
        </View>
      </Card>

      <Portal>
        <Modal
          visible={selectedOrderId !== null}
          onDismiss={closeDetails}
          contentContainerStyle={styles.modalContainer}
          style={styles.modalShell}
        >
          <Card>
            <View style={styles.modalHeader}>
              <View style={styles.modalHeaderText}>
                <Text style={styles.modalTitle}>Szczegóły WZ</Text>
                <Text
                  numberOfLines={1}
                  ellipsizeMode="middle"
                  style={styles.modalDocNo}
                >
                  {details?.number ?? `WZ #${selectedOrderId ?? ""}`}
                </Text>
              </View>
              <View style={styles.modalHeaderAction}>
                <ActionButton
                  label="Zamknij"
                  onPress={closeDetails}
                  variant="ghost"
                />
              </View>
            </View>

            {detailsLoading && !details ? (
              <LoadingBlock label="Pobieranie szczegółów..." />
            ) : details ? (
              <ScrollView style={{ maxHeight: 420 }}>
                <InlineRow style={{ marginBottom: 10 }}>
                  <View style={styles.pillWrap}>
                    <Pill
                      label={statusLabel(details.status, "wz")}
                      tone={statusTone(details.status)}
                    />
                  </View>
                  <View style={styles.pillWrap}>
                    <Pill
                      label={`Suma: ${formatNumber(details.totalQuantity)}`}
                    />
                  </View>
                </InlineRow>
                <DataRow label="Magazyn" value={details.warehouseName} />
                <DataRow
                  label="Wydano"
                  value={formatDateTime(details.issuedAtUtc)}
                />
                <DataRow
                  label="Zaksięgowano"
                  value={formatDateTime(details.postedAtUtc)}
                />
                <DataRow label="Notatka" value={details.note} />

                <Text style={styles.subsection}>Pozycje</Text>
                {details.items.map((row) => (
                  <ListItem
                    key={row.itemId}
                    title={`${row.lineNo}. ${row.productCode} — ${row.productName}`}
                    subtitle={`Ilość: ${formatNumber(row.quantity)} • Lokacja: ${row.locationCode ?? "-"}`}
                  />
                ))}
              </ScrollView>
            ) : (
              <EmptyBlock title="Brak danych szczegółowych" />
            )}
          </Card>
        </Modal>
      </Portal>
    </>
  );
}

const styles = StyleSheet.create({
  sectionTitle: {
    color: colors.text,
    fontSize: 17,
    fontWeight: "800",
    marginBottom: 10,
  },
  metricText: { color: colors.muted, fontSize: 11, marginTop: 4 },
  openHint: {
    color: colors.accent,
    fontSize: 10,
    marginTop: 2,
    fontWeight: "700",
  },
  subsection: {
    color: colors.text,
    fontWeight: "800",
    fontSize: 13,
    marginTop: 12,
    marginBottom: 6,
  },
  modalTitle: {
    color: colors.text,
    fontSize: 15,
    lineHeight: 19,
    fontWeight: "800",
  },
  modalDocNo: {
    color: colors.accent,
    fontSize: 14,
    lineHeight: 18,
    fontWeight: "800",
    marginTop: 2,
  },
  pillWrap: { justifyContent: "center" },
  modalHeader: {
    flexDirection: "row",
    alignItems: "center",
    gap: 10,
    marginBottom: 8,
  },
  modalHeaderText: { flex: 1, minWidth: 0 },
  modalHeaderAction: { width: 122 },
  modalContainer: {
    marginHorizontal: 24,
    marginVertical: 32,
    alignSelf: "center",
    width: "88%",
    maxWidth: 560,
  },
  modalShell: { justifyContent: "center" },
});
