export function formatDateTime(value?: string | null): string {
  if (!value) {
    return "-";
  }

  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  return new Intl.DateTimeFormat("pl-PL", {
    dateStyle: "short",
    timeStyle: "short",
  }).format(date);
}

export function formatNumber(
  value?: number | null,
  fractionDigits = 2,
): string {
  if (value == null) {
    return "-";
  }

  return new Intl.NumberFormat("pl-PL", {
    minimumFractionDigits: fractionDigits,
    maximumFractionDigits: fractionDigits,
  }).format(value);
}

type StatusKind = "generic" | "reservation" | "wz";

function normalizeStatus(status?: string | null): string {
  return (status ?? "").trim().toLowerCase();
}

export function statusLabel(
  status?: string | null,
  kind: StatusKind = "generic",
): string {
  const normalized = normalizeStatus(status);

  if (!normalized) {
    return "-";
  }

  const genericLabels: Record<string, string> = {
    draft: "Roboczy",
    posted: "Zaksięgowane",
    closed: "Zamknięte",
    cancelled: "Anulowane",
    canceled: "Anulowane",
    rejected: "Odrzucone",
    expired: "Przeterminowane",
    accepted: "Zaakceptowane",
    confirmed: "Potwierdzone",
    active: "Aktywne",
    released: "Zwolnione",
    completed: "Zrealizowane",
    convertedtowz: "Zrealizowane (WZ)",
    realized: "Zrealizowane",
    issued: "Wydane",
  };

  const reservationLabels: Record<string, string> = {
    draft: "Robocza",
    accepted: "Zaakceptowana",
    confirmed: "Zaakceptowana",
    active: "Aktywna",
    convertedtowz: "Zrealizowana (WZ)",
    realized: "Zrealizowana",
    completed: "Zrealizowana",
    rejected: "Odrzucona",
    cancelled: "Anulowana",
    canceled: "Anulowana",
    expired: "Przeterminowana",
    released: "Zrealizowana",
  };

  const wzLabels: Record<string, string> = {
    draft: "Robocze",
    issued: "Wydane",
    posted: "Zaksięgowane",
    cancelled: "Anulowane",
    canceled: "Anulowane",
  };

  const map =
    kind === "reservation"
      ? reservationLabels
      : kind === "wz"
        ? wzLabels
        : genericLabels;
  return map[normalized] ?? status!;
}

export function statusTone(
  status?: string | null,
): "good" | "warn" | "neutral" {
  const normalized = normalizeStatus(status);
  if (
    normalized === "posted" ||
    normalized === "closed" ||
    normalized === "completed" ||
    normalized === "convertedtowz" ||
    normalized === "realized" ||
    normalized === "issued" ||
    normalized === "accepted" ||
    normalized === "confirmed" ||
    normalized === "active"
  ) {
    return "good";
  }
  if (
    normalized === "cancelled" ||
    normalized === "canceled" ||
    normalized === "draft" ||
    normalized === "rejected" ||
    normalized === "expired"
  ) {
    return "warn";
  }
  if (normalized === "released") {
    return "good";
  }
  return "neutral";
}
