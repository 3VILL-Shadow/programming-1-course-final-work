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
    private readonly Vector liikeVasemmalle = new Vector(-100, 0);
    private readonly Vector liikeOikealle = new Vector(100, 0);
    public override void Begin()
    {
        
        Camera.ZoomToLevel(20);
        LuoKentta();
        PhysicsObject hahmo = LisaaHahmo(this, Level.Left + 100, Level.Bottom + 100);

        //HahmonOhjaus(hahmo);
        Keyboard.Listen(Key.D, ButtonState.Down, Liiku, "Hahmo liikkuu oikealle", this, hahmo, liikeOikealle);
        Keyboard.Listen(Key.D, ButtonState.Released, Liiku, null, this, hahmo, Vector.Zero);
        Keyboard.Listen(Key.A, ButtonState.Down, Liiku, "Hahmo liikkuu vasemmalle", this, hahmo, liikeVasemmalle);
        Keyboard.Listen(Key.A, ButtonState.Released, Liiku, null, this, hahmo, Vector.Zero);
        
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }

    void LuoKentta()
    {
        Surface alareuna = Surface.CreateBottom(Level);
        alareuna.Position = new Vector(0, Level.Bottom);
        Add(alareuna);
    }
    public static PhysicsObject LisaaHahmo(PhysicsGame peli, double x, double y)
    {
        PhysicsObject hahmo = new PhysicsObject(71, 226);
        hahmo.Shape = Shape.Rectangle;
        hahmo.Position = new Vector(x, y);
        hahmo.Restitution = 0;
        hahmo.CanRotate = false; //Pidetaan hahmo pystyssa
        hahmo.Tag = "hahmo";
        Image hahmonkuva = LoadImage("hahmo"); //Pelaajan kuva
        hahmo.Image = hahmonkuva; // Laitetaan kuva linnusta pallon päälle
        hahmo.MaxVelocity = 500;  // Linnun maksiminopeus
        peli.Add(hahmo);
        return hahmo;
    }
    
    
    // private void HahmonOhjaus(PhysicsObject hahmo)
    // {
    //    
    // }


    public static void Liiku(PhysicsGame peli, PhysicsObject hahmo, Vector nopeus)
    {
        hahmo.Velocity = nopeus;
    }


    
    
}