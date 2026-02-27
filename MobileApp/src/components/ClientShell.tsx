import * as React from "react";
import { useEffect, useMemo, useState } from "react";
import { Animated, ScrollView, StyleSheet, Text, View } from "react-native";
import {
  Button as PaperButton,
  BottomNavigation,
  Dialog,
  FAB,
  IconButton,
  Modal,
  Portal,
  TextInput as PaperTextInput,
} from "react-native-paper";

import type {
  ClientReservationCartItem,
  ClientTabKey,
  Session,
} from "../appTypes";
import type {
  ClientCreateReservationRequestDto,
  ClientProductLookupDto,
  ClientWarehouseLookupDto,
} from "../types";
import { mobileApi } from "../lib/api";
import { formatNumber } from "../lib/format";
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
} from "./ui";
import { CmsScreen } from "../screens/client/CmsScreen";
import { DashboardScreen } from "../screens/client/DashboardScreen";
import { NotificationsScreen } from "../screens/client/NotificationsScreen";
import { OrdersScreen } from "../screens/client/OrdersScreen";
import { ProfileScreen } from "../screens/client/ProfileScreen";
import { ProductsScreen } from "../screens/client/ProductsScreen";
import { ReservationsScreen } from "../screens/client/ReservationsScreen";

type RouteItem = {
  key: ClientTabKey;
  title: string;
  focusedIcon: string;
};

