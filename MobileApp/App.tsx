import * as React from "react";
import { useEffect, useState } from "react";
import {
  KeyboardAvoidingView,
  Platform,
  Pressable,
  SafeAreaView,
  ScrollView,
  StatusBar,
  StyleSheet,
  Text,
  TextInput,
  View
} from "react-native";
import { StatusBar as ExpoStatusBar } from "expo-status-bar";

import { ApiError, mobileApi } from "./src/lib/api";
import { formatDateTime, formatNumber, statusTone } from "./src/lib/format";
import { clearSessionStorage, getStoredJson, setStoredJson, STORAGE_KEYS } from "./src/lib/storage";
import {
  ActionButton,
  Card,
  colors,
  DataRow,
  EmptyBlock,
  ErrorBlock,
  InlineRow,
  ListItem,
  LoadingBlock,
  MetricCard,
  Pill,
  SectionTitle
} from "./src/components/ui";
import type {
  ClientDashboardDto,
  ClientNotificationDto,
  ClientOrderDetailsDto,
  ClientOrderListItemDto,
  ClientProfileDto,
  ClientReservationDetailsDto,
  ClientReservationListItemDto,
  CurrentUserDto,
  LoginResponseDto,
  MobileNewsItemDto,
  MobilePageDetailsDto,
  MobilePageListItemDto
} from "./src/types";

type Session = LoginResponseDto;
type TabKey = "start" | "wz" | "rezerwacje" | "alerty" | "profil" | "cms";

const DEFAULT_API_BASE_URL =
  process.env.EXPO_PUBLIC_API_BASE_URL ?? (Platform.OS === "android" ? "http://10.0.2.2:5095" : "http://localhost:5095");

export default function App() {
  const [booting, setBooting] = useState(true);
  const [session, setSession] = useState<Session | null>(null);
  const [apiBaseUrl, setApiBaseUrl] = useState(DEFAULT_API_BASE_URL);

  useEffect(() => {
    (async () => {
      const [savedSession, savedApiBaseUrl] = await Promise.all([
        getStoredJson<Session>(STORAGE_KEYS.session),
        getStoredJson<string>(STORAGE_KEYS.apiBaseUrl)
      ]);
      if (savedSession?.accessToken) {
        setSession(savedSession);
      }
      if (savedApiBaseUrl?.trim()) {
        setApiBaseUrl(savedApiBaseUrl);
      }
      setBooting(false);
    })();
  }, []);

  async function handleLogin(nextSession: Session, nextApiBaseUrl: string) {
    setSession(nextSession);
    setApiBaseUrl(nextApiBaseUrl);
    await Promise.all([
      setStoredJson(STORAGE_KEYS.session, nextSession),
      setStoredJson(STORAGE_KEYS.apiBaseUrl, nextApiBaseUrl)
    ]);
  }

  async function handleLogout() {
    setSession(null);
    await clearSessionStorage();
  }

  return (
    <Frame>
      {booting ? (
        <LoadingBlock label="Uruchamianie aplikacji..." />
      ) : session ? (
        <ClientShell session={session} apiBaseUrl={apiBaseUrl} onLogout={handleLogout} />
      ) : (
        <LoginScreen defaultApiBaseUrl={apiBaseUrl} onLogin={handleLogin} />
      )}
    </Frame>
  );
}

function Frame({ children }: React.PropsWithChildren) {
  return (
    <SafeAreaView style={styles.safe}>
      <ExpoStatusBar style="light" />
      <StatusBar barStyle="light-content" />
      <View style={styles.glowA} />
      <View style={styles.glowB} />
      <View style={styles.root}>{children}</View>
    </SafeAreaView>
  );
}

