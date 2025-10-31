using JellyPlayer.Shared.Enums;

namespace JellyPlayer.Shared.Services;

public interface IFileService
{
    public Task<byte[]?> GetFileAsync(FileType type, Guid id);
}