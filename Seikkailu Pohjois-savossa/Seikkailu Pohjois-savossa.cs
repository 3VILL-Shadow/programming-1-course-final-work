using System;
using System.Collections.Generic;
using System.Net;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;
using static Jypeli.Color;

namespace Seikkailu_Pohjois_savossa;

/// @author ville
/// @version 13.10.2024
/// <summary>
/// Peli, jossa ukkeli seikkailee Pohjois-Savossa ja keräilee vaakunoita ja kalakukkoja
/// samalla väistellen piikkejä
/// </summary>
public class Seikkailu_Pohjois_savossa : PhysicsGame
{
    private const double Kavely = 200;
    private const double Hyppy = 500;
    private PlatformCharacter _hahmo;
    private IntMeter _pistelaskuri;
    private const int RuudunKoko = 40;
    
    public override void Begin()
    {
        
        Camera.ZoomToLevel(600);
        LuoKentta();
        Gravity = new Vector(0.0, -981.0);
        
        LisaaHahmo(new Vector(Level.Left + 10, Level.Bottom + 100),71, 226);
        // LuoVaakuna(RandomGen.NextVector(Level.Left, Level.Bottom + 100, Level.Right, Level.Bottom + 400), 74, 85);
        // LuoPiikki(RandomGen.NextVector(Level.Left + 100, Level.Bottom + 75, Level.Right, Level.Bottom + 75), 50, 50);
        // LuoKalakukko(RandomGen.NextVector(Level.Left, Level.Bottom + 100, Level.Right, Level.Bottom + 400), 128, 60);

        LisaaPistelaskuri();
        HahmonOhjaus();
        
    }

    
    private void LuoKentta()
    {
        TileMap kentta = TileMap.FromLevelAsset("kentta_1");
        kentta.SetTileMethod('#', LuoTaso);
        kentta.SetTileMethod('V', LuoVaakuna);
        kentta.SetTileMethod('K', LuoKalakukko);
        kentta.SetTileMethod('P', LuoPiikki);
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
        _hahmo = new PlatformCharacter(leveys, korkeus); //tehdään hahmosta PlatformCharacter
        _hahmo.Position = paikka;
        _hahmo.Mass = 4.0;
        _hahmo.Image = LoadImage("hahmo"); //hahmon kuva tiedosto
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
        PhysicsObject vaakuna = PhysicsObject.CreateStaticObject(leveys, korkeus); //lisätään uusi physics object joka on myös staattinen, jotta vaakunat saadaan pysymään paikallaan
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
        PhysicsObject kalakukko = PhysicsObject.CreateStaticObject(leveys, korkeus); //lisätään uusi physics object joka on myös staattinen, jotta kalakukot saadaan pysymään paikallaan
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
        _pistelaskuri.Value += 1;
    }
    
    
    /// <summary>
    /// Poistetaan kalakukko kun siihen osutaan ja kasvatetaan pistemäärää ja mikäli elämäpisteitä on vähemmän kuin viisi lisätään yksi
    /// </summary>
    /// <param name="pelaaja">hahmo jota liikutellaan</param>
    /// <param name="kalakukko">kalakukko joka kerätään</param>
    private void TormaaKalakukkoon(PhysicsObject pelaaja, PhysicsObject kalakukko)
    {
        kalakukko.Destroy();
        _pistelaskuri.Value += 5;
    } 
    
    /// <summary>
    /// vähennetään hahmon elämäpisteitä ja lopetetaan peli, mikäli elämäpisteett loppuvat
    /// </summary>
    /// <param name="pelaaja">hahmo jota liikutellaan</param>
    /// <param name="piikki">kalakukko joka kerätään</param>
    private void TormaaPiikkiin(PhysicsObject pelaaja, PhysicsObject piikki)
    {
        pelaaja.Destroy();
        MessageDisplay.Add("Hävisit pelin");
    }


    void LisaaPistelaskuri()
    {
        _pistelaskuri = new IntMeter(0);
        Label pistenaytto = new Label();
        pistenaytto.Position = new Vector(0, Level.Top - 100);
        pistenaytto.TextColor = Black;
        pistenaytto.Color = White;
        
        pistenaytto.BindTo(_pistelaskuri);
        pistenaytto.Title = "Pisteet: ";
        Add(pistenaytto);
    }


    /// <summary>
    /// Näppäinkomennot
    /// </summary>
    private void HahmonOhjaus()
    {
        Keyboard.Listen(Key.D, ButtonState.Down, Liiku, "Hahmo liikkuu oikealle",_hahmo, Kavely);
        Keyboard.Listen(Key.A, ButtonState.Down, Liiku, "Hahmo liikkuu vasemmalle", _hahmo, -Kavely);
        Keyboard.Listen(Key.W,ButtonState.Pressed, Hyppaa, "Hahmo hyppää", _hahmo,Hyppy);
        Keyboard.Listen(Key.Space,ButtonState.Pressed, Hyppaa, "Hahmo hyppää", _hahmo,Hyppy);
        
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
        Keyboard.Listen(Key.F1, ButtonState.Pressed, ShowControlHelp, "Näytä näppäimet");
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


    
    
}