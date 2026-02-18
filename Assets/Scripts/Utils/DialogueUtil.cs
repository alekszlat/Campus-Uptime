using Game.Core.EventSystem;
using Game.Core.GameSystem;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using Unity.Collections;
using UnityEngine;


public enum QuestionState { NoQuestion, HasQuestion, QuestionAlreadyAsked };
class dialogBoxInfo
{
    QuestionState questionState = QuestionState.NoQuestion;
    List<string> sentence = new List<string>();
    Sprite characterPortrait;
    string name;
    List<Questions> questions = new List<Questions>();
    //Име на диалог
    string id;
    //Връзка към следващ диалог, ако е нъл при избор се взема един от id-тата от questions
    string nextId;
    
    public dialogBoxInfo(string id,string nextId,List<string> sentence, List<Questions> questions, Sprite characterPortrait, string name)
    {
        this.sentence = sentence;
        this.characterPortrait = characterPortrait;
        this.name = name;
        this.id = id;
        this.nextId = nextId;
        this.questions = questions;
        
        if (questions!=null)
        {
            questionState = QuestionState.HasQuestion;
        }
        else
        {
            questionState = QuestionState.NoQuestion;
        }

    }
   
    public string GetName()
    {
        return name;
    }
    public Sprite getCharacterPortrait()
    {
        return characterPortrait;
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
        return id;
    }
    public string GetNextId()
    {
        return nextId;
    }
}


public class DialogueUtil : MonoBehaviour,IEventHandler<onDialogueQuestionAnsweredEvent>
{

    [SerializeField] private bool frezeCharacterDuringDialogue = false;

    Dictionary<string, dialogBoxInfo> dialogueBoxDictionary = new Dictionary<string, dialogBoxInfo>();
    private string currentSentence;//сегашно изречение от масива с изречения на един dialogueBox

    private string temp ="";//събира сегашното изречение буква по буква

    [SerializeField] private float startingCharDelay= 0.005f;
    private float currentCharDelay;
   
    dialogBoxInfo currentDialogue;//сегашния dialogueBox
    private int currentBoxCharIndx = 0;//общ чар в целия dialogBox 
    private int currentCharIndx = 0;//сегашен чар на сегашното изречение
    private string currentDialogueKey = " ";//ключ на кой диалог сме
    private int currentDialogueLineIndx = 0;//индекс на коя линия в диалога сме
    
    enum dialogueStates{inactive,initiazlizeDialogue,StartCurrDialogue,dialogueQuestionState, WaitingForNextLine, Typing,EndCurrDialogue,Close}
    string startKey="sad";
    dialogueStates currentState=dialogueStates.initiazlizeDialogue;

    //EVENTS
    OnDialogueBoxInfoEvent dialogueNewSentenceEvent = new OnDialogueBoxInfoEvent();
    OnDialogueIndxChangedEvent dialogueIndxChangedEvent = new OnDialogueIndxChangedEvent();
    OnNextLineIndicatorOn onNextLineIndicatorOn = new OnNextLineIndicatorOn();
    OnNextLineIndicatorOff onNextLineIndicatorOff = new OnNextLineIndicatorOff();
    OnDialogueEndEvent onDialogueEndEvent = new OnDialogueEndEvent();
    OnDialogueStartEvent onDialogueStartEvent = new OnDialogueStartEvent();
    OnFreezePlayerDuringDialogue onPlayerFreezePlayerDuringDialogue = new OnFreezePlayerDuringDialogue();
    OnUnFreezePlayerOnDialogueEnd onPlayerUnFreezePlayerOnDialogueEnd = new OnUnFreezePlayerOnDialogueEnd();
    onSendDialogueQuestions onSendDialogueQuestions = new onSendDialogueQuestions();
    TimerUtil dialogueCharTimer;//CustomTimer
    dialogBoxInfo a;
    dialogBoxInfo b;
    dialogBoxInfo c;
    string lastKey;
    bool hasAlreadyConversed = false;
    private void Awake()
    {
        List<string> list1 = new List<string>();
        list1.Add("AAAAAAAA.\n");
        list1.Add("<color=#FF0000>DIMOFF</color> MAMKA MU\n");
        list1.Add("MAZNA");

        List<string> list2 = new List<string>();
        list2.Add("<size=400%>brrrrrrrrrrrrr.</size>");
        list2.Add("Losho \n");
        list2.Add("MAZNA\n");
        List<string> list3 = new List<string>();
        list3.Add("Кuchjeto e ok :)");
 
        List<Questions> questions = new List<Questions>();

        questions.Add(new Questions("Da dog not Fine", "depressed"));
        questions.Add(new Questions("Da dog fine", "happy"));
        questions.Add(new Questions("Da dog not Fine", "depressed"));
        questions.Add(new Questions("Da dog fine", "happy"));

        a = new dialogBoxInfo("sad", "", list1, questions, null, "Dave");
        b = new dialogBoxInfo("depressed", "", list2, null, null, "Dave tujniq");
        c = new dialogBoxInfo("happy", "", list3, null, null, "Dave shtastliviq");

        dialogueBoxDictionary[a.GetId()] = a;
        dialogueBoxDictionary[b.GetId()] = b;
        dialogueBoxDictionary[c.GetId()] = c;
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


            //началният ключ от тук ще започне диалога
            if (!hasAlreadyConversed)
            {
                currentDialogueKey = startKey;
                hasAlreadyConversed = true;
            }
            else
            {
                currentDialogueKey = lastKey;
            }

            EventManager.Instance.Publish(onDialogueStartEvent);
            switchState(dialogueStates.StartCurrDialogue);
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
        print(currentState);

        print(currentDialogueKey);
        if (currentDialogueKey != ""&&lastKey!=currentDialogueKey)
        {
            lastKey = currentDialogueKey;
        }
    }


