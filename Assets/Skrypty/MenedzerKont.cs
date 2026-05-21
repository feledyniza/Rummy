using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System.Linq;

namespace Assets.Skrypty
{
    /// <summary>
    /// Porządkowanie informacji w plikach bazy danych.
    /// </summary>
    [Serializable] public class WpisUzytkownik { public string nick; public string haslo; }
    [Serializable] public class WpisStatystyki { public string nick; public int wygraneGry; public int przegraneGry; public int sumaPunktow; public List<int> historiaWynikow = new List<int>(); }
    [Serializable] public class WpisRanking { public string nick; public int miejsce; public int wygraneGry; public int sumaPunktow; }

    [Serializable] public class ListU { public List<WpisUzytkownik> lista = new List<WpisUzytkownik>(); }
    [Serializable] public class ListS { public List<WpisStatystyki> lista = new List<WpisStatystyki>(); }
    [Serializable] public class ListR { public List<WpisRanking> lista = new List<WpisRanking>(); }

    public class MenedzerKont : MonoBehaviour
    {
        private string sciezkaU, sciezkaS, sciezkaR;
        /// <summary>
        /// Inicjalizuje ścieżki dostępu do plików bazy danych.
        /// Tworzy folder bazy danych jeżeli jeszcze nie istnieje.
        /// </summary>
        void Awake()
        {
            string dir = Application.persistentDataPath + "/BazaDanych/";
            if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);

            sciezkaU = dir + "uzytkownicy.json";
            sciezkaS = dir + "statystyki.json";
            sciezkaR = dir + "ranking.json";
            Debug.Log("Dane są tutaj: " + dir);
        }
        /// <summary>
        /// Tworzy nowy profil gracza.
        /// Sprawdza unikalność nicku, a następnie tworzy wpisy w 3 plikach:
        /// użytkownicy, statystyki i ranking.
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="haslo"></param>
        /// <returns></returns>
        public bool RejestrujUzytkownika(string nick, string haslo)
        {
            var uzytkownicy = Wczytaj<ListU>(sciezkaU);
            if (uzytkownicy.lista.Any(x => x.nick == nick)) return false;

            uzytkownicy.lista.Add(new WpisUzytkownik { nick = nick, haslo = haslo });
            Zapisz(sciezkaU, uzytkownicy);

            var staty = Wczytaj<ListS>(sciezkaS);
            staty.lista.Add(new WpisStatystyki { nick = nick });
            Zapisz(sciezkaS, staty);

            var ranking = Wczytaj<ListR>(sciezkaR);
            ranking.lista.Add(new WpisRanking { nick = nick });
            Zapisz(sciezkaR, ranking);

            return true;
        }
        /// <summary>
        /// Weryfikuje poprawność danych logowania. Po pomyślnym sprawdzeniu hasła wczytuje i zwraca pełny obiekt 
        /// DaneUzytkownika wraz z jego statystykami.
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="haslo"></param>
        /// <returns></returns>
        public DaneUzytkownika Zaloguj(string nick, string haslo)
        {
            var uzytkownicy = Wczytaj<ListU>(sciezkaU);
            var u = uzytkownicy.lista.FirstOrDefault(x => x.nick == nick && x.haslo == haslo);

            if (u == null) return null;

            var dane = new DaneUzytkownika(u.nick, u.haslo);
            var s = Wczytaj<ListS>(sciezkaS).lista.FirstOrDefault(x => x.nick == nick);
            if (s != null)
            {
                dane.sumaPunktow = s.sumaPunktow;
                dane.wygraneGry = s.wygraneGry;
                dane.przegraneGry = s.przegraneGry;
            }
            return dane;
        }
        /// <summary>
        /// Zwraca listę wszystkich zarejestrowanych kont z pliku uzytkownicy.
        /// </summary>
        /// <returns></returns>
        public List<WpisUzytkownik> PobierzWszystkichUzytkownikow()
        {
            var u = Wczytaj<ListU>(sciezkaU);
            return u.lista;
        }
        /// <summary>
        /// Kluczowa metoda po zakończeniu partii. Aktualizuje sumę punktów, licznik wygranych/przegranych
        /// oraz dopisuje wynik do historii 3 ostatnich rozgrywek. 
        /// Dodatkowo przelicza pozycje w rankingu wszystkich graczy.
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="punktyZTejPartii"></param>
        /// <param name="czyWygral"></param>
        public void AktualizujStatystyki(string nick, int punktyZTejPartii, bool czyWygral)
        {
            var s = Wczytaj<ListS>(sciezkaS);
            var graczS = s.lista.FirstOrDefault(x => x.nick == nick);
            if (graczS != null)
            {
                graczS.historiaWynikow.Insert(0, punktyZTejPartii);
                if (graczS.historiaWynikow.Count > 3) graczS.historiaWynikow.RemoveAt(3);

                graczS.sumaPunktow += punktyZTejPartii;
                if (czyWygral) graczS.wygraneGry++; else graczS.przegraneGry++;

                Zapisz(sciezkaS, s);
            }

            var r = Wczytaj<ListR>(sciezkaR);
            var graczR = r.lista.FirstOrDefault(x => x.nick == nick);
            if (graczR != null)
            {
                graczR.sumaPunktow += punktyZTejPartii;
                if (czyWygral) graczR.wygraneGry++;

                var posortowani = r.lista.OrderByDescending(x => x.wygraneGry).ThenByDescending(x => x.sumaPunktow).ToList();
                for (int i = 0; i < posortowani.Count; i++) posortowani[i].miejsce = i + 1;

                r.lista = posortowani;
                Zapisz(sciezkaR, r);
            }
        }
        /// <summary>
        /// Pobiera statystyki dla konkretnego gracza.
        /// </summary>
        /// <param name="nick"></param>
        /// <returns></returns>
        public WpisStatystyki WczytajStaty(string nick)
        {
            var s = Wczytaj<ListS>(sciezkaS);
            return s.lista.FirstOrDefault(x => x.nick == nick);
        }
        /// <summary>
        /// Metoda zmieniajaca dowolny obiekt do formatu JSON.
        /// Zapisuje go fizycznie na dysku urządzenia.
        /// </summary>
        /// <param name="sciezka"></param>
        /// <param name="obiekt"></param>
        private void Zapisz(string sciezka, object obiekt)
        {
            string json = JsonUtility.ToJson(obiekt, true);
            File.WriteAllText(sciezka, json);
        }
        /// <summary>
        /// Zwraca posortowaną listę graczy do wyświetlenia w tabeli wyników.
        /// </summary>
        /// <returns></returns>
        public List<WpisRanking> PobierzRanking()
        {
            ListR dane = Wczytaj<ListR>(sciezkaR);

            if (dane == null || dane.lista == null)
            {
                return new List<WpisRanking>();
            }

            return dane.lista;
        }
        /// <summary>
        /// Metoda wczytująca plik tekstowy i zmieniająca go na obiekt wybranej klasy. 
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="sciezka"></param>
        /// <returns></returns>
        private T Wczytaj<T>(string sciezka) where T : new()
        {
            if (!File.Exists(sciezka)) return new T();
            string json = File.ReadAllText(sciezka);
            return JsonUtility.FromJson<T>(json);
        }
    }
}