const routes: RouteItem[] = [
  { key: "start", title: "Start", focusedIcon: "view-dashboard-outline" },
  { key: "produkty", title: "Prod.", focusedIcon: "package-variant-closed" },
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
  const fabSize = 56;
  const fabRightOffset = 8;
  const fabBottomGap = 8;
  const [tabIndex, setTabIndex] = useState(0);
  const [orderOpenRequest, setOrderOpenRequest] = useState<{
    orderId: number;
    nonce: number;
  } | null>(null);
  const [reservationOpenRequest, setReservationOpenRequest] = useState<{
    reservationId: number;
    nonce: number;
  } | null>(null);
  const [cartItems, setCartItems] = useState<ClientReservationCartItem[]>([]);
  const [cartOpen, setCartOpen] = useState(false);
  const [warehouses, setWarehouses] = useState<ClientWarehouseLookupDto[]>([]);
  const [warehousesLoading, setWarehousesLoading] = useState(false);
  const [warehousesError, setWarehousesError] = useState<string | null>(null);
  const [selectedWarehouseId, setSelectedWarehouseId] = useState<number | null>(
    null,
  );
  const [reservationNote, setReservationNote] = useState("");
  const [cartError, setCartError] = useState<string | null>(null);
  const [cartSubmitBusy, setCartSubmitBusy] = useState(false);
  const [bottomBarHeight, setBottomBarHeight] = useState(72);
  const [clearCartConfirmOpen, setClearCartConfirmOpen] = useState(false);
  const cartBadgeScale = React.useRef(new Animated.Value(1)).current;
  const previousCartCountRef = React.useRef(0);

  async function loadReservationWarehouses() {
    setWarehousesLoading(true);
    setWarehousesError(null);
    try {
      const result = await mobileApi.getReservationWarehouses(
        apiBaseUrl,
        session.accessToken,
      );
      setWarehouses(result);
      setSelectedWarehouseId((prev) => prev ?? result[0]?.warehouseId ?? null);
    } catch (e) {
      setWarehousesError(
        e instanceof Error ? e.message : "Nie udało się pobrać magazynów.",
      );
    } finally {
      setWarehousesLoading(false);
    }
  }

  useEffect(() => {
    void loadReservationWarehouses();
  }, [apiBaseUrl, session.accessToken]);

  useEffect(() => {
    const previous = previousCartCountRef.current;
    previousCartCountRef.current = cartItems.length;

    if (cartItems.length <= 0 || cartItems.length === previous) {
      return;
    }

    Animated.sequence([
      Animated.timing(cartBadgeScale, {
        toValue: 1.18,
        duration: 120,
        useNativeDriver: true,
      }),
      Animated.timing(cartBadgeScale, {
        toValue: 1,
        duration: 180,
        useNativeDriver: true,
      }),
    ]).start();
  }, [cartBadgeScale, cartItems.length]);

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

  function openProductsCatalog() {
    const nextIndex = routes.findIndex((r) => r.key === "produkty");
    if (nextIndex >= 0) setTabIndex(nextIndex);
  }

  function handleReservationCreated(reservationId: number) {
    openReservationFromDashboard(reservationId);
  }

  function addProductToCart(product: ClientProductLookupDto, quantity: number) {
    setCartError(null);
    setCartItems((prev) => {
      const existing = prev.find((x) => x.productId === product.productId);
      if (existing) {
        return prev.map((x) =>
          x.productId === product.productId
            ? { ...x, quantity: x.quantity + quantity }
            : x,
        );
      }

      return [
        ...prev,
        {
          productId: product.productId,
          code: product.code,
          name: product.name,
          defaultUom: product.defaultUom,
          quantity,
        },
      ];
    });
  }

  function changeCartQty(productId: number, delta: number) {
    setCartItems((prev) =>
      prev
        .map((x) =>
          x.productId === productId
            ? { ...x, quantity: Math.max(0, x.quantity + delta) }
            : x,
        )
        .filter((x) => x.quantity > 0),
    );
  }

  function removeFromCart(productId: number) {
    setCartItems((prev) => prev.filter((x) => x.productId !== productId));
  }

  function clearCartWithConfirm() {
    if (cartItems.length === 0 && !reservationNote.trim()) {
      return;
    }
    setClearCartConfirmOpen(true);
  }

  function confirmClearCart() {
    setCartItems([]);
    setReservationNote("");
    setCartError(null);
    setClearCartConfirmOpen(false);
  }

  async function submitReservationFromCart() {
    setCartError(null);
    if (!selectedWarehouseId) {
      setCartError("Wybierz magazyn.");
      return;
    }
    if (cartItems.length === 0) {
      setCartError("Dodaj przynajmniej jedną pozycję do koszyka.");
      return;
    }

    const payload: ClientCreateReservationRequestDto = {
      warehouseId: selectedWarehouseId,
      expiresAtUtc: null,
      note: reservationNote.trim() || null,
      items: cartItems.map((x) => ({
        productId: x.productId,
        quantity: x.quantity,
      })),
    };

    setCartSubmitBusy(true);
    try {
      const created = await mobileApi.createReservation(
        apiBaseUrl,
        session.accessToken,
        payload,
      );
      setCartItems([]);
      setReservationNote("");
      setCartOpen(false);
      handleReservationCreated(created.reservationId);
    } catch (e) {
      setCartError(
        e instanceof Error ? e.message : "Nie udało się utworzyć rezerwacji.",
      );
    } finally {
      setCartSubmitBusy(false);
    }
  }

  function handleWarehouseSelection(nextWarehouseId: number) {
    if (selectedWarehouseId === nextWarehouseId) {
      return;
    }

    if (cartItems.length > 0) {
      setCartError(
        "Nie można zmienić magazynu, gdy koszyk zawiera pozycje. Wyślij rezerwację albo usuń pozycje z koszyka.",
      );
      setCartOpen(true);
      return;
    }

    setCartError(null);
    setSelectedWarehouseId(nextWarehouseId);
  }

  const hasClientRole = session.roles.some((r) => r.toLowerCase() === "klient");
  if (!hasClientRole) {
    return (
      <ScrollView contentContainerStyle={styles.scrollContent}>
        <Card>
          <SectionTitle
            title="Konto bez roli Klient"
            subtitle="Logowanie poprawne, ale brak autoryzacji kanału klienta."
          />
          <DataRow label="Login" value={session.login} />
          <DataRow label="Email" value={session.email} />
          <DataRow label="Role" value={session.roles.join(", ")} />
          <Text style={styles.hint}>
            MobileApi wymaga roli Klient i powiązania klienta biznesowego
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
  const selectedWarehouseName = useMemo(
    () =>
      warehouses.find((w) => w.warehouseId === selectedWarehouseId)?.name ??
      "-",
    [warehouses, selectedWarehouseId],
  );
  const cartTotalQty = useMemo(
    () => cartItems.reduce((sum, x) => sum + x.quantity, 0),
    [cartItems],
  );

  const content = useMemo(() => {
    switch (currentRoute.key) {
      case "start":
        return (
          <DashboardScreen
            apiBaseUrl={apiBaseUrl}
            token={session.accessToken}
            onOpenOrder={openOrderFromDashboard}
            onOpenReservation={openReservationFromDashboard}
            onOpenCatalog={openProductsCatalog}
          />
        );
      case "produkty":
        return (
          <ProductsScreen
            apiBaseUrl={apiBaseUrl}
            token={session.accessToken}
            warehouses={warehouses}
            warehousesLoading={warehousesLoading}
            warehousesError={warehousesError}
            selectedWarehouseId={selectedWarehouseId}
            onSelectWarehouse={handleWarehouseSelection}
            cartItems={cartItems}
            cartItemsCount={cartItems.length}
            onAddToCart={addProductToCart}
            onOpenCart={() => setCartOpen(true)}
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
          />
        );
      case "cms":
        return <CmsScreen apiBaseUrl={apiBaseUrl} />;
      default:
        return null;
    }
  }, [
    apiBaseUrl,
    cartItems.length,
    currentRoute.key,
    orderOpenRequest,
    reservationOpenRequest,
    selectedWarehouseId,
    session,
    warehouses,
    warehousesError,
    warehousesLoading,
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
          <Pill label="Klient" tone="good" />
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

      <Portal>
        <Modal
          visible={cartOpen}
          onDismiss={() => setCartOpen(false)}
          contentContainerStyle={styles.cartModalContainer}
          style={styles.cartModalShell}
        >
          <Card style={styles.cartModalCard}>
            <View style={styles.cartModalHeader}>
              <View style={{ flex: 1, minWidth: 0 }}>
                <SectionTitle
                  title="Koszyk rezerwacji"
                  subtitle="Podsumowanie i wysyłka rezerwacji"
                />
              </View>
              <View style={styles.cartModalHeaderActions}>
                <IconButton
                  icon="trash-can-outline"
                  size={20}
                  disabled={cartItems.length === 0 && !reservationNote.trim()}
                  onPress={clearCartWithConfirm}
                  iconColor={colors.danger}
                  style={[
                    styles.cartHeaderIconButton,
                    styles.cartHeaderIconDanger,
                  ]}
                />
                <IconButton
                  icon="close"
                  size={20}
                  onPress={() => setCartOpen(false)}
                  iconColor={colors.text}
                  style={styles.cartHeaderIconButton}
                />
              </View>
            </View>

            {cartError ? <ErrorBlock message={cartError} /> : null}
            {warehousesError ? <ErrorBlock message={warehousesError} /> : null}

            <ScrollView style={styles.cartModalScroll} nestedScrollEnabled>
              <Text style={styles.label}>Magazyn odbioru</Text>
              {warehousesLoading && warehouses.length === 0 ? (
                <LoadingBlock label="Pobieranie magazynów..." />
              ) : selectedWarehouseId && warehouses.length > 0 ? (
                <View style={styles.cartInfoBox}>
                  <Text style={styles.cartInfoValue}>
                    {selectedWarehouseName}
                  </Text>
                  <Text style={styles.cartInfoHint}>
                    Zmiana magazynu w zakładce Prod.
                  </Text>
                </View>
              ) : (
                <EmptyBlock
                  title="Brak wybranego magazynu"
                  subtitle="Wybierz magazyn odbioru w zakładce Prod."
                />
              )}

              {cartItems.length === 0 ? (
                <EmptyBlock
                  title="Koszyk jest pusty"
                  subtitle="Dodaj produkty w zakładce Prod."
                />
              ) : (
                <>
                  {cartItems.map((item) => (
                    <View key={item.productId} style={styles.cartRow}>
                      <View style={{ flex: 1, minWidth: 0 }}>
                        <Text numberOfLines={1} style={styles.cartTitle}>
                          {item.code} - {item.name}
                        </Text>
                        <Text style={styles.cartSubtitle}>
                          {formatNumber(item.quantity)} {item.defaultUom ?? ""}
                        </Text>
                      </View>
                      <View style={styles.cartRowActions}>
                        <View style={styles.qtyStepper}>
                          <IconButton
                            icon="minus"
                            size={18}
                            onPress={() => changeCartQty(item.productId, -1)}
                            iconColor={colors.text}
                            style={styles.qtyIconButton}
                          />
                          <View style={styles.qtyValueBox}>
                            <Text style={styles.qtyValueText}>
                              {formatNumber(item.quantity)}
                            </Text>
                          </View>
                          <IconButton
                            icon="plus"
                            size={18}
                            onPress={() => changeCartQty(item.productId, +1)}
                            iconColor={colors.text}
                            style={styles.qtyIconButton}
                          />
                        </View>
                        <IconButton
                          icon="trash-can-outline"
                          size={18}
                          onPress={() => removeFromCart(item.productId)}
                          iconColor={colors.danger}
                          style={styles.cartRemoveButton}
                        />
                      </View>
                    </View>
                  ))}
                </>
              )}

              <Text style={styles.label}>
                Notatka do rezerwacji (opcjonalnie)
              </Text>
              <PaperTextInput
                mode="outlined"
                value={reservationNote}
                onChangeText={setReservationNote}
                multiline
                numberOfLines={3}
                placeholder={`Magazyn: ${selectedWarehouseName}`}
                textColor={colors.text}
                outlineColor={colors.line}
                activeOutlineColor={colors.accent}
                contentStyle={styles.multilineInputContent}
                style={[styles.input, styles.multilineInput]}
              />
            </ScrollView>

            <View style={styles.cartFooter}>
              {cartItems.length > 0 ? (
                <>
                  <Text style={styles.cartFooterSectionTitle}>
                    Podsumowanie
                  </Text>
                  <View style={styles.cartSummary}>
                    <Pill label={`Pozycji: ${cartItems.length}`} />
                    <Pill
                      label={`Suma ilości: ${formatNumber(cartTotalQty)}`}
                      tone="good"
                    />
                  </View>
                </>
              ) : null}
              {cartItems.length === 0 ? (
                <Text style={styles.cartFooterHint}>
                  Koszyk jest pusty. Dodaj produkty w zakładce Prod.
                </Text>
              ) : null}
              <ActionButton
                label={
                  cartSubmitBusy
                    ? "Wysyłanie rezerwacji..."
                    : "Wyślij rezerwację"
                }
                onPress={() => void submitReservationFromCart()}
                disabled={cartSubmitBusy || cartItems.length === 0}
              />
            </View>
          </Card>
        </Modal>
      </Portal>

      {clearCartConfirmOpen ? (
        <Portal>
        <Dialog
          visible={clearCartConfirmOpen}
          onDismiss={() => setClearCartConfirmOpen(false)}
          style={styles.confirmDialog}
        >
          <View style={styles.confirmDialogTitleRow}>
            <View style={styles.confirmDialogIconWrap}>
              <IconButton
                icon="alert-circle-outline"
                size={18}
                iconColor={colors.danger}
                style={styles.confirmDialogIcon}
              />
            </View>
            <Dialog.Title style={styles.confirmDialogTitle}>
              Wyczyść koszyk rezerwacji
            </Dialog.Title>
          </View>
          <Dialog.Content style={styles.confirmDialogContent}>
            <Text style={styles.confirmDialogText}>
              {`Czy na pewno chcesz usunąć ${cartItems.length > 0 ? `wszystkie pozycje (${cartItems.length})` : "zawartość koszyka"}${reservationNote.trim() ? " oraz notatkę" : ""}?`}
            </Text>
          </Dialog.Content>
          <Dialog.Actions style={styles.confirmDialogActions}>
            <View style={styles.confirmDialogButtonRow}>
              <PaperButton
                mode="outlined"
                onPress={() => setClearCartConfirmOpen(false)}
                style={[
                  styles.confirmDialogButton,
                  styles.confirmDialogCancelButton,
                ]}
                labelStyle={styles.confirmDialogCancelLabel}
              >
                Anuluj
              </PaperButton>
              <PaperButton
                mode="contained-tonal"
                onPress={confirmClearCart}
                style={[
                  styles.confirmDialogButton,
                  styles.confirmDialogDeleteButton,
                ]}
                labelStyle={styles.confirmDialogDeleteLabel}
              >
                Usuń
              </PaperButton>
            </View>
          </Dialog.Actions>
        </Dialog>
        </Portal>
      ) : null}

      {!cartOpen ? (
        <View pointerEvents="box-none" style={styles.fabLayer}>
          <View
            style={[
              styles.fabWrap,
              {
                width: fabSize,
                height: fabSize,
                marginRight: fabRightOffset,
                marginBottom: bottomBarHeight + fabBottomGap,
              },
            ]}
          >
            {cartItems.length > 0 ? (
              <Animated.View
                style={[
                  styles.fabBadge,
                  styles.fabBadgeActive,
                  { transform: [{ scale: cartBadgeScale }] },
                ]}
              >
                <Text style={styles.fabBadgeText}>{cartItems.length}</Text>
              </Animated.View>
            ) : null}
            <FAB
              icon="cart-outline"
              onPress={() => setCartOpen(true)}
              style={[
                styles.fab,
                cartItems.length > 0 ? styles.fabActive : styles.fabIdle,
              ]}
              color={colors.text}
              customSize={fabSize}
            />
          </View>
        </View>
      ) : null}

      <View
        onLayout={(event) => {
          const nextHeight = Math.round(event.nativeEvent.layout.height);
          if (nextHeight > 0 && nextHeight !== bottomBarHeight) {
            setBottomBarHeight(nextHeight);
          }
        }}
      >
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
  label: {
    color: colors.muted,
    fontSize: 12,
    fontWeight: "700",
    marginBottom: 6,
    marginTop: 2,
  },
  input: {
    backgroundColor: "rgba(22,35,61,.45)",
    borderRadius: 12,
    overflow: "hidden",
    marginBottom: 10,
  },
  cartModalShell: { justifyContent: "center" },
  cartModalContainer: {
    marginHorizontal: 22,
    marginTop: 16,
    marginBottom: 16,
    alignSelf: "stretch",
    maxWidth: 560,
    maxHeight: "84%",
  },
  cartModalCard: {
    borderColor: "rgba(56,189,248,.32)",
    borderWidth: 1.25,
    backgroundColor: "#09162B",
    shadowColor: "#000",
    shadowOpacity: 0.28,
    shadowRadius: 18,
    shadowOffset: { width: 0, height: 10 },
    elevation: 14,
  },
  cartModalHeader: {
    flexDirection: "row",
    alignItems: "flex-start",
    gap: 8,
    marginBottom: 6,
  },
  cartModalHeaderActions: {
    flexDirection: "row",
    alignItems: "center",
    gap: 4,
    marginTop: -4,
    marginRight: -6,
  },
  cartHeaderIconButton: {
    margin: 0,
    width: 38,
    height: 38,
    borderWidth: 1,
    borderColor: colors.line,
    borderRadius: 10,
    backgroundColor: "rgba(22,35,61,.28)",
  },
  cartHeaderIconDanger: {
    borderColor: "rgba(251,113,133,.25)",
    backgroundColor: "rgba(127,29,29,.10)",
  },
  cartModalScroll: { maxHeight: 460, marginBottom: 10 },
  cartInfoBox: {
    borderWidth: 1,
    borderColor: colors.line,
    borderRadius: 12,
    backgroundColor: "rgba(22,35,61,.35)",
    paddingHorizontal: 12,
    paddingVertical: 10,
    marginBottom: 10,
  },
  cartInfoValue: {
    color: colors.text,
    fontSize: 13,
    fontWeight: "700",
  },
  cartInfoHint: {
    color: colors.muted,
    fontSize: 11,
    marginTop: 4,
  },
  cartRow: {
    paddingVertical: 10,
    borderBottomWidth: StyleSheet.hairlineWidth,
    borderBottomColor: colors.line,
    gap: 8,
  },
  cartRowActions: {
    flexDirection: "row",
    alignItems: "center",
    justifyContent: "space-between",
    gap: 10,
  },
  qtyStepper: {
    flex: 1,
    minWidth: 0,
    flexDirection: "row",
    alignItems: "center",
    borderWidth: 1,
    borderColor: colors.line,
    borderRadius: 12,
    backgroundColor: "rgba(22,35,61,.30)",
    paddingHorizontal: 4,
    minHeight: 40,
  },
  qtyIconButton: {
    margin: 0,
    width: 32,
    height: 32,
  },
  qtyValueBox: {
    flex: 1,
    alignItems: "center",
    justifyContent: "center",
  },
  qtyValueText: {
    color: colors.text,
    fontSize: 13,
    fontWeight: "800",
  },
  cartRemoveButton: {
    margin: 0,
    width: 36,
    height: 36,
    borderWidth: 1,
    borderColor: "rgba(251,113,133,.25)",
    borderRadius: 10,
    backgroundColor: "rgba(127,29,29,.12)",
  },
  cartTitle: { color: colors.text, fontWeight: "700", fontSize: 13 },
  cartSubtitle: { color: colors.muted, fontSize: 12, marginTop: 2 },
  cartSummary: {
    flexDirection: "row",
    gap: 8,
    marginTop: 6,
    marginBottom: 10,
    flexWrap: "wrap",
  },
  cartFooter: {
    borderTopWidth: 1,
    borderTopColor: colors.line,
    paddingTop: 10,
    backgroundColor: "rgba(10,19,36,.72)",
    borderRadius: 12,
  },
  cartFooterSectionTitle: {
    color: colors.muted,
    fontSize: 11,
    fontWeight: "700",
    textTransform: "uppercase",
    letterSpacing: 0.5,
  },
  cartFooterHint: {
    color: colors.muted,
    fontSize: 11,
    lineHeight: 16,
    marginBottom: 8,
  },
  multilineInput: { minHeight: 94 },
  multilineInputContent: { textAlignVertical: "top", paddingTop: 12 },
  fabLayer: {
    ...StyleSheet.absoluteFillObject,
    justifyContent: "flex-end",
    alignItems: "flex-end",
  },
  fabWrap: {
    alignItems: "flex-end",
    justifyContent: "flex-end",
  },
  fab: {
    width: "100%",
    height: "100%",
    borderRadius: 999,
    backgroundColor: "rgba(16,26,46,.96)",
    borderWidth: 1,
  },
  fabIdle: {
    borderColor: "rgba(56,189,248,.22)",
  },
  fabActive: {
    borderColor: "rgba(56,189,248,.42)",
    shadowColor: colors.accent,
    shadowOpacity: 0.22,
    shadowRadius: 10,
    shadowOffset: { width: 0, height: 4 },
    elevation: 7,
  },
  fabBadge: {
    position: "absolute",
    top: -4,
    right: -4,
    minWidth: 22,
    height: 22,
    borderRadius: 11,
    backgroundColor: colors.accent,
    alignItems: "center",
    justifyContent: "center",
    zIndex: 2,
    paddingHorizontal: 5,
  },
  fabBadgeActive: {
    shadowColor: colors.accent,
    shadowOpacity: 0.35,
    shadowRadius: 8,
    shadowOffset: { width: 0, height: 2 },
    elevation: 8,
  },
  fabBadgeText: {
    color: "#031321",
    fontSize: 11,
    fontWeight: "900",
  },
  confirmDialog: {
    backgroundColor: "#09162B",
    borderWidth: 1,
    borderColor: "rgba(56,189,248,.24)",
    borderRadius: 18,
    marginHorizontal: 24,
    alignSelf: "center",
    width: "88%",
    maxWidth: 420,
    overflow: "hidden",
  },
  confirmDialogTitle: {
    color: colors.text,
    fontSize: 18,
    fontWeight: "800",
    paddingTop: 14,
    paddingLeft: 0,
    marginLeft: 0,
    flex: 1,
  },
  confirmDialogTitleRow: {
    flexDirection: "row",
    alignItems: "center",
    paddingRight: 18,
  },
  confirmDialogIconWrap: {
    marginLeft: 18,
    marginTop: 10,
    marginRight: 4,
    width: 28,
    height: 28,
    borderRadius: 14,
    borderWidth: 1,
    borderColor: "rgba(251,113,133,.28)",
    backgroundColor: "rgba(127,29,29,.12)",
    alignItems: "center",
    justifyContent: "center",
  },
  confirmDialogIcon: {
    margin: 0,
    width: 24,
    height: 24,
  },
  confirmDialogContent: {
    paddingBottom: 6,
  },
  confirmDialogText: {
    color: colors.muted,
    fontSize: 13,
    lineHeight: 19,
  },
  confirmDialogActions: {
    paddingHorizontal: 18,
    paddingBottom: 16,
    paddingTop: 2,
  },
  confirmDialogButtonRow: {
    width: "100%",
    flexDirection: "row",
    gap: 10,
  },
  confirmDialogButton: {
    flex: 1,
    borderRadius: 12,
  },
  confirmDialogCancelButton: {
    borderColor: colors.line,
    backgroundColor: "rgba(22,35,61,.18)",
  },
  confirmDialogCancelLabel: {
    color: colors.text,
    fontWeight: "700",
  },
  confirmDialogDeleteButton: {
    backgroundColor: "rgba(127,29,29,.28)",
    borderColor: "rgba(251,113,133,.30)",
    borderWidth: 1,
  },
  confirmDialogDeleteLabel: {
    color: "#FECACA",
    fontWeight: "800",
  },
});
