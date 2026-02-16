using Game.Core.EventSystem;
using Game.Core.GameSystem;
using System;
using System.Xml;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueUiManager : MonoBehaviour, IEventHandler<OnDialogueIndxChangedEvent>,
    IEventHandler<OnDialogueStartEvent>,
    IEventHandler<OnDialogueEndEvent>,
    IEventHandler<OnDialogueBoxInfoEvent>,
    IEventHandler<OnNextLineIndicatorOn>,
    IEventHandler<OnNextLineIndicatorOff>
{
    //MainDialogueObject
    GameObject dialogueContainer;
    TMP_Text dialogueText;
    TMP_Text characterNameText;
    GameObject nextLinendicator;
    Sprite DialogueImage;
  


    void OnEnable()
    {
        EventManager.Instance.Subscribe<OnDialogueIndxChangedEvent, DialogueUiManager>(this);
        EventManager.Instance.Subscribe<OnDialogueStartEvent, DialogueUiManager>(this);
        EventManager.Instance.Subscribe<OnDialogueEndEvent, DialogueUiManager>(this);
        EventManager.Instance.Subscribe<OnDialogueBoxInfoEvent, DialogueUiManager>(this);
        EventManager.Instance.Subscribe<OnNextLineIndicatorOn, DialogueUiManager>(this);
        EventManager.Instance.Subscribe<OnNextLineIndicatorOff, DialogueUiManager>(this);
    }

    void OnDisable()
    {
        EventManager.Instance.Unsubscribe<OnDialogueIndxChangedEvent, DialogueUiManager>();
        EventManager.Instance.Unsubscribe<OnDialogueStartEvent, DialogueUiManager>();
        EventManager.Instance.Unsubscribe<OnDialogueEndEvent, DialogueUiManager>();
        EventManager.Instance.Unsubscribe<OnDialogueBoxInfoEvent, DialogueUiManager>();
        EventManager.Instance.Unsubscribe<OnNextLineIndicatorOn, DialogueUiManager>();
        EventManager.Instance.Unsubscribe<OnNextLineIndicatorOff, DialogueUiManager>();
    }

   //подаваме на кой индекс сме и го правим видим
    public void Handle(OnDialogueIndxChangedEvent @event)
    {
        int indx = @event.NewDialogueIndx;
        dialogueText.maxVisibleCharacters = indx;
   
    }

    //подаваме сегашния текст и го правим невидим
    public void Handle(OnDialogueBoxInfoEvent @event)
    {
        dialogueText.text = @event.fullDialogueBoxText;
        dialogueText.maxVisibleCharacters = 0;
        if (@event.portrait!=null) {
            DialogueImage = @event.portrait;
        }
        characterNameText.text = @event.name;
    }

    public void Handle(OnDialogueStartEvent @event)
    {
        dialogueContainer.SetActive(true);
    }

    public void Handle(OnDialogueEndEvent @event)
    {
        dialogueContainer.SetActive(false);
    }
    public void Handle(OnNextLineIndicatorOff @event)
    {
        nextLinendicator.SetActive(false);
    }

    public void Handle(OnNextLineIndicatorOn @event)
    {
        nextLinendicator.SetActive(true);
    }
    private void Awake()
    {
        
        dialogueContainer = findObj("DialogueContainer").gameObject;
        dialogueText = findObj("DialogueText").GetComponent<TMP_Text>();
        characterNameText=findObj("CharacterNameText").GetComponent<TMP_Text>();
        nextLinendicator = findObj("NextLinendicator").gameObject;



    }

    void Start()
    {

       dialogueContainer.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    Transform findObj(string name)
    {
        Transform[] all = GetComponentsInChildren<Transform>(true);

        for(int i=0;i<all.Length;i++)
        {
            if (all[i].name == name)
            {
                return all[i];
               
            }
        }
        return null;
    }

    
}
