using Jypeli;
using static Jypeli.ButtonState;
using static Jypeli.Color;
using Image = Jypeli.Image;

namespace Seikkailu_Pohjois_savossa;

/// @author ville
/// @version 19.11.2024
/// <summary>
/// Peli, jossa ukkeli seikkailee Pohjois-Savossa ja keräilee vaakunoita ja kalakukkoja
/// samalla väistellen piikkejä
/// </summary>
public class Seikkailu_Pohjois_savossa : PhysicsGame
{
    private const double Kavely = 200;
    private const double Hyppy = 800;
    private PlatformCharacter _hahmo;
    private IntMeter _Pistelaskuri;
    private IntMeter _ElamaPistelaskuri;
    private const int RuudunKoko = 50;
    private readonly Image[] HahmonKavely =LoadImages("hahmo_walk_0", "hahmo_walk_1", "hahmo_walk_2", "hahmo_walk_3","hahmo_walk_0");
    private readonly Image HahmonPaikallaanolo =LoadImage( "hahmo_walk_0");
    private readonly Image HahmonHyppy =LoadImage( "hahmo_jump");
    private int _KentanNro = 2;
    
    
    public override void Begin()
    {
        IsPaused = false;
        Kentta();

        
        Camera.ZoomToAllObjects(-866);
        SetWindowSize(1075, 770, false);
        Gravity = new Vector(0.0, -981.0);
        
        // TODO: silmukka, ks: https://tim.jyu.fi/view/kurssit/tie/ohj1/v/2024/syksy/demot/demo9#poistapisin
    }



    /// <summary>
    /// Valitaan kenttä kenttä listauksesta ja luodaan uusi kentta kun on törmätty maaliin
    /// </summary>
    private void Kentta()
    {
        ClearGameObjects();

        LuoKentta($"Kentta_{_KentanNro}");
        
        LisaaPistelaskuri();
        LisaaElamaPistelaskuri();
        HahmonOhjaus();
        
    }
    
