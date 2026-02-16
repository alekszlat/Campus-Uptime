using Game.Core.EventSystem;
using Game.Core.GameSystem;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


class dialogBoxInfo
{
    public enum QuestionState {NoQuestion,HasQuestion,QuestionAlreadyAsked};
    List<string> sentence = new List<string>();
    Sprite characterPortrait;
    string name;
    List<string> questions = new List<string>();

    QuestionState questionState = QuestionState.NoQuestion;
    
    public dialogBoxInfo(List<string> sentence, Sprite characterPortrait, string name)
    {
        this.sentence = sentence;
        this.characterPortrait = characterPortrait;
        this.name = name;

        if (questions.Count > 0)
        {
            questionState = QuestionState.HasQuestion;
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
    public List<string> GetQuestions()
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
  
}


public class DialogueUtil : MonoBehaviour
{

    [SerializeField] private bool frezeCharacterDuringDialogue = false;
    private List<dialogBoxInfo> dialogueLinesArray = new List<dialogBoxInfo>();
 
    private string currentSentence;//сегашно изречение от масива с изречения на един dialogueBox

    private string temp ="";//събира сегашното изречение буква по буква

    [SerializeField] private float startingCharDelay= 0.005f;
    private float currentCharDelay;
   
    dialogBoxInfo currentDialogue;//сегашния dialogueBox
    private int currentBoxCharIndx = 0;//общ чар в целия dialogBox 
    private int currentCharIndx = 0;//сегашен чар на сегашното изречение
    private int currentDialogueBoxIndx = 0;//индекс на кой диалог сме
    private int currentDialogueLineIndx = 0;//индекс на коя линия в диалога сме
    enum dialogueStates{inactive,initiazlizeDialogue,StartCurrDialogue,dialogueQuestionState, WaitingForNextLine, Typing,EndCurrDialogue,Close}

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
    TimerUtil dialogueCharTimer;//CustomTimer


  

    private void Start()
    {
      

        List<string> list1 = new List<string>();
        list1.Add("AAAAAAAA.\n");
        list1.Add("<color=#FF0000>DIMOFF</color> MAMKA MU\n");
        list1.Add("MAZNA");

        List<string> list2 = new List<string>();
        list2.Add("<size=400%>brrrrrrrrrrrrr.</size>");
        list2.Add("ZZZZZ MU\n");
        list2.Add("MAZNA\n");


        dialogBoxInfo a = new dialogBoxInfo(list1,null,"Dave");
        dialogBoxInfo b = new dialogBoxInfo(list2, null, "Dave tupiq");
        dialogueLinesArray.Add(a);
        dialogueLinesArray.Add(b);

     
        currentCharDelay = startingCharDelay;
        dialogueCharTimer = new TimerUtil(currentCharDelay, true);


    }
    // Update is called once per frame
    void Update()
    {
        if (currentState == dialogueStates.initiazlizeDialogue)
        {
            EventManager.Instance.Publish(onDialogueStartEvent);
            if (frezeCharacterDuringDialogue)
            {
                EventManager.Instance.Publish(onPlayerFreezePlayerDuringDialogue);
            }
            switchState(dialogueStates.StartCurrDialogue);
       
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

            endDialogueState();

        }
        else if(currentState== dialogueStates.dialogueQuestionState)
        {
            //IF SIGNAL CONFIRMATION SENT FROM UI TO DIALOGUE SwitchToDialogue
        }
        else if (currentState == dialogueStates.Close)
        {
            closeDialogueState();
        }
        print(currentState);

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
         currentDialogueBoxIndx = 0;
         switchState(dialogueStates.inactive);

    }
  
    void endDialogueState()
    {
        //Tози стейт само ни пренасочва към стейт
        //Ако има още диалози ни насочва към стейт за диалог, ако не затватя диалога
       
        if (currentDialogue.GetQuestionState()==dialogBoxInfo.QuestionState.HasQuestion)
        {
            switchState(dialogueStates.dialogueQuestionState);
            //EMIT SIGNAL
        }
        else if (currentDialogueBoxIndx+1 < dialogueLinesArray.Count)
        {
                currentDialogueBoxIndx++;
                currentDialogueLineIndx = 0;
                currentBoxCharIndx = 0;
                switchState(dialogueStates.StartCurrDialogue);
        }
        else
        {
            switchState(dialogueStates.Close);

        }
    }
    void dialogueStartState()
    {
        //Тук е когато всеки нов Dialogue box започва 
        //Взима се сашния масив с dialogue lines, което държи dialogue object, currentDialogue е dialogue object
        //Изпращаме всички данни за сегашние dialogBox чрез евент и ресетваме charIndx.

        currentCharIndx = 0; 
        currentDialogue = dialogueLinesArray[currentDialogueBoxIndx];

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
        //Взима сегашното изречение от масива с изречение, и го изчиства от тагове, те са нужни само когато изпращаме изреченията на ui
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
 

    //OLD
    IEnumerator typeCurrentWord()
    {
        while (currentCharIndx < currentSentence.Length)
        {
         
            temp += currentSentence[currentCharIndx++];
            currentBoxCharIndx++;
            dialogueIndxChangedEvent.NewDialogueIndx = currentBoxCharIndx;

            EventManager.Instance.Publish(dialogueIndxChangedEvent);

            yield return new WaitForSeconds(currentCharDelay);
        }

        currentDialogueLineIndx++;
        switchState(dialogueStates.WaitingForNextLine);
    }

    
}
