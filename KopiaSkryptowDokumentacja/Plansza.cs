using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Skrypty
{
    /// <summary>
    /// Klasa Plansza odpowiada za logike gry. Przechowuje min. grajacych uzytkownikow, pule wszystkich dostepnych kostek,
    /// oraz wszytskie ciagi znajdujace sie na ekranie gry. 
    /// </summary>
    public class Plansza
    {
        private int idxGracza;
        public List<Gracz> gracze;
        public List<Kostka> pulaKostek;
        public List<List<Kostka>> ciagiKostek;
        public bool czyGraAktywna = true;
        /// <summary>
        /// Konstruktor, ktory inicjalizuje zmienne potrzebne do wykorzystania pola Plansza.
        /// </summary>
        public Plansza() 
        {
            idxGracza = 0;
            gracze = new List<Gracz>();
            pulaKostek = new List<Kostka>();
            ciagiKostek = new List<List<Kostka>>();
        }
        /// <summary>
        /// Inicjalizuje pule kostek na podstawie przekazanej listy, tasuje jej zawartosc, rozdaje kazdemu graczowi po 
        /// 10 kostek na poczatek gry, a nastepnie usuwa przyznane kostki z puli.
        /// </summary>
        public void PrzygotujGre(List<Kostka> noweKostki)
        {
            this.pulaKostek = noweKostki;
            Random rnd = new Random();
            pulaKostek = pulaKostek.OrderBy(k => rnd.Next()).ToList();
            foreach (var gracz in gracze) 
            {
                List<Kostka> kostkiDlaGracza = pulaKostek.GetRange(0, 10);
                gracz.kostkiGracza.AddRange(kostkiDlaGracza);
                pulaKostek.RemoveRange(0, 10);
            }
        }

        /// <summary>
        /// Metoda sprawdzajaca czy ciag wskazany przez uzytkownika tworzy poprawna grupe skladajaca sie z minimum 3 kostek
        /// w dostepnych 4 roznych kolorach, ale o idetycznych wartosciach liczbowych.
        /// </summary>
        /// <param name="ciag"></param>
        /// <returns></returns>
        private bool CzyGrupa(List<Kostka> ciag)
        {
            if (ciag.Count < 3 ) return false;

            int? wartoscGrupy = null;
            List<KolorKostki> koloryGrupy = new List<KolorKostki>();

            foreach (var kostka in ciag)
            {
                if (kostka.czyJoker) continue;

                if (wartoscGrupy == null) wartoscGrupy = kostka.wartosc;

                if (kostka.wartosc != wartoscGrupy || koloryGrupy.Contains(kostka.kolor))
                    return false;

                koloryGrupy.Add(kostka.kolor);
            }
            return true;
        }
        /// <summary>
        /// Metoda sprawdzajaca czy ciag wskazany przez uzytkownika tworzy poprawna serie skladajaca sie z co 
        /// najmniej 3 kostek tego samego koloru uporzadkowanych rosnaco.
        /// </summary>
        /// <param name="ciag"></param>
        /// <returns></returns>
        private bool CzySeria(List<Kostka> ciag)
        {
            if (ciag.Count < 3 || ciag.Count > 13) return false;

            Kostka pierwszaZwykla = ciag.FirstOrDefault(k => !k.czyJoker);
            if (pierwszaZwykla == null) return true;

            KolorKostki kolorSerii = pierwszaZwykla.kolor;

            for (int i = 0; i < ciag.Count - 1; i++)
            {
                if (!ciag[i].czyJoker && ciag[i].kolor != kolorSerii) return false;
                if (!ciag[i + 1].czyJoker && ciag[i + 1].kolor != kolorSerii) return false;

                int vObecna = ciag[i].czyJoker ? WyliczJokera(ciag[i], ciag) : ciag[i].wartosc;
                int vNastepna = ciag[i + 1].czyJoker ? WyliczJokera(ciag[i + 1], ciag) : ciag[i + 1].wartosc;

                if (vNastepna != vObecna + 1 || vObecna < 1 || vNastepna > 13) return false;
            }
            return true;
        }
        /// <summary>
        /// Oblicza i ustawia wartość Jokera na podstawie jego pozycji w układzie na stole
        /// W grupach przyjmuje wartość cyfr grupy. Natomiast w seriach oblicz wartość na podstawie odległości od najbliższej zwykłej kostki. 
        /// </summary>
        /// <param name="joker"></param>
        /// <param name="uklad"></param>
        /// <returns></returns>
        public int WyliczJokera(Kostka joker, List<Kostka> uklad)
        {
            int index = uklad.IndexOf(joker);

            Kostka wzorzec = uklad.FirstOrDefault(k => !k.czyJoker);

            if (wzorzec == null) return 0;

            bool czyToGrupa = uklad.Where(k => !k.czyJoker).All(k => k.wartosc == wzorzec.wartosc);

            if (czyToGrupa)
            {
                joker.wartosc = wzorzec.wartosc;
            }
            else
            {
                int indexWzorca = uklad.IndexOf(wzorzec);
                int roznicaPozycji = index - indexWzorca;
                joker.wartosc = wzorzec.wartosc + roznicaPozycji;
            }

            return joker.wartosc;
        }

        /// <summary>
        /// Metoda dodajaca nowy ciag kostek do zbioru wszystkich istniejacych ciagow. 
        /// </summary>
        /// <param name="nowyCiag"></param>
        public void DodajLubModyfikujCiag(List<Kostka> nowyCiag) 
        {
            ciagiKostek.Add(nowyCiag);
        }
        /// <summary>
        /// Metoda weryfikujaca poprawnosc wprowadzanego ciagu. 
        /// </summary>
        /// <param name="nowyCiag"></param>
        /// <returns></returns>
        public bool CzyPoprawnyRuch(List<Kostka> nowyCiag)
        {
            ciagiKostek.Add(nowyCiag);
            foreach (var ciag in ciagiKostek)
            {
                if ((CzySeria(ciag) != true && CzyGrupa(ciag) != true)||(ciag.Count < 3)) return false;
            }
            return true;
        }
        /// <summary>
        /// Sprawdza globalną poprawność wszytskich kostek leżących na stole. 
        /// Przeszukuje listę układów kostek i weryfikuje czy każda z nich spełnia warunki dotyczące serii lub grupy.
        /// </summary>
        /// <param name="stanStolu"></param>
        /// <returns></returns>
        public bool CzyWszystkieUkladyPoprawne(List<List<Kostka>> stanStolu)
        {
            if (stanStolu.Count == 0) return false;

            foreach (var uklad in stanStolu)
            {
                bool poprawnyUklad = CzySeria(uklad) || CzyGrupa(uklad);

                if (!poprawnyUklad)
                {
                    UnityEngine.Debug.Log("Jeden z układów na stole jest błędny!");
                    return false;
                }
            }
            return true;
        }
        /// <summary>
        /// Pobiera losową kostkę z puli wszystkich kostek. 
        /// Metoda jest wykorzystywana przy dobieraniu, na początku gry oraz przy działaniu kół ratunkowych
        /// 
        /// </summary>
        /// <returns></returns>
        public Kostka WydajLosowaKostkeZPuli()
        {
            if (pulaKostek.Count > 0)
            {
                int index = UnityEngine.Random.Range(0, pulaKostek.Count);
                Kostka wylosowana = pulaKostek[index];

                pulaKostek.RemoveAt(index);

                return wylosowana;
            }
            UnityEngine.Debug.LogWarning("Pula kostek jest pusta!");
            return null;
        }
        /// <summary>
        /// Metoda zwracajaca indeks nastepnego gracza - metoda pomocnicza.
        /// </summary>
        /// <returns></returns>
        public int NastepnyGracz()
        {
            idxGracza = (idxGracza + 1) % gracze.Count;
            return idxGracza;
        }
        /// <summary>
        /// Metoda zwracajaca aktualnego gracza - metoda pomocnicza. 
        /// </summary>
        /// <returns></returns>
        public Gracz AktualnyGracz() { return gracze[idxGracza]; }
    }

}