    /// <summary>
    /// Luodaan kenttä valmiista tiedostosta
    /// </summary>
    private void LuoKentta(string kentanTiedosto)
    {
        TileMap kentta = TileMap.FromLevelAsset(kentanTiedosto);
        kentta.SetTileMethod('#', LuoTaso);
        kentta.SetTileMethod('V', LuoVaakuna);
        kentta.SetTileMethod('K', LuoKalakukko);
        kentta.SetTileMethod('P', LuoPiikki);
        kentta.SetTileMethod('H', LisaaHahmo);
        kentta.SetTileMethod('M', LisaaMaali);
        kentta.SetTileMethod('W', LisaaViimeinenMaali);
        kentta.Optimize('#');
        kentta.Execute(RuudunKoko, RuudunKoko);
        Level.CreateBorders(false);
    }
      
    
    /// <summary>
    /// Lisätään hahmo peliin vasempaan ala nurkaan
    /// </summary>
    /// <param name="paikka">hahmon paikka</param>
    /// <param name="leveys">hahmon leveys</param>
    /// <param name="korkeus">hahmon korkeus</param>
    private void LisaaHahmo(Vector paikka, double leveys, double korkeus)
    {
        _hahmo = new PlatformCharacter(71, 226); //tehdään hahmosta PlatformCharacter
        _hahmo.AnimWalk = new Animation(HahmonKavely); //hahmon kävely animaatio
        _hahmo.AnimIdle = new Animation(HahmonPaikallaanolo); //hahmon paikallaan oli animaatio
        _hahmo.AnimJump = new Animation(HahmonHyppy); //hahmon hyppy animaatio
        _hahmo.LoopJumpAnim = false;
        _hahmo.AnimWalk.FPS = 10;
        _hahmo.Position = paikka;
        _hahmo.Mass = 4.0;
        AddCollisionHandler(_hahmo, "vaakuna", TormaaVaakunaan); //lisätään CollisionHandler hahmon ja vaakunan välille, jotta saadaan poistettua vaakuna ja kasvatettua pisteitä 
        AddCollisionHandler(_hahmo, "kalakukko", TormaaKalakukkoon); //lisätään CollisionHandler hahmon ja kalakukon välille, jotta saadaan poistettua vaakuna ja kasvatettua pisteitä ja elämäpisteitä
        AddCollisionHandler(_hahmo, "piikki", TormaaPiikkiin); //lisätään CollisionHandler hahmon ja piikin välille, jotta saadaan vähennettyä hahmon elämäpisteitä ja lopetettua peli, mikäli elämäpisteett loppuvat
        AddCollisionHandler(_hahmo, "maali", TormaaMaaliin); //lisätään CollisionHandler hahmon ja maalin välille, jotta päästään siirtymään seuraavaan tasoon
        AddCollisionHandler(_hahmo, "viimeinenMaali", TormaaViimeiseenMaaliin); //lisätään CollisionHandler hahmon ja maalin välille, jotta päästään siirtymään seuraavaan tasoon
        Add(_hahmo);
    }
    
    
    /// <summary>
    /// Luodaan vaakuna
    /// </summary>
    /// <param name="paikka">vaakunan paikka</param>
    /// <param name="leveys">vaakunan leveys</param>
    /// <param name="korkeus">vaakunan korkeus</param>
    private void LuoVaakuna(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject vaakuna = PhysicsObject.CreateStaticObject(73.5, 84); //lisätään uusi physics object joka on myös staattinen, jotta vaakunat saadaan pysymään paikallaan
        vaakuna.IgnoresCollisionResponse = true;
        vaakuna.Position = paikka;
        vaakuna.Image = LoadImage("vaakuna"); //vaakunan kuva tiedosto
        vaakuna.Tag = "vaakuna";
        Add(vaakuna);
    }
    
    
    /// <summary>
    /// Luodaan kalakukko
    /// </summary>
    /// <param name="paikka">kalakukon paikka</param>
    /// <param name="leveys">kalakukon leveys</param>
    /// <param name="korkeus">kalakukon korkeus</param>
    private void LuoKalakukko(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject kalakukko = PhysicsObject.CreateStaticObject(102, 48); //lisätään uusi physics object joka on myös staattinen, jotta kalakukot saadaan pysymään paikallaan
        kalakukko.IgnoresCollisionResponse = true;
        kalakukko.Position = paikka;
        kalakukko.Image = LoadImage("kalakukko"); //kalakukon kuva tiedosto
        kalakukko.Tag = "kalakukko";
        Add(kalakukko);
    }
    
    /// <summary>
    /// Luodaan piikki jota pitää varoa
    /// </summary>
    /// <param name="paikka">piikin paikka</param>
    /// <param name="leveys">piikin leveys</param>
    /// <param name="korkeus">piikin korkeus</param>
    private void LuoPiikki(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject piikki = PhysicsObject.CreateStaticObject(leveys, korkeus); //lisätään uusi physics object joka on myös staattinen, jotta piikit saadaan pysymään paikallaan
        piikki.Shape = Shape.Triangle;
        piikki.Color = Black;
        piikki.IgnoresCollisionResponse = false;
        piikki.Position = paikka;
        piikki.Tag = "piikki";
        Add(piikki);
    }
    
