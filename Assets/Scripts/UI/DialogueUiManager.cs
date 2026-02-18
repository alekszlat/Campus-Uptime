using Game.Core.EventSystem;
using Game.Core.GameSystem;
using System;
using System.Collections.Generic;
using System.Xml;
using TMPro;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class DialogueUiManager : MonoBehaviour, IEventHandler<OnDialogueIndxChangedEvent>,
    IEventHandler<OnDialogueStartEvent>,
    IEventHandler<OnDialogueEndEvent>,
    IEventHandler<OnDialogueBoxInfoEvent>,
    IEventHandler<OnNextLineIndicatorOn>,
    IEventHandler<OnNextLineIndicatorOff>,
    IEventHandler<onSendDialogueQuestions>
{
    //MainDialogueObject
    GameObject dialogueContainer;
    TMP_Text dialogueText;
    TMP_Text characterNameText;
    GameObject nextLinendicator;
    Sprite DialogueImage;
    GameObject questionsContainer;
    GameObject questionBackground;
    [SerializeField] GameObject buttonPrefab;
    List<GameObject> buttonRefrences = new List<GameObject>();
    onDialogueQuestionAnsweredEvent onDialogueQuestionAnswered = new onDialogueQuestionAnsweredEvent();


    void OnEnable()
    {
        EventManager.Instance.Subscribe<OnDialogueIndxChangedEvent, DialogueUiManager>(this);
        EventManager.Instance.Subscribe<OnDialogueStartEvent, DialogueUiManager>(this);
        EventManager.Instance.Subscribe<OnDialogueEndEvent, DialogueUiManager>(this);
        EventManager.Instance.Subscribe<OnDialogueBoxInfoEvent, DialogueUiManager>(this);
        EventManager.Instance.Subscribe<OnNextLineIndicatorOn, DialogueUiManager>(this);
        EventManager.Instance.Subscribe<OnNextLineIndicatorOff, DialogueUiManager>(this);
        EventManager.Instance.Subscribe<onSendDialogueQuestions, DialogueUiManager>(this);
    }

    void OnDisable()
    {
        EventManager.Instance.Unsubscribe<OnDialogueIndxChangedEvent, DialogueUiManager>();
        EventManager.Instance.Unsubscribe<OnDialogueStartEvent, DialogueUiManager>();
        EventManager.Instance.Unsubscribe<OnDialogueEndEvent, DialogueUiManager>();
        EventManager.Instance.Unsubscribe<OnDialogueBoxInfoEvent, DialogueUiManager>();
        EventManager.Instance.Unsubscribe<OnNextLineIndicatorOn, DialogueUiManager>();
        EventManager.Instance.Unsubscribe<OnNextLineIndicatorOff, DialogueUiManager>();
        EventManager.Instance.Unsubscribe<onSendDialogueQuestions, DialogueUiManager>();
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

    public void Handle(onSendDialogueQuestions @event)
    {
        questionsContainer.SetActive(true);

        List<Questions> questions = @event.dialogueQuestions;
      
        for (int i = 0; i < questions.Count; i++)
        {
            GameObject newButton = Instantiate(buttonPrefab, questionsContainer.transform);
            
            buttonRefrences.Add(newButton);
            UnityEngine.UI.Button buttonScript = newButton.GetComponentInChildren<UnityEngine.UI.Button>();
            TextMeshProUGUI buttonText = newButton.GetComponentInChildren<TextMeshProUGUI>();
            buttonText.text = questions[i].GetQuestion();

            string nextId = questions[i].getNextId();
            buttonScript.onClick.AddListener(() => sendQuestionAnswerEvent(nextId));
        }

    }
    
    public void sendQuestionAnswerEvent(string dialogueOption)
    {
        //когато изберем опция-натиснем бутон
        //Изпращаме евент с избраната опция към DialogueManager

        onDialogueQuestionAnswered.nextDialogueNode = dialogueOption;
        EventManager.Instance.Publish(onDialogueQuestionAnswered);

        //Изтриваме бутоните
        for (int i = 0; i < buttonRefrences.Count; i++)
        {
            Destroy(buttonRefrences[i]);
        }
        //премахваме референциите 
        buttonRefrences.Clear();
        //Деактивираме контейнера 
        questionsContainer.SetActive(false);
    }

    private void Awake()
    {
        dialogueContainer = findObj("DialogueContainer").gameObject;
        dialogueText = findObj("DialogueText").GetComponent<TMP_Text>();
        characterNameText=findObj("CharacterNameText").GetComponent<TMP_Text>();
        nextLinendicator = findObj("NextLinendicator").gameObject;
        questionsContainer = findObj("QuestionsContainer").gameObject;
        questionBackground = findObj("QuestionsBackground").gameObject;
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
