using System;
using System.Collections.Generic;
using System.Net;
using Jypeli;
using Jypeli.Assets;
using Jypeli.Controls;
using Jypeli.Widgets;

namespace Seikkailu_Pohjois_savossa;

/// @author ville
/// @version 13.10.2024
/// <summary>
/// 
/// </summary>
public class Seikkailu_Pohjois_savossa : PhysicsGame
{
    private const double KAVELY = 200;
    private const double HYPPY = 1000;
    
    public override void Begin()
    {
        
        Camera.ZoomToLevel(20);
        LuoKentta();
        Gravity = new Vector(0.0, -981.0);
        Vector sijainti = new Vector(Level.Left + 100, Level.Bottom + 100);
        LisaaHahmo(sijainti, 71, 226);

        HahmonOhjaus();
        
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }

    void LuoKentta()
    {
        Surface alareuna = Surface.CreateBottom(Level);
        alareuna.Position = new Vector(0, Level.Bottom);
        Add(alareuna);
    }
    
    private void LisaaHahmo(Vector paikka, double leveys, double korkeus)
    {
        PlatformCharacter hahmo = new PlatformCharacter(leveys, korkeus);
        hahmo.Position = paikka;
        hahmo.Mass = 4.0;
        hahmo.Image = LoadImage("hahmo");
        //AddCollisionHandler(pelaaja1, "tahti", TormaaTahteen);
        Add(hahmo);
    }
    // private LisaaHahmo(PhysicsGame peli, double x, double y)
    // {
    //     PlatformCharacter hahmo = new PlatformCharacter(71, 226);
    //     hahmo.Shape = Shape.Rectangle;
    //     hahmo.Position = new Vector(x, y);
    //     hahmo.Tag = "hahmo";
    //     Image hahmonkuva = LoadImage("hahmo"); //Pelaajan kuva
    //     hahmo.Image = hahmonkuva; 
    //     peli.Add(hahmo);
    //     //return hahmo;
    // }
    
    
    private void HahmonOhjaus()
    {
        Keyboard.Listen(Key.D, ButtonState.Down, Liiku, "Hahmo liikkuu oikealle",hahmo, KAVELY);
        Keyboard.Listen(Key.D, ButtonState.Released, Liiku, null, this, hahmo, Vector.Zero);
        Keyboard.Listen(Key.A, ButtonState.Down, Liiku, "Hahmo liikkuu vasemmalle", hahmo, -KAVELY);
        Keyboard.Listen(Key.A, ButtonState.Released, Liiku, null, this, hahmo, Vector.Zero);
        Keyboard.Listen(Key.W,ButtonState.Pressed, Hyppy, "Hahmo hyppää", hahmo,HYPPY);
        Keyboard.Listen(Key.W,ButtonState.Released, Hyppy, null, this, hahmo, Vector.Zero);
        Keyboard.Listen(Key.Space,ButtonState.Pressed, Hyppy, "Hahmo hyppää", hahmo,HYPPY);
        Keyboard.Listen(Key.Space,ButtonState.Released, Hyppy, null, this, hahmo, Vector.Zero);
    }


    private void Liiku(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Walk(nopeus);
    }

    private void Hyppy(PlatformCharacter hahmo, double nopeus)
    {
        hahmo.Jump(nopeus);
    }


    
    
}