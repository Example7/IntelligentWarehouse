import * as React from "react";
import { useMemo, useState } from "react";
import { ScrollView, StyleSheet, Text, View } from "react-native";
import { BottomNavigation, IconButton } from "react-native-paper";

import type { ClientTabKey, Session } from "../appTypes";
import { ActionButton, Card, DataRow, Pill, SectionTitle, colors } from "./ui";
import { CmsScreen } from "../screens/client/CmsScreen";
import { DashboardScreen } from "../screens/client/DashboardScreen";
import { NotificationsScreen } from "../screens/client/NotificationsScreen";
import { OrdersScreen } from "../screens/client/OrdersScreen";
import { ProfileScreen } from "../screens/client/ProfileScreen";
import { ReservationsScreen } from "../screens/client/ReservationsScreen";

type RouteItem = {
  key: ClientTabKey;
  title: string;
  focusedIcon: string;
};

const routes: RouteItem[] = [
  { key: "start", title: "Start", focusedIcon: "view-dashboard-outline" },
  { key: "wz", title: "WZ", focusedIcon: "clipboard-list-outline" },
  { key: "rezerwacje", title: "Rez.", focusedIcon: "bookmark-outline" },
  { key: "alerty", title: "Alerty", focusedIcon: "bell-outline" },
  { key: "profil", title: "Profil", focusedIcon: "account-circle-outline" },
  { key: "cms", title: "Info", focusedIcon: "information-outline" },
];

export function ClientShell({
  session,
  apiBaseUrl,
  onLogout,
}: {
  session: Session;
  apiBaseUrl: string;
  onLogout: () => Promise<void>;
}) {
  const [tabIndex, setTabIndex] = useState(0);
  const [orderOpenRequest, setOrderOpenRequest] = useState<{
    orderId: number;
    nonce: number;
  } | null>(null);
  const [reservationOpenRequest, setReservationOpenRequest] = useState<{
    reservationId: number;
    nonce: number;
  } | null>(null);

  function openOrderFromDashboard(orderId: number) {
    const nextIndex = routes.findIndex((r) => r.key === "wz");
    if (nextIndex >= 0) setTabIndex(nextIndex);
    setOrderOpenRequest({ orderId, nonce: Date.now() });
  }

  function openReservationFromDashboard(reservationId: number) {
    const nextIndex = routes.findIndex((r) => r.key === "rezerwacje");
    if (nextIndex >= 0) setTabIndex(nextIndex);
    setReservationOpenRequest({ reservationId, nonce: Date.now() });
  }

  const hasClientRole = session.roles.some((r) => r.toLowerCase() === "client");
  if (!hasClientRole) {
    return (
      <ScrollView contentContainerStyle={styles.scrollContent}>
        <Card>
          <SectionTitle
            title="Konto bez roli Client"
            subtitle="Logowanie poprawne, ale brak autoryzacji kanału klienta."
          />
          <DataRow label="Login" value={session.login} />
          <DataRow label="Email" value={session.email} />
          <DataRow label="Role" value={session.roles.join(", ")} />
          <Text style={styles.hint}>
            MobileApi wymaga roli Client i powiązania klienta biznesowego
            (Customers.UserId) z zalogowanym użytkownikiem.
          </Text>
          <View style={{ marginTop: 12 }}>
            <ActionButton
              label="Wyloguj"
              onPress={() => void onLogout()}
              variant="secondary"
            />
          </View>
        </Card>
      </ScrollView>
    );
  }

  const currentRoute = routes[tabIndex] ?? routes[0];
  const content = useMemo(() => {
    switch (currentRoute.key) {
      case "start":
        return (
          <DashboardScreen
            apiBaseUrl={apiBaseUrl}
            token={session.accessToken}
            onOpenOrder={openOrderFromDashboard}
            onOpenReservation={openReservationFromDashboard}
          />
        );
      case "wz":
        return (
          <OrdersScreen
            apiBaseUrl={apiBaseUrl}
            token={session.accessToken}
            openRequest={orderOpenRequest}
            onOpenRequestHandled={() => setOrderOpenRequest(null)}
          />
        );
      case "rezerwacje":
        return (
          <ReservationsScreen
            apiBaseUrl={apiBaseUrl}
            token={session.accessToken}
            openRequest={reservationOpenRequest}
            onOpenRequestHandled={() => setReservationOpenRequest(null)}
          />
        );
      case "alerty":
        return (
          <NotificationsScreen
            apiBaseUrl={apiBaseUrl}
            token={session.accessToken}
          />
        );
      case "profil":
        return (
          <ProfileScreen
            apiBaseUrl={apiBaseUrl}
            token={session.accessToken}
            session={session}
          />
        );
      case "cms":
        return <CmsScreen apiBaseUrl={apiBaseUrl} />;
      default:
        return null;
    }
  }, [
    apiBaseUrl,
    currentRoute.key,
    onLogout,
    orderOpenRequest,
    reservationOpenRequest,
    session,
  ]);

  return (
    <View style={{ flex: 1 }}>
      <View style={styles.headerWrap}>
        <View style={styles.headerLeft}>
          <Text style={styles.headerTitle}>Strefa klienta</Text>
          <Text numberOfLines={1} style={styles.headerSubtitle}>
            {session.email || session.login}
          </Text>
        </View>
        <View style={styles.headerRight}>
          <Pill label="Client" tone="good" />
          <IconButton
            icon="logout"
            size={20}
            onPress={() => void onLogout()}
            iconColor={colors.text}
          />
        </View>
      </View>

      <ScrollView contentContainerStyle={styles.scrollContent}>
        {content}
      </ScrollView>

      <BottomNavigation.Bar
        navigationState={{ index: tabIndex, routes }}
        compact
        onTabPress={({ route, preventDefault }) => {
          const nextIndex = routes.findIndex((r) => r.key === route.key);
          if (nextIndex < 0) {
            preventDefault();
            return;
          }
          setTabIndex(nextIndex);
        }}
        activeColor={colors.accent}
        inactiveColor={colors.muted}
        safeAreaInsets={{ bottom: 0 }}
        labeled
        style={styles.bottomBar}
      />
    </View>
  );
}

const styles = StyleSheet.create({
  headerWrap: {
    minHeight: 72,
    paddingHorizontal: 14,
    paddingVertical: 10,
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    gap: 10,
    backgroundColor: "rgba(16,26,46,.96)",
    borderBottomWidth: 1,
    borderBottomColor: colors.line,
  },
  headerLeft: {
    flex: 1,
    minWidth: 0,
  },
  headerRight: {
    flexDirection: "row",
    alignItems: "center",
    gap: 2,
  },
  headerTitle: {
    color: colors.text,
    fontWeight: "800",
    fontSize: 16,
    lineHeight: 20,
  },
  headerSubtitle: {
    color: colors.muted,
    fontSize: 11,
    lineHeight: 15,
    marginTop: 2,
  },
  scrollContent: {
    padding: 14,
    gap: 12,
    paddingBottom: 90,
  },
  hint: {
    color: colors.muted,
    fontSize: 12,
    lineHeight: 17,
    marginTop: 10,
  },
  bottomBar: {
    backgroundColor: "rgba(16,26,46,.98)",
    borderTopWidth: 1,
    borderTopColor: colors.line,
    elevation: 12,
  },
});
