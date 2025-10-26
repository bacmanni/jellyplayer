using JellyPlayer.Shared.Enums;
using JellyPlayer.Shared.Models;

namespace JellyPlayer.Shared.Services;

public interface IFileService
{
    public Task<byte[]?> GetFileAsync(FileType type, Guid id);
}