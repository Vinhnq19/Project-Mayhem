using System;
using System.Collections.Generic;

public static class EventBus
{

    private static readonly Dictionary<string, List<Delegate>> listeners = new();

    public static void On(string eventName, Delegate callback)
    {
        if (!listeners.ContainsKey(eventName)) listeners[eventName] = new();
        listeners[eventName].Add(callback);
    }

    public static void On(GameEvent gameEvent, Delegate callback)
    {

        string eventName = gameEvent.ToString();
        if (!listeners.ContainsKey(eventName)) listeners[eventName] = new();
        listeners[eventName].Add(callback);
    }


    public static void Off(string eventName, Delegate callback)
    {
        if (listeners.ContainsKey(eventName))
        {
            listeners[eventName].Remove(callback);
            if (listeners[eventName] == null)
                listeners.Remove(eventName);
        }
    }

    public static void Off(GameEvent gameEvent, Delegate callback)
    {
        string eventName = gameEvent.ToString();
        if (listeners.ContainsKey(eventName))
        {
            listeners[eventName].Remove(callback);
            if (listeners[eventName] == null)
                listeners.Remove(eventName);
        }
    }

    public static void Emit(string eventName, params object[] args)
    {
        if (!listeners.TryGetValue(eventName, out var delegates))
            return;

        foreach (var callback in delegates)
        {
            var parameters = callback.Method.GetParameters();
            if (parameters.Length == args.Length)
            {
                try
                {
                    callback.DynamicInvoke(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"⚠️ Event '{eventName}' invoke failed: {e.Message}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ Event '{eventName}' argument mismatch ({parameters.Length} expected, got {args.Length}).");
            }
        }
    }

    public static void Emit(GameEvent gameEvent, params object[] args)
    {
        string eventName = gameEvent.ToString();
        if (!listeners.TryGetValue(eventName, out var delegates))
            return;

        foreach (var callback in delegates)
        {
            var parameters = callback.Method.GetParameters();
            if (parameters.Length == args.Length)
            {
                try
                {
                    callback.DynamicInvoke(args);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"⚠️ Event '{eventName}' invoke failed: {e.Message}");
                }
            }
            else
            {
                Console.WriteLine($"⚠️ Event '{eventName}' argument mismatch ({parameters.Length} expected, got {args.Length}).");
            }
        }
    }
}
