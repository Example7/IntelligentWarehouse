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
    timeStyle: "short"
  }).format(date);
}

export function formatNumber(value?: number | null, fractionDigits = 2): string {
  if (value == null) {
    return "-";
  }

  return new Intl.NumberFormat("pl-PL", {
    minimumFractionDigits: fractionDigits,
    maximumFractionDigits: fractionDigits
  }).format(value);
}

export function statusTone(status?: string | null): "good" | "warn" | "neutral" {
  const normalized = (status ?? "").toLowerCase();
  if (normalized === "posted" || normalized === "closed") {
    return "good";
  }
  if (normalized === "cancelled" || normalized === "draft") {
    return "warn";
  }
  return "neutral";
}
