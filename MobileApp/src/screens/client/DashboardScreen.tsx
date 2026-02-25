import * as React from "react";
import { useEffect, useState } from "react";

import { mobileApi } from "../../lib/api";
import { formatDateTime, statusTone } from "../../lib/format";
import {
  Card,
  EmptyBlock,
  ErrorBlock,
  InlineRow,
  ListItem,
  LoadingBlock,
  MetricCard,
  Pill,
  SectionTitle,
} from "../../components/ui";
import type { ClientDashboardDto } from "../../types";

export function DashboardScreen({
  apiBaseUrl,
  token,
  onOpenOrder,
  onOpenReservation,
}: {
  apiBaseUrl: string;
  token: string;
  onOpenOrder?: (orderId: number) => void;
  onOpenReservation?: (reservationId: number) => void;
}) {
  const [data, setData] = useState<ClientDashboardDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setError(null);
    try {
      setData(await mobileApi.getDashboard(apiBaseUrl, token));
    } catch (e) {
      setError(
        e instanceof Error ? e.message : "Nie udało się pobrać dashboardu.",
      );
    }
  }

  useEffect(() => {
    void load();
  }, [apiBaseUrl, token]);

  if (!data && !error) return <LoadingBlock label="Pobieranie dashboardu..." />;
  if (!data && error) return <ErrorBlock message={error} />;

  return (
    <>
      <Card>
        <SectionTitle
          title="Dashboard klienta"
          subtitle="Podsumowanie aktywnosci"
        />
        <InlineRow>
          <MetricCard
            label="Aktywne WZ"
            value={String(data?.activeOrdersCount ?? 0)}
            accent="amber"
          />
          <MetricCard
            label="Posted WZ"
            value={String(data?.postedOrdersCount ?? 0)}
            accent="teal"
          />
          <MetricCard
            label="Rezerwacje open"
            value={String(data?.openReservationsCount ?? 0)}
          />
        </InlineRow>
      </Card>

      <Card>
        <SectionTitle title="Ostatnie zamówienia" />
        {data?.recentOrders.length ? (
          data.recentOrders.map((item) => (
            <ListItem
              key={item.orderId}
              title={item.number}
              subtitle={`${item.warehouseName} • ${formatDateTime(item.issuedAtUtc)}`}
              right={
                <Pill label={item.status} tone={statusTone(item.status)} />
              }
              onPress={
                onOpenOrder ? () => onOpenOrder(item.orderId) : undefined
              }
            />
          ))
        ) : (
          <EmptyBlock title="Brak zamowień" />
        )}
      </Card>

      <Card>
        <SectionTitle title="Ostatnie rezerwacje" />
        {data?.recentReservations.length ? (
          data.recentReservations.map((item) => (
            <ListItem
              key={item.reservationId}
              title={item.number}
              subtitle={`${item.warehouseName} • ${formatDateTime(item.createdAtUtc)}`}
              right={
                <Pill label={item.status} tone={statusTone(item.status)} />
              }
              onPress={
                onOpenReservation
                  ? () => onOpenReservation(item.reservationId)
                  : undefined
              }
            />
          ))
        ) : (
          <EmptyBlock title="Brak rezerwacji" />
        )}
      </Card>
    </>
  );
}
