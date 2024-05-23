using UnityEngine;
using UnityEngine.UI;

public class BlockChanger : MonoBehaviour
{
    // Prefab of the second and third blocks to instantiate
    public GameObject block2Prefab;
    public GameObject block3Prefab;
    // Reference to the player object
    private Transform playerTransform;
    // Maximum allowed distance to change blocks
    public float maxDistance = 3f;
    public Text BlocksCountText;
    private int blocks = 0;
    

    void Start()
    {
        // Automatically find the player object by tag
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerTransform = player.transform;
        }
        else
        {
            Debug.LogError("Player object not found. Make sure the player has the tag 'Player'.");
        }
    }

    void Update()
    {
        // Detect mouse click
        if (Input.GetMouseButtonDown(0))
        {
            // Convert mouse position to world position
            Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            // Check if the mouse position overlaps with this block's collider
            Collider2D hitCollider = Physics2D.OverlapPoint(mousePosition);

            if (hitCollider != null && hitCollider.transform == transform)
            {
                // Check if the block is within the allowed distance from the player
                if (playerTransform != null && Vector2.Distance(playerTransform.position, transform.position) <= maxDistance)
                {
                    ChangeBlock();
                }
                else
                {
                    Debug.Log("Block is too far from the player to change or playerTransform is not set.");
                }
            }
        }
    }

    void ChangeBlock()
    {
        // Check for any block with tag "Plant" above the current block
        Vector2 abovePosition = new Vector2(transform.position.x, transform.position.y + 1); // Adjust the offset as needed
        Collider2D[] colliders = Physics2D.OverlapPointAll(abovePosition);

        bool foundPlant = false;
        foreach (Collider2D collider in colliders)
        {
            // Check if the collider's tag is "Plant"
            if (collider.CompareTag("Plant"))
            {
                // Instantiate block3Prefab at the same position as the plant block
                Instantiate(block3Prefab, collider.transform.position, collider.transform.rotation);
                // Destroy the plant block
                Destroy(collider.gameObject);
                foundPlant = true;
                break; // If a plant block is found, no need to continue searching
            }
        }

        // Only instantiate block2Prefab if no plant block was found above
        if (!foundPlant)
        {
            Instantiate(block2Prefab, transform.position, transform.rotation);
            // Destroy the current block
            Destroy(gameObject);
        }
    }

        public void AddBlocks()
    {
        blocks++;
        BlocksCountText.text = blocks.ToString();
    }
}
