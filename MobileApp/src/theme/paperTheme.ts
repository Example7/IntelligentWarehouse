import { MD3DarkTheme } from "react-native-paper";
import { colors } from "../components/ui";

export const paperTheme = {
  ...MD3DarkTheme,
  colors: {
    ...MD3DarkTheme.colors,
    primary: colors.accent,
    secondary: colors.teal,
    surface: colors.card,
    surfaceVariant: colors.cardAlt,
    outline: colors.line,
    onSurface: colors.text,
    onSurfaceVariant: colors.muted,
    background: colors.bg
  },
  roundness: 14
};
