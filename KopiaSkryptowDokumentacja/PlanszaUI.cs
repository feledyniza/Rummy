using UnityEngine;
using System.Collections.Generic;
using Assets.Skrypty;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Linq;
using TMPro;

public class PlanszaUI : MonoBehaviour
{
    public Plansza plansza;
    private bool blokadaNastepnejTury = false;
    public MenedzerKont menedzerKont;
    public Transform zbiorCiagow;
    public Transform panelGracza;
    public GameObject kostkaAnimowana;
    public List<Kostka> kostkiWBuforze = new List<Kostka>();
    public TextMeshProUGUI punktyGracz1;
    public TextMeshProUGUI punktyGracz2;
    public GameObject panelWygranej;
    public TextMeshProUGUI napisZwyciezcy;

    public GameObject panelStartowy;
    public GameObject interfejsGry;
    public GameObject panelWyboruTla;
    public TMP_InputField inputGracz1;
    public TMP_InputField inputGracz2;
    public TextMeshProUGUI nazwaNickGracz1;
    public TextMeshProUGUI nazwaNickGracz2;

    [Header("Panel Wygranej - Napisy")]
    public TextMeshProUGUI tekstNickWygrany;
    public TextMeshProUGUI tekstNickPrzegrany;
    public TextMeshProUGUI tekstPunktyWygrany;
    public TextMeshProUGUI tekstPunktyPrzegrany;

    public GameObject btnBlokada;
    public GameObject btnKolor;
    public GameObject btnPlus;
    public GameObject panelWyboruKoloru;

    [Header("Personalizacja T³a")]
    public Image obrazTlaGlownego;
    public Sprite[] wariantyTla;
    private int indeksWybranegoTla = 0;

    /// <summary>
    /// Metoda inicjalizuj¹ca pocz¹tek gry. 
    /// Ustawia panel Menu g³ównego, w którym gracz mo¿e zalogowaæ lub zarejestrowaæ siê lub rozpocz¹æ grê.
    /// </summary>

