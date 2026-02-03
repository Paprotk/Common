using System;
using System.Linq;
using System.Reflection;
using Sims3.SimIFace;
using static Arro.Common.Logger;

namespace Arro.Common;

/// <summary>
/// Marks a method to be automatically invoked when a specific <see cref="Arro.Common.Event"/> occurs.
/// </summary>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal class InvokeOnWorldEvent(Event type) : Attribute //Must be defined as public
{
    public Event EventType { get; } = type;
}

internal enum Event
{
    OnStartupApp,
    OnEnterNotInWorld,
    OnLeaveNotInWorld,
    OnWorldQuit,
    OnQuitApp,
    OnWorldLoadFinished,
}

internal static class InvokeOnEvent
{
    public static void Initialize()
    {
        var methodsWithAttrs = AttributeCache.GetMethodsWithAttributeEx<InvokeOnWorldEvent>();
        if (methodsWithAttrs.Count == 0) return;
        
        var sortedMethods = methodsWithAttrs.OrderByDescending(item => 
            item.Method.DeclaringType?.Namespace?.StartsWith("Arro.Common") ?? false
        ).ToList();
        
        foreach (var item in sortedMethods)
        {
            SubscribeMethodToEvent(item.Method, item.Attribute.EventType);
        }
    }

    private static void SubscribeMethodToEvent(MethodInfo method, Event eventType)
    {
        var parameters = method.GetParameters();
        EventHandler handler;

        if (parameters.Length == 0)
        {
            handler = (sender, e) => method.Invoke(null, null);
        }
        else if (parameters.Length == 2 && 
                 parameters[0].ParameterType == typeof(object) &&
                 parameters[1].ParameterType == typeof(EventArgs))
        {
            handler = (EventHandler)Delegate.CreateDelegate(
                typeof(EventHandler), method);
        }
        else
        {
            Log($"Method {method.Name} has invalid signature. " +
                "Must be either parameterless or have (object, EventArgs) parameters.");
            return;
        }
            
        switch (eventType)
        {
            case Event.OnWorldLoadFinished:
                World.sOnWorldLoadFinishedEventHandler -= handler;
                World.sOnWorldLoadFinishedEventHandler += handler;
                break;
            case Event.OnStartupApp:
                World.sOnStartupAppEventHandler -= handler;
                World.sOnStartupAppEventHandler += handler;
                break;
            case Event.OnEnterNotInWorld:
                World.sOnEnterNotInWorldEventHandler -= handler;
                World.sOnEnterNotInWorldEventHandler += handler;
                break;
            case Event.OnLeaveNotInWorld:
                World.sOnLeaveNotInWorldEventHandler -= handler;
                World.sOnLeaveNotInWorldEventHandler += handler;
                break;
            case Event.OnWorldQuit:
                World.sOnWorldQuitEventHandler -= handler;
                World.sOnWorldQuitEventHandler += handler;
                break;
            case Event.OnQuitApp:
                World.sOnQuitAppEventHandler -= handler;
                World.sOnQuitAppEventHandler += handler;
                break;
            default: 
                Log($"Unknown event type: {eventType}");
                break;
        }
    }
}