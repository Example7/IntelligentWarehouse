# IntelligentWarehouse

Zintegrowany System Informatyczny (ZSI) dla obszaru magazynowego / WMS z modułami CMS, raportowaniem oraz wydrukami szablonowymi.

## Architektura (skrót)

- `IntranetWeb` - panel wewnętrzny (pracownicy: admin / magazynier / operator), autoryzacja `cookie`
- `MobileApi` - API dla aplikacji mobilnej (docelowo frontend React Native dla klientów), autoryzacja `JWT`
- `Data` - model danych + EF Core + SQL Server
- `Services` - logika biznesowa (współdzielona)
- `Interfaces` - kontrakty / DTO

## Dodatkowe biblioteki (aktualny stan)

### MobileApp (React Native / Expo)

- `react-native-paper` - komponenty UI (Material Design)
- `react-native-safe-area-context` - obsługa bezpiecznych obszarów ekranu
- `@react-native-async-storage/async-storage` - lokalne przechowywanie sesji/konfiguracji
- `react-native-vector-icons` - ikony UI
- `expo-linear-gradient`, `expo-asset`, `expo-status-bar` - elementy UI i zasoby w Expo

### IntranetWeb / backend współdzielony

- `Microsoft.EntityFrameworkCore.Tools` / `Design` - migracje i narzędzia EF Core
- `DocumentFormat.OpenXml` - obsługa szablonów i generowania dokumentów (np. DOCX)
- `QuestPDF` - generowanie PDF (np. raporty/wydruki)
- `ClosedXML` - generowanie plików Excel (XLSX)

### Joby i zadania cykliczne

- Joby są realizowane przez wbudowane `HostedService` (`BackgroundService`) w `IntranetWeb`
- Aktualnie brak zewnętrznego schedulera typu Hangfire/Quartz

### Szablony i załączniki

- Szablony wydruków: logika oparta o `DocumentFormat.OpenXml` (+ renderowanie prostych szablonów tekstowych/HTML)
- Załączniki: upload i walidacja realizowane natywnie w ASP.NET Core (`IFormFile`), bez dodatkowej dedykowanej biblioteki

## Uwierzytelnianie i autoryzacja

### IntranetWeb

- Logowanie formularzem (`Konto/Login`)
- Sesja oparta o `Cookie Authentication`
- Role użytkownika pobierane z tabel: `Users`, `Roles`, `UserRoles`
- Uprawnienia realizowane przez `RBAC` (`[Authorize(Roles = ...)]`)

### MobileApi

- Logowanie przez endpoint API (`/api/auth/login`)
- Autoryzacja tokenem `JWT`
- Role użytkownika dodawane do claims tokena

## Role (RBAC) - IntranetWeb

W systemie używane są role:

- `Admin`
- `Magazynier`
- `Operator`

Stałe ról:

- `IntranetWeb/Security/AppRoles.cs`

## Macierz dostępu (aktualny stan)

### Admin (`Admin`)

Pełny dostęp do systemu, w tym:

- bezpieczeństwo: `Użytkownicy`, `Role`, `Role użytkowników`
- ustawienia systemowe i log audytu
- CMS (`Strony`, `Aktualności`, `Media`, `Załączniki`, `Szablony wydruków`)
- dokumenty WMS (`PZ`, `WZ`, `MM`) i pozycje
- stany / ruchy / inwentaryzacje / rezerwacje
- raporty
- dane podstawowe i master data

Dodatkowo tylko `Admin` może wykonywać część operacji destrukcyjnych (`Delete`) w kluczowych modułach.

### Magazynier (`Magazynier`)

Dostęp do operacyjnych modułów magazynowych:

- `WZ`, `MM` (+ pozycje)
- stany magazynowe i ruchy magazynowe
- inwentaryzacje (+ pozycje)
- rezerwacje (+ pozycje)
- alerty i reguły alertów
- raporty
- `PZ` (+ pozycje)
- wybrane dane podstawowe / master data (produkty, magazyny, lokacje, kontrahenci itd.)

Brak dostępu do:

- modułów bezpieczeństwa (`Użytkownicy`, `Role`, `Role użytkowników`)
- logu audytu
- ustawień aplikacji
- CMS / szablonów wydruków (panel administracyjny CMS)

### Operator (`Operator`)

Dostęp do modułów biurowo-operacyjnych / podstawowych:

- `PZ` (+ pozycje)
- raporty
- dane podstawowe / master data:
  - produkty
  - kategorie
  - jednostki miary
  - magazyny
  - lokacje
  - klienci / dostawcy
  - kody produktu / jednostki produktu / partie

