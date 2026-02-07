/* Central coordinator for core systems and global events.
 * Ensures essential managers are initialized and persists across scenes.
 *
 * Author: H. Hristov (milkeles)
 * Created: 11/10/2025
 * Updated: 11/10/2025
 */

using UnityEngine;
using Game.Core.EventSystem;
using Game.Core.TimeSystem;

namespace Game.Core.GameSystem
{
    /**********************
    * Event Declarations  *
    ***********************/

    /// <summary>
    /// Published when a day ends.
    /// </summary>
    public class OnDayEndEvent : GameEvent
    {
        public int DayNumber { get; set; }
        public int TasksCompleted { get; set; }
    }

    /**********************
    *    Event Handlers   *
    ***********************/

    /// <summary>
    /// Example handler for OnDayEndEvent.
    /// </summary>
    public class ExampleDayEndHandler : IEventHandler<OnDayEndEvent>
    {
        public void Handle(OnDayEndEvent @event)
        {
            Debug.Log($"Day {@event.DayNumber} ended. Tasks: {@event.TasksCompleted}");
        }
    }

    /// <summary>
    /// Initializes core systems and manages global event subscriptions.
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        private static GameManager _instance;

        public static GameManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<GameManager>();
                    if (_instance == null)
                    {
                        var go = new GameObject("GameManager");
                        _instance = go.AddComponent<GameManager>();
                    }
                }
                return _instance;
            }
            private set => _instance = value;
        }

        protected virtual void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeSystems();
        }

        /// <summary>
        /// Sets up core managers and subscribes event handlers.
        /// </summary>
        private void InitializeSystems()
        {
            _ = TimeManager.Instance;
            _ = EventManager.Instance;

            EventManager.Instance.Subscribe<OnDayEndEvent, ExampleDayEndHandler>();

            Debug.Log("Initialized core systems.");
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                EventManager.Instance.Unsubscribe<OnDayEndEvent, ExampleDayEndHandler>();
                _instance = null;
            }
        }
    }
}
