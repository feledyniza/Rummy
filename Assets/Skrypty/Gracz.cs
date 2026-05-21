using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Skrypty
{
    /// <summary>
    /// Klasa abstrakcyjna reprezentująca gracza w grze.
    /// Przechowuje podstawowe informacje.
    /// </summary>
    public abstract class Gracz
    {
        public string nick;
        public List<Kostka> kostkiGracza;
        public int iloscPunktow;
        public bool[] uzyteKola = new bool[3] { false, false, false };
        public bool czyPierwszeWylozenieZrobione = false;
        /// <summary>
        /// Konstruktor inicjalizujący podstawowe dane gracza.
        /// </summary>
        public Gracz()
        {
            nick = " ";
            kostkiGracza = new List<Kostka>();
        }
        /// <summary>
        /// Metoda dodająca metody do puli kostek gracza.
        /// </summary>
        public void DobierzKostke(Kostka nowaKostka)
        {
            kostkiGracza.Add(nowaKostka);
        }
    }
    /// <summary>
    /// Klasa reprezentująca gracza sterowanego przez użytkownika.
    /// Dziedziczy po klasie Gracz i rozszerza ją o dodatkowe mechaniki pomocnicze.
    /// </summary>
    public class Uzytkownik : Gracz
    {
        public DaneUzytkownika daneKonta;
        public Uzytkownik(DaneUzytkownika konto) : base()
        {
            this.daneKonta = konto;
            this.nick = konto.nick;
            this.kostkiGracza = new List<Kostka>();
        }
        public Uzytkownik() : base()
        {
            this.kostkiGracza = new List<Kostka>();
        }
    }
}