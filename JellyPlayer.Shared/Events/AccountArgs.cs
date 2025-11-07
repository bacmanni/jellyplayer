using JellyPlayer.Shared.Models;

namespace JellyPlayer.Shared.Events;

public class AccountArgs : EventArgs
{
    public bool Validate {  get; set; }
    public Configuration Configuration { get; set; }
}