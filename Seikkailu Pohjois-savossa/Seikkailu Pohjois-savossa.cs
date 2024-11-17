using System;
using System.Collections.Generic;
using System.Net;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using Silk.NET.GLFW;
using static Jypeli.ButtonState;
using static Jypeli.Color;
using Image = Jypeli.Image;

namespace Seikkailu_Pohjois_savossa;

/// @author ville
/// @version 17.11.2024
/// <summary>
/// Peli, jossa ukkeli seikkailee Pohjois-Savossa ja keräilee vaakunoita ja kalakukkoja
/// samalla väistellen piikkejä
/// </summary>
public class Seikkailu_Pohjois_savossa : PhysicsGame
{
    private const double Kavely = 200;
    private const double Hyppy = 600;
    private PlatformCharacter _hahmo;
    private IntMeter _Pistelaskuri;
    private IntMeter _ElamaPistelaskuri;
    private const int RuudunKoko = 50;
    private readonly Image [] HahmonKavely =LoadImages("hahmo_walk_0", "hahmo_walk_1", "hahmo_walk_2", "hahmo_walk_3","hahmo_walk_0");
    private readonly Image HahmonPaikallaanolo =LoadImage( "hahmo_walk_0");
    private readonly Image HahmonHyppy =LoadImage( "hahmo_jump");
    
    
    public override void Begin()
    {
        LuoKentta();

        Camera.X = 0;
        Camera.Y = 100;
        Camera.Zoom(0.25);
        Gravity = new Vector(0.0, -981.0);
        
        LisaaPistelaskuri();
        LisaaElamaPistelaskuri();
        HahmonOhjaus();
        
        // TODO: silmukka, ks: https://tim.jyu.fi/view/kurssit/tie/ohj1/v/2024/syksy/demot/demo9#poistapisin

    }
    
    
    /// <summary>
    /// Luodaan kenttä valmiista tiedostosta
    /// </summary>
    private void LuoKentta()
    {
        TileMap kentta = TileMap.FromLevelAsset("Kentta_1");
        kentta.SetTileMethod('#', LuoTaso);
        kentta.SetTileMethod('V', LuoVaakuna);
        kentta.SetTileMethod('K', LuoKalakukko);
        kentta.SetTileMethod('P', LuoPiikki);
        kentta.SetTileMethod('H', LisaaHahmo);
        //kentta.Optimize('#');
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
        //_hahmo.AnimFall = new Animation(HahmonTippuminen); //hahmon laskeutumis animaatio
        _hahmo.AnimWalk.Start();
        _hahmo.LoopJumpAnim = false;
        _hahmo.AnimWalk.FPS = 10;
        _hahmo.Position = paikka;
        _hahmo.Mass = 4.0;
        //_hahmo.Image = LoadImage("hahmo_walk_0"); //hahmon kuva tiedosto
        AddCollisionHandler(_hahmo, "vaakuna", TormaaVaakunaan); //lisätään CollisionHandler hahmon ja vaakunan välille, jotta saadaan poistettua vaakuna ja kasvatettua pisteitä 
        AddCollisionHandler(_hahmo, "kalakukko", TormaaKalakukkoon); //lisätään CollisionHandler hahmon ja kalakukon välille, jotta saadaan poistettua vaakuna ja kasvatettua pisteitä ja elämäpisteitä
        AddCollisionHandler(_hahmo, "piikki", TormaaPiikkiin); //lisätään CollisionHandler hahmon ja piikin välille, jotta saadaan vähennettyä hahmon elämäpisteitä ja lopetettua peli, mikäli elämäpisteett loppuvat
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
        PhysicsObject vaakuna = PhysicsObject.CreateStaticObject(49, 56); //lisätään uusi physics object joka on myös staattinen, jotta vaakunat saadaan pysymään paikallaan
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
        PhysicsObject kalakukko = PhysicsObject.CreateStaticObject(68, 32); //lisätään uusi physics object joka on myös staattinen, jotta kalakukot saadaan pysymään paikallaan
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
    /// <param name="piikki">kalakukko joka kerätään</param>
    private void TormaaPiikkiin(PhysicsObject pelaaja, PhysicsObject piikki)
    {
        //pelaaja.Destroy();
        _ElamaPistelaskuri.Value -= 1;
        if (_ElamaPistelaskuri.Value == 0)
        {
            Havisit();
        }
    }

    /// <summary>
    /// Laskuri joka näytää paljonko pelaajalla on pisteitä 
    /// </summary>
    void LisaaPistelaskuri()
    {
        _Pistelaskuri = new IntMeter(0);
        Label pistenaytto = new Label();
        pistenaytto.Position = new Vector(0, Screen.Top - 100);
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
        elamapistenaytto.Position = new Vector(Screen.Right - 300, Screen.Top - 100);
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
    /// Kerrotaan pelaajan hävinneen jos elämäpisteet loppuvat
    /// </summary>
    void Havisit()
    {
        IsPaused = true;
        MessageDisplay.Add("Hävisit pelin");
    }
    
}
