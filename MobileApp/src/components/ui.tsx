import * as React from "react";
import { Pressable, StyleSheet, Text, View, type ViewStyle } from "react-native";
import {
  ActivityIndicator,
  Button as PaperButton,
  Card as PaperCard,
  Chip as PaperChip
} from "react-native-paper";

export const colors = {
  bg: "#070B14",
  card: "#101A2E",
  cardAlt: "#16233D",
  line: "#263857",
  text: "#EFF4FF",
  muted: "#9DB0D0",
  accent: "#38BDF8",
  teal: "#14B8A6",
  warn: "#F59E0B",
  danger: "#FB7185",
  success: "#22C55E"
} as const;

export function Card(props: React.PropsWithChildren<{ style?: ViewStyle }>) {
  return (
    <PaperCard mode="outlined" style={[styles.card, props.style]}>
      <PaperCard.Content style={styles.cardContent}>{props.children}</PaperCard.Content>
    </PaperCard>
  );
}

export function SectionTitle({ title, subtitle }: { title: string; subtitle?: string }) {
  return (
    <View style={styles.sectionHeader}>
      <Text style={styles.sectionTitle}>{title}</Text>
      {subtitle ? <Text style={styles.sectionSubtitle}>{subtitle}</Text> : null}
    </View>
  );
}

export function Pill({ label, tone = "neutral" }: { label: string; tone?: "neutral" | "good" | "warn" | "danger" }) {
  const toneStyles =
    tone === "good"
      ? { backgroundColor: "rgba(34,197,94,.15)", borderColor: "rgba(34,197,94,.35)", color: colors.success }
      : tone === "warn"
        ? { backgroundColor: "rgba(245,158,11,.15)", borderColor: "rgba(245,158,11,.35)", color: colors.warn }
        : tone === "danger"
          ? { backgroundColor: "rgba(251,113,133,.15)", borderColor: "rgba(251,113,133,.35)", color: colors.danger }
          : { backgroundColor: "rgba(157,176,208,.10)", borderColor: "rgba(157,176,208,.25)", color: colors.muted };

  return (
    <PaperChip
      compact
      style={[styles.pill, { backgroundColor: toneStyles.backgroundColor, borderColor: toneStyles.borderColor }]}
      textStyle={[styles.pillText, { color: toneStyles.color }]}
    >
      {label}
    </PaperChip>
  );
}

export function MetricCard({
  label,
  value,
  accent = "blue"
}: {
  label: string;
  value: string;
  accent?: "blue" | "teal" | "amber";
}) {
  const accentStyles =
    accent === "teal"
      ? { backgroundColor: "rgba(20,184,166,.12)", borderColor: "rgba(20,184,166,.30)" }
      : accent === "amber"
        ? { backgroundColor: "rgba(245,158,11,.12)", borderColor: "rgba(245,158,11,.30)" }
        : { backgroundColor: "rgba(56,189,248,.12)", borderColor: "rgba(56,189,248,.30)" };

  return (
    <View style={[styles.metricCard, accentStyles]}>
      <Text style={styles.metricLabel}>{label}</Text>
      <Text style={styles.metricValue}>{value}</Text>
    </View>
  );
}

export function ActionButton({
  label,
  onPress,
  variant = "primary",
  disabled = false
}: {
  label: string;
  onPress: () => void;
  variant?: "primary" | "secondary" | "ghost";
  disabled?: boolean;
}) {
  return (
    <PaperButton
      mode={variant === "primary" ? "contained" : variant === "secondary" ? "contained-tonal" : "outlined"}
      onPress={onPress}
      disabled={disabled}
      style={[
        styles.button,
        variant === "primary" ? styles.buttonPrimary : variant === "secondary" ? styles.buttonSecondary : styles.buttonGhost
      ]}
      labelStyle={variant === "primary" ? styles.buttonPrimaryText : styles.buttonGhostText}
    >
      {label}
    </PaperButton>
  );
}

export function InlineRow(props: React.PropsWithChildren<{ style?: ViewStyle }>) {
  return <View style={[styles.inlineRow, props.style]}>{props.children}</View>;
}

export function DataRow({ label, value }: { label: string; value?: string | null }) {
  return (
    <View style={styles.dataRow}>
      <Text style={styles.dataLabel}>{label}</Text>
      <Text style={styles.dataValue}>{value?.trim() ? value : "-"}</Text>
    </View>
  );
}