    private void Start()
    {
        panelWyboruKoloru.SetActive(false);
        panelStartowy.SetActive(true);
        interfejsGry.SetActive(false);
        if (panelWyboruTla!=null)panelWyboruTla.SetActive(false);
        if (panelWygranej != null) panelWygranej.SetActive(false);
        Debug.Log("Skrypt PlanszaUI ruszy³! Czekam na graczy...");
    }
    /// <summary>
    /// Tworzy pe³n¹ taliê kostek do gry.
    /// Generuje kostki dla ka¿dego koloru w wartoœciach od 1 - 13. Dodatkowo dodaje dwa Jokery.
    /// Zwraca listê zainicjalizowanych obiektów. 
    /// </summary>
    /// <returns></returns>
    private List<Kostka> WygenerujKostki()
    {
        List<Kostka> lista = new List<Kostka>();
        foreach (KolorKostki kolor in System.Enum.GetValues(typeof(KolorKostki)))
        {
            for (int wartosc = 1; wartosc <= 13; wartosc++)
            {
                GameObject go = Instantiate(kostkaAnimowana, transform);
                Kostka skrypt = go.GetComponent<Kostka>();
                skrypt.Inicjalizuj(wartosc, kolor, false);
                go.SetActive(false);
                lista.Add(skrypt);
            }
        }
        for (int i = 0; i < 2; i++)
        {
            GameObject go = Instantiate(kostkaAnimowana, transform);
            Kostka skrypt = go.GetComponent<Kostka>();

            skrypt.Inicjalizuj(0, KolorKostki.Czarny, true);

            go.SetActive(false);
            lista.Add(skrypt);
        }
        return lista;
    }
    /// <summary>
    /// G³ówna metoda startowa.
    /// Odczytuje nicki graczy, ustawia wybrane t³o, tworzy instancjê logiki Plansza,
    /// a takze losuje po 10 kostek startowych dla ka¿dego gracza i odœwie¿a widok.
    /// </summary>
    public void RozpocznijGre()
    {
        if (obrazTlaGlownego != null && wariantyTla.Length > indeksWybranegoTla)
        {
            obrazTlaGlownego.sprite = wariantyTla[indeksWybranegoTla];
        }

        string nick1 = string.IsNullOrWhiteSpace(inputGracz1.text) ? "Gracz 1" : inputGracz1.text;
        string nick2 = string.IsNullOrWhiteSpace(inputGracz2.text) ? "Gracz 2" : inputGracz2.text;

        panelStartowy.SetActive(false);
        interfejsGry.SetActive(true);

        plansza = new Plansza();
        List<Kostka> wszystkieKostki = WygenerujKostki();
        plansza.pulaKostek = wszystkieKostki;

        plansza.gracze.Clear();
        plansza.gracze.Add(new Uzytkownik { nick = nick1 });
        plansza.gracze.Add(new Uzytkownik { nick = nick2 });

        foreach (var gracz in plansza.gracze)
        {
            for (int i = 0; i < 10; i++)
            {
                if (plansza.pulaKostek.Count > 0)
                {
                    int index = Random.Range(0, plansza.pulaKostek.Count);
                    Kostka wylosowana = plansza.pulaKostek[index];
                    gracz.kostkiGracza.Add(wylosowana);
                    plansza.pulaKostek.RemoveAt(index);
                }
            }
        }

        OdswiezPlansze();

        if (nazwaNickGracz1 != null) nazwaNickGracz1.text = nick1;
        if (nazwaNickGracz2!= null) nazwaNickGracz2.text = nick2;


        Debug.Log($"Gra rozpoczêta! {nick1} vs {nick2}. W puli zosta³o: {plansza.pulaKostek.Count}");
    }
    /// <summary>
    /// Synchronizuje widok stojaka gracza z jego aktualnym stanem posiadania.
    /// Odpowiada za wyœwietlanie odpowiednich kostek, ustawienie wygl¹du, 
    /// a takze pokazywanie/ukrywanie przycisków "Kó³ Ratunkowych" w zale¿noœci od ich u¿ycia przez danego gracza.
    /// </summary>
    public void OdswiezPlansze()
    {
        foreach (Transform dziecko in panelGracza)
        {
            dziecko.gameObject.SetActive(false);
        }

        Gracz aktualny = plansza.AktualnyGracz();
        List<Kostka> mojeKostki = aktualny.kostkiGracza;

        foreach (Kostka k in mojeKostki)
        {
            k.transform.SetParent(panelGracza, false);
            k.gameObject.SetActive(true);
            k.UstawWyglad();

            k.transform.localScale = Vector3.one;
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(panelGracza as RectTransform);

        if (btnBlokada != null) btnBlokada.SetActive(!aktualny.uzyteKola[0]);
        if (btnKolor != null) btnKolor.SetActive(!aktualny.uzyteKola[1]);
        if (btnPlus != null) btnPlus.SetActive(!aktualny.uzyteKola[2]);

        Debug.Log("Stojak odœwie¿ony dla: " + aktualny.nick);
    }

    /// <summary>
    /// Sprawdza, czy kostka zosta³a upuszczona nad obszarem sto³u, czy poza nim.
    /// Decyduje o dodaniu kostki do bufora ruchu lub o jej powrocie na stojak.
    /// </summary>
    /// <param name="kostka"></param>
    /// <param name="eventData"></param>
    public void ObsluzUpuszczenie(Kostka kostka, PointerEventData eventData)
    {
        bool nadStolem = RectTransformUtility.RectangleContainsScreenPoint(zbiorCiagow as RectTransform,eventData.position,eventData.pressEventCamera);

        if (nadStolem)
        {
            kostka.czyZaznaczona = false;
            kostka.transform.SetParent(zbiorCiagow, true);
            if (!kostkiWBuforze.Contains(kostka))
                kostkiWBuforze.Add(kostka);

            Debug.Log("Kostka na stole. Bufor: " + kostkiWBuforze.Count);
        }
        else
        {
            WrocKostkeNaStojak(kostka);
        }
        kostka.GrajDzwiekPoloz();
    }
    /// <summary>
    /// Przenosi kostke z powrotem do panelu gracza, resetuje jej status u¿ycia, a takze 
    /// usuwa j¹ z bufora aktualnego ruchu.
    /// </summary>
    /// <param name="kostka"></param>
    private void WrocKostkeNaStojak(Kostka kostka)
    {
        kostka.transform.SetParent(panelGracza, false);
        kostka.czyUzyta = false;

        RectTransform rt = kostka.GetComponent<RectTransform>();
        rt.anchoredPosition = Vector2.zero;
        rt.localScale = Vector3.one;

        if (kostkiWBuforze.Contains(kostka))
            kostkiWBuforze.Remove(kostka);

        LayoutRebuilder.ForceRebuildLayoutImmediate(panelGracza as RectTransform);
        Debug.Log("Kostka wróci³a na stojak za pomoc¹ WrocKostkeNaStojak");
    }
    /// <summary>
    /// Kluczowa metoda weryfikuj¹ca turê. Sprawdza poprawnoœæ u³o¿eñ na stole, nalicza punkty za nowe kostki, uwzglêdniaj¹c przy tym Jokery.
    /// Weryfikuje równie¿ zasadê pierwszego wy³o¿enia. 
    /// Obs³uguje logikê zakoñczenia gry, jeœli któryœ z graczy pozby³ siê wszystkich kostek.
    /// </summary>
    public void ZatwierdzRuch()
    {
        if (plansza == null || !plansza.czyGraAktywna)
        {
            Debug.Log("Gra jest zakoñczona, ruch zablokowany.");
            return;
        }

        List<List<Kostka>> ukladyNaStole = PobierzWszystkieUkladyZeStolu();
        Gracz aktualny = plansza.AktualnyGracz();

        if (plansza.CzyWszystkieUkladyPoprawne(ukladyNaStole))
        {
            int sumaPunktowZTurzy = 0;
            int licznikNowychKostek = 0;

            foreach (Kostka k in kostkiWBuforze)
            {
                if (k.czyUzyta == false) licznikNowychKostek++;
                if (k.czyJoker)
                {
                    var ukladZJokerem = ukladyNaStole.FirstOrDefault(u => u.Contains(k));
                    if (ukladZJokerem != null)
                        if (k.czyUzyta==false)sumaPunktowZTurzy += plansza.WyliczJokera(k, ukladZJokerem);
                }
                else
                {
                    if (k.czyUzyta==false) sumaPunktowZTurzy += k.wartosc;
                }
                k.czyUzyta = true;
            }

            if (!aktualny.czyPierwszeWylozenieZrobione)
            {
                if (licznikNowychKostek < 3)
                {
                    Debug.Log("<color=orange>Pierwsze wy³o¿enie musi zawieraæ min. 3 kostki z Twojej rêki!</color>");
                    AnulujRuch();
                    return;
                }

                aktualny.czyPierwszeWylozenieZrobione = true;
                Debug.Log("<color=green>Gracz otworzy³ rozgrywkê!</color>");
            }

            aktualny.iloscPunktow += sumaPunktowZTurzy;

            foreach (Kostka k in new List<Kostka>(kostkiWBuforze))
            {
                if (aktualny.kostkiGracza.Contains(k))
                    aktualny.kostkiGracza.Remove(k);
            }

            if (punktyGracz1 != null) punktyGracz1.text = plansza.gracze[0].iloscPunktow.ToString();
            if (punktyGracz2 != null) punktyGracz2.text = plansza.gracze[1].iloscPunktow.ToString();

            if (aktualny.kostkiGracza.Count == 0)
            {
                Debug.Log("<color=gold>WYGRANA! Zatrzymujê wszystko.</color>");

                plansza.czyGraAktywna = false;
                Gracz zwyciezca = aktualny;
                Gracz przegrany = (aktualny == plansza.gracze[0]) ? plansza.gracze[1] : plansza.gracze[0];

                int sumaKarna = 0;
                foreach (Kostka k in przegrany.kostkiGracza)
                {
                    if (k.czyJoker)
                    {
                        sumaKarna += 30;
                    }
                    else
                    {
                        sumaKarna += k.wartosc;
                    }
                }

                przegrany.iloscPunktow -= sumaKarna;

                if (menedzerKont != null)
                {
                    menedzerKont.AktualizujStatystyki(zwyciezca.nick, zwyciezca.iloscPunktow, true);

                    menedzerKont.AktualizujStatystyki(przegrany.nick, przegrany.iloscPunktow, false);
                }

                if (panelWygranej != null)
                {
                    panelWygranej.SetActive(true);
                    panelWygranej.transform.SetAsLastSibling();
                    if (tekstNickWygrany != null) tekstNickWygrany.text = zwyciezca.nick;
                    if (tekstPunktyWygrany != null) tekstPunktyWygrany.text = zwyciezca.iloscPunktow.ToString();

                    if (tekstNickPrzegrany != null) tekstNickPrzegrany.text = przegrany.nick;
                    if (tekstPunktyPrzegrany != null) tekstPunktyPrzegrany.text = przegrany.iloscPunktow.ToString();

                }

                Button[] wszystkiePrzyciski = Object.FindObjectsByType<Button>(FindObjectsInactive.Exclude);
                foreach (Button b in wszystkiePrzyciski)
                {
                    if (panelWygranej != null && !b.transform.IsChildOf(panelWygranej.transform))
                    {
                        b.interactable = false;
                    }
                }
                UstawBlokadeInterakcji(zbiorCiagow, false);
                UstawBlokadeInterakcji(panelGracza, false);

                return;
            }

            kostkiWBuforze.Clear();

            if (blokadaNastepnejTury)
            {
                blokadaNastepnejTury = false;
                Debug.Log("<color=blue>Blokada aktywowana - ten sam gracz rusza siê ponownie!</color>");
            }
            else
            {
                plansza.NastepnyGracz();
            }
            Debug.Log("Tura zakoñczona przez dobranie. Teraz ruch ma: " + plansza.AktualnyGracz().nick);
            OdswiezPlansze();
        }
        else
        {
            Debug.Log("<color=red>Niepoprawny uk³ad!</color>");
            AnulujRuch();
        }
    }
    /// <summary>
    /// W³¹cza lub wy³¹cza mo¿liwoœæ klikania na dany panel.
    /// </summary>
    /// <param name="panel"></param>
    /// <param name="stan"></param>
    private void UstawBlokadeInterakcji(Transform panel, bool stan)
    {
        if (panel == null) return;
        CanvasGroup cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.gameObject.AddComponent<CanvasGroup>();

        cg.interactable = stan;
        cg.blocksRaycasts = stan;
    }
    /// <summary>
    /// Wycofuje na stojak wszystkie kostki wy³o¿one w bie¿¹cej turze, które znajduj¹ siê w buforze ruchu.
    /// </summary>
    public void AnulujRuch()
    {
        List<Kostka> doZwrotu = new List<Kostka>(kostkiWBuforze);

        foreach (Kostka k in doZwrotu)
        {
            WrocKostkeNaStojak(k);
        }

        kostkiWBuforze.Clear();
    }
   /// <summary>
   /// Analizuje po³o¿enie kostek w zbiorCiagow.
   /// Grupuje kostki w rzêdy.
   /// </summary>
   /// <returns></returns>
    private List<List<Kostka>> PobierzWszystkieUkladyZeStolu()
    {
        List<Kostka> wszystkieNaStole = new List<Kostka>();
        foreach (Transform t in zbiorCiagow)
        {
            Kostka k = t.GetComponent<Kostka>();
            if (k != null)
            {
                wszystkieNaStole.Add(k);
                if (k.czyJoker) k.wartosc = 0;
            }
        }

        List<List<Kostka>> grupy = new List<List<Kostka>>();
        if (wszystkieNaStole.Count == 0) return grupy;

        var rzedy = wszystkieNaStole.GroupBy(k => Mathf.Round(k.transform.localPosition.y / 60f)).ToList();

        foreach (var rzad in rzedy)
        {
            var posortowaneWRzedzie = rzad.OrderBy(k => k.transform.localPosition.x).ToList();
            List<Kostka> aktualnaGrupa = new List<Kostka> { posortowaneWRzedzie[0] };

            for (int i = 1; i < posortowaneWRzedzie.Count; i++)
            {
                float dystans = Mathf.Abs(posortowaneWRzedzie[i].transform.localPosition.x - posortowaneWRzedzie[i - 1].transform.localPosition.x);

                if (dystans < 120f)
                {
                    aktualnaGrupa.Add(posortowaneWRzedzie[i]);
                }
                else
                {
                    grupy.Add(new List<Kostka>(aktualnaGrupa));
                    aktualnaGrupa = new List<Kostka> { posortowaneWRzedzie[i] };
                }
            }
            grupy.Add(new List<Kostka>(aktualnaGrupa));
        }
        return grupy;
    }
    /// <summary>
    /// Obd³uga pobrania nowej kostki z puli.
    /// Jeœli gracz mia³coœ wy³o¿one na stole, ruch jest anulowany,
    /// a tura koñczy siê automatycznie po dobraniu.
    /// </summary>
    public void KliknietoDobierzKostke()
    {
        if (!plansza.czyGraAktywna) return;
        Gracz aktualny = plansza.AktualnyGracz();

        Kostka nowa = plansza.WydajLosowaKostkeZPuli();

        if (nowa != null)
        {
            aktualny.DobierzKostke(nowa);

            if (kostkiWBuforze.Count > 0)
            {
                AnulujRuch();
            }

            plansza.NastepnyGracz();
            OdswiezPlansze();

            LayoutRebuilder.ForceRebuildLayoutImmediate(panelGracza as RectTransform);

            Debug.Log($"Gracz {aktualny.nick} dobra³ kostkê. W puli zosta³o: {plansza.pulaKostek.Count}");
        }
    }
    /// <summary>
    /// Realizuje mechanizm ko³a ratunkowego blokady przeciwnika.
    /// Po zatwierdzeniu obecnego ruchu, tura nie przechodzi na przeciwnika. 
    /// Gracz ma mo¿liwoœæ zrealizowania dwóch tur pod rz¹d
    /// </summary>
    public void Kolo_Blokada()
    {
        Gracz g = plansza.AktualnyGracz();
        if (g.uzyteKola[0]) return;
        blokadaNastepnejTury = true;
        g.uzyteKola[0] = true;
    }
    /// <summary>
    /// Realizuje mechanizm ko³a ratunkowego zmiany koloru kostek.
    /// Sprawdza, czy gracz poprawnie zaznaczy³ dok³adnie 3 kostki na swoim stojaku.
    /// Jeœli warunek jest spe³niony to otwierany jest panel wyboru nowego koloru.
    /// </summary>
    public void Kolo_Pedzel()
    {
        Gracz g = plansza.AktualnyGracz();
        if (g.uzyteKola[1]) return;

        var zaznaczone = g.kostkiGracza.Where(k => k.czyZaznaczona && k.transform.parent == panelGracza).ToList();

        if (zaznaczone.Count == 3)
        {
            if (panelWyboruKoloru != null)
            {
                panelWyboruKoloru.SetActive(true);
            }
        }
        else
        {
            Debug.Log("Musisz zaznaczyæ dok³adnie 3 kostki na stojaku!");
        }
    }
    /// <summary>
    /// G³ówna logika ko³a ratunkowego zmieniaj¹cego kolor kostek.
    /// Sprawdza, czy w puli wszystkich kostek znajduje siê kostka o tej samej wartoœci, ale w wybranym kolorze - jeœli tak, nastêpuje zamiana.
    /// Jeœli w puli nie ma odpowiedniej kostki, przeszukiwany jest stojak przeciwnika. 
    /// W przypadku gdy szukana kostka wystêpuje na stojaku przeciwnika: kostka jest mu zabierana,
    /// a oddawana jest pierwotna kostka gracza zmieniajacego kolor.
    /// Mechanizm podmiany kostek zosta³ wprowadzony, aby liczba kostek u obu graczy zosta³a taka sama.
    /// </summary>
    /// <param name="indeksKoloru"></param>
    public void WybierzKolorZListy(int indeksKoloru)
    {
        KolorKostki wybranyKolor = (KolorKostki)indeksKoloru;
        Gracz g = plansza.AktualnyGracz();
        Gracz przeciwnik = (g == plansza.gracze[0]) ? plansza.gracze[1] : plansza.gracze[0];

        var zaznaczone = g.kostkiGracza.Where(k => k.czyZaznaczona && k.transform.parent == panelGracza).ToList();

        foreach (var k in zaznaczone)
        {
            if (k.kolor == wybranyKolor) continue;

            var zamiennik = plansza.pulaKostek.FirstOrDefault(p => p.wartosc == k.wartosc && p.kolor == wybranyKolor);

            if (zamiennik != null)
            {
                plansza.pulaKostek.Remove(zamiennik);
                plansza.pulaKostek.Add(k);
                int index = g.kostkiGracza.IndexOf(k);
                g.kostkiGracza[index] = zamiennik;
                k.gameObject.SetActive(false);
                Debug.Log($"Podmieniono {k.wartosc} z puli.");
            }
            else
            {
                var uPrzeciwnika = przeciwnik.kostkiGracza.FirstOrDefault(p => p.wartosc == k.wartosc && p.kolor == wybranyKolor);

                if (uPrzeciwnika != null)
                {
                    przeciwnik.kostkiGracza.Remove(uPrzeciwnika);

                    przeciwnik.kostkiGracza.Add(k);
                    k.czyZaznaczona = false;
                    k.transform.SetParent(null);

                    int index = g.kostkiGracza.IndexOf(k);
                    g.kostkiGracza[index] = uPrzeciwnika;

                    Debug.Log($"Zabrano {uPrzeciwnika.wartosc} {wybranyKolor} przeciwnikowi, oddano mu nasz {k.kolor}.");
                }
            }
        }

        g.uzyteKola[1] = true;
        panelWyboruKoloru.SetActive(false);
        OdswiezPlansze();
    }
    /// <summary>
    /// Realizuje mechanizm ko³a ratunkowego uzupe³nienia ci¹gu brakuj¹c¹ kostk¹.
    /// Gracz uk³ada na sto³ kostki tego samego koloru, miêdzy którymi brakuje jednej wartoœci.
    /// Oblicza ró¿nicê miêdzy wartoœciami kostek w buforze, a nastêpnie szuka odpowiedniej kostki.
    /// Pocz¹tkowo przeszukiwana jest pula wszystkich kostek. 
    /// Je¿eli pula jest pusta lub nie zawiera szukanej kostki,
    /// system bezpoœrednio wyci¹ga j¹ ze stojaka przeciwnika. W miejsce zabranej kostki przypisywana jest nowa, przypadkowa kostka z puli.
    /// Znaleziona kostka jest automatycznie umieszczana na stole miêdzy dwiema kostkami gracza, tworz¹c poprawny ci¹g.
    /// </summary>
    public void Kolo_Plus()
    {
        Gracz g = plansza.AktualnyGracz();
        if (g.uzyteKola[2]) return;

        if (kostkiWBuforze.Count < 2)
        {
            Debug.LogWarning("Po³ó¿ co najmniej 2 kostki tego samego koloru!");
            return;
        }

        for (int i = 0; i < kostkiWBuforze.Count; i++)
        {
            for (int j = i + 1; j < kostkiWBuforze.Count; j++)
            {
                Kostka k1 = kostkiWBuforze[i];
                Kostka k2 = kostkiWBuforze[j];

                if (k1.kolor == k2.kolor)
                {
                    int roznica = Mathf.Abs(k1.wartosc - k2.wartosc);

                    if (roznica == 2)
                    {
                        int mniejsza = Mathf.Min(k1.wartosc, k2.wartosc);
                        int szukanaW = mniejsza + 1;

                        var zPuli = plansza.pulaKostek.FirstOrDefault(p => p.wartosc == szukanaW && p.kolor == k1.kolor);

                        if (zPuli != null)
                        {
                            WyciagnijKostkeIPoloz(zPuli, k1, k2, g);
                            return;
                        }
                        else
                        {
                            Gracz przeciwnik = (g == plansza.gracze[0]) ? plansza.gracze[1] : plansza.gracze[0];
                            var uPrzeciwnika = przeciwnik.kostkiGracza.FirstOrDefault(p => p.wartosc == szukanaW && p.kolor == k1.kolor);

                            if (uPrzeciwnika != null)
                            {
                                Debug.Log($"<color=orange>ZABIERAM {szukanaW} przeciwnikowi!</color>");

                                przeciwnik.kostkiGracza.Remove(uPrzeciwnika);

                                if (plansza.pulaKostek.Count > 0)
                                {
                                    Kostka zamiennik = plansza.WydajLosowaKostkeZPuli();
                                    przeciwnik.kostkiGracza.Add(zamiennik);
                                    Debug.Log("Przeciwnik dosta³ kostkê zastêpcz¹ do rêki.");
                                }

                                WyciagnijKostkeIPoloz(uPrzeciwnika, k1, k2, g);
                                return;
                            }
                        }
                    }
                }
            }
        }
        Debug.Log("Nie znaleziono odpowiedniej luki.");
    }
    /// <summary>
    /// Metoda realizuj¹ca transfer kostki z puli wszystkich kostek lub stojaka przeciwnika.
    /// Odpowiada za wyliczenie pozycji œrodkowej, aby kostka ustawi³a siê w lukê miêdzy kostkami oraz
    /// dodanie nowej kostki do buforu, co pozwala na zatwierdzenie ruchu w tej samej turze i doliczenie punktow tak¿e za now¹ kostkê.
    /// </summary>
    /// <param name="znaleziona"></param>
    /// <param name="k1"></param>
    /// <param name="k2"></param>
    /// <param name="g"></param>
    private void WyciagnijKostkeIPoloz(Kostka znaleziona, Kostka k1, Kostka k2, Gracz g)
    {
        znaleziona.transform.SetParent(zbiorCiagow, false);
        znaleziona.gameObject.SetActive(true);

        RectTransform rt = znaleziona.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(80,100);

        znaleziona.UstawWyglad();

        znaleziona.GetComponent<Image>().color = Color.white;

        znaleziona.transform.localPosition = (k1.transform.localPosition + k2.transform.localPosition) / 2;

        var img = znaleziona.GetComponent<UnityEngine.UI.Image>();
        if (img != null) img.color = Color.white;

        znaleziona.transform.localPosition = (k1.transform.localPosition + k2.transform.localPosition) / 2;

        kostkiWBuforze.Add(znaleziona);
        g.uzyteKola[2] = true;
        if (btnPlus != null) btnPlus.SetActive(false);

        Debug.Log($" Wstawiono {znaleziona.wartosc} {znaleziona.kolor}");
    }
    /// <summary>
    /// Metdoa obs³uguj¹ca konkretny wybór gracza. 
    /// Przypisuje wybrane t³o, które zostanie wykorzystane w momencie wywo³ania RozpocznijGre().
    /// Po dokonaniu wyboru automatycznie zamyka panel personalizacji. 
    /// </summary>
    /// <param name="nr"></param>
    public void WybierzIndeksTla(int nr)
    {
        indeksWybranegoTla = nr;
        panelWyboruTla.SetActive(false);
    }
    /// <summary>
    /// Metoda aktywuj¹ca interfejs personalizacji gry. 
    /// Wyœwietla panel, w którym gracz mo¿e przegl¹daæ dostêpne warianty t³a planszy.
    /// </summary>
    public void OtworzPanelWyboruTla()
    {
        panelWyboruTla.SetActive(true);
    }

}