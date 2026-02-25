import * as React from "react";
import { StatusBar, StyleSheet, View } from "react-native";
import { LinearGradient } from "expo-linear-gradient";
import { StatusBar as ExpoStatusBar } from "expo-status-bar";
import { SafeAreaView } from "react-native-safe-area-context";
import { colors } from "./ui";

export function AppFrame({ children }: React.PropsWithChildren) {
  return (
    <SafeAreaView style={styles.safe} edges={["top", "left", "right", "bottom"]}>
      <ExpoStatusBar style="light" backgroundColor={colors.bg} translucent={false} />
      <StatusBar barStyle="light-content" backgroundColor={colors.bg} translucent={false} />
      <LinearGradient
        colors={["#131F36", "#0D1527", "#070B14"]}
        locations={[0, 0.42, 1]}
        style={styles.fullGradient}
      />
      <View style={styles.topAccent} />
      <LinearGradient
        colors={["rgba(56,189,248,0.06)", "rgba(20,184,166,0.03)", "rgba(7,11,20,0)"]}
        locations={[0, 0.5, 1]}
        start={{ x: 0, y: 0 }}
        end={{ x: 1, y: 0.6 }}
        style={styles.softOverlay}
      />
      <View style={styles.root}>{children}</View>
    </SafeAreaView>
  );
}

const styles = StyleSheet.create({
  safe: { flex: 1, backgroundColor: colors.bg },
  root: { flex: 1 },
  fullGradient: {
    position: "absolute",
    top: 0,
    left: 0,
    right: 0,
    bottom: 0
  },
  topAccent: {
    position: "absolute",
    top: 0,
    left: 0,
    right: 0,
    height: 3,
    backgroundColor: "rgba(56,189,248,.55)"
  },
  softOverlay: {
    position: "absolute",
    top: 0,
    left: 0,
    right: 0,
    bottom: 0
  }
});
