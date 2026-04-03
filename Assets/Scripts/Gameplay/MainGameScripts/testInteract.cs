using Game.Core.GameSystem;
using UnityEngine;

public class testInteract : MonoBehaviour,Iinteractable
{
    DialogueManager dialogueManager;
    public void interact()
    {
        if (dialogueManager.GetDialogueState() == dialogueStates.inactive)
        {
            dialogueManager.SetDialogueState(dialogueStates.initiazlizeDialogue);
        }
       
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        dialogueManager = GetComponent<DialogueManager>();
        dialogueManager.GetDialogueState();
        
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
