import * as React from "react";
import { useEffect, useState } from "react";
import { ScrollView, StyleSheet, Text, View } from "react-native";
import { TextInput as PaperTextInput } from "react-native-paper";

import {
  ActionButton,
  Card,
  EmptyBlock,
  ErrorBlock,
  InlineRow,
  ListItem,
  LoadingBlock,
  Pill,
  SectionTitle,
  colors,
} from "../../components/ui";
import { formatNumber } from "../../lib/format";
import { mobileApi } from "../../lib/api";
import type {
  ClientProductLookupDto,
  ClientWarehouseLookupDto,
} from "../../types";
import type { ClientReservationCartItem } from "../../appTypes";

export function ProductsScreen({
  apiBaseUrl,
  token,
  warehouses,
  warehousesLoading,
  warehousesError,
  selectedWarehouseId,
  onSelectWarehouse,
  cartItems,
  cartItemsCount,
  onAddToCart,
  onOpenCart,
}: {
  apiBaseUrl: string;
  token: string;
  warehouses: ClientWarehouseLookupDto[];
  warehousesLoading: boolean;
  warehousesError?: string | null;
  selectedWarehouseId: number | null;
  onSelectWarehouse: (warehouseId: number) => void;
  cartItems: ClientReservationCartItem[];
  cartItemsCount: number;
  onAddToCart: (product: ClientProductLookupDto, quantity: number) => void;
  onOpenCart?: () => void;
}) {
  const [query, setQuery] = useState("");
  const [products, setProducts] = useState<ClientProductLookupDto[] | null>(
    null,
  );
  const [productsLoading, setProductsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);
  const [formError, setFormError] = useState<string | null>(null);
  const [selectedProduct, setSelectedProduct] =
    useState<ClientProductLookupDto | null>(null);
  const [quantityText, setQuantityText] = useState("1");

  async function loadProducts(search = query) {
    const term = search.trim();
    if (term.length > 0 && term.length < 2) {
      setProducts([]);
      return;
    }

    setProductsLoading(true);
    setError(null);
    try {
      const result = await mobileApi.searchReservableProducts(
        apiBaseUrl,
        token,
        term,
        30,
        selectedWarehouseId,
      );
      setProducts(result);
    } catch (e) {
      setError(
        e instanceof Error ? e.message : "Nie udało się pobrać produktów.",
      );
    } finally {
      setProductsLoading(false);
    }
  }

  useEffect(() => {
    void loadProducts("");
  }, [apiBaseUrl, token]);

  useEffect(() => {
    if (!selectedWarehouseId) return;
    void loadProducts(query);
  }, [selectedWarehouseId]);

  function addSelectedToCart() {
    setFormError(null);

    if (!selectedProduct) {
      setFormError("Wybierz produkt do dodania.");
      return;
    }

    const qty = Number(quantityText.replace(",", "."));
    if (!Number.isFinite(qty) || qty <= 0) {
      setFormError("Ilość musi być większa od zera.");
      return;
    }

    const available = selectedProduct.availableQuantity;
    if (available != null) {
      const existingQty =
        cartItems.find((x) => x.productId === selectedProduct.productId)
          ?.quantity ?? 0;
      const totalAfterAdd = existingQty + qty;

      if (available <= 0) {
        setFormError("Produkt jest obecnie niedostępny w wybranym magazynie.");
        return;
      }

      if (totalAfterAdd > available) {
        setFormError(
          `Ilość przekracza dostępny stan. Dostępne: ${formatNumber(
            available,
          )}, w koszyku po dodaniu byłoby: ${formatNumber(totalAfterAdd)}.`,
        );
        return;
      }
    }

    onAddToCart(selectedProduct, qty);
    setQuantityText("1");
  }

  const shortQuery = query.trim().length > 0 && query.trim().length < 2;

  return (
    <Card>
      <SectionTitle
        title="Katalog produktów"
        subtitle="Buduj koszyk rezerwacji. WZ tworzy obsługa magazynu po stronie firmy."
      />

      <InlineRow style={{ marginBottom: 10 }}>
        <View style={{ flex: 1 }}>
          <ActionButton
            label={`Koszyk (${cartItemsCount})`}
            variant="ghost"
            onPress={() => onOpenCart?.()}
          />
        </View>
      </InlineRow>

      {warehousesError ? <ErrorBlock message={warehousesError} /> : null}
      {error ? <ErrorBlock message={error} /> : null}
      {formError ? <ErrorBlock message={formError} /> : null}

      <Text style={styles.label}>Magazyn odbioru</Text>
      {warehousesLoading ? (
        <LoadingBlock label="Pobieranie magazynów..." />
      ) : (
        <View style={styles.choiceWrap}>
          {warehouses.map((w) => (
            <View key={w.warehouseId} style={styles.choiceButtonSlot}>
              <ActionButton
                label={w.name}
                variant={
                  selectedWarehouseId === w.warehouseId ? "secondary" : "ghost"
                }
                onPress={() => onSelectWarehouse(w.warehouseId)}
              />
            </View>
          ))}
        </View>
      )}

      <Text style={styles.label}>Wyszukaj produkt</Text>
      <PaperTextInput
        mode="outlined"
        dense
        value={query}
        onChangeText={(value) => {
          setQuery(value);
          if (!value.trim()) {
            void loadProducts("");
          }
        }}
        placeholder="np. LAB-050 lub skaner"
        textColor={colors.text}
        outlineColor={colors.line}
        activeOutlineColor={colors.accent}
        style={styles.input}
        right={
          <PaperTextInput.Icon
            icon="magnify"
            onPress={() => void loadProducts()}
          />
        }
        onSubmitEditing={() => void loadProducts()}
      />
      {shortQuery ? (
        <Text style={styles.helperWarn}>
          Wpisz minimum 2 znaki, aby zawęzić listę.
        </Text>
      ) : null}

      {selectedProduct ? (
        <View style={{ marginBottom: 10 }}>
          <InlineRow style={{ flexWrap: "wrap" }}>
            <Pill
              label={`Wybrany: ${selectedProduct.code} (${selectedProduct.defaultUom ?? "-"})`}
              tone="good"
            />
            {selectedProduct.availableQuantity != null &&
            selectedProduct.availableQuantity <= 0 ? (
              <Pill label="Brak dostępnego stanu" tone="danger" />
            ) : null}
          </InlineRow>
        </View>
      ) : null}

      <View style={styles.addToCartBlock}>
        <Text style={styles.label}>Ilość do dodania</Text>
        <InlineRow style={styles.addToCartRow}>
          <View style={styles.quantitySlot}>
            <PaperTextInput
              mode="outlined"
              dense
              value={quantityText}
              onChangeText={setQuantityText}
              keyboardType="decimal-pad"
              placeholder="1"
              textColor={colors.text}
              outlineColor={colors.line}
              activeOutlineColor={colors.accent}
              style={styles.quantityInput}
            />
          </View>
          <View style={styles.addButtonSlot}>
            <ActionButton label="Dodaj" onPress={addSelectedToCart} />
          </View>
        </InlineRow>
      </View>

      {productsLoading ? (
        <LoadingBlock label="Pobieranie produktów..." />
      ) : products && products.length > 0 ? (
        <ScrollView style={{ maxHeight: 260 }} nestedScrollEnabled>
          {products.map((p) => (
            <ListItem
              key={p.productId}
              title={`${p.code} - ${p.name}`}
              subtitle={`${p.defaultUom ? `JM: ${p.defaultUom}` : "JM: -"}${
                p.availableQuantity != null
                  ? ` | Dostępne: ${formatNumber(p.availableQuantity)}`
                  : ""
              }`}
              right={
                selectedProduct?.productId === p.productId ? (
                  <Pill label="Wybrany" tone="good" />
                ) : undefined
              }
              onPress={() => setSelectedProduct(p)}
            />
          ))}
        </ScrollView>
      ) : (
        <EmptyBlock
          title={
            query.trim().length >= 2
              ? "Brak produktów"
              : "Brak produktów katalogowych"
          }
          subtitle={
            query.trim().length >= 2
              ? "Spróbuj innej frazy."
              : "Brak danych do wyświetlenia."
          }
        />
      )}
    </Card>
  );
}

const styles = StyleSheet.create({
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
  helperWarn: {
    color: colors.warn,
    fontSize: 11,
    marginTop: -4,
    marginBottom: 8,
  },
  choiceWrap: {
    flexDirection: "row",
    flexWrap: "wrap",
    gap: 8,
    marginBottom: 10,
  },
  choiceButtonSlot: {
    width: "100%",
  },
  addToCartBlock: {
    marginBottom: 8,
  },
  addToCartRow: {
    alignItems: "center",
  },
  quantitySlot: {
    flex: 1.15,
  },
  addButtonSlot: {
    flex: 1,
    marginTop: -2,
  },
  quantityInput: {
    backgroundColor: "rgba(22,35,61,.45)",
    borderRadius: 12,
    overflow: "hidden",
    marginBottom: 0,
  },
});
