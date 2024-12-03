using System.IO.MemoryMappedFiles;
using System.Runtime.InteropServices;

namespace RaceElement.Data.Games.Automobilista2.SharedMemory;

internal static class SharedMemory
{
    private static Shared Memory;
    private static MemoryMappedFile _file;

    public static Shared ReadSharedMemory(bool fromCache = false)
    {
        if (fromCache)
        {
            return Memory;
        }

        try
        {
            _file = MemoryMappedFile.OpenExisting(Constants.SharedMemoryName);
        }
        catch (FileNotFoundException)
        {
            return Memory;
        }

        var view = _file.CreateViewStream();
        BinaryReader stream = new(view);

        var buffer = stream.ReadBytes(Marshal.SizeOf(typeof(Shared)));
        GCHandle handle = GCHandle.Alloc(buffer, GCHandleType.Pinned);

        Memory = (Shared)Marshal.PtrToStructure(handle.AddrOfPinnedObject(), typeof(Shared));
        handle.Free();

        return Memory;
    }

    public static void Clean()
    {
        Memory = new();
        _file?.Dispose();
    }
}
