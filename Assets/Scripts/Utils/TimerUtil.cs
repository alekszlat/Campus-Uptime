using UnityEngine;

    public class TimerUtil
    {
        private float currentTime;
        private float duration;
        bool isRepeated;


        public TimerUtil(float duration=5, bool isRepeated=false)
        {
            currentTime = 0;
            this.duration = duration;
            this.isRepeated = isRepeated;
        }
    
        //Updates time and tells us when time is ready
        public bool UpdateTimer(float currentTime)
        {
            this.currentTime += currentTime;
            return IsTimerReady();
        }
        public bool IsTimerReady()
        {
            bool isReady = currentTime >= duration;
        
            if (isRepeated && isReady) { ResetTimer(); };
            return isReady;
        }
        private bool IsTimerReady2()
        {
            bool isReady = currentTime >= duration;

            if (currentTime >= duration)
            {
                if (isRepeated)
                {
                    currentTime -= duration;
                }
                else
                {
                    currentTime = duration;
                }

                return true;
            }

            return false;
        }
    public void ResetTimer()
        {
            currentTime = 0;
        }
        public float getCurrentTime()
        {
            return currentTime;
        }
        public void setIsRepeated(bool isRepeated)
        {
            this.isRepeated = isRepeated;
        }
  
    }
