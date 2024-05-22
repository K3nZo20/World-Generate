using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveFunction2 : MonoBehaviour
{
    public int radius;
    private int minradius = 5;
    public GameObject cellPrefab;
    public GameObject player;
    private Moving moving;
    public List<Tile> tileOptionsList;
    public float generationDelay = 0.01f; // Opóźnienie między wypełnianiem kolejnych komórek

    private Dictionary<Vector3Int, GameObject> cells = new Dictionary<Vector3Int, GameObject>();
    private Vector3Int previousPlayerPosition;
    private Coroutine generationCoroutine;
    private HashSet<Vector3Int> processedCells = new HashSet<Vector3Int>();
    int iterations = 0;
    private List<Vector3Int> positionsToProcess;

    void Start()
    {
        GenerateInitialCells();
        previousPlayerPosition = Vector3Int.RoundToInt(player.transform.position);
        StartCoroutine(ContinuousFillCells());
        moving = player.GetComponent<Moving>();
        
    }

    void Update()
    {
        Vector3Int playerPosition = Vector3Int.RoundToInt(player.transform.position);
        if (playerPosition != previousPlayerPosition)
        {
            GenerateSkeletonCells(playerPosition);
            previousPlayerPosition = playerPosition;
        }
    }

    void GenerateInitialCells()
    {
        Vector3Int playerPosition = Vector3Int.RoundToInt(player.transform.position);
        GenerateSkeletonCells(playerPosition);
        SetInitialBlock(playerPosition);
        positionsToProcess = new List<Vector3Int>(cells.Keys);
    }

    void GenerateSkeletonCells(Vector3Int playerPosition)
    {
        HashSet<Vector3Int> existingCellPositions = new HashSet<Vector3Int>(cells.Keys);

        for (int x = -minradius; x <= minradius; x++)
        {
            for (int y = -minradius / 2; y <= minradius / 2; y++)
            {
                Vector3Int cellPosition = playerPosition + new Vector3Int(x, y, 0);
                if (!existingCellPositions.Contains(cellPosition))
                {
                    GameObject cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                    Cell cellComponent = cell.GetComponent<Cell>();
                    cellComponent.CreateCell(false, new List<Tile>(tileOptionsList));
                    cells[cellPosition] = cell;
                }
            }
        }
    }

    void SetInitialBlock(Vector3Int position)
    {
        if (cells.TryGetValue(position, out GameObject initialCell))
        {
            Cell initialCellComponent = initialCell.GetComponent<Cell>();
            Tile initialTile = tileOptionsList[0]; // Ustaw pierwszy blok jako początkowy
            initialCellComponent.tileOptions = new List<Tile> { initialTile };
            Instantiate(initialTile.gameObject, initialCell.transform.position, Quaternion.identity);
            initialCellComponent.collapsed = true;
            processedCells.Add(position);
            
        }
    }

    IEnumerator ContinuousFillCells()
    {
        while (true)
        {
            yield return StartCoroutine(FillCells());
            yield return null; // Wait for the end of frame before checking for new cells
        }
    }

    IEnumerator FillCells()
    {
        positionsToProcess.Sort((a, b) => Vector3Int.Distance(a, previousPlayerPosition).CompareTo(Vector3Int.Distance(b, previousPlayerPosition)));

        Dictionary<Vector3Int, Tile> selectedTiles = new Dictionary<Vector3Int, Tile>();

        foreach (Vector3Int position in positionsToProcess)
        {
            if (processedCells.Contains(position)) continue;
            if (!cells.TryGetValue(position, out GameObject cell)) continue;

            Cell cellComponent = cell.GetComponent<Cell>();
            if (cellComponent.collapsed)
            {
                selectedTiles[position] = cellComponent.tileOptions[0];
                continue;
            }

            List<Tile> possibleTiles = new List<Tile>(cellComponent.tileOptions);
            List<Tile> validTiles = new List<Tile>();

            foreach (Tile tile in possibleTiles)
            {
                if (IsTileCompatibleWithNeighbors(position, tile, selectedTiles))
                {
                    validTiles.Add(tile);
                }
            }

            if (validTiles.Count == 0)
            {
                // Debug.LogWarning($"No valid tiles found for position {position}. Regenerating options.");
                cellComponent.tileOptions = new List<Tile>(tileOptionsList);
                // print(iterations);
                // print(positionsToProcess.Count);
                if (iterations >= 5 & minradius >= radius)
                {
                    if (positionsToProcess.Count >= 500)
                    positionsToProcess = positionsToProcess.GetRange(0, positionsToProcess.Count-500);
                    iterations = 0;
                }
                iterations++;
                yield break; // Restart the coroutine
            }

            Tile selectedTile = SelectTileBasedOnWeight(validTiles);
            selectedTiles[position] = selectedTile;
            
        }

        foreach (KeyValuePair<Vector3Int, Tile> entry in selectedTiles)
        {
            Vector3Int position = entry.Key;
            Tile selectedTile = entry.Value;

            if (cells.TryGetValue(position, out GameObject cell))
            {
                Cell cellComponent = cell.GetComponent<Cell>();
                Instantiate(selectedTile.gameObject, cell.transform.position, Quaternion.identity);
                cellComponent.tileOptions = new List<Tile> { selectedTile };
                cellComponent.collapsed = true;
                processedCells.Add(position);
                // yield return new WaitForSeconds(generationDelay);
            }
        }
        if (minradius < radius)
        {
            minradius++;
            Vector3Int playerPosition = Vector3Int.RoundToInt(player.transform.position);
            GenerateSkeletonCells(playerPosition);
            if (minradius == radius)
            {
                moving.MoveUp();
            }
        }
        positionsToProcess = new List<Vector3Int>(cells.Keys);
    }

    bool IsTileCompatibleWithNeighbors(Vector3Int position, Tile tile, Dictionary<Vector3Int, Tile> selectedTiles)
    {
        Vector3Int[] directions = {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        foreach (Vector3Int direction in directions)
        {
            Vector3Int neighborPosition = position + direction;
            if (selectedTiles.TryGetValue(neighborPosition, out Tile neighborTile))
            {
                if (direction == Vector3Int.up && !System.Array.Exists(neighborTile.downNeighbours, t => t == tile))
                {
                    return false;
                }
                if (direction == Vector3Int.down && !System.Array.Exists(neighborTile.upNeighbours, t => t == tile))
                {
                    return false;
                }
                if (direction == Vector3Int.left && !System.Array.Exists(neighborTile.rightNeighbours, t => t == tile))
                {
                    return false;
                }
                if (direction == Vector3Int.right && !System.Array.Exists(neighborTile.leftNeighbours, t => t == tile))
                {
                    return false;
                }
            }
            else if (cells.TryGetValue(neighborPosition, out GameObject neighborCell))
            {
                Cell neighborCellComponent = neighborCell.GetComponent<Cell>();
                if (neighborCellComponent.collapsed)
                {
                    Tile collapsedTile = neighborCellComponent.tileOptions[0];
                    if (direction == Vector3Int.up && !System.Array.Exists(collapsedTile.downNeighbours, t => t == tile))
                    {
                        return false;
                    }
                    if (direction == Vector3Int.down && !System.Array.Exists(collapsedTile.upNeighbours, t => t == tile))
                    {
                        return false;
                    }
                    if (direction == Vector3Int.left && !System.Array.Exists(collapsedTile.rightNeighbours, t => t == tile))
                    {
                        return false;
                    }
                    if (direction == Vector3Int.right && !System.Array.Exists(collapsedTile.leftNeighbours, t => t == tile))
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }

    Tile SelectTileBasedOnWeight(List<Tile> tiles)
    {
        float totalWeight = 0;
        foreach (Tile tile in tiles)
        {
            totalWeight += tile.getWeight();
        }

        float randomValue = Random.Range(0, totalWeight);
        float cumulativeWeight = 0;

        foreach (Tile tile in tiles)
        {
            cumulativeWeight += tile.getWeight();
            if (randomValue < cumulativeWeight)
            {
                return tile;
            }
        }

        return tiles[tiles.Count - 1]; // Return the last tile as a fallback
    }
}