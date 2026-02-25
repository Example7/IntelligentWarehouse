# MobileApp (React Native / Expo)

Frontend mobilny klienta dla `MobileApi`.

## Zakres (MVP)
- logowanie JWT (`api/auth/login`)
- dashboard klienta (`api/client/dashboard`)
- lista + szczegóły dokumentów `WZ`
- lista + szczegóły rezerwacji
- powiadomienia/alerty klienta
- profil klienta (`api/client/profile`) + `api/auth/me`
- publiczne treści CMS (`api/mobile/content/news`, `pages`)

## Wymagania testowe po stronie backendu
- uruchomione `MobileApi`
- użytkownik z rolą `Client`
- rekord `Klient` powiązany z użytkownikiem (`Klient.IdUzytkownika`)

## Uruchomienie
1. Włącz `MobileApi` (domyślnie `http://localhost:5095`).
2. Przejdź do katalogu:
   - `cd MobileApp`
3. Zainstaluj zależności:
   - `npm install`
4. Uruchom Expo:
   - `npm run start`

## Konfiguracja adresu API
Aplikacja ma pole `Adres API (MobileApi)` na ekranie logowania.

Przykłady:
- Windows + emulator Android: `http://10.0.2.2:5095`
- iOS Simulator: `http://localhost:5095`
- telefon w tej samej sieci Wi-Fi: `http://<IP-komputera>:5095`

Możesz też ustawić domyślny adres przez `.env`:
- skopiuj `.env.example` -> `.env`
- ustaw `EXPO_PUBLIC_API_BASE_URL=...`

## Uwagi
- Endpointy klienta są chronione `[Authorize(Roles = "Client")]`, więc konta pracownicze (`Admin`, `Magazynier`, `Operator`) zalogują się, ale nie uzyskają dostępu do strefy klienta.
- To MVP nie używa jeszcze biblioteki nawigacji (React Navigation) — przełączanie ekranów jest zrobione lekko, wewnątrz jednego `App.tsx`, żeby szybciej domknąć funkcjonalny frontend pod `MobileApi`.
