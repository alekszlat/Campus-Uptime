using Game.Core.EventSystem;
using Game.Core.GameSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Unity.Collections;
using UnityEngine;
using UnityEngine.UI;


public enum QuestionState { NoQuestion, HasQuestion, QuestionAlreadyAsked };


[Serializable]
public class dialogBoxInfo: ISerializationCallbackReceiver
{
    //искам това да се преценява от questions лист, дали има или няма въпроси
    private QuestionState questionState = QuestionState.NoQuestion;

 

    //Име на герой
    [SerializeField] string name;
    //Лист с въпроси, ако има такива
    //Класа въпроси съдържа въпрос и следващо id(към кой диалог води)
    [SerializeField] List<Questions> questions = new List<Questions>();
    //Лист с изречения в един диалог бокс
    [SerializeField] List<string> sentence = new List<string>();
    //Снимка на герой
    [SerializeField] Sprite portrait;

    //Име на диалог
    [SerializeField] string thisDialogueId;

    //Връзка към следващ диалог, ако е нъл при избор се взема един от id-тата от questions
    [SerializeField] string nextDialogueId;




    //За да работи сериализацията използваме вместо конструктор тази функция,работи подобно на конструктор
    //Тя изпълнява кода след сериализацията на обекта
    public void OnAfterDeserialize()
    {
        //Ако има въпроси в листа, стейта се променя, на има въпрос.
        if (questions != null && questions.Count > 0)
        {
            questionState = QuestionState.HasQuestion;
        }
        else
        {
            questionState = QuestionState.NoQuestion;
        }
    }
    //Ако трябва да стане нещо преди сериализацията на обекта
    public void Init(string name,List<Questions>questions,List<string> sentence, Sprite characterPortrait, string thisDialogueId,string nextDialogueId)
    {
        this.name = name;
        this.questions = questions;
        this.sentence = sentence;
        this.portrait = characterPortrait;
        this.thisDialogueId = thisDialogueId;
        this.nextDialogueId = nextDialogueId;
        if (questions != null && questions.Count > 0)
        {
            questionState = QuestionState.HasQuestion;
        }
        else
        {
            questionState = QuestionState.NoQuestion;
        }
    }
    public void OnBeforeSerialize()
    {
    }

 
    public string GetName()
    {
        return name;
    }
    public Sprite getCharacterPortrait()
    {
        return portrait;
    }
    public List<string> GetSentence()
    {
        return sentence;
    }
    public List<Questions> GetQuestions()
    {
        return questions;
    }
    public QuestionState GetQuestionState()
    {
        return questionState;
    }
    public void SetQuestionState(QuestionState questionState)
    {
        this.questionState = questionState;
    }
    public string GetId()
    {
        return thisDialogueId;
    }
    public string GetNextId()
    {
        return nextDialogueId;
    }
    public void SetCharacterName(string characterName)
    {
        name = characterName;
    }
  
}


public class DialogueManager : MonoBehaviour,IEventHandler<onDialogueQuestionAnsweredEvent>
{

    [SerializeField] private bool frezeCharacterDuringDialogue = false;


    //понеже Unity не може да сериализира речник, пълним диалозите в лист, след което ги слагаме в речник,който диалог системата използва
    [SerializeField]
    private List<dialogBoxInfo> dialogueList;

    Dictionary<string, dialogBoxInfo> dialogueBoxDictionary = new Dictionary<string, dialogBoxInfo>();//речник с диалози

    private string currentSentence;//сегашно изречение от масива с изречения на един dialogueBox

    private string temp ="";//събира сегашното изречение буква по буква

    [SerializeField] private float startingCharDelay= 0.005f;
    private float currentCharDelay;
   
    dialogBoxInfo currentDialogue;//сегашния dialogueBox
    private int currentBoxCharIndx = 0;//общ чар в целия dialogBox 
    private int currentCharIndx = 0;//сегашен чар на сегашното изречение
    private string currentDialogueKey = " ";//ключ на кой диалог сме
    private int currentDialogueLineIndx = 0;//индекс на коя линия в диалога сме


    [SerializeField] string startDialogueId;//От кой диалог да започне диалога
    dialogueStates currentState=dialogueStates.inactive;

    //EVENTS
    OnDialogueBoxInfoEvent dialogueBoxSendInfoEvent = new OnDialogueBoxInfoEvent();
    OnDialogueIndxChangedEvent dialogueIndxChangedEvent = new OnDialogueIndxChangedEvent();
    OnNextLineIndicatorOn onNextLineIndicatorOn = new OnNextLineIndicatorOn();
    OnNextLineIndicatorOff onNextLineIndicatorOff = new OnNextLineIndicatorOff();
    OnDialogueEndEvent onDialogueEndEvent = new OnDialogueEndEvent();
    OnDialogueStartEvent onDialogueStartEvent = new OnDialogueStartEvent();
    OnFreezePlayerDuringDialogue onPlayerFreezePlayerDuringDialogue = new OnFreezePlayerDuringDialogue();
    OnUnFreezePlayerOnDialogueEnd onPlayerUnFreezePlayerOnDialogueEnd = new OnUnFreezePlayerOnDialogueEnd();
    onSendDialogueQuestions onSendDialogueQuestions = new onSendDialogueQuestions();
    TimerUtil dialogueCharTimer;//CustomTimer
    
