using System;
using System.Collections.Generic;

public static class EventBus
{
    public delegate void EventCallback(params object[] data);

    private static readonly Dictionary<string, EventCallback> listeners = new();

    public static void On(string eventName, EventCallback callback)
    {
        if (listeners.ContainsKey(eventName))
            listeners[eventName] += callback;
        else
            listeners[eventName] = callback;
    }

    public static void Off(string eventName, EventCallback callback)
    {
        if (listeners.ContainsKey(eventName))
        {
            listeners[eventName] -= callback;
            if (listeners[eventName] == null)
                listeners.Remove(eventName);
        }
    }

    public static void Emit(string eventName, params object[] data)
    {
        if (listeners.ContainsKey(eventName))
            listeners[eventName]?.Invoke(data);
    }
}
