using System;
using SecretAdmin.API.Events.Arguments;
using SecretAdmin.Features.Console;
using Spectre.Console;

namespace SecretAdmin.API.Events;

public static class Handler
{
    public delegate void SecretEventHandler<in T>(T ev) where T : EventArgs;

    private static void InvokeEvent<T>(this SecretEventHandler<T> eventHandler, T args) where T : EventArgs
    {
        if (eventHandler is null)
            return;
        
        foreach (Delegate sub in eventHandler.GetInvocationList())
        {
            try
            {
                sub.DynamicInvoke(args);
            }
            catch (Exception e)
            {
                Log.WriteLine("An error occurred while handling the event " + eventHandler.GetType().Name);
                AnsiConsole.WriteException(e);
                throw;
            }
        }
    }
    
    public static event SecretEventHandler<ReceivingActionEventArgs> ReceivingAction;
    public static event SecretEventHandler<ReceivingInputEventArgs> ReceivingInput;
    public static event SecretEventHandler<ReceivingMessageEventArgs> ReceivingMessage;
    public static event SecretEventHandler<RestartingServerEventArgs> RestartingServer;
    public static event SecretEventHandler<StartingServerEventArgs> StartingServer;
    public static event SecretEventHandler<StoppingServerEventArgs> StoppingServer;

    public static void OnReceivingAction(ReceivingActionEventArgs args) => ReceivingAction.InvokeEvent(args);
    public static void OnReceivingInput(ReceivingInputEventArgs args) => ReceivingInput.InvokeEvent(args);
    public static void OnReceivingMessage(ReceivingMessageEventArgs args) => ReceivingMessage.InvokeEvent(args);
    public static void OnRestartingServer(RestartingServerEventArgs args) => RestartingServer.InvokeEvent(args);
    public static void OnStartingServer(StartingServerEventArgs args) => StartingServer.InvokeEvent(args);
    public static void OnStoppingServer(StoppingServerEventArgs args) => StoppingServer.InvokeEvent(args);
}