    //default отговор, който се изпълнява, когато въпрос няма следващо id
    dialogBoxInfo defaultDialogue;
    string lastKey;
    bool hasAlreadyConversed = false;
    private void Awake()
    {
        
        //Понеже иснпектора не работи с речник, в инспектора слагаме данните в лист и ги добавяме в речник
        for(int i = 0;i< dialogueList.Count; i++)
        {
            dialogueBoxDictionary.Add(dialogueList[i].GetId(), dialogueList[i]);
        }
        defaultDialogue = new dialogBoxInfo();

        List<string> sentence = new List<string>();
        sentence.Add("...");
        defaultDialogue.Init("", null, sentence, null, "default","");
        dialogueBoxDictionary.Add(defaultDialogue.GetId(), defaultDialogue);

    }
    private void Start()
    {
      
        currentCharDelay = startingCharDelay;
        dialogueCharTimer = new TimerUtil(currentCharDelay, true);

    }
    // Update is called once per frame
    void Update()
    {
        if (currentState == dialogueStates.initiazlizeDialogue)
        {
            //Ако няма диалози не се пуска диалог системата
            if (dialogueBoxDictionary.Count <= 0)
            {
                SetDialogueState(dialogueStates.inactive);
                print("NO DIALOGUES FOR THIS CHARACTER!!!");
                return;
            }

            //Ако вече сме говорили взима последния ключ
            if (!hasAlreadyConversed)
            {
                currentDialogueKey = startDialogueId;
                hasAlreadyConversed = true;
            }
            else
            {
                currentDialogueKey = lastKey;
            }


            EventManager.Instance.Publish(onDialogueStartEvent);
            SetDialogueState(dialogueStates.StartCurrDialogue);
            if (frezeCharacterDuringDialogue)
            {
                EventManager.Instance.Publish(onPlayerFreezePlayerDuringDialogue);
            }
         

        }
        else if (currentState == dialogueStates.StartCurrDialogue)
        {
            dialogueStartState();
        }
        else if (currentState == dialogueStates.Typing)
        {
            typingState();
        }
        else if (currentState == dialogueStates.WaitingForNextLine)
        {

            waitingForNextLine();

        }
        else if (currentState == dialogueStates.EndCurrDialogue)
        {

            endCurrentDialogueState();

        }
        else if (currentState == dialogueStates.dialogueQuestionState)
        {

            //Empty state while player is choising an answer
            //When player answers an event will switch state to EndCurrentDialogue, witch will decide weather to continiue dialogue
        }
        else if (currentState == dialogueStates.Close)
        {
            closeDialogueState();
        }
 
        if (currentDialogueKey != "" && lastKey!=currentDialogueKey)
        {
            lastKey = currentDialogueKey;
        }


        if (currentDialogue!=null)
        {
            print("CURR Question STATE " + currentDialogue.GetQuestionState());
        }
    }


    void typingState()
    {
        //Когато таймера е верен се изпълнява,което става на всеки currentCharDelay секунди
        //Тук увеличаваме индекса на сегашното изречение, и на всички изречения в dialogueBox, и пращаме индекса наза целия dialogue box,защото
        //ui manager има целия текст от dialogueBox,докато тук обработваме само едно изречение, за да се знае, кога DialogueUI, да прави char visible
        //Затова се изпраща евент към DialogueUI със index на целия dialogueBox

        //Ако сегашното изречение е свършило минаваме на следващо минаваме на следващата линия в диалога
        
        //Изключваме индикатора за следваща линия
        EventManager.Instance.Publish(onNextLineIndicatorOff);
        while (dialogueCharTimer.UpdateTimer(Time.deltaTime))
        {
            if (currentCharIndx < currentSentence.Length)
            {
                temp += currentSentence[currentCharIndx++];
                currentBoxCharIndx++;
                dialogueIndxChangedEvent.NewDialogueIndx = currentBoxCharIndx;

                EventManager.Instance.Publish(dialogueIndxChangedEvent);
            }
            else
            {
                currentDialogueLineIndx++;
                SetDialogueState(dialogueStates.WaitingForNextLine);
                break;
            }
        }
    }

    void closeDialogueState()
    {
         if (frezeCharacterDuringDialogue)
         {
            EventManager.Instance.Publish(onPlayerUnFreezePlayerOnDialogueEnd);
         }
         EventManager.Instance.Publish(onDialogueEndEvent);
   
         SetDialogueState(dialogueStates.inactive);

    }
    //Фунцкията,която се изпълнява след като отговорим на Въпроса
    public void Handle(onDialogueQuestionAnsweredEvent @event)
    {

        if (@event.nextDialogueNode == "default")
        {
            //Задаваме името на героя,който ще отговори с default отговора
            defaultDialogue.SetCharacterName(currentDialogue.GetName());
            
        }
     
  
        //когато отговорим на въпроса сменяме сегашния диалог,сменяме стейта и маркираме,че въпросът е зададен
        currentDialogueKey = @event.nextDialogueNode;
        print("Event klucha e " + @event.nextDialogueNode);
  
        SetDialogueState(dialogueStates.EndCurrDialogue);
        currentDialogue.SetQuestionState(QuestionState.QuestionAlreadyAsked);
    }

