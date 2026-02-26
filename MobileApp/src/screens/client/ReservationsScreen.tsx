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
  SectionTitle,
  colors,
} from "../../components/ui";
import type {
  ClientReservationDetailsDto,
  ClientReservationListItemDto,
} from "../../types";

export function ReservationsScreen({
  apiBaseUrl,
  token,
  openRequest,
  onOpenRequestHandled,
}: {
  apiBaseUrl: string;
  token: string;
  openRequest?: { reservationId: number; nonce: number } | null;
  onOpenRequestHandled?: () => void;
}) {
  const LIST_PAGE_SIZE = 10;
  const [items, setItems] = useState<ClientReservationListItemDto[] | null>(
    null,
  );
  const [details, setDetails] = useState<ClientReservationDetailsDto | null>(
    null,
  );
  const [selectedReservationId, setSelectedReservationId] = useState<
    number | null
  >(null);
  const [detailsLoading, setDetailsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [successBanner, setSuccessBanner] = useState<{
    message: string;
    reservationId: number;
  } | null>(null);
  const [visibleCount, setVisibleCount] = useState(LIST_PAGE_SIZE);

  async function loadList() {
    setError(null);
    setVisibleCount(LIST_PAGE_SIZE);
    try {
      setItems(await mobileApi.getReservations(apiBaseUrl, token));
    } catch (e) {
      setError(
        e instanceof Error ? e.message : "Nie udało się pobrać rezerwacji.",
      );
    }
  }

  async function showDetails(id: number) {
    setError(null);
    setSelectedReservationId(id);
    setDetailsLoading(true);
    try {
      setDetails(await mobileApi.getReservationDetails(apiBaseUrl, token, id));
    } catch (e) {
      setError(
        e instanceof Error
          ? e.message
          : "Nie udało się pobrać szczegółów rezerwacji.",
      );
      setSelectedReservationId(null);
      setDetails(null);
    } finally {
      setDetailsLoading(false);
    }
  }

  function closeDetails() {
    setSelectedReservationId(null);
    setDetails(null);
  }

  useEffect(() => {
    void loadList();
  }, [apiBaseUrl, token]);

  useEffect(() => {
    if (!openRequest?.reservationId) return;
    void loadList();
    void showDetails(openRequest.reservationId);
    setSuccessBanner({
      message: `Utworzono rezerwację i odświeżono listę.`,
      reservationId: openRequest.reservationId,
    });
    onOpenRequestHandled?.();
  }, [openRequest?.nonce]);

  return (
    <>
      <Card>
        <SectionTitle
          title="Rezerwacje"
          subtitle="Lista, statusy i podgląd szczegółów"
        />
        {successBanner ? (
          <View style={styles.successBanner}>
            <View style={styles.successBannerIconCircle}>
              <Text style={styles.successBannerIconText}>✓</Text>
            </View>
            <Text style={styles.successBannerText}>
              {successBanner.message}
            </Text>
            <ActionButton
              label="Szczegóły"
              variant="ghost"
              onPress={() => {
                const id = successBanner.reservationId;
                setSuccessBanner(null);
                void showDetails(id);
              }}
            />
          </View>
        ) : null}

        {!items && !error ? (
          <LoadingBlock label="Pobieranie rezerwacji..." />
        ) : error && !items ? (
          <ErrorBlock message={error} />
        ) : items && items.length === 0 ? (
          <EmptyBlock
            title="Brak rezerwacji"
            subtitle="Utwórz pierwszą rezerwację w zakładce Prod."
          />
        ) : (
          items?.slice(0, visibleCount).map((item) => (
            <ListItem
              key={item.reservationId}
              title={item.number}
              subtitle={`${item.warehouseName} • ${formatDateTime(item.createdAtUtc)} • ${item.itemsCount} poz.`}
              right={
                <View style={{ alignItems: "flex-end" }}>
                  <Pill
                    label={statusLabel(item.status, "reservation")}
                    tone={statusTone(item.status)}
                  />
                  {selectedReservationId === item.reservationId ? (
                    <Text style={styles.openHint}>Otwarte</Text>
                  ) : null}
                </View>
              }
              onPress={() => void showDetails(item.reservationId)}
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
          visible={selectedReservationId !== null}
          onDismiss={closeDetails}
          contentContainerStyle={styles.modalContainer}
          style={styles.modalShell}
        >
          <Card>
            <View style={styles.modalHeader}>
              <View style={styles.modalHeaderText}>
                <Text style={styles.modalTitle}>Szczegóły rezerwacji</Text>
                <Text
                  numberOfLines={1}
                  ellipsizeMode="middle"
                  style={styles.modalDocNo}
                >
                  {details?.number ??
                    `Rezerwacja #${selectedReservationId ?? ""}`}
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
                      label={statusLabel(details.status, "reservation")}
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
                  label="Utworzono"
                  value={formatDateTime(details.createdAtUtc)}
                />
                <DataRow
                  label="Wygasa"
                  value={formatDateTime(details.expiresAtUtc)}
                />
                <DataRow label="Notatka" value={details.note} />
                <Text style={styles.subsection}>Pozycje</Text>
                {details.items.map((row) => (
                  <ListItem
                    key={row.itemId}
                    title={`${row.lineNo}. ${row.productCode} - ${row.productName}`}
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
  subsection: {
    color: colors.text,
    fontWeight: "800",
    fontSize: 13,
    marginTop: 12,
    marginBottom: 6,
  },
  openHint: {
    color: colors.accent,
    fontSize: 10,
    marginTop: 2,
    fontWeight: "700",
  },
  modalTitle: {
    color: colors.text,
    fontSize: 15,
    lineHeight: 19,
    fontWeight: "800",
  },
  modalDocNo: {
    color: colors.accent,
    fontSize: 13,
    lineHeight: 17,
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
  successBanner: {
    backgroundColor: "rgba(21,51,90,.62)",
    borderWidth: 1,
    borderColor: "rgba(56,189,248,.35)",
    borderRadius: 14,
    padding: 10,
    marginBottom: 10,
    flexDirection: "row",
    alignItems: "center",
    gap: 10,
  },
  successBannerText: {
    color: colors.text,
    fontSize: 12,
    lineHeight: 17,
    fontWeight: "600",
    flex: 1,
  },
  successBannerIconCircle: {
    width: 20,
    height: 20,
    borderRadius: 999,
    backgroundColor: "rgba(56,189,248,.18)",
    borderWidth: 1,
    borderColor: "rgba(56,189,248,.45)",
    alignItems: "center",
    justifyContent: "center",
  },
  successBannerIconText: {
    color: colors.accent,
    fontWeight: "900",
    fontSize: 12,
    lineHeight: 14,
  },
});