    /// <summary>
    /// Luodaan taso joita saadaan tehtyä maa ja tasot joilta hypitään toisille tasoille
    /// </summary>
    /// <param name="paikka">Tason paikka</param>
    /// <param name="leveys">Tason leveys</param>
    /// <param name="korkeus">Tason korkeus</param>
    private void LuoTaso(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject taso = PhysicsObject.CreateStaticObject(leveys, korkeus);
        taso.Position = paikka;
        taso.Color = Green;
        Add(taso);
    }
    
    
    /// <summary>
    /// Luodaan maali johon törmätessä siirrytään seuraavaan tasoon
    /// </summary>
    /// <param name="paikka">Maalin paikka</param>
    /// <param name="leveys">Maalin leveys</param>
    /// <param name="korkeus">Maalin korkeus</param>
    private void LisaaMaali(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject maali = PhysicsObject.CreateStaticObject(65, 177);
        maali.IgnoresCollisionResponse = true;
        maali.Position = paikka;
        maali.Image = LoadImage("maali");
        maali.Tag = "maali";
        Add(maali);
    }

    /// <summary>
    /// Luodaan maali johon törmätessä lopetetaan peli
    /// </summary>
    /// <param name="paikka">viimeisen maalin paikka</param>
    /// <param name="leveys">viimeisen maalin leveys</param>
    /// <param name="korkeus">viimeisen maalin korkeus</param>
    private void LisaaViimeinenMaali(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject viimeinenMaali = PhysicsObject.CreateStaticObject(65, 177);
        viimeinenMaali.IgnoresExplosions = true;
        viimeinenMaali.Position = paikka;
        viimeinenMaali.Image = LoadImage("maali");
        viimeinenMaali.Tag = "viimeinenMaali";
        Add(viimeinenMaali);
    }
    
    /// <summary>
    /// Poistetaan vaakuna kun siihen osutaan ja kasvatetaan pistemäärää
    /// </summary>
    /// <param name="pelaaja">hahmo jota liikutellaan</param>
    /// <param name="vaakuna">vaakuna joka kerätään</param>
    private void TormaaVaakunaan(PhysicsObject pelaaja, PhysicsObject vaakuna)
    {
        vaakuna.Destroy();
        _Pistelaskuri.Value += 1;
    }
    
    
    /// <summary>
    /// Poistetaan kalakukko kun siihen osutaan ja kasvatetaan pistemäärää ja mikäli elämäpisteitä on vähemmän kuin viisi lisätään yksi
    /// </summary>
    /// <param name="pelaaja">hahmo jota liikutellaan</param>
    /// <param name="kalakukko">kalakukko joka kerätään</param>
    private void TormaaKalakukkoon(PhysicsObject pelaaja, PhysicsObject kalakukko)
    {
        kalakukko.Destroy();
        _Pistelaskuri.Value += 5;
        if (_ElamaPistelaskuri.Value < 5)
        {
            _ElamaPistelaskuri.Value += 1;
        }
    } 
    
    /// <summary>
    /// Vähennetään hahmon elämäpisteitä ja lopetetaan peli, mikäli elämäpisteett loppuvat
    /// </summary>
    /// <param name="pelaaja">hahmo jota liikutellaan</param>
    /// <param name="piikki">Piikki jota pitää varoa</param>
    private void TormaaPiikkiin(PhysicsObject pelaaja, PhysicsObject piikki)
    {
        _ElamaPistelaskuri.Value -= 1;
        if (_ElamaPistelaskuri.Value == 0)
        {
            Havisit();
        }
    }
    /// <summary>
    /// Kasvatetaan muuttujan _KentanIndeksi arvoa jotta päästään seuraavaan tasoon
    /// </summary>
    /// <param name="pelaaja">hahmo jota liikutellaan</param>
    /// <param name="maali">maali josta törmäämisen jälkeen päästään seuraavaan tasoon</param>
    private void TormaaMaaliin(PhysicsObject pelaaja, PhysicsObject maali)
    {
        _KentanNro++;
        Kentta();
    }

