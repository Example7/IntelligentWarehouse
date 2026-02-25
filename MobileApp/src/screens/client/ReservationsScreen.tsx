import * as React from "react";
import { useEffect, useState } from "react";
import { Alert, ScrollView, StyleSheet, Text, View } from "react-native";
import { Modal, Portal } from "react-native-paper";

import { mobileApi } from "../../lib/api";
import { formatDateTime, formatNumber, statusTone } from "../../lib/format";
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

  async function loadList() {
    setError(null);
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
    void showDetails(openRequest.reservationId);
    onOpenRequestHandled?.();
  }, [openRequest?.nonce]);

  return (
    <>
      <Card>
        <SectionTitle
          title="Rezerwacje"
          subtitle="Lista i podgląd szczegółów"
        />
        <View style={{ marginBottom: 10 }}>
          <ActionButton
            label="Złóż rezerwację (w przygotowaniu)"
            variant="secondary"
            onPress={() =>
              Alert.alert(
                "Brak funkcji w API",
                "MobileApi udostępnia teraz tylko podgląd rezerwacji (GET). Aby składać rezerwacje z aplikacji, trzeba dodać endpoint POST /api/client/reservations.",
              )
            }
          />
        </View>
        {!items && !error ? (
          <LoadingBlock label="Pobieranie rezerwacji..." />
        ) : error && !items ? (
          <ErrorBlock message={error} />
        ) : items && items.length === 0 ? (
          <EmptyBlock title="Brak rezerwacji" />
        ) : (
          items?.map((item) => (
            <ListItem
              key={item.reservationId}
              title={item.number}
              subtitle={`${item.warehouseName} • ${formatDateTime(item.createdAtUtc)} • ${item.itemsCount} poz.`}
              right={
                <View style={{ alignItems: "flex-end" }}>
                  <Pill label={item.status} tone={statusTone(item.status)} />
                  {selectedReservationId === item.reservationId ? (
                    <Text style={styles.openHint}>Otwarte</Text>
                  ) : null}
                </View>
              }
              onPress={() => void showDetails(item.reservationId)}
            />
          ))
        )}
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
                <Text style={styles.modalTitle}>Szczegoly rezerwacji</Text>
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
              <LoadingBlock label="Pobieranie szczegolow..." />
            ) : details ? (
              <ScrollView style={{ maxHeight: 420 }}>
                <InlineRow style={{ marginBottom: 10 }}>
                  <View style={styles.pillWrap}>
                    <Pill
                      label={details.status}
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
                    title={`${row.lineNo}. ${row.productCode} — ${row.productName}`}
                    subtitle={`Ilosc: ${formatNumber(row.quantity)} • Lokacja: ${row.locationCode ?? "-"}`}
                  />
                ))}
              </ScrollView>
            ) : (
              <EmptyBlock title="Brak danych szczegolowych" />
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
});
