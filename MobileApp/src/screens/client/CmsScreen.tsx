import * as React from "react";
import { useEffect, useMemo, useState } from "react";
import { StyleSheet, Text, View } from "react-native";

import { mobileApi } from "../../lib/api";
import { ActionButton, Card, EmptyBlock, ErrorBlock, ListItem, LoadingBlock, SectionTitle, colors } from "../../components/ui";
import type { MobileNewsItemDto, MobilePageDetailsDto, MobilePageListItemDto } from "../../types";

export function CmsScreen({ apiBaseUrl }: { apiBaseUrl: string }) {
  const [news, setNews] = useState<MobileNewsItemDto[] | null>(null);
  const [pages, setPages] = useState<MobilePageListItemDto[] | null>(null);
  const [page, setPage] = useState<MobilePageDetailsDto | null>(null);
  const [error, setError] = useState<string | null>(null);

  const { contactPages, otherPages } = useMemo(() => {
    const all = pages ?? [];
    const contacts = all.filter((p) => {
      const text = `${p.slug} ${p.title}`.toLowerCase();
      return text.includes("kontakt") || text.includes("contact");
    });
    return {
      contactPages: contacts,
      otherPages: all.filter((p) => !contacts.some((c) => c.id === p.id))
    };
  }, [pages]);

  async function load() {
    setError(null);
    try {
      const [newsData, pagesData] = await Promise.all([mobileApi.getNews(apiBaseUrl), mobileApi.getPages(apiBaseUrl)]);
      setNews(newsData);
      setPages(pagesData);
    } catch (e) {
      setError(e instanceof Error ? e.message : "Nie udalo sie pobrac informacji.");
    }
  }

  async function loadPage(slug: string) {
    setError(null);
    try {
      setPage(await mobileApi.getPage(apiBaseUrl, slug));
    } catch (e) {
      setError(e instanceof Error ? e.message : "Nie udalo sie pobrac strony.");
    }
  }

  useEffect(() => {
    void load();
  }, [apiBaseUrl]);

  return (
    <>
      <Card>
        <SectionTitle title="Informacje" subtitle="Aktualnosci i strony informacyjne" />
        {!news && !pages && !error ? (
          <LoadingBlock label="Pobieranie informacji..." />
        ) : error && !news && !pages ? (
          <ErrorBlock message={error} />
        ) : (
          <>
            <Text style={styles.subsection}>Aktualnosci</Text>
            {news?.length ? (
              news.slice(0, 5).map((item) => <ListItem key={item.id} title={item.title} subtitle={item.content.slice(0, 120)} />)
            ) : (
              <EmptyBlock title="Brak aktualnosci" />
            )}

            <Text style={styles.subsection}>Kontakt</Text>
            {contactPages.length ? (
              contactPages.map((item) => (
                <ListItem key={item.id} title={item.title} subtitle={`Strona kontaktowa: ${item.slug}`} onPress={() => void loadPage(item.slug)} />
              ))
            ) : (
              <EmptyBlock title="Brak strony kontaktowej" subtitle="Dodaj strone CMS ze slugiem np. kontakt." />
            )}

            <Text style={styles.subsection}>Pozostale informacje</Text>
            {otherPages.length ? (
              otherPages.map((item) => (
                <ListItem key={item.id} title={item.title} subtitle={`slug: ${item.slug}`} onPress={() => void loadPage(item.slug)} />
              ))
            ) : (
              <EmptyBlock title="Brak stron informacyjnych" />
            )}

            <View style={{ marginTop: 10 }}>
              <ActionButton label="Odswiez informacje" onPress={() => void load()} variant="ghost" />
            </View>
          </>
        )}
      </Card>

      {page ? (
        <Card>
          <SectionTitle title={page.title} subtitle={page.slug} />
          <Text style={styles.cmsContent}>{page.content || "(pusta tresc)"}</Text>
          <View style={{ marginTop: 10 }}>
            <ActionButton label="Zamknij strone" onPress={() => setPage(null)} variant="ghost" />
          </View>
        </Card>
      ) : null}
    </>
  );
}

const styles = StyleSheet.create({
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
