using Sims3.SimIFace;

namespace Arro.Common;

internal static class ObjectGuidExtensions
{
    /// <summary>
    /// Destroys the object associated with this GUID in the simulator and invalidates the GUID.
    /// </summary>
    public static void Dispose(this ObjectGuid guid)
    {
        Simulator.DestroyObject(guid);
        guid.Value = ObjectGuid.kInvalidObjectGuidValue;
    }
}