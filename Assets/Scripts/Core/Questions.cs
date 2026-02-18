

namespace Game.Core.GameSystem
{
    public class Questions
    {
        string question;
        string nextId;

        public Questions(string question, string nextId)
        {
            this.question = question;
            this.nextId = nextId;
        }
        public string GetQuestion()
        {
            return question;
        }
        public string getNextId()
        {
            return nextId;
        }
    }
}