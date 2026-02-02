using System;
using Sims3.Gameplay.Core;
using Sims3.SimIFace;
using static Arro.Common.Logger;

namespace Arro.Common;

/// <summary>
/// Marks a static method as a console command (cheat) that can be executed from the game's console.
/// </summary>
/// <remarks>
/// The method must match the <see cref="CommandHandler"/> delegate signature: 
/// <c>static int MethodName(object[] parameters)</c>
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
public class RegisterCommandAttribute(string name, string description, Commands.CommandType commandType = Commands.CommandType.General)
    : Attribute //Must be defined as public
{
    public string Name { get; } = name;
    public string Description { get; } = description;
    public Commands.CommandType CommandType { get; } = commandType;
}
    
internal static class RegisterCommand
{
    [InvokeOnWorldEvent(Event.OnStartupApp)]
    public static void Initialize()
    {
        Log("Register  initialized");
        var methodsWithAttrs = AttributeCache.GetMethodsWithAttributeEx<RegisterCommandAttribute>();
        if (methodsWithAttrs.Count == 0) return;
            
        foreach (var item in methodsWithAttrs)
        {
            var method = item.Method;
            var attr = item.Attribute;
            
            var parameters = method.GetParameters();
            if (method.ReturnType != typeof(int) || parameters.Length != 1 || parameters[0].ParameterType != typeof(object[]))
            {
                Log($"Method {method.Name} has invalid signature for GameCommand!");
                continue;
            }
            var handler = (CommandHandler)Delegate.CreateDelegate(typeof(CommandHandler), method);
            
            if (!Commands.sGameCommands.mCommands.ContainsKey(attr.Name))
            {
                Commands.sGameCommands.Register(attr.Name, attr.Description, attr.CommandType, handler);
                Log($"Registered {attr.Name} with handler {handler.Method}");
            }
            else
            {
                Log($"Duplicate attribute found {attr.Name} with handler {handler.Method} - CHEAT WAS NOT REGISTERED!");
            }
        }
    }
}