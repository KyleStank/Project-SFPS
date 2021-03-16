using System;
using System.Collections.Generic;

namespace ProjectSFPS.Core.EventSystem
{
    public abstract class SFPSBaseAction {}

    public abstract class SFPSBaseEventAction<T> : SFPSBaseAction
    {
        protected T m_Action;

        public SFPSBaseEventAction(T action)
        {
            m_Action = action;
        }

        public bool EqualsAction(T action)
        {
            return m_Action.Equals(action);
        }
    }

    public class SFPSEventAction : SFPSBaseEventAction<Action>
    {
        public SFPSEventAction(Action action) : base(action) {}

        public void InvokeAction()
        {
            m_Action?.Invoke();
        }
    }

    public class SFPSEventAction<T1> : SFPSBaseEventAction<Action<T1>>
    {
        public SFPSEventAction(Action<T1> action) : base (action) {}

        public void InvokeAction(T1 arg1)
        {
            m_Action?.Invoke(arg1);
        }
    }

    /// <summary>
    /// Simple event system that registers, unregisters, and raises events based on a string name and a list of callbacks.
    /// </summary>
    public static class EventSystem
    {
        private static Dictionary<string, List<SFPSBaseAction>> m_EventsDict = new Dictionary<string, List<SFPSBaseAction>>();

        /// <summary>
        /// Registers a base action to an event.
        /// </summary>
        /// <param name="name">Name of event.</param>
        /// <param name="action">Action to add.</param>
        private static void Subscribe(string name, SFPSBaseAction action)
        {
            // Get or create list of actions and add provided action to list.
            List<SFPSBaseAction> actions;
            if (!m_EventsDict.TryGetValue(name, out actions))
            {
                actions = new List<SFPSBaseAction>();
                actions.Add(action);
                m_EventsDict.Add(name, actions);
            }
            else
            {
                if (!actions.Contains(action))
                    actions.Add(action);
            }
        }

        /// <summary>
        /// Registers an action to an event.
        /// </summary>
        /// <param name="name">Name of event.</param>
        /// <param name="action">Action to add.</param>
        public static void Subscribe(string name, Action action)
        {
            SFPSEventAction eventAction = new SFPSEventAction(action);
            Subscribe(name, eventAction);
        }

        /// <summary>
        /// Registers an action to an event.
        /// </summary>
        /// <param name="name">Name of event.</param>
        /// <param name="action">Action to add.</param>
        /// <typeparam name="T1">Type of action's first parameter.</typeparam>
        public static void Subscribe<T1>(string name, Action<T1> action)
        {
            SFPSEventAction<T1> eventAction = new SFPSEventAction<T1>(action);
            Subscribe(name, eventAction);
        }

        /// <summary>
        /// Unregisters an action from an event.
        /// </summary>
        /// <param name="name">Name of event.</param>
        /// <param name="action">Action to remove.</param>
        public static void Unsubscribe(string name, Action action)
        {
            // Get list of actions.
            List<SFPSBaseAction> actions;
            if (!m_EventsDict.TryGetValue(name, out actions)) return;

            // Search for action to remove.
            for (int i = 0; i < actions.Count; i++)
            {
                SFPSEventAction eventAction = actions[i] as SFPSEventAction;
                if (eventAction.EqualsAction(action))
                {
                    actions.RemoveAt(i);
                    break;
                }
            }

            if (actions.Count == 0)
                m_EventsDict.Remove(name);
        }

        /// <summary>
        /// Unregisters an action from an event.
        /// </summary>
        /// <param name="name">Name of event.</param>
        /// <param name="action">Action to remove.</param>
        /// <typeparam name="T1">Type of action's first paramter.</typeparam>
        public static void Unsubscribe<T1>(string name, Action<T1> action)
        {
            // Get list of actions.
            List<SFPSBaseAction> actions;
            if (!m_EventsDict.TryGetValue(name, out actions)) return;

            // Search for action to remove.
            for (int i = 0; i < actions.Count; i++)
            {
                SFPSEventAction<T1> eventAction = actions[i] as SFPSEventAction<T1>;
                if (eventAction.EqualsAction(action))
                {
                    actions.RemoveAt(i);
                    break;
                }
            }

            if (actions.Count == 0)
                m_EventsDict.Remove(name);
        }

        /// <summary>
        /// Invokes all actions associated with an event.
        /// </summary>
        /// <param name="name">Name of event.</param>
        public static void Raise(string name)
        {
            // Get list of actions.
            List<SFPSBaseAction> actions;
            if (!m_EventsDict.TryGetValue(name, out actions)) return;

            for (int i = 0; i < actions.Count; i++)
                (actions[i] as SFPSEventAction)?.InvokeAction();
        }

        /// <summary>
        /// Invokes all actions associated with an event.
        /// </summary>
        /// <param name="name">Name of event.</param>
        /// <typeparam name="T1">Type of action's first parameter.</typeparam>
        public static void Raise<T1>(string name, T1 arg1)
        {
            // Get list of actions.
            List<SFPSBaseAction> actions;
            if (!m_EventsDict.TryGetValue(name, out actions)) return;

            for (int i = 0; i < actions.Count; i++)
                (actions[i] as SFPSEventAction<T1>)?.InvokeAction(arg1);
        }
    }
}
