using System.Collections.Generic;
using UnityEngine;

public class WaveFunction2 : MonoBehaviour
{
    public int radius;
    public GameObject cellPrefab;
    public GameObject player;
    public List<Tile> tileOptionsList;

    private List<GameObject> cells = new List<GameObject>();
    private Vector3Int previousPlayerPosition;

    void Start()
    {
        GenerateInitialCells();
        previousPlayerPosition = Vector3Int.RoundToInt(player.transform.position);
    }

    void Update()
    {
        Vector3Int playerPosition = Vector3Int.RoundToInt(player.transform.position);
        if (playerPosition != previousPlayerPosition)
        {
            UpdateCells(playerPosition);
            previousPlayerPosition = playerPosition;
        }
    }

    void GenerateInitialCells()
    {
        Vector3Int playerPosition = Vector3Int.RoundToInt(player.transform.position);

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    Vector3Int cellPosition = playerPosition + new Vector3Int(x, y, 0);
                    GameObject cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                    Cell cellComponent = cell.GetComponent<Cell>();
                    cellComponent.CreateCell(false, new List<Tile>(tileOptionsList));
                    cells.Add(cell);
                }
            }
        }

        // Ustawienie początkowego bloku
        SetInitialBlock(playerPosition);

        CollapseCells();
    }

    void SetInitialBlock(Vector3Int position)
    {
        GameObject initialCell = cells.Find(c => Vector3Int.RoundToInt(c.transform.position) == position);
        if (initialCell != null)
        {
            Cell initialCellComponent = initialCell.GetComponent<Cell>();
            Tile initialTile = tileOptionsList[0]; // Ustaw pierwszy blok jako początkowy
            initialCellComponent.tileOptions = new List<Tile> { initialTile };
            Instantiate(initialTile.gameObject, initialCell.transform.position, Quaternion.identity);
            initialCellComponent.collapsed = true;
        }
    }

    void UpdateCells(Vector3Int playerPosition)
    {
        HashSet<Vector3Int> existingCellPositions = new HashSet<Vector3Int>();
        foreach (GameObject cell in cells)
        {
            existingCellPositions.Add(Vector3Int.RoundToInt(cell.transform.position));
        }

        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y <= radius * radius)
                {
                    Vector3Int cellPosition = playerPosition + new Vector3Int(x, y, 0);
                    if (!existingCellPositions.Contains(cellPosition))
                    {
                        GameObject cell = Instantiate(cellPrefab, cellPosition, Quaternion.identity);
                        Cell cellComponent = cell.GetComponent<Cell>();
                        cellComponent.CreateCell(false, new List<Tile>(tileOptionsList));
                        cells.Add(cell);
                    }
                }
            }
        }

        CollapseCells();
    }

    void CollapseCells()
    {
        bool changed = true;

        while (changed)
        {
            changed = false;

            foreach (GameObject cell in cells)
            {
                Cell cellComponent = cell.GetComponent<Cell>();
                if (cellComponent.collapsed)
                {
                    continue; // Skip cells with already determined block
                }

                List<Tile> possibleTiles = new List<Tile>(cellComponent.tileOptions);
                List<Tile> toRemove = new List<Tile>();

                foreach (Tile tile in possibleTiles)
                {
                    if (!IsTileCompatibleWithNeighbors(cell.transform.position, tile))
                    {
                        toRemove.Add(tile);
                        changed = true;
                    }
                }

                foreach (Tile tile in toRemove)
                {
                    cellComponent.tileOptions.Remove(tile);
                }

                if (cellComponent.tileOptions.Count == 1)
                {
                    Instantiate(cellComponent.tileOptions[0].gameObject, cell.transform.position, Quaternion.identity);
                    cellComponent.collapsed = true; // Mark cell as collapsed
                }
                else if (cellComponent.tileOptions.Count > 1 && !changed)
                {
                    Tile selectedTile = SelectTileBasedOnWeight(cellComponent.tileOptions);
                    cellComponent.tileOptions = new List<Tile> { selectedTile };
                    Instantiate(selectedTile.gameObject, cell.transform.position, Quaternion.identity);
                    cellComponent.collapsed = true; // Mark cell as collapsed
                }
            }
        }
    }

    bool IsTileCompatibleWithNeighbors(Vector3 position, Tile tile)
    {
        Vector3Int[] directions = {
            Vector3Int.up,
            Vector3Int.down,
            Vector3Int.left,
            Vector3Int.right
        };

        foreach (Vector3Int direction in directions)
        {
            Vector3Int neighborPosition = Vector3Int.RoundToInt(position) + direction;
            GameObject neighborCell = cells.Find(c => Vector3Int.RoundToInt(c.transform.position) == neighborPosition);
            if (neighborCell != null)
            {
                Cell neighborCellComponent = neighborCell.GetComponent<Cell>();
                if (neighborCellComponent.tileOptions.Count == 1)
                {
                    Tile neighborTile = neighborCellComponent.tileOptions[0];
                    if (direction == Vector3Int.up && !new List<Tile>(neighborTile.downNeighbours).Contains(tile))
                    {
                        return false;
                    }
                    if (direction == Vector3Int.down && !new List<Tile>(neighborTile.upNeighbours).Contains(tile))
                    {
                        return false;
                    }
                    if (direction == Vector3Int.left && !new List<Tile>(neighborTile.rightNeighbours).Contains(tile))
                    {
                        return false;
                    }
                    if (direction == Vector3Int.right && !new List<Tile>(neighborTile.leftNeighbours).Contains(tile))
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
