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
using PrimeTween;
using Image = UnityEngine.UI.Image;

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
    GameObject questionsContainer;
    GameObject potrtraitContainer;
    Image CharacterPortrait;

    Vector2 indicatorStartLocalPos;

    [SerializeField] GameObject buttonPrefab;
    List<GameObject> buttonRefrences = new List<GameObject>();
    onDialogueQuestionAnsweredEvent onDialogueQuestionAnswered = new onDialogueQuestionAnsweredEvent();

    CanvasGroup nextIndicatorCanvasGroup;
    RectTransform nextIndicatorRectTransform;

    private String lastPersonName = "Null";
    Sequence indicatorSequence;
    Tween indicatorMove;


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

    //подаваме сегашния текст,който е подаден от системата за диалог и го правим невидим
    public void Handle(OnDialogueBoxInfoEvent @event)
    {
        dialogueText.text = @event.fullDialogueBoxText;
        dialogueText.maxVisibleCharacters = 0;
        
        if (@event.portrait!=null) {
            potrtraitContainer.SetActive(true);
            if (lastPersonName == "Null") {
              


                //само отваряне
            }
            else if (lastPersonName != @event.name)
            {
                ///TODO анимация за затраряне и отваряне
            }
            else
            {
                //няма за отваряне,защото играча е същия

            }
               CharacterPortrait.sprite = @event.portrait;
        }
        else
        {
            potrtraitContainer.SetActive(false);
        }
        characterNameText.text = @event.name;
        lastPersonName = @event.name;
    }

    public void Handle(OnDialogueStartEvent @event)
    {
        dialogueContainer.SetActive(true);
        Tween.ScaleY(dialogueContainer.transform, endValue: 1f, duration: 0.5f);
    }

    public void Handle(OnDialogueEndEvent @event)
    {
        Tween.ScaleY(dialogueContainer.transform, endValue: 0f, duration: 0.5f).OnComplete(()=> dialogueContainer.SetActive(false));
        
    }
 
    public void Handle(OnNextLineIndicatorOn @event)
    {
        

        indicatorSequence = Sequence.Create(1)
       .Group(Tween.Alpha(nextIndicatorCanvasGroup, 1,0)).Chain(
        Tween.UIAnchoredPositionX(nextIndicatorRectTransform,
                                              indicatorStartLocalPos.x + 10f,
                                              0.5f));
       

    }

    public void Handle(OnNextLineIndicatorOff @event)
    {
        nextIndicatorRectTransform.anchoredPosition = indicatorStartLocalPos;
        nextIndicatorCanvasGroup.alpha = 0;
    }

  
    public void Handle(onSendDialogueQuestions @event)
    {
        //Ако има портрет го изключваме
        if (CharacterPortrait != null)
        {
            potrtraitContainer.gameObject.SetActive(false);
        }
        Tween.ScaleY(questionsContainer.transform, 1, 1);

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

            //ако няма следващо ид,автоматично го правим дефалт
            if (nextId == "")
            {
                nextId = "default";
            }

            buttonScript.onClick.AddListener(() => sendQuestionAnswerEvent(nextId));
            print("NEXT ID IS "+nextId);
        }

    }
    
    public void sendQuestionAnswerEvent(string dialogueOption)
    {

        Tween.ScaleY(questionsContainer.transform, 0, 1).OnComplete(() => afterEventAnswered(dialogueOption));
    }
    public void afterEventAnswered(string dialogueOption)
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

    }

    private void Awake()
    {
        dialogueContainer = findObj("DialogueContainer").gameObject;
        dialogueText = findObj("DialogueText").GetComponent<TMP_Text>();
        characterNameText=findObj("CharacterNameText").GetComponent<TMP_Text>();
        nextLinendicator = findObj("NextLinendicator").gameObject;
        questionsContainer = findObj("QuestionsContainer").gameObject;
        potrtraitContainer= findObj("PortraitContainer").gameObject;
        CharacterPortrait = findObj("CharacterPortraitImage").GetComponent<Image>();

        //задаваме x на диаглога 1, а y=0 за да бъде контролирано от tween
        dialogueContainer.transform.localScale = new Vector3(1, 0, 0);
        questionsContainer.transform.localScale = new Vector3(1, 0, 0);

        nextIndicatorCanvasGroup = nextLinendicator.gameObject.GetComponent<CanvasGroup>();
        nextIndicatorRectTransform = nextLinendicator.gameObject.GetComponent<RectTransform>();

    }

    void Start()
    {
       dialogueContainer.SetActive(false);
       questionsContainer.SetActive(false);
       potrtraitContainer.SetActive(false);
       indicatorStartLocalPos = nextLinendicator.gameObject.transform.localPosition;


    }

    // Update is called once per frame
    void Update()
    {
        print("SPRITE RENDERER "+ nextIndicatorCanvasGroup);
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
