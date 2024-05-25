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
    private List<Vector3Int> positionsToProcess;
    private int iterations = 0;
    private int stone;
    private int gold;
    private int diamond;

    void Start()
    {
        stone = Random.Range(-30, -50);
        gold = Random.Range(-50, -60);
        diamond = Random.Range(-60, -80);
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

                    if (cellPosition.y == stone)
                    {
                        SetTileWeight("Stone", 100);
                    }
                    if (cellPosition.y == gold)
                    {
                        SetTileWeight("Gold", 0.5f);
                    }
                    if (cellPosition.y == diamond)
                    {
                        SetTileWeight("Diamond", 0.1f);
                    }
                }
            }
        }
    }

    void SetTileWeight(string tag, float weight)
    {
        foreach (Tile tile in tileOptionsList)
        {
            if (tile.CompareTag(tag))
            {
                tile.weight = weight;
                break;
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
            yield return FillCellsBatch();
            yield return null; // Wait for the end of frame before checking for new cells
        }
    }

    IEnumerator FillCellsBatch()
    {
        int batchSize = 10; // Number of cells to process per batch
        int processedCount = 0;

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
                if (iterations >= 10 && minradius >= radius)
                {
                    if (positionsToProcess.Count > 1)
                        positionsToProcess = positionsToProcess.GetRange(0, positionsToProcess.Count / 2);
                    iterations = 0;
                }
                iterations++;
                yield break; // Restart the coroutine
            }

            Tile selectedTile = SelectTileBasedOnWeight(validTiles);
            selectedTiles[position] = selectedTile;

            processedCount++;
            if (processedCount >= batchSize)
            {
                processedCount = 0;
                yield return new WaitForSeconds(generationDelay); // Delay between batches
            }
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
                if (!IsCompatible(tile, neighborTile, direction)) return false;
            }
            else if (cells.TryGetValue(neighborPosition, out GameObject neighborCell))
            {
                Cell neighborCellComponent = neighborCell.GetComponent<Cell>();
                if (neighborCellComponent.collapsed)
                {
                    Tile collapsedTile = neighborCellComponent.tileOptions[0];
                    if (!IsCompatible(tile, collapsedTile, direction)) return false;
                }
            }
        }

        return true;
    }

    bool IsCompatible(Tile tile, Tile neighborTile, Vector3Int direction)
    {
        if (direction == Vector3Int.up && !System.Array.Exists(neighborTile.downNeighbours, t => t == tile))
            return false;
        if (direction == Vector3Int.down && !System.Array.Exists(neighborTile.upNeighbours, t => t == tile))
            return false;
        if (direction == Vector3Int.left && !System.Array.Exists(neighborTile.rightNeighbours, t => t == tile))
            return false;
        if (direction == Vector3Int.right && !System.Array.Exists(neighborTile.leftNeighbours, t => t == tile))
            return false;
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
