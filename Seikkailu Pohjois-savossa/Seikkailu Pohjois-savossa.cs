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
    private const double HYPPY = 500;
    private PlatformCharacter hahmo;
    
    public override void Begin()
    {
        
        Camera.ZoomToLevel(20);
        LuoKentta();
        Gravity = new Vector(0.0, -981.0);
        
        LisaaHahmo(new Vector(Level.Left + 100, Level.Bottom + 100),71, 226);
        LuoVaakuna(RandomGen.NextVector(Level.Left, Level.Bottom + 100, Level.Right, Level.Bottom + 400), 74, 85);
        HahmonOhjaus();
        
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }

    void LuoKentta()
    {
        Surface alareuna = Surface.CreateBottom(Level);
        alareuna.Position = new Vector(0, Level.Bottom);
        Level.CreateBorders();
        Add(alareuna);
    }
    
    private void LisaaHahmo(Vector paikka, double leveys, double korkeus)
    {
        hahmo = new PlatformCharacter(leveys, korkeus);
        hahmo.Position = paikka;
        hahmo.Mass = 4.0;
        hahmo.Image = LoadImage("hahmo");
        AddCollisionHandler(hahmo, "vaakuna", TormaaVaakunaan);
        Add(hahmo);
    }
    
    
    private void LuoVaakuna(Vector paikka, double leveys, double korkeus)
    {
        PhysicsObject vaakuna = PhysicsObject.CreateStaticObject(leveys, korkeus);
        vaakuna.IgnoresCollisionResponse = true;
        vaakuna.Position = paikka;
        vaakuna.Image = LoadImage("vaakuna");
        vaakuna.Tag = "vaakuna";
        Add(vaakuna);
    }
    
    
    private void TormaaVaakunaan(PhysicsObject pelaaja, PhysicsObject vaakuna)
    {
        vaakuna.Destroy();
    }
    
    
    private void HahmonOhjaus()
    {
        Keyboard.Listen(Key.D, ButtonState.Down, Liiku, "Hahmo liikkuu oikealle",hahmo, KAVELY);
        Keyboard.Listen(Key.A, ButtonState.Down, Liiku, "Hahmo liikkuu vasemmalle", hahmo, -KAVELY);
        Keyboard.Listen(Key.W,ButtonState.Pressed, Hyppy, "Hahmo hyppää", hahmo,HYPPY);
        Keyboard.Listen(Key.Space,ButtonState.Pressed, Hyppy, "Hahmo hyppää", hahmo,HYPPY);
    }


    private void Liiku(PlatformCharacter pelaaja, double nopeus)
    {
        pelaaja.Walk(nopeus);
    }

    private void Hyppy(PlatformCharacter pelaaja, double nopeus)
    {
        pelaaja.Jump(nopeus);
    }


    
    
}