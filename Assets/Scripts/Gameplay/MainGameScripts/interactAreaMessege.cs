using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class InteractUI : MonoBehaviour
{
    private Canvas canvas;
    private TMP_Text uiText;
    private int uiOffset = -1;
    private CanvasGroup canvasGroup;

    private void Awake()
    {

        canvas = gameObject.transform.parent.Find("Canvas").GetComponent<Canvas>(); 
        uiText = canvas.gameObject.transform.GetChild(0).GetComponent<TMP_Text>(); 
        canvasGroup = canvas.GetComponent<CanvasGroup>(); 
        uiText.transform.position = transform.position + new Vector3(0, uiOffset, 0);
        canvas.gameObject.SetActive(false);

    }
 
    private void OnTriggerEnter2D(Collider2D other)
    {
        // Проверяваме дали collider-ът е от групата "Player"
        if (other.CompareTag("Player"))
        {
            print("entered");
            canvas.gameObject.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            print("Exited");
            canvas.gameObject.SetActive(false);
        }
    }
}