    void typingState()
    {
        //Когато таймера е верен се изпълнява,което става на всеки currentCharDelay секунди
        //Тук увеличаваме индекса на сегашното изречение, и на всички изречения в dialogueBox, и пращаме индекса наза целия dialogue box,защото
        //ui manager има целия текст от dialogueBox,докато тук обработваме само едно изречение, за да се знае, кога DialogueUI, да прави char visible
        //Затова се изпраща евент към DialogueII със index на целия dialogueBox

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
                switchState(dialogueStates.WaitingForNextLine);
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
   
         switchState(dialogueStates.inactive);

    }

    public void Handle(onDialogueQuestionAnsweredEvent @event)
    {
     
        //когато отговорим на въпроса сменяме сегашния диалог,сменяме стейта и маркираме,че въпросът е зададен
        currentDialogueKey = @event.nextDialogueNode;
        print("Event klucha e "+@event.nextDialogueNode);
        switchState(dialogueStates.EndCurrDialogue);
        currentDialogue.SetQuestionState(QuestionState.QuestionAlreadyAsked);
    }
    void endCurrentDialogueState()
    {
        //Tози стейт само ни пренасочва към стейт
        //Ако има още диалози ни насочва към стейт за диалог, ако не затватя диалога

        if (currentDialogue.GetQuestionState() == QuestionState.HasQuestion)
        {
            //Ако има въпрос влизаме в празен стейт,докато чакаме отговор и изпращаме въпросите на DialogueManager
            switchState(dialogueStates.dialogueQuestionState);
            onSendDialogueQuestions.dialogueQuestions = currentDialogue.GetQuestions();
            EventManager.Instance.Publish(onSendDialogueQuestions);
        } //Ako има някакъв диалог
        else if (currentDialogue.GetNextId().Length != 0 || currentDialogue.GetQuestionState() == QuestionState.QuestionAlreadyAsked)
        {
            //Ако има следващо Id го взима в противен случай, взима диалога избран от играча
            //запазваме последния ключ диалог
            if (currentDialogue.GetNextId().Length != 0)
            {
                currentDialogueKey = currentDialogue.GetNextId();
         
            }
           
            currentDialogueLineIndx = 0;
            currentBoxCharIndx = 0;
            switchState(dialogueStates.StartCurrDialogue);
        }
        else
        {
            currentDialogueLineIndx = 0;
            switchState(dialogueStates.Close);
        }
    }
  
    void dialogueStartState()
    {
        //Тук е когато всеки нов Dialogue box започва 
        //Взима се сашния масив с dialogue lines, което държи dialogue object, currentDialogue е dialogue object
        //Изпращаме всички данни за сегашние dialogBox чрез евент и ресетваме charIndx.
        //взимаме сегашния диалог с ключ
        currentCharIndx = 0;
        //Ако сме минали през диалога веднъж ключа ще е празен низ, за това всимаме последния ключ
        
 
        currentDialogue = dialogueBoxDictionary[currentDialogueKey];

        //Събираме диалога от всички изречения в сегашния dialogueBox
        string fullDialogueBoxText="";
        for (int i=0; i < currentDialogue.GetSentence().Count; i++)
        {
            fullDialogueBoxText += currentDialogue.GetSentence()[i];
        }
       

        dialogueNewSentenceEvent.fullDialogueBoxText = fullDialogueBoxText;
        dialogueNewSentenceEvent.portrait = currentDialogue.getCharacterPortrait();
        dialogueNewSentenceEvent.name = currentDialogue.GetName();


        EventManager.Instance.Publish(dialogueNewSentenceEvent);
        temp = "";

   
 
        setuiDialogue();


        switchState(dialogueStates.Typing);

    }
    void setuiDialogue()
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
            if (Input.GetKeyDown(KeyCode.Space))
            {
                switchState(dialogueStates.EndCurrDialogue);
            }

        }
        else if (Input.GetKeyDown(KeyCode.Space))
        {
            setuiDialogue();

            currentCharIndx = 0;
            switchState(dialogueStates.Typing);
          
        }
    }
  
   
    void switchState(dialogueStates newState)
    {
        //Сменяме стейа
        currentState = newState;


    }
 

    public string getCurrentDialogueKey()
    {
        return currentDialogueKey;
    }

    public void setCurrentDialogueKey(string newDialogueKey) {
        currentDialogueKey = newDialogueKey;
    }

    void OnEnable()
    {
        EventManager.Instance.Subscribe<onDialogueQuestionAnsweredEvent, DialogueUtil>(this);
    }

    void OnDisable()
    {
        EventManager.Instance.Unsubscribe<onDialogueQuestionAnsweredEvent, DialogueUtil>();
    }
}
