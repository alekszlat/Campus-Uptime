

using System;
using UnityEngine;

namespace Game.Core.GameSystem
{
    [Serializable]
    public class Questions
    {
        [SerializeField] string question;
        [SerializeField] string nextId;

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