using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using Assets.Skrypty;
using System.Collections.Generic;

public class MenuUI : MonoBehaviour
{
    [Header("Referencje do skryptów")]
    public MenedzerKont menedzerKont;
    public PlanszaUI planszaWidok;

    [Header("Pola logowania")]
    public TMP_InputField inputNick;
    public TMP_InputField inputHaslo;

    [Header("Panele do prze³¹czania")]
    public GameObject panelMenuGlowne;
    public GameObject panelStartowy;
    public GameObject panelLogowania;
    public GameObject panelKonta;
    public GameObject interfejsGry;
    public GameObject panelRanking;
    public GameObject panelWygrana;
    public TextMeshProUGUI[] tekstyNickiRanking;
    public TextMeshProUGUI[] tekstyWygraneRanking;

    public TextMeshProUGUI tekstWitajNick;
    public TextMeshProUGUI tekstWygrane;
    public TextMeshProUGUI tekstPrzegrane;
    public TextMeshProUGUI tekstSumaPunktow;
    public TextMeshProUGUI tekstWynik1;
    public TextMeshProUGUI tekstWynik2;
    public TextMeshProUGUI tekstWynik3;
    [Header("Komunikaty")]
    public TextMeshProUGUI tekstBledow;
    public Image obrazekPrzyciskuMute;
    [Header("Ikony Mute")]
    public GameObject ikonaOn;
    public GameObject ikonaOff;

    public GameObject panelRejestracji;

    [Header("Pola Rejestracji")]
    public TMP_InputField inputNickRejestracja;
    public TMP_InputField inputHasloRejestracja;

    [Header("Komunikaty B³êdów")]
    public TextMeshProUGUI tekstBladLogowania;
    public TextMeshProUGUI tekstBladRejestracji;

    private bool jestWyciszony = false;
    public void OtworzLogowanie()
    {
        WszystkiePaneleOff();
        panelLogowania.SetActive(true);
    }
    public void OtworzPanelStartowy()
    {
        WszystkiePaneleOff();
        panelStartowy.SetActive(true);
    }

    public void OtworzMenuGlowne()
    {
        WszystkiePaneleOff();
        panelMenuGlowne.SetActive(true);
    }

    public void OdswiezDanePaneluKonta(string nick)
    {
        var s = menedzerKont.WczytajStaty(nick);

        if (s != null)
        {
            tekstWitajNick.text = "Witaj, " + nick;
            tekstWygrane.text = s.wygraneGry.ToString();
            tekstPrzegrane.text = s.przegraneGry.ToString();
            tekstSumaPunktow.text = s.sumaPunktow.ToString();

            if (tekstWynik1 != null) tekstWynik1.text = "";
            if (tekstWynik2 != null) tekstWynik2.text = "";
            if (tekstWynik3 != null) tekstWynik3.text = "";

            if (s.historiaWynikow != null)
            {

                if (s.historiaWynikow.Count >= 1 && tekstWynik1 != null)
                    tekstWynik1.text = "+"+s.historiaWynikow[0].ToString();

                if (s.historiaWynikow.Count >= 2 && tekstWynik2 != null)
                    tekstWynik2.text = "+" + s.historiaWynikow[1].ToString();

                if (s.historiaWynikow.Count >= 3 && tekstWynik3 != null)
                    tekstWynik3.text = "+" + s.historiaWynikow[2].ToString();
            }
        }
    }
    public void Wyloguj()
    {
        WszystkiePaneleOff();
        panelMenuGlowne.SetActive(true);
    }

    private void WszystkiePaneleOff()
    {
        panelMenuGlowne.SetActive(false);
        panelStartowy.SetActive(false);
        panelLogowania.SetActive(false);
        panelKonta.SetActive(false);
        interfejsGry.SetActive(false);
        panelRanking.SetActive(false);
        panelWygrana.SetActive(false);
        panelRejestracji.SetActive(false);

    }
    public void ObslugaPrzyciskuZaloguj()
    {
        string n = inputNick.text.Trim();
        string h = inputHaslo.text.Trim();

        if (tekstBladLogowania != null) tekstBladLogowania.text = "";

        if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(h))
        {
            if (tekstBladLogowania != null) tekstBladLogowania.text = "Wpisz nick i has³o!";
            return;
        }

        var uzytkownicy = menedzerKont.PobierzWszystkichUzytkownikow();
        bool czyIstnieje = uzytkownicy.Any(x => x.nick == n);

        if (!czyIstnieje)
        {
            if (tekstBladLogowania != null) tekstBladLogowania.text = "U¿ytkownik nie istnieje!";
            return;
        }

        DaneUzytkownika konto = menedzerKont.Zaloguj(n, h);
        if (konto != null)
        {
            PrzejdzDoKonta(n, h);
        }
        else
        {
            if (tekstBladLogowania != null) tekstBladLogowania.text = "Nieprawid³owe has³o!";
        }
    }

    private void WyswietlBlad(string tresc)
    {
        if (tekstBledow != null) tekstBledow.text = tresc;
        Debug.LogWarning(tresc);
    }

    private void PrzejdzDoKonta(string n, string h)
    {
        var konto = menedzerKont.Zaloguj(n, h);

        if (konto != null)
        {
            OdswiezDanePaneluKonta(konto.nick);
            WszystkiePaneleOff();
            panelKonta.SetActive(true);
        }
    }

    public void OtworzRanking()
    {
        WszystkiePaneleOff();
        panelRanking.SetActive(true);

        List<WpisRanking> lista = menedzerKont.PobierzRanking();

        for (int i = 0; i < tekstyNickiRanking.Length; i++)
        {
            tekstyNickiRanking[i].text = "-";
            tekstyWygraneRanking[i].text = "0";
        }

        int ileGraczy = Mathf.Min(lista.Count, tekstyNickiRanking.Length);
        for (int i = 0; i < ileGraczy; i++)
        {
            tekstyNickiRanking[i].text = lista[i].nick;
            tekstyWygraneRanking[i].text = lista[i].wygraneGry.ToString();
        }
    }
    public void StartSzybkaGra()
    {
        WszystkiePaneleOff();
        panelMenuGlowne.SetActive(false);
        interfejsGry.SetActive(true);
        planszaWidok.RozpocznijGre();
    }

    public void ObslugaPrzyciskuStworzKonto()
    {
        string n = inputNickRejestracja.text.Trim();
        string h = inputHasloRejestracja.text.Trim();

        if (tekstBladRejestracji != null) tekstBladRejestracji.text = "";

        if (string.IsNullOrEmpty(n) || string.IsNullOrEmpty(h))
        {
            if (tekstBladRejestracji != null) tekstBladRejestracji.text = "Uzupe³nij wszystkie pola!";
            return;
        }

        bool sukces = menedzerKont.RejestrujUzytkownika(n, h);

        if (sukces)
        {
            Debug.Log("Konto utworzone!");
            PrzejdzDoKonta(n, h);
        }
        else
        {
            if (tekstBladRejestracji != null) tekstBladRejestracji.text = "Ten nick jest ju¿ zajêty!";
        }
    }

    public void OtworzPanelRejestracji()
    {
        WszystkiePaneleOff();
        panelRejestracji.SetActive(true);
    }

    public void PrzelaczMute()
    {
        jestWyciszony = !jestWyciszony;
        AudioListener.pause = jestWyciszony;

        if (ikonaOn != null && ikonaOff != null)
        {
            ikonaOn.SetActive(!jestWyciszony);
            ikonaOff.SetActive(jestWyciszony);
        }
    }
}