Brak dostępu do:

- `WZ`, `MM`
- stany i ruchy magazynowe
- inwentaryzacje
- rezerwacje
- alerty / reguły alertów
- bezpieczeństwo
- CMS
- ustawienia / log audytu

## Ograniczenia akcji destrukcyjnych (`Delete`)

Obecnie `Delete` jest ograniczone do roli `Admin` m.in. dla:

- dokumentów `PZ`, `WZ`, `MM`
- `Produkt`
- `Magazyn`
- `Lokacja`
- `Klient`
- `Dostawca`
- `Kategoria`
- `JednostkaMiary`
- `KodProduktu`
- `Partia`
- `ProduktJednostka`

## Proces klienta: Rezerwacja -> WZ (założenie biznesowe)

### Różnica między `Rezerwacją` a `WZ`

- `Rezerwacja` - zgłoszenie zapotrzebowania klienta / blokada towaru do obsługi przez firmę
- `WZ` (Wydanie Zewnętrzne) - dokument realizacji wydania towaru z magazynu

W praktyce:

- klient składa `Rezerwację` w kanale mobilnym
- pracownik firmy (magazyn / operator) weryfikuje i realizuje proces
- po akceptacji i realizacji powstaje dokument `WZ`, widoczny dla klienta

### Założenia projektowe (aktualne)

- `1 rezerwacja = 1 magazyn`
- uproszczenie projektowe: `1 rezerwacja -> 1 WZ`
- możliwa przyszła rozbudowa: częściowa realizacja (`1 rezerwacja -> wiele WZ`)

### Docelowy przepływ statusów (biznesowy)

#### Rezerwacja

1. `Draft` (`Robocza`) - utworzona przez klienta, oczekuje na obsługę
2. `Accepted` / `Confirmed` (`Zaakceptowana`) - potwierdzona do realizacji
3. `ConvertedToWz` / `Realized` (`Zrealizowana (WZ utworzone)`) - na podstawie rezerwacji utworzono WZ
4. `Rejected` (`Odrzucona`) - odrzucona przez operatora
5. `Cancelled` (`Anulowana`) - anulowana przed realizacją
6. `Expired` (`Przeterminowana`) - minął termin ważności rezerwacji

#### WZ

1. `Draft` (`Robocze`) - dokument utworzony, jeszcze niezamknięty
2. `Issued` (`Wydane`) - wydanie zrealizowane operacyjnie
3. `Posted` (`Zaksięgowane`) - dokument końcowo zatwierdzony / zaksięgowany
4. `Cancelled` (`Anulowane`, opcjonalnie) - anulacja zgodnie z procedurą

### Uwaga implementacyjna (aktualny stan)

Aktualna implementacja usług magazynowych używa technicznych statusów m.in.:

- rezerwacje: `Draft`, `Active`, `Released`, `Cancelled` (oraz statusów pochodnych zależnie od modułu)
- dokumenty WZ: `Draft`, `Posted` (z możliwością rozbudowy o `Issued`)

W warstwie UI (MobileApp) statusy mogą być prezentowane klientowi jako etykiety biznesowe w języku polskim, niezależnie od technicznych wartości przechowywanych w bazie.

### Aktualnie zaimplementowany obieg E2E (`MobileApp -> IntranetWeb -> WZ`)

Poniższy scenariusz jest aktualnie zaimplementowany i możliwy do demonstracji:

1. `MobileApp` (klient)

- klient loguje się do aplikacji mobilnej
- wybiera magazyn
- wyszukuje produkty
- dodaje pozycje do koszyka rezerwacji
- wysyła rezerwację (tworzy się `Rezerwacja` w statusie `Draft`)
- API wykonuje automatyczną próbę aktywacji (`Draft -> Active`)
- jeśli aktywacja nie powiedzie się (np. brak dostępności przy weryfikacji), rezerwacja pozostaje `Draft`

2. `IntranetWeb` (magazynier / operator)

- otwiera szczegóły rezerwacji
- uruchamia akcję `Aktywuj` (`Draft -> Active`) jako fallback dla rezerwacji niepotwierdzonych automatycznie
- system waliduje dostępność stanów magazynowych (ilości / lokacje)
- w przypadku braku dostępności aktywacja jest blokowana komunikatem błędu

3. `IntranetWeb` (konwersja rezerwacji do WZ)

