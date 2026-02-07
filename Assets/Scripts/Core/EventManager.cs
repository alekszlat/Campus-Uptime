/* Global event bus for the game. Handles event subscribe-publish-unsubscribe mechanism.
 * Utilizes the Singleton pattern to ensure global access to the
 * event mechanism and a single instance of the manager.
 * It also uses the Publish/Subscribe pattern, implemented through generic classes,
 * to ensure clean and scalable event bus implementation.
 *
 * Author: H. Hristov (milkeles)
 * Created: 11/10/2025 (dd/mm/yyyy)
 * Updated: 11/10/2025 (dd/mm/yyyy)
 */
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Core.EventSystem
{
    /// <summary>
    /// Base class for all game events. Provides a unique identifier and timestamp.
    /// </summary>
    public abstract class GameEvent
    {
        /// <summary>Time (UTC) when the event was created.</summary>
        public DateTime Timestamp { get; private set; }

        /// <summary>Unique ID assigned to this event instance.</summary>
        public string EventId { get; private set; }

        /// <summary>Creates a new game event with a timestamp and unique identifier.</summary>
        protected GameEvent()
        {
            Timestamp = DateTime.UtcNow;
            EventId = Guid.NewGuid().ToString();
        }
    }

    /// <summary>
    /// Defines the interface for a handler that reacts to a specific type of <see cref="GameEvent"/>.
    /// </summary>
    /// <typeparam name="T">Type of event this handler processes.</typeparam>
    public interface IEventHandler<in T> where T : GameEvent
    {
        /// <summary>Handles the provided event.</summary>
        /// <param name="event">The event instance to handle.</param>
        void Handle(T @event);
    }

    /// <summary>
    /// Interface for the event bus system. Provides methods for publishing, subscribing, and unsubscribing.
    /// </summary>
    public interface IEventBus
    {
        /// <summary>
        /// Publishes an event to all subscribed handlers of its type.
        /// </summary>
        /// <typeparam name="T">Type of the event to publish.</typeparam>
        /// <param name="event">The event instance being published.</param>
        void Publish<T>(T @event) where T : GameEvent;

        /// <summary>
        /// Subscribes a handler type to a specific event type.
        /// </summary>
        /// <typeparam name="T1">Type of the event.</typeparam>
        /// <typeparam name="T2">Type of the event handler.</typeparam>
        void Subscribe<T1, T2>()
            where T1 : GameEvent
            where T2 : IEventHandler<T1>;

        /// <summary>
        /// Unsubscribes a handler type from a specific event type.
        /// </summary>
        /// <typeparam name="T1">Type of the event.</typeparam>
        /// <typeparam name="T2">Type of the event handler.</typeparam>
        void Unsubscribe<T1, T2>()
            where T1 : GameEvent
            where T2 : IEventHandler<T1>;
    }

    /// <summary>
    /// Implementation of <see cref="IEventBus"/> using a singleton pattern.
    /// Manages subscriptions and dispatching of game events.
    /// </summary>
    public class EventManager : IEventBus
    {
        /// <summary>Singleton instance of the EventManager.</summary>
        private static readonly Lazy<EventManager> _instance = new(() => new EventManager());

        /// <summary>Global accessor for the singleton instance.</summary>
        public static EventManager Instance => _instance.Value;

        /// <summary>Private constructor to prevent external instantiation.</summary>
        private EventManager() { }

        /// <summary>Maps event types to their handler type lists.</summary>
        private readonly Dictionary<Type, List<Type>> _handlers = new();

        /// <summary>Caches instances of handler objects to avoid repeated instantiation.</summary>
        private readonly Dictionary<Type, object> _handlerInstances = new();

        /// <summary>
        /// Subscribes a handler type to a given event type.
        /// </summary>
        /// <typeparam name="T">The type of event.</typeparam>
        /// <typeparam name="TH">The type of the event handler.</typeparam>
        public void Subscribe<T, TH>()
            where T : GameEvent
            where TH : IEventHandler<T>
        {
            Type eventType = typeof(T);
            Type handlerType = typeof(TH);

            if (!_handlers.ContainsKey(eventType))
                _handlers[eventType] = new List<Type>();

            if (_handlers[eventType].Contains(handlerType))
            {
                Debug.LogWarning($"Handler {handlerType.Name} already subscribed to {eventType.Name}");
                return;
            }

            _handlers[eventType].Add(handlerType);
            Debug.Log($"Subscribed {handlerType.Name} to {eventType.Name}");
        }

        /// <summary>
        /// Removes a handler type from the subscription list for a given event type.
        /// </summary>
        /// <typeparam name="T">The type of event.</typeparam>
        /// <typeparam name="TH">The type of event handler.</typeparam>
        public void Unsubscribe<T, TH>()
            where T : GameEvent
            where TH : IEventHandler<T>
        {
            Type eventType = typeof(T);
            Type handlerType = typeof(TH);

            if (_handlers.ContainsKey(eventType))
            {
                _handlers[eventType].Remove(handlerType);
                if (_handlers[eventType].Count == 0)
                    _handlers.Remove(eventType);
            }

            _handlerInstances.Remove(handlerType);
        }

        /// <summary>
        /// Publishes an event to all subscribed handlers of its type.
        /// Automatically instantiates handler objects if they don't exist yet.
        /// </summary>
        /// <typeparam name="T">The type of event being published.</typeparam>
        /// <param name="event">The event instance to publish.</param>
        public void Publish<T>(T @event) where T : GameEvent
        {
            Type eventType = typeof(T);

            if (!_handlers.ContainsKey(eventType))
            {
                Debug.Log($"No handlers for event {eventType.Name}");
                return;
            }

            foreach (Type handlerType in _handlers[eventType])
            {
                if (!_handlerInstances.ContainsKey(handlerType))
                    _handlerInstances[handlerType] = Activator.CreateInstance(handlerType);

                if (_handlerInstances[handlerType] is IEventHandler<T> handler)
                    handler.Handle(@event);
            }
        }

        /// <summary>
        /// Clears all event subscriptions and cached handler instances.
        /// </summary>
        public void Clear()
        {
            _handlers.Clear();
            _handlerInstances.Clear();
        }
    }
}