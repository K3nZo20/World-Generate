using UnityEngine;

public class BlockChanger : MonoBehaviour
{
    public GameObject block2Prefab;
    public GameObject block3Prefab;
    private Transform playerTransform;
    public float maxDistance = 3f;
    private BlockCounter blockCounter;

    // Nowe pola określające wymagania
    public int requiredCoal = 0;
    public int requiredGold = 0;
    public int requiredDiamond = 0;

    void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Nie znaleziono obiektu gracza. Upewnij się, że gracz ma tag 'Player'.");
        }

        blockCounter = FindObjectOfType<BlockCounter>();
        if (blockCounter == null)
        {
            Debug.LogError("Nie znaleziono BlockCounter w scenie.");
        }
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);

            if (hitCollider != null && hitCollider.transform == transform)
            {
                if (playerTransform != null && Vector2.Distance(playerTransform.position, transform.position) <= maxDistance)
                {
                    if (CanChangeBlock())
                    {
                        ChangeBlock();
                        AddBlocks(hitCollider.gameObject);
                    }
                    else
                    {
                        Debug.Log("Nie spełniasz wymagań, aby zmienić ten blok.");
                    }
                }
                else
                {
                    Debug.Log("Blok jest za daleko od gracza, aby go zmienić lub playerTransform nie jest ustawiony.");
                }
            }
        }
    }

    bool CanChangeBlock()
    {
        bool canChange = true;

        if (blockCounter.GetGoldCount() < requiredGold)
        {
            blockCounter.FlashElement("Gold");
            canChange = false;
        }

        if (blockCounter.GetCoalCount() < requiredCoal)
        {
            blockCounter.FlashElement("Coal");
            canChange = false;
        }

        return canChange;
    }

    void ChangeBlock()
    {
        Vector2 abovePosition = new Vector2(transform.position.x, transform.position.y + 1);
        Collider2D[] colliders = Physics2D.OverlapPointAll(abovePosition);

        bool foundPlant = false;
        foreach (Collider2D collider in colliders)
        {
            if (collider.CompareTag("Plant"))
            {
                Instantiate(block3Prefab, collider.transform.position, collider.transform.rotation);
                Destroy(collider.gameObject);
                foundPlant = true;
                break;
            }
        }

        if (!foundPlant)
        {
            Instantiate(block2Prefab, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

    void AddBlocks(GameObject block)
    {
        if (blockCounter == null)
        {
            Debug.LogError("BlockCounter nie jest ustawiony.");
            return;
        }

        if (block.CompareTag("Gold"))
        {
            blockCounter.AddGold();
        }
        else if (block.CompareTag("Diamond"))
        {
            blockCounter.AddDiamond();
        }
        else if (block.CompareTag("Coal"))
        {
            blockCounter.AddCoal();
        }
    }
}
