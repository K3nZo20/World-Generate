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
    public float weight;

    // List of available block options for this tile
    public GameObject[] blockOptions;

    public float getWeight()
    {
        return weight;
    }
}
