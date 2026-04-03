/* Controls in-game time flow and triggers time-based events.
 * Handles countdowns, pauses, and publishing events such as OnDayEndEvent.
 * 
 * Author: H. Hristov (milkeles)
 * Created: 11/10/2025 (dd/mm/yyyy)
 * Updated: 11/10/2025 (dd/mm/yyyy)
 */

using System;
using Game.Core.EventSystem;
using Game.Core.GameSystem;
using UnityEngine;

namespace Game.Core.TimeSystem
{
    /// <summary>
    /// Singleton component that manages in-game time progression and timing events.
    /// </summary>
    public class TimeManager : MonoBehaviour
    {
        private static TimeManager _instance;

        public static TimeManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<TimeManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("TimeManager");
                        _instance = go.AddComponent<TimeManager>();
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        [SerializeField] private float startingTime = 15f;

        private TimeManagerCore _core;

        public float RemainingTime => _core.RemainingTime;
        public bool IsPaused => _core.IsPaused;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            _core = new TimeManagerCore(startingTime);
            _core.OnDayEnded += HandleDayEnd;
        }

        private void Update()
        {
            _core.Tick(Time.deltaTime);
        }

        private void HandleDayEnd()
        {
            EventManager.Instance.Publish(new OnDayEndEvent
            {
                DayNumber = 1,
                TasksCompleted = 0
            });
        }
        // Simple wrappers for the core logic
        public void AddTime(float seconds) => _core.AddTime(seconds);
        public void SubtractTime(float seconds) => _core.SubtractTime(seconds);
        public void Pause() => _core.Pause();
        public void Resume() => _core.Resume();
        public void ResetTime() => _core.Reset(startingTime);
    }

    /// <summary>
    /// Core time logic used by the TimeManager.
    /// Handles ticking, pausing, and triggering end-of-day events.
    /// </summary>
    public class TimeManagerCore
    {
        public event Action OnDayEnded;
        public float RemainingTime { get; private set; }
        public bool IsPaused { get; private set; }

        private bool dayEnded;

        public TimeManagerCore(float startTime)
        {
            RemainingTime = startTime;
            dayEnded = false;
        }

        public void Tick(float deltaTime)
        {
            if (IsPaused || dayEnded) return;

            RemainingTime -= deltaTime;
            if (RemainingTime > 0f) return;

            RemainingTime = 0;
            dayEnded = true;
            OnDayEnded?.Invoke();
        }

        public void AddTime(float seconds)
        {
            if (seconds < 0f)
            {
                throw new ArgumentOutOfRangeException();
            }
            RemainingTime += seconds;
        }

        public void SubtractTime(float seconds)
        {
            if (seconds < 0f)
            {
                throw new ArgumentOutOfRangeException();
            }
            RemainingTime = Math.Max(0f, RemainingTime - seconds);
        }

        public void Reset(float startTime)
        {
            RemainingTime = startTime;
            dayEnded = false;
        }

        public void Pause() => IsPaused = true;
        public void Resume() => IsPaused = false;
    }
}