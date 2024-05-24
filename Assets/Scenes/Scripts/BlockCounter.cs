using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class BlockCounter : MonoBehaviour
{
    public Text goldCountText;
    public Text coalCountText;
    public Text diamondCountText;
    public GameObject goldElement; // Element UI do migania
    public GameObject coalElement; // Element UI do migania

    private int goldCount = 0;
    private int coalCount = 0;
    private int diamondCount = 0;

    private Coroutine goldFlashCoroutine;
    private Coroutine coalFlashCoroutine;

    void Start()
    {
        UpdateUI();
    }

    public void AddGold()
    {
        goldCount++;
        UpdateUI();
    }

    public void AddCoal()
    {
        coalCount++;
        UpdateUI();
    }

    public void AddDiamond()
    {
        diamondCount++;
        UpdateUI();
    }

    public int GetGoldCount()
    {
        return goldCount;
    }

    public int GetCoalCount()
    {
        return coalCount;
    }

    public int GetDiamondCount()
    {
        return diamondCount;
    }

    private void UpdateUI()
    {
        goldCountText.text = goldCount.ToString();
        coalCountText.text = coalCount.ToString();
        diamondCountText.text = diamondCount.ToString();
    }

    public void FlashElement(string resource)
    {
        if (resource == "Gold")
        {
            if (goldFlashCoroutine == null)
            {
                goldFlashCoroutine = StartCoroutine(FlashRoutine(goldCountText, goldElement));
            }
        }
        else if (resource == "Coal")
        {
            if (coalFlashCoroutine == null)
            {
                coalFlashCoroutine = StartCoroutine(FlashRoutine(coalCountText, coalElement));
            }
        }
    }

    private IEnumerator FlashRoutine(Text text, GameObject element)
    {
        Color originalTextColor = text.color;
        Color flashColor = Color.red;

        Color originalElementColor = Color.white;
        Image elementImage = element.GetComponent<Image>();
        if (elementImage != null)
        {
            originalElementColor = elementImage.color;
        }

        for (int i = 0; i < 6; i++)
        {
            text.color = flashColor;
            if (elementImage != null)
            {
                elementImage.color = flashColor;
            }
            yield return new WaitForSeconds(0.2f);
            text.color = originalTextColor;
            if (elementImage != null)
            {
                elementImage.color = originalElementColor;
            }
            yield return new WaitForSeconds(0.2f);
        }

        // Resetuj Coroutine po zakończeniu migania
        if (text == goldCountText)
        {
            goldFlashCoroutine = null;
        }
        else if (text == coalCountText)
        {
            coalFlashCoroutine = null;
        }
    }
}