function LoginScreen({
  defaultApiBaseUrl,
  onLogin
}: {
  defaultApiBaseUrl: string;
  onLogin: (session: Session, apiBaseUrl: string) => Promise<void>;
}) {
  const [apiBaseUrl, setApiBaseUrl] = useState(defaultApiBaseUrl);
  const [loginOrEmail, setLoginOrEmail] = useState("");
  const [password, setPassword] = useState("");
  const [busy, setBusy] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [news, setNews] = useState<MobileNewsItemDto[] | null>(null);

  useEffect(() => {
    let cancelled = false;
    mobileApi
      .getNews(apiBaseUrl)
      .then((items) => {
        if (!cancelled) {
          setNews(items.slice(0, 3));
        }
      })
      .catch(() => {
        if (!cancelled) {
          setNews([]);
        }
      });
    return () => {
      cancelled = true;
    };
  }, [apiBaseUrl]);

  async function submit() {
    setBusy(true);
    setError(null);
    try {
      const response = await mobileApi.login(apiBaseUrl.trim(), loginOrEmail.trim(), password);
      await onLogin(response, apiBaseUrl.trim());
    } catch (e) {
      setError(e instanceof Error ? e.message : "Nie udało się zalogować.");
    } finally {
      setBusy(false);
    }
  }

  return (
    <KeyboardAvoidingView style={{ flex: 1 }} behavior={Platform.OS === "ios" ? "padding" : undefined}>
      <ScrollView contentContainerStyle={styles.scrollContent}>
        <View style={styles.hero}>
          <Text style={styles.kicker}>Intelligent Warehouse</Text>
          <Text style={styles.title}>Aplikacja mobilna klienta</Text>
          <Text style={styles.subtitle}>
            Frontend React Native (Expo) dla `MobileApi`: logowanie JWT, zamówienia WZ, rezerwacje, profil i treści CMS.
          </Text>
        </View>

        <Card>
          <SectionTitle title="Logowanie" subtitle="api/auth/login" />
          {error ? <ErrorBlock message={error} /> : null}

          <Field label="Adres API (MobileApi)">
            <TextInput
              style={styles.input}
              value={apiBaseUrl}
              onChangeText={setApiBaseUrl}
              autoCapitalize="none"
              autoCorrect={false}
              placeholder="http://localhost:5095"
              placeholderTextColor={colors.muted}
            />
          </Field>

          <Field label="Login lub email">
            <TextInput
              style={styles.input}
              value={loginOrEmail}
              onChangeText={setLoginOrEmail}
              autoCapitalize="none"
              autoCorrect={false}
              placeholder="np. klient@demo.local"
              placeholderTextColor={colors.muted}
            />
          </Field>

          <Field label="Hasło">
            <TextInput
              style={styles.input}
              value={password}
              onChangeText={setPassword}
              secureTextEntry
              autoCapitalize="none"
              autoCorrect={false}
              placeholder="Hasło"
              placeholderTextColor={colors.muted}
            />
          </Field>

          <ActionButton label={busy ? "Logowanie..." : "Zaloguj"} onPress={submit} disabled={busy} />
          <Text style={styles.hint}>
            Uwaga: endpointy klienta wymagają roli `Client` i powiązanego rekordu `Klient` (`IdUzytkownika`) w bazie.
          </Text>
        </Card>

        <Card>
          <SectionTitle title="Aktualności (publiczne)" subtitle="api/mobile/content/news" />
          {news === null ? (
            <LoadingBlock label="Pobieranie..." />
          ) : news.length === 0 ? (
            <EmptyBlock title="Brak aktualności" subtitle="Lub API jest nieosiągalne pod podanym adresem." />
          ) : (
            news.map((item) => <ListItem key={item.id} title={item.title} subtitle={item.content.slice(0, 120)} />)
          )}
        </Card>
      </ScrollView>
    </KeyboardAvoidingView>
  );
}

