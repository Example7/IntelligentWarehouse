# MobileApp (React Native / Expo)

Frontend mobilny klienta dla `MobileApi`.

## Zakres (MVP)

- logowanie JWT (`api/auth/login`)
- dashboard klienta (`api/client/dashboard`)
- lista + szczegoly dokumentow `WZ`
- lista + szczegoly rezerwacji
- powiadomienia/alerty klienta
- profil klienta (`api/client/profile`) + `api/auth/me`
- publiczne tresci CMS (`api/mobile/content/news`, `pages`)

## Wymagania testowe po stronie backendu

- uruchomione `MobileApi`
- uzytkownik z rola `Client`
- rekord `Klient` powiazany z uzytkownikiem (`Klient.IdUzytkownika`)

## Uruchomienie

1. Wlacz `MobileApi` na porcie `5095`.
2. Przejdz do katalogu:
   - `cd MobileApp`
3. Zainstaluj zaleznosci:
   - `npm install`
4. Uruchom Expo:
   - `npm run start`

## Konfiguracja adresu API

Aplikacja domyslnie wykrywa host komputera z Expo i buduje adres:

- `http://<host-z-Expo>:5095`

Jesli wykrywanie nie zadziala, fallback jest taki:

- Android emulator: `http://10.0.2.2:5095`
- iOS simulator: `http://localhost:5095`

Mozesz wymusic adres przez `.env`:

- skopiuj `.env.example` -> `.env`
- ustaw `EXPO_PUBLIC_API_BASE_URL=auto` (rekomendowane)
- albo wpisz pelny URL, np. `http://192.168.1.28:5095`

## Uwagi

- Endpointy klienta sa chronione `[Authorize(Roles = "Klient")]`, wiec konta pracownicze (`Admin`, `Magazynier`, `Operator`) zaloguja sie, ale nie uzyskaja dostepu do strefy klienta.
- To MVP nie uzywa jeszcze biblioteki nawigacji (React Navigation) - przelaczanie ekranow jest zrobione lekko, wewnatrz jednego `App.tsx`.
