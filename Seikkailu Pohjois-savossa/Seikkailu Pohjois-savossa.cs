using System;
using System.Collections.Generic;
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
    static Image hahmonkuva = LoadImage( "hahmo.png" );
    
    public override void Begin()
    {
        // Kirjoita ohjelmakoodisi tähän

        LuoKentta();
       
        Keyboard.Listen(Key.Escape, ButtonState.Pressed, ConfirmExit, "Lopeta peli");
    }

    void LuoKentta()
    {
        PhysicsObject hahmo = HahmoCollision(this, 100, 200);
        hahmo.Image = hahmonkuva;
        Add(hahmo);
    }


    public static PhysicsObject HahmoCollision(PhysicsGame peli, int width, int height)
    {
        PhysicsObject hahmo = PhysicsObject.CreateStaticObject(20.0, 100.0);
        hahmo.Shape = Shape.Rectangle;
        return hahmo;
    }

}