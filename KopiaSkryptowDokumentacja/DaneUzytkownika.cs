using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Assets.Skrypty
{
    [Serializable]
    public class DaneUzytkownika
    {
        public string nick;
        public string haslo;
        public int sumaPunktow;
        public int wygraneGry;
        public int przegraneGry;
        public DaneUzytkownika() { }
        /// <summary>
        /// Tworzy profil gracza zawierający nick, hasło oraz aktualne liczniki punktów, 
        /// rozegranych partii - tych przegranych i wygranych.
        /// </summary>
        /// <param name="nick"></param>
        /// <param name="haslo"></param>
        public DaneUzytkownika(string nick, string haslo)
        {
            this.nick = nick;
            this.haslo = haslo;
            this.sumaPunktow = 0;
            this.wygraneGry = 0;
            this.przegraneGry = 0;
        }
    }
}
