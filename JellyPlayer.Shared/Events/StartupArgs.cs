using JellyPlayer.Shared.Enums;

namespace JellyPlayer.Shared.Events;

public class StartupArgs : EventArgs
{
    public StartupState  State { get; set; }
}