function ClientShell({
  session,
  apiBaseUrl,
  onLogout
}: {
  session: Session;
  apiBaseUrl: string;
  onLogout: () => Promise<void>;
}) {
  const [tab, setTab] = useState<TabKey>("start");

  const hasClientRole = session.roles.some((r) => r.toLowerCase() === "client");
  if (!hasClientRole) {
    return (
      <ScrollView contentContainerStyle={styles.scrollContent}>
        <Card>
          <SectionTitle title="Konto bez roli Client" subtitle="Logowanie poprawne, ale brak autoryzacji kanału klienta." />
          <DataRow label="Login" value={session.login} />
          <DataRow label="Email" value={session.email} />
          <DataRow label="Role" value={session.roles.join(", ")} />
          <Text style={styles.hint}>
            `MobileApi` używa `[Authorize(Roles = "Client")]`. Dodaj rolę `Client` oraz rekord klienta powiązany z użytkownikiem.
          </Text>
          <View style={{ marginTop: 12 }}>
            <ActionButton label="Wyloguj" onPress={() => void onLogout()} variant="secondary" />
          </View>
        </Card>
      </ScrollView>
    );
  }

  return (
    <>
      <View style={styles.header}>
        <View style={{ flex: 1 }}>
          <Text style={styles.kicker}>Intelligent Warehouse</Text>
          <Text style={styles.headerTitle}>Strefa klienta</Text>
          <Text style={styles.headerSub}>{session.email || session.login}</Text>
        </View>
        <Pill label="Client" tone="good" />
      </View>

      <ScrollView horizontal showsHorizontalScrollIndicator={false} contentContainerStyle={styles.tabBar}>
        {[
          ["start", "Start"],
          ["wz", "WZ"],
          ["rezerwacje", "Rezerwacje"],
          ["alerty", "Alerty"],
          ["profil", "Profil"],
          ["cms", "CMS"]
        ].map(([key, label]) => (
          <Pressable key={key} onPress={() => setTab(key as TabKey)} style={[styles.tabChip, tab === key ? styles.tabChipActive : null]}>
            <Text style={[styles.tabChipText, tab === key ? styles.tabChipTextActive : null]}>{label}</Text>
          </Pressable>
        ))}
      </ScrollView>

      <ScrollView contentContainerStyle={styles.scrollContent}>
        {tab === "start" ? <DashboardScreen apiBaseUrl={apiBaseUrl} token={session.accessToken} /> : null}
        {tab === "wz" ? <OrdersScreen apiBaseUrl={apiBaseUrl} token={session.accessToken} /> : null}
        {tab === "rezerwacje" ? <ReservationsScreen apiBaseUrl={apiBaseUrl} token={session.accessToken} /> : null}
        {tab === "alerty" ? <NotificationsScreen apiBaseUrl={apiBaseUrl} token={session.accessToken} /> : null}
        {tab === "profil" ? <ProfileScreen apiBaseUrl={apiBaseUrl} token={session.accessToken} session={session} /> : null}
        {tab === "cms" ? <CmsScreen apiBaseUrl={apiBaseUrl} /> : null}

        <Card>
          <SectionTitle title="Sesja" subtitle="Diagnostyka frontu mobilnego" />
          <DataRow label="API" value={apiBaseUrl} />
          <DataRow label="Token wygasa" value={formatDateTime(session.expiresAtUtc)} />
          <ActionButton label="Wyloguj" onPress={() => void onLogout()} variant="secondary" />
        </Card>
      </ScrollView>
    </>
  );
}

function DashboardScreen({ apiBaseUrl, token }: { apiBaseUrl: string; token: string }) {
  const [data, setData] = useState<ClientDashboardDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setError(null);
    try {
      setData(await mobileApi.getDashboard(apiBaseUrl, token));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Nie udało się pobrać dashboardu.");
    }
  }

  useEffect(() => {
    void load();
  }, [apiBaseUrl, token]);

  if (!data && !error) {
    return <LoadingBlock label="Pobieranie dashboardu..." />;
  }
  if (!data && error) {
    return <ErrorBlock message={error} />;
  }

  return (
    <>
      <Card>
        <SectionTitle title="Dashboard klienta" subtitle="api/client/dashboard" />
        <InlineRow>
          <MetricCard label="Aktywne WZ" value={String(data?.activeOrdersCount ?? 0)} accent="amber" />
          <MetricCard label="Posted WZ" value={String(data?.postedOrdersCount ?? 0)} accent="teal" />
          <MetricCard label="Rezerwacje open" value={String(data?.openReservationsCount ?? 0)} />
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
              right={<Pill label={item.status} tone={statusTone(item.status)} />}
            />
          ))
        ) : (
          <EmptyBlock title="Brak zamówień" />
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
              right={<Pill label={item.status} tone={statusTone(item.status)} />}
            />
          ))
        ) : (
          <EmptyBlock title="Brak rezerwacji" />
        )}
        <View style={{ marginTop: 10 }}>
          <ActionButton label="Odśwież" onPress={() => void load()} variant="ghost" />
        </View>
      </Card>
    </>
  );
}

