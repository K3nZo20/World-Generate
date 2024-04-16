using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WaveFunction : MonoBehaviour
{
    public int dimensions;
    public Tile[] tileObjects;
    public List<Cell> gridComponents;
    public Cell cellObj;
    private Vector3 playerPosition;
    private Moving Position;

    int iterations = 0;

    void Awake()
    {
        gridComponents = new List<Cell>();
        InitializeGrid();
        Position = FindObjectOfType<Moving>();

    }

    void Update()
    {
        playerPosition = Position.getPosition();
    }


    void InitializeGrid()
    {  
        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                Cell newCell = Instantiate(cellObj, new Vector2(x, y), Quaternion.identity);
                newCell.CreateCell(false, tileObjects);
                gridComponents.Add(newCell);
                
            }
        }

        StartCoroutine(CheckEntropy());
    }


    IEnumerator CheckEntropy()
    {
        List<Cell> tempGrid = new List<Cell>(gridComponents);
        tempGrid.RemoveAll(c => c.collapsed);
        
        tempGrid.Sort((a, b) => { return a.tileOptions.Length - b.tileOptions.Length; });

        int arrLength = tempGrid[0].tileOptions.Length;
        int stopIndex = default;

        for (int i = 1; i < tempGrid.Count; i++)
        {
            if (tempGrid[i].tileOptions.Length > arrLength)
            {
                stopIndex = i;
                break;
            }
        }

        if (stopIndex > 0)
        {
            tempGrid.RemoveRange(stopIndex, tempGrid.Count - stopIndex);
        }

        yield return new WaitForSeconds(0.01f);

        CollapseCell(tempGrid);
    }

    void CollapseCell(List<Cell> tempGrid)
    {   
        // Obliczanie sumy wag dla wszystkich dostępnych płytek
        float totalWeight = tempGrid.Sum(cell => cell.tileOptions.Sum(tile => tile.getWeight()));

        // Losowanie liczby z przedziału [0, totalWeight)
        float randomValue = UnityEngine.Random.Range(0, totalWeight);

        float cumulativeWeight = 0;
        Tile selectedTile = null;

        // Iterowanie po dostępnych komórkach
        foreach (Cell cell in tempGrid)
        {
            // Iterowanie po dostępnych płytkach w komórce
            foreach (Tile tile in cell.tileOptions)
            {
                cumulativeWeight += tile.getWeight();

                // Jeśli kumulatywna waga przekroczy losową wartość, wybierz tę płytkę
                if (cumulativeWeight >= randomValue)
                {
                    selectedTile = tile;
                    break;
                }
            }

            if (selectedTile != null)
            {
                break; // Przerwij iterację, jeśli płytka została wybrana
            }

            else
            {
                
                if (cell.tileOptions.Length > 0)
                {
                    selectedTile = cell.tileOptions[UnityEngine.Random.Range(0, cell.tileOptions.Length)];
                }
                else
                {
                    // print(cell.tileOptions[0]);
                    selectedTile = cell.tileOptions[0];
                }
                break;
            }
        }
        Cell cellToCollapse = tempGrid[UnityEngine.Random.Range(0, tempGrid.Count)];
        cellToCollapse.collapsed = true;
        cellToCollapse.tileOptions = new Tile[] { selectedTile };
        Tile foundTile = cellToCollapse.tileOptions[0];
        if (foundTile != null)
        {
            Instantiate(foundTile, cellToCollapse.transform.position, Quaternion.identity);
        }
        else
        {
            Debug.LogWarning("FoundTile is null!");
            Debug.LogWarning(foundTile);
            Debug.LogWarning(cellToCollapse.tileOptions);
            Debug.LogWarning(selectedTile);
            Debug.LogWarning(cellToCollapse);
            Debug.LogWarning(selectedTile.getWeight());
        }

        // Instantiate(foundTile, cellToCollapse.transform.position, Quaternion.identity);
        UpdateGeneration();
    }

    void UpdateGeneration()
    {
        List<Cell> newGenerationCell = new List<Cell>(gridComponents);

        for (int y = 0; y < dimensions; y++)
        {
            for (int x = 0; x < dimensions; x++)
            {
                var index = x + y * dimensions;
                if (gridComponents[index].collapsed)
                {
                    // Debug.Log("called");
                    newGenerationCell[index] = gridComponents[index];
                }
                else
                {
                    List<Tile> options = new List<Tile>();
                    foreach (Tile t in tileObjects)
                    {
                        options.Add(t);
                    }

                    //update above
                    if (y > 0)
                    {
                        Cell up = gridComponents[x + (y - 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in up.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].upNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //update right
                    if (x < dimensions - 1)
                    {
                        Cell right = gridComponents[x + 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in right.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].leftNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //look down
                    if (y < dimensions - 1)
                    {
                        Cell down = gridComponents[x + (y + 1) * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in down.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].downNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    //look left
                    if (x > 0)
                    {
                        Cell left = gridComponents[x - 1 + y * dimensions];
                        List<Tile> validOptions = new List<Tile>();

                        foreach (Tile possibleOptions in left.tileOptions)
                        {
                            var valOption = Array.FindIndex(tileObjects, obj => obj == possibleOptions);
                            var valid = tileObjects[valOption].rightNeighbours;

                            validOptions = validOptions.Concat(valid).ToList();
                        }

                        CheckValidity(options, validOptions);
                    }

                    Tile[] newTileList = new Tile[options.Count];

                    for (int i = 0; i < options.Count; i++)
                    {
                        newTileList[i] = options[i];
                    }

                    newGenerationCell[index].RecreateCell(newTileList);
                }
            }
        }

        gridComponents = newGenerationCell;
        iterations++;

        if(iterations < dimensions * dimensions)
        {
            StartCoroutine(CheckEntropy());
        }

    }

    void CheckValidity(List<Tile> optionList, List<Tile> validOption)
    {
        for (int x = optionList.Count - 1; x >= 0; x--)
        {
            var element = optionList[x];
            if (!validOption.Contains(element))
            {
                optionList.RemoveAt(x);
            }
        }
    }
}