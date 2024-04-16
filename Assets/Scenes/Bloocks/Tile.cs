using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    public Tile[] upNeighbours;
    public Tile[] downNeighbours;
    public Tile[] leftNeighbours;
    public Tile[] rightNeighbours;

    // Property to track the prevalence of the tile
    public float weight = 1;

    public float getWeight()
    {
        return weight;
    }
}