    /// <summary>
    /// Kun on päästy viimeisen tason maaliin, niin pysäytetään peli ja kerrotaan pelaajan voittaneen peli
    /// </summary>
    /// <param name="pelaaja">hahmo jota liikutellaan</param>
    /// <param name="maali">maali johon törmättyään pelaajalle kerrotaan hänen voittaneensa peli</param>
    private void TormaaViimeiseenMaaliin(PhysicsObject pelaaja, PhysicsObject maali)
    {
        IsPaused = true;
        MessageDisplay.Add("Voiti pelin kaikki tasot! Voit nyt olla onnellinen!");
    }
    
    
    /// <summary>
    /// Laskuri joka näytää paljonko pelaajalla on pisteitä 
    /// </summary>
    void LisaaPistelaskuri()
    {
        _Pistelaskuri = new IntMeter(0);
        Label pistenaytto = new Label();
        pistenaytto.Position = new Vector(0, Screen.Top - 40);
        pistenaytto.TextColor = Black;
        pistenaytto.Color = White;
        
        pistenaytto.BindTo(_Pistelaskuri);
        pistenaytto.Title = "Pisteet: ";
        Add(pistenaytto);
    }
    
    /// <summary>
    /// Laskuri joka näyttää paljonko pelaajalla on elämäpisteitä
    /// </summary>
    void LisaaElamaPistelaskuri()
    {
        _ElamaPistelaskuri = new IntMeter(5, 0, 5);
        Label elamapistenaytto = new Label();
        elamapistenaytto.Position = new Vector(Screen.Right - 100, Screen.Top - 40);
        elamapistenaytto.TextColor = Black;
        elamapistenaytto.Color = White;
        
        elamapistenaytto.BindTo(_ElamaPistelaskuri);
        elamapistenaytto.Title = "Elämä pisteet: ";
        Add(elamapistenaytto);
    }


    /// <summary>
    /// Näppäinkomennot
    /// </summary>
    private void HahmonOhjaus()
    {
        Keyboard.Listen(Key.D, Down, Liiku, "Hahmo liikkuu oikealle",_hahmo, Kavely);
        Keyboard.Listen(Key.A, Down, Liiku, "Hahmo liikkuu vasemmalle", _hahmo, -Kavely);
        Keyboard.Listen(Key.W,Pressed, Hyppaa, "Hahmo hyppää", _hahmo,Hyppy);
        Keyboard.Listen(Key.Space,Pressed, Hyppaa, "Hahmo hyppää", _hahmo,Hyppy);
        
        Keyboard.Listen(Key.Escape, Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.F1, Pressed, ShowControlHelp, "Näytä näppäimet");
    }

    
    /// <summary>
    /// hahmon kävely
    /// </summary>
    /// <param name="pelaaja">hahmo jota liikutellaan</param>
    /// <param name="nopeus">nopeus jolla hahmo liikkuu</param>
    private void Liiku(PlatformCharacter pelaaja, double nopeus)
    {
        pelaaja.Walk(nopeus);
    }

    
    /// <summary>
    /// hahmon hyppy
    /// </summary>
    /// <param name="pelaaja">hahmo joka hyppää</param>
    /// <param name="hyppyVoima">voima jolla hahmo hyppää</param>
    private void Hyppaa(PlatformCharacter pelaaja, double hyppyVoima)
    {
        pelaaja.Jump(hyppyVoima);
    }


    /// <summary>
    /// Mikäli pelaaja kuolee hän voi aloittaa pelin uudesta ilman pelin uudelleen käynnistämistä
    /// </summary>
    void AloitaAlusta()
    {
        ClearGameObjects();
        Begin();
    }


    /// <summary>
    /// Kerrotaan pelaajan hävinneen jos elämäpisteet loppuvat
    /// </summary>
    void Havisit()
    {
        _hahmo.Destroy();
        IsPaused = true;
        MessageDisplay.Add("Kuolit, voit aloittaa pelin alusta painamalla 'R' näppäintä");
        ClearControls();
        Keyboard.Listen(Key.Escape, Pressed, ConfirmExit, "Lopeta peli");

        if (IsPaused)
        { 
            Keyboard.Listen(Key.R,Pressed, AloitaAlusta, "Aloita peli alusta");
        }
    }
    
}