export function ListItem({
  title,
  subtitle,
  right,
  onPress
}: {
  title: string;
  subtitle?: string;
  right?: React.ReactNode;
  onPress?: () => void;
}) {
  const content = (
    <View style={styles.listItem}>
      <View style={{ flex: 1 }}>
        <Text style={styles.listTitle}>{title}</Text>
        {subtitle ? <Text style={styles.listSubtitle}>{subtitle}</Text> : null}
      </View>
      {right ? <View style={styles.listRight}>{right}</View> : null}
    </View>
  );

  if (!onPress) {
    return content;
  }

  return (
    <Pressable onPress={onPress} style={({ pressed }) => (pressed ? { opacity: 0.82 } : null)}>
      {content}
    </Pressable>
  );
}

export function LoadingBlock({ label = "Ładowanie..." }: { label?: string }) {
  return (
    <Card>
      <View style={styles.centerRow}>
        <ActivityIndicator color={colors.accent} />
        <Text style={styles.sectionSubtitle}>{label}</Text>
      </View>
    </Card>
  );
}

export function ErrorBlock({ message }: { message: string }) {
  return (
    <Card style={{ borderColor: "rgba(251,113,133,.4)", backgroundColor: "rgba(127,29,29,.20)" }}>
      <Text style={{ color: "#FECACA", fontWeight: "700" }}>Błąd</Text>
      <Text style={{ color: "#FECACA", marginTop: 6 }}>{message}</Text>
    </Card>
  );
}

export function EmptyBlock({ title, subtitle }: { title: string; subtitle?: string }) {
  return (
    <Card>
      <Text style={styles.listTitle}>{title}</Text>
      {subtitle ? <Text style={[styles.sectionSubtitle, { marginTop: 4 }]}>{subtitle}</Text> : null}
    </Card>
  );
}

const styles = StyleSheet.create({
  card: {
    backgroundColor: colors.card,
    borderColor: colors.line,
    borderWidth: 1,
    borderRadius: 16
  },
  cardContent: {
    padding: 14
  },
  sectionHeader: {
    marginBottom: 10
  },
  sectionTitle: {
    color: colors.text,
    fontSize: 17,
    fontWeight: "800"
  },
  sectionSubtitle: {
    color: colors.muted,
    fontSize: 12
  },
  pill: {
    borderWidth: 1,
    borderRadius: 999,
    minHeight: 30,
    justifyContent: "center",
    paddingVertical: 0
  },
  pillText: {
    fontSize: 11,
    fontWeight: "700",
    lineHeight: 16,
    marginVertical: 0
  },
  metricCard: {
    borderWidth: 1,
    borderRadius: 14,
    padding: 12,
    flex: 1,
    minWidth: 90
  },
  metricLabel: {
    color: colors.muted,
    fontSize: 10,
    textTransform: "uppercase",
    letterSpacing: 0.5,
    fontWeight: "700"
  },
  metricValue: {
    color: colors.text,
    fontSize: 20,
    fontWeight: "900",
    marginTop: 4
  },
  button: {
    borderRadius: 12,
    justifyContent: "center"
  },
  buttonPrimary: {
    backgroundColor: colors.accent
  },
  buttonSecondary: {
    backgroundColor: colors.cardAlt,
    borderColor: colors.line,
    borderWidth: 1
  },
  buttonGhost: {
    backgroundColor: "transparent",
    borderColor: colors.line,
    borderWidth: 1
  },
  buttonPrimaryText: {
    color: "#031321",
    fontWeight: "800"
  },
  buttonGhostText: {
    color: colors.text,
    fontWeight: "700"
  },
  inlineRow: {
    flexDirection: "row",
    gap: 10
  },
  dataRow: {
    flexDirection: "row",
    justifyContent: "space-between",
    gap: 12,
    paddingVertical: 6,
    borderBottomColor: colors.line,
    borderBottomWidth: StyleSheet.hairlineWidth
  },
  dataLabel: {
    color: colors.muted,
    fontSize: 12,
    flex: 1
  },
  dataValue: {
    color: colors.text,
    fontWeight: "600",
    fontSize: 12,
    flex: 1.4,
    textAlign: "right"
  },
  listItem: {
    flexDirection: "row",
    gap: 10,
    alignItems: "center",
    paddingVertical: 10,
    borderBottomColor: colors.line,
    borderBottomWidth: StyleSheet.hairlineWidth
  },
  listTitle: {
    color: colors.text,
    fontWeight: "700",
    fontSize: 13
  },
  listSubtitle: {
    color: colors.muted,
    fontSize: 12,
    marginTop: 2
  },
  listRight: {
    alignItems: "flex-end"
  },
  centerRow: {
    flexDirection: "row",
    alignItems: "center",
    gap: 10
  }
});
