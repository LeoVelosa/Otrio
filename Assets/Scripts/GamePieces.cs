using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePieces : MonoBehaviour
{
    
}

//Each tile contains 3 Slots
public class Tile
{
    public Slot[] slots;
    public Tile(Slot s1, Slot s2, Slot s3)
    {
        slots = new Slot[3];
        slots[0] = s1;
        slots[1] = s2;
        slots[2] = s3;
    }
}

//Each slot has a filled and a color 
public class Slot
{
    public string color;
    public bool filled = false;
    public Slot(string color)
    {
        this.color = color;
    }
}