function OrdersScreen({ apiBaseUrl, token }: { apiBaseUrl: string; token: string }) {
  const [items, setItems] = useState<ClientOrderListItemDto[] | null>(null);
  const [details, setDetails] = useState<ClientOrderDetailsDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function loadList() {
    setError(null);
    try {
      setItems(await mobileApi.getOrders(apiBaseUrl, token));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Nie udało się pobrać zamówień.");
    }
  }

  async function showDetails(id: number) {
    setError(null);
    try {
      setDetails(await mobileApi.getOrderDetails(apiBaseUrl, token, id));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Nie udało się pobrać szczegółów WZ.");
    }
  }

  useEffect(() => {
    void loadList();
  }, [apiBaseUrl, token]);

  return (
    <>
      <Card>
        <SectionTitle title="Dokumenty WZ klienta" subtitle="api/client/orders" />
        {!items && !error ? (
          <LoadingBlock label="Pobieranie WZ..." />
        ) : error && !items ? (
          <ErrorBlock message={error} />
        ) : items && items.length === 0 ? (
          <EmptyBlock title="Brak dokumentów WZ" />
        ) : (
          items?.map((item) => (
            <ListItem
              key={item.orderId}
              title={item.number}
              subtitle={`${item.warehouseName} • ${formatDateTime(item.issuedAtUtc)} • ${item.itemsCount} poz.`}
              right={
                <View style={{ alignItems: "flex-end" }}>
                  <Pill label={item.status} tone={statusTone(item.status)} />
                  <Text style={styles.metricText}>{formatNumber(item.totalQuantity)}</Text>
                </View>
              }
              onPress={() => void showDetails(item.orderId)}
            />
          ))
        )}
        <View style={{ marginTop: 10 }}>
          <ActionButton label="Odśwież listę" onPress={() => void loadList()} variant="ghost" />
        </View>
      </Card>

      {details ? (
        <Card>
          <SectionTitle title={`Szczegóły WZ: ${details.number}`} subtitle="api/client/orders/{id}" />
          <InlineRow style={{ marginBottom: 10 }}>
            <Pill label={details.status} tone={statusTone(details.status)} />
            <Pill label={`Suma: ${formatNumber(details.totalQuantity)}`} />
          </InlineRow>
          <DataRow label="Magazyn" value={details.warehouseName} />
          <DataRow label="Wydano" value={formatDateTime(details.issuedAtUtc)} />
          <DataRow label="Zaksięgowano" value={formatDateTime(details.postedAtUtc)} />
          <DataRow label="Notatka" value={details.note} />
          <Text style={styles.subsection}>Pozycje</Text>
          {details.items.map((row) => (
            <ListItem
              key={row.itemId}
              title={`${row.lineNo}. ${row.productCode} — ${row.productName}`}
              subtitle={`Ilość: ${formatNumber(row.quantity)} • Lokacja: ${row.locationCode ?? "-"}`}
            />
          ))}
        </Card>
      ) : null}
    </>
  );
}