    //Tози стейт само ни пренасочва към други стейтове според състоянието на диалога
    //След като свършим с текста в диалога ни праща тук и диалога прецелява в кой стейт да отиде
    void endCurrentDialogueState()
    {

        //Ако в диалога има изббор
        if (currentDialogue.GetQuestionState() == QuestionState.HasQuestion)
        {
            //Ако има въпрос влизаме в празен стейт,докато чакаме отговор и изпращаме въпросите на DialogueManager
            SetDialogueState(dialogueStates.dialogueQuestionState);
            onSendDialogueQuestions.dialogueQuestions = currentDialogue.GetQuestions();
            EventManager.Instance.Publish(onSendDialogueQuestions);
        } 


        //Подготвяне за следващ диалог,разделени са за да има логическо разделяне,когато сме имали избор и когато сме нямали

        //Ako сме имали избор
        else if (currentDialogue.GetQuestionState() == QuestionState.QuestionAlreadyAsked)
        {
            //Вече сме взели ключа за диалог от  Handle(onDialogueQuestionAnsweredEvent)

            //подготвяме се за следващ диалог
            currentDialogueLineIndx = 0;
            currentBoxCharIndx = 0;
            SetDialogueState(dialogueStates.StartCurrDialogue);
        }
        //Гледа ако следващия диалог не е празен значи сме нямали избор
        else if (currentDialogue.GetNextId().Length != 0)
        {
            //Взимаме следващич ключ за диалог 
            currentDialogueKey = currentDialogue.GetNextId();


            //подготвяме се за следващ диалог
            currentDialogueLineIndx = 0;
            currentBoxCharIndx = 0;
            SetDialogueState(dialogueStates.StartCurrDialogue);
        }

        //Ако няма други диалози затваряме 
        else
        {
            currentDialogueLineIndx = 0;
            currentBoxCharIndx = 0;
            SetDialogueState(dialogueStates.Close);
        }
    }
  
    void dialogueStartState()
    {
        //Тук е когато всеки нов Dialogue box започва 
        //Взима се сашния масив с dialogue lines, което държи dialogue object, currentDialogue е dialogue object
        //Изпращаме всички данни за сегашние dialogBox чрез евент и ресетваме charIndx.
        //взимаме сегашния диалог с ключ
      

        currentCharIndx = 0;
        currentDialogue = dialogueBoxDictionary[currentDialogueKey];

        //Събираме диалога от всички изречения в сегашния dialogueBox
        string fullDialogueBoxText="";
        for (int i=0; i < currentDialogue.GetSentence().Count; i++)
        {
            fullDialogueBoxText += currentDialogue.GetSentence()[i];
        }
       

        dialogueBoxSendInfoEvent.fullDialogueBoxText = fullDialogueBoxText;
        dialogueBoxSendInfoEvent.portrait = currentDialogue.getCharacterPortrait();
        dialogueBoxSendInfoEvent.name = currentDialogue.GetName();
        dialogueBoxSendInfoEvent.portrait = currentDialogue.getCharacterPortrait();

        EventManager.Instance.Publish(dialogueBoxSendInfoEvent);
        temp = "";

 
        setUiDialogue();


        SetDialogueState(dialogueStates.Typing);

    }

    void setUiDialogue()
    {
        //Всеки dialogue бокс има масив с изречения, тук взимаме сегашното,изчистваме го и изпащаме думата
        currentSentence = currentDialogue.GetSentence()[currentDialogueLineIndx];
        currentSentence = Regex.Replace(currentSentence, "<[^>]+>", "");
        dialogueCharTimer.ResetTimer();
    }

    void waitingForNextLine()
    {
        //Показва че можем да преминем на към следваща линия или диалог
        EventManager.Instance.Publish(onNextLineIndicatorOn);

        if (currentDialogueLineIndx >= currentDialogue.GetSentence().Count)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                SetDialogueState(dialogueStates.EndCurrDialogue);
            }

        }
        else if (Input.GetKeyDown(KeyCode.E))
        {
            setUiDialogue();

            currentCharIndx = 0;
            SetDialogueState(dialogueStates.Typing);
          
        }
    }
  
   
    public void SetDialogueState(dialogueStates newState)
    {
        currentState = newState;
    }
    public dialogueStates GetDialogueState()
    {
        return currentState;
    }

    public string GetCurrentDialogueKey()
    {
        return currentDialogueKey;
    }

    public void SetCurrentDialogueKey(string newDialogueKey) {
        currentDialogueKey = newDialogueKey;
    }

    void OnEnable()
    {
        EventManager.Instance.Subscribe<onDialogueQuestionAnsweredEvent, DialogueManager>(this);
    }

    void OnDisable()
    {
        EventManager.Instance.Unsubscribe<onDialogueQuestionAnsweredEvent, DialogueManager>();
    }
}
