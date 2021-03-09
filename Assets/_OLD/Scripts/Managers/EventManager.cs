using UnityEngine;
using UnityEngine.Events;
using System.Collections;

public static class EventManager {
    //Removes all listeners of an event, adds one listener, and then invokes the event depending on parameters
    public static void RemoveAllAndAddListener(UnityEvent e, UnityAction listenerToAdd, bool invoke) {
        e.RemoveAllListeners(); //Remove all listeners
        e.AddListener(listenerToAdd); //Add the listen

        if(invoke) //If the event should be invoked
            e.Invoke(); //Invoke all listeners
    }
}