function ReservationsScreen({ apiBaseUrl, token }: { apiBaseUrl: string; token: string }) {
  const [items, setItems] = useState<ClientReservationListItemDto[] | null>(null);
  const [details, setDetails] = useState<ClientReservationDetailsDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function loadList() {
    setError(null);
    try {
      setItems(await mobileApi.getReservations(apiBaseUrl, token));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Nie udało się pobrać rezerwacji.");
    }
  }

  async function showDetails(id: number) {
    setError(null);
    try {
      setDetails(await mobileApi.getReservationDetails(apiBaseUrl, token, id));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Nie udało się pobrać szczegółów rezerwacji.");
    }
  }

  useEffect(() => {
    void loadList();
  }, [apiBaseUrl, token]);

  return (
    <>
      <Card>
        <SectionTitle title="Rezerwacje" subtitle="api/client/reservations" />
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
              right={<Pill label={item.status} tone={statusTone(item.status)} />}
              onPress={() => void showDetails(item.reservationId)}
            />
          ))
        )}
      </Card>

      {details ? (
        <Card>
          <SectionTitle title={`Szczegóły rezerwacji: ${details.number}`} />
          <InlineRow style={{ marginBottom: 10 }}>
            <Pill label={details.status} tone={statusTone(details.status)} />
            <Pill label={`Suma: ${formatNumber(details.totalQuantity)}`} />
          </InlineRow>
          <DataRow label="Magazyn" value={details.warehouseName} />
          <DataRow label="Utworzono" value={formatDateTime(details.createdAtUtc)} />
          <DataRow label="Wygasa" value={formatDateTime(details.expiresAtUtc)} />
          <DataRow label="Notatka" value={details.note} />
          <Text style={styles.subsection}>Pozycje</Text>
          {details.items.map((row) => (
            <ListItem
              key={row.itemId}
              title={`${row.lineNo}. ${row.productCode} — ${row.productName}`}
              subtitle={`Ilość: ${formatNumber(row.quantity)} • Lokacja: ${row.locationCode ?? "-"}`}
            />
          ))}
        </Card>
      ) : null}
    </>
  );
}