- z poziomu `Rezerwacja/Details` dostępna jest akcja `Utworz WZ` (dla aktywnej rezerwacji)
- system automatycznie tworzy dokument `WZ` w statusie `Draft`
- system automatycznie kopiuje pozycje rezerwacji do pozycji `WZ`
- rezerwacja przechodzi do statusu `Released` (zwolniona po utworzeniu WZ)

4. Automatyczne przypisywanie lokacji przy tworzeniu `WZ`

- jeśli pozycja rezerwacji ma wskazaną lokację:
  - ta lokacja jest przenoszona do pozycji `WZ` (po weryfikacji stanu)
- jeśli pozycja rezerwacji nie ma lokacji:
  - system próbuje dobrać lokację automatycznie na podstawie stanów magazynowych
  - jeżeli jedna lokacja nie wystarcza, system może rozdzielić pozycję na wiele pozycji `WZ` (split po lokacjach)
  - przykład: rezerwacja `4 szt.`, lokacje `2 + 3` -> `WZ` dostanie pozycje `2` i `2` w dwóch lokacjach
- jeśli łączny stan magazynu nie wystarcza:
  - `WZ` nie zostanie utworzone (komunikat błędu)

5. `IntranetWeb` (księgowanie WZ)

- operator otwiera nowo utworzone `WZ`
- uruchamia akcję `Zaksieguj`
- system waliduje pozycje `WZ` (m.in. ilości > 0, poprawne lokacje, zgodność lokacji z magazynem dokumentu, wystarczający stan)
- po sukcesie dokument `WZ` przechodzi do statusu `Posted`

6. `MobileApp` (klient)

- klient widzi dokument `WZ` na liście i w szczegółach w kanale mobilnym
- statusy są prezentowane w etykietach biznesowych (PL), niezależnie od technicznych wartości statusów

### Ograniczenia / założenia obecnej implementacji obiegu

- `1 rezerwacja = 1 magazyn`
- aktualny przepływ operacyjny zakłada `1 rezerwacja -> 1 dokument WZ`, ale pozycje dokumentu `WZ` mogą zostać rozbite na wiele lokacji
- powiązanie źródła rezerwacji do `WZ` jest obecnie utrzymywane operacyjnie (akcja systemowa + informacja w notatce), bez osobnego pola relacyjnego `ReservationId` w tabeli `WZ`

### Wielokanałowa obsługa procesu WZ (aktualny stan)

System wspiera obecnie dwa kanały wejścia do procesu wydania (`WZ`):

1. Kanał klienta (`MobileApp`) - przez rezerwację

- klient składa rezerwację w aplikacji mobilnej
- system próbuje automatycznie potwierdzić rezerwację (auto-activate)
- jeśli auto-aktywacja się nie powiedzie, rezerwacja jest obsługiwana przez pracownika w `IntranetWeb`
- na podstawie rezerwacji tworzony jest dokument `WZ`

2. Kanał wewnętrzny (`IntranetWeb`) - bezpośrednie utworzenie `WZ`

- operator / magazynier / administrator może utworzyć dokument `WZ` ręcznie
- scenariusz używany np. dla zgłoszeń telefonicznych, mailowych lub zamówień przyjętych poza aplikacją mobilną

Oba kanały kończą się w tym samym modelu dokumentu `WZ` oraz w tej samej logice księgowania i ruchów magazynowych.

## Uwagi projektowe

- Ukrywanie linków w menu (`sidebar`) zależnie od roli jest elementem UX.
- Wymuszenie uprawnień realizowane jest po stronie backendu (`[Authorize]`) i to stanowi właściwe zabezpieczenie.
- Komunikat `Brak dostępu` po wejściu bez uprawnień jest zachowaniem oczekiwanym.

## Ceny (aktualny zakres)

W bieżącym zakresie projektu system **nie posiada centralnego modułu cennika** (brak osobnej encji/ekranu do zarządzania cenami produktów).

Aktualnie ceny są traktowane jako **dane dokumentowe**:

- cena jest podawana na poziomie pozycji dokumentu PZ (`PozycjaPZ.CenaJednostkowa`),
- wartość pozycji i sumy dokumentu liczone są z danych zapisanych na pozycjach PZ,
- wydruki i raporty wykorzystują ceny z pozycji dokumentu.

Walidacja operacyjna:

- `CenaJednostkowa` nie może być ujemna,
- brak ceny (`null`) jest dozwolony tylko na etapie `Draft`; przed ksiegowaniem PZ wszystkie pozycje musza miec uzupelniona cene jednostkowa.
