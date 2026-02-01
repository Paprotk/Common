namespace Arro.Common;

internal static class TinyUIFix
{
    /// <summary>
    /// Gets the current Tiny UI Fix scale multiplier.
    /// </summary>
    public static float Scale => TinyUIFixForTS3Integration.getUIScale();
}
    
public static class TinyUIFixForTS3Integration
{
    public delegate float FloatGetter();

    public static FloatGetter getUIScale = () => 1f;
}