using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using Assets.Skrypty;

public enum KolorKostki {
    Czerwony,
    Niebieski,
    Pomaranczowy,
    Czarny
}

/// <summary>
/// Klasa reprezentujaca kostke do gry w 'Rummy'. Posiada informacje o wartosci liczbowej, kolorze oraz o tym, czy dana
/// kostka jest Jokerem.
/// </summary>

public class Kostka : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int wartosc;
    public KolorKostki kolor;
    public bool czyJoker;
    public bool czyUzyta = false;
    public bool czyZaznaczona = false;
    public Transform poprzedniRodzic;
    public TextMeshProUGUI liczbaNaKostce;
    public Image ikonkaBuzkiJokera;
    public Image wygladKostka;
    public CanvasGroup kontroler;
    public AudioSource dzwiekGlosnik;

    public void Awake()
    {
        kontroler = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
    }
    /// <summary>
    /// Metoda techniczna systemu UI odpowiedzialna za rozpoczecie przeciagania kostki.
    /// Ustawia aktualny rodzic kostki, przenosi ja na wierzch canvasu oraz wylacza blokowanie promieni raycast.
    /// </summary>
    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log("DETEKCJA: Zacząłem ciągnąć kostkę!");
        poprzedniRodzic = transform.parent;
        transform.SetParent(GetComponentInParent<Canvas>().transform);
        kontroler.blocksRaycasts = false;
        transform.SetAsLastSibling();
    }
    /// <summary>
    /// Metoda techniczna systemu UI odpowiedzialna za aktualizacje pozycji kostki podczas jej przeciagania.
    /// Ustawia pozycje kostki zgodnie z pozycja kursora.
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }
    /// <summary>
    /// Metoda techniczna systemu UI wywolywana po zakonczeniu przeciagania kostki.
    /// Przywraca obsluge raycastow i przekazuje obsluge upuszczenia kostki do logiki planszy.
    /// </summary>
    public void OnEndDrag(PointerEventData eventData)
    {
        kontroler.blocksRaycasts = true;
        PlanszaUI ui = Object.FindAnyObjectByType<PlanszaUI>();

        kontroler.blocksRaycasts = true;

        if (ui != null)
        {
            ui.ObsluzUpuszczenie(this, eventData);
        }
        else
        {
            Debug.LogError("BŁĄD: Nie znaleziono PlanszaUI!");
        }
    }
    /// <summary>
    /// Metoda inicjalizujaca kostke oraz ustawiajaca wyglad.
    /// </summary>
    /// <param name="liczba"></param>
    /// <param name="nowyKolor"></param>
    /// <param name="joker"></param>
    public void Inicjalizuj(int liczba, KolorKostki nowyKolor, bool joker = false)
    {
        this.wartosc = liczba;
        this.kolor = nowyKolor;
        this.czyJoker = joker;
        UstawWyglad();
    }
    /// <summary>
    /// Metoda pomocnicza, wykorzystywana w metodzie inicjalizujacej kostke. Ustawia wyglad i kolor kostki. 
    /// </summary>
    public void UstawWyglad()
    {
        RectTransform rt = GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.sizeDelta = new Vector2(80, 100);
        }
        Image tlo = GetComponent<Image>();
        if (tlo != null)
        {
            tlo.color = Color.white;
            if (wygladKostka != null) tlo.sprite = wygladKostka.sprite;
        }
        if (czyJoker)
        {
            if (liczbaNaKostce != null) liczbaNaKostce.gameObject.SetActive(false);
            if (ikonkaBuzkiJokera != null) ikonkaBuzkiJokera.gameObject.SetActive(true);
        }
        else
        {
            if (liczbaNaKostce != null)
            {
                liczbaNaKostce.gameObject.SetActive(true);
                liczbaNaKostce.text = wartosc.ToString();

                switch (kolor)
                {
                    case KolorKostki.Czerwony: liczbaNaKostce.color = Color.red; break;
                    case KolorKostki.Niebieski: liczbaNaKostce.color = new Color(0.1f, 0.4f, 0.8f); break;
                    case KolorKostki.Pomaranczowy: liczbaNaKostce.color = new Color(1f, 0.5f, 0f); break;
                    case KolorKostki.Czarny: liczbaNaKostce.color = Color.black; break;
                }
            }

            if (ikonkaBuzkiJokera != null) ikonkaBuzkiJokera.gameObject.SetActive(false);
        }
    }
    /// <summary>
    /// Realizuje płynne przemieszczanie kostki z jednego punktu do drugiego.
    /// </summary>
    /// <param name="cel"></param>
    /// <param name="czas"></param>
    /// <returns></returns>
    private IEnumerator AnimujRuch(Vector3 cel, float czas)
    {
        float t = 0;
        Vector3 start = transform.localPosition;
        while (t < czas)
        {
            t += Time.deltaTime;
            transform.localPosition = Vector3.Lerp(start, cel, t / czas);
            yield return null;
        }
        transform.localPosition = cel;
    }
    /// <summary>
    /// Metoda inicjujaca animacje przesuniecia kostki.
    /// </summary>
    /// <param name="cel"></param>
    /// <param name="czas"></param>
    public void Przemiesc(Vector3 cel, float czas)
    {
        StopAllCoroutines();
        StartCoroutine(AnimujRuch(cel, czas));
    }
        public void KliknietoKostke()
        {
            if (transform.parent != null && transform.parent.name == "Stojak")
            {
                czyZaznaczona = !czyZaznaczona;

                var ramka = GetComponent<UnityEngine.UI.Outline>();
                if (ramka != null)
                {
                    ramka.enabled = czyZaznaczona;
                }

                Debug.Log($"Kostka {wartosc} {kolor} zaznaczona: {czyZaznaczona}");
            }
    }
    /// <summary>
    /// Odtwarza efekt dźwiękowy przy umieszczaniu kostki na planszy, z losową zmianą tonu dla naturalnego brzmienia. 
    /// </summary>
    public void GrajDzwiekPoloz()
    {
        if (dzwiekGlosnik != null && dzwiekGlosnik.clip != null)
        {
            dzwiekGlosnik.pitch = Random.Range(0.9f, 1.1f);
            dzwiekGlosnik.PlayOneShot(dzwiekGlosnik.clip);
        }
    }
}