function NotificationsScreen({ apiBaseUrl, token }: { apiBaseUrl: string; token: string }) {
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
      <SectionTitle title="Alerty / powiadomienia" subtitle="api/client/notifications" />
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

function ProfileScreen({
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

  if (!profile && !error) {
    return <LoadingBlock label="Pobieranie profilu..." />;
  }
  if (!profile && error) {
    return <ErrorBlock message={error} />;
  }

  return (
    <>
      <Card>
        <SectionTitle title="Profil klienta" subtitle="api/client/profile" />
        <DataRow label="Nazwa" value={profile?.name} />
        <DataRow label="Email" value={profile?.email} />
        <DataRow label="Telefon" value={profile?.phone} />
        <DataRow label="Adres" value={profile?.address} />
        <DataRow label="Status" value={profile?.isActive ? "Aktywny" : "Nieaktywny"} />
        <DataRow label="Utworzono" value={formatDateTime(profile?.createdAtUtc)} />
      </Card>
      <Card>
        <SectionTitle title="JWT / auth me" subtitle="api/auth/me" />
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

function CmsScreen({ apiBaseUrl }: { apiBaseUrl: string }) {
  const [news, setNews] = useState<MobileNewsItemDto[] | null>(null);
  const [pages, setPages] = useState<MobilePageListItemDto[] | null>(null);
  const [page, setPage] = useState<MobilePageDetailsDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  async function load() {
    setError(null);
    try {
      const [newsData, pagesData] = await Promise.all([mobileApi.getNews(apiBaseUrl), mobileApi.getPages(apiBaseUrl)]);
      setNews(newsData);
      setPages(pagesData);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Nie udało się pobrać treści CMS.");
    }
  }

  async function loadPage(slug: string) {
    try {
      setPage(await mobileApi.getPage(apiBaseUrl, slug));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Nie udało się pobrać strony CMS.");
    }
  }

  useEffect(() => {
    void load();
  }, [apiBaseUrl]);

  return (
    <>
      <Card>
        <SectionTitle title="Treści mobilne CMS" subtitle="api/mobile/content/*" />
        {!news && !pages && !error ? (
          <LoadingBlock label="Pobieranie treści..." />
        ) : error && !news && !pages ? (
          <ErrorBlock message={error} />
        ) : (
          <>
            <Text style={styles.subsection}>Aktualności</Text>
            {news?.length ? news.slice(0, 5).map((item) => <ListItem key={item.id} title={item.title} subtitle={item.content.slice(0, 120)} />) : <EmptyBlock title="Brak aktualności" />}
            <Text style={styles.subsection}>Strony</Text>
            {pages?.length ? pages.map((item) => <ListItem key={item.id} title={item.title} subtitle={`slug: ${item.slug}`} onPress={() => void loadPage(item.slug)} />) : <EmptyBlock title="Brak stron" />}
            <View style={{ marginTop: 10 }}>
              <ActionButton label="Odśwież treści" onPress={() => void load()} variant="ghost" />
            </View>
          </>
        )}
      </Card>
      {page ? (
        <Card>
          <SectionTitle title={`Strona: ${page.title}`} subtitle={page.slug} />
          <Text style={styles.cmsContent}>{page.content || "(pusta treść)"}</Text>
        </Card>
      ) : null}
    </>
  );
}

function Field({ label, children }: React.PropsWithChildren<{ label: string }>) {
  return (
    <View style={{ marginBottom: 12 }}>
      <Text style={styles.label}>{label}</Text>
      {children}
    </View>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: colors.bg },
  root: { flex: 1 },
  glowA: {
    position: "absolute",
    top: -70,
    left: -30,
    width: 220,
    height: 220,
    borderRadius: 999,
    backgroundColor: "rgba(20,184,166,.12)"
  },
  glowB: {
    position: "absolute",
    top: 30,
    right: -40,
    width: 180,
    height: 180,
    borderRadius: 999,
    backgroundColor: "rgba(56,189,248,.12)"
  },
  scrollContent: { padding: 14, gap: 12 },
  hero: {
    backgroundColor: "rgba(16,26,46,.88)",
    borderWidth: 1,
    borderColor: "rgba(56,189,248,.22)",
    borderRadius: 18,
    padding: 16
  },
  kicker: {
    color: colors.accent,
    fontSize: 11,
    fontWeight: "800",
    letterSpacing: 1.1,
    textTransform: "uppercase"
  },
  title: { color: colors.text, fontSize: 24, fontWeight: "900", marginTop: 6 },
  subtitle: { color: colors.muted, fontSize: 13, lineHeight: 19, marginTop: 8 },
  hint: { color: colors.muted, fontSize: 12, lineHeight: 17, marginTop: 10 },
  label: { color: colors.muted, fontSize: 12, fontWeight: "700", marginBottom: 6 },
  input: {
    borderWidth: 1,
    borderColor: colors.line,
    backgroundColor: "rgba(22,35,61,.65)",
    borderRadius: 12,
    color: colors.text,
    paddingHorizontal: 12,
    paddingVertical: 10
  },
  header: { paddingHorizontal: 14, paddingTop: 10, paddingBottom: 6, flexDirection: "row", alignItems: "center", gap: 12 },
  headerTitle: { color: colors.text, fontSize: 22, fontWeight: "900" },
  headerSub: { color: colors.muted, fontSize: 12 },
  tabBar: { paddingHorizontal: 14, paddingBottom: 8, gap: 8 },
  tabChip: {
    borderWidth: 1,
    borderColor: colors.line,
    backgroundColor: "rgba(16,26,46,.65)",
    borderRadius: 999,
    paddingVertical: 8,
    paddingHorizontal: 12
  },
  tabChipActive: { backgroundColor: "rgba(56,189,248,.16)", borderColor: "rgba(56,189,248,.45)" },
  tabChipText: { color: colors.muted, fontWeight: "700", fontSize: 12 },
  tabChipTextActive: { color: colors.text },
  metricText: { color: colors.muted, fontSize: 11, marginTop: 4 },
  subsection: { color: colors.text, fontWeight: "800", fontSize: 13, marginTop: 12, marginBottom: 6 },
  cmsContent: {
    color: colors.text,
    lineHeight: 20,
    borderWidth: 1,
    borderColor: colors.line,
    borderRadius: 10,
    backgroundColor: "rgba(22,35,61,.4)",
    padding: 10
  }
});
