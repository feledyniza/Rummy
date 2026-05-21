//using UnityEngine;

//public abstract class KoloRatunkowe
//{
//    protected PlanszaUI planszaUI;
//    protected bool czyUzyte = false;

//    public KoloRatunkowe(PlanszaUI ui) { planszaUI = ui; }
//    public abstract void uzyj();
//}

//public class UzupelnijBrakujacyCiag : KoloRatunkowe
//{
//    public UzupelnijBrakujacyCiag(PlanszaUI ui) : base(ui) { }
//    public override void uzyj()
//    {
//        if (czyUzyte) return;
//        planszaUI.UzupelnijCiag();
//        czyUzyte = true;
//    }
//}

//public class ZablokujPrzeciwnika : KoloRatunkowe
//{
//    public ZablokujPrzeciwnika(PlanszaUI ui) : base(ui) { }
//    public override void uzyj()
//    {
//        if (czyUzyte) return;
//        planszaUI.Blokada();
//        czyUzyte = true;
//    }
//}

//public class ZmienKolor : KoloRatunkowe
//{
//    public ZmienKolor(PlanszaUI ui) : base(ui) { }
//    public override void uzyj()
//    {
//        if (czyUzyte) return;
//        planszaUI.ZmienKolor();
//        czyUzyte = true;
//    }
//}