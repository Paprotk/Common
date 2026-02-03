using Arro.MCR.Common.Tasks;
using Sims3.SimIFace;
using Sims3.UI;

namespace Arro.Common;

internal static class WindowExtensions 
{
    /// <summary>
    /// Gets a child window by ID and casts it to the specified type.
    /// </summary>
    /// <typeparam name="T">The type to cast the child to (e.g., Button, ItemGrid).</typeparam>
    /// <param name="id">The Control ID of the child.</param>
    /// <returns>The child as type T, or null if not found.</returns>
    public static T GetChildByID<T>(this WindowBase window, uint id) where T : class
    {
        if (window == null) return null;
        return window.GetChildByID(id, true) as T;
    }
    
    /// <summary>
    /// Retrieves a window from this layout using its Export ID and casts it to type T.
    /// </summary>
    /// <typeparam name="T">The type to cast the window to.</typeparam>
    /// <param name="exportId">The Export ID defined in the layout file (signed int).</param>
    /// <returns>The window as type T, or null if not found.</returns>
    public static T GetWindowByExportID<T>(this Layout layout, int exportId) where T : class
    {
        if (layout == null) return null;
        return layout.GetWindowByExportID(exportId) as T;
    }
    
    /// <summary>
    /// Sets the position of this window and automatically applies the TinyUIFix scale.
    /// </summary>
    /// <param name="x">Unscaled X coordinate.</param>
    /// <param name="y">Unscaled Y coordinate.</param>
    public static void SetPosition(this WindowBase window, float x, float y)
    {
        if (window == null) return;
        window.Position = new Vector2(x * TinyUIFix.Scale, y * TinyUIFix.Scale);
    }
    
    /// <summary>
    /// Sets the size of this window (Width and Height) and automatically applies the TinyUIFix scale.
    /// </summary>
    /// <param name="width">Unscaled width.</param>
    /// <param name="height">Unscaled height.</param>
    public static void SetSize(this WindowBase window, float width, float height)
    {
        if (window == null) return;
        Rect area = window.Area;
        area.Width = width * TinyUIFix.Scale;
        area.Height = height * TinyUIFix.Scale;
        window.Area = area;
    }
    
    /// <summary>
    /// Sets only the height of this window while maintaining current width. TinyUIFix scale is applied automatically.
    /// </summary>
    /// <param name="height">Unscaled height.</param>
    public static void SetHeight(this WindowBase window, float height)
    {
        if (window == null) return;
        Rect area = window.Area;
        area.Height = height * TinyUIFix.Scale;
        window.Area = area;
    }
    
    /// <summary>
    /// Sets only the width of this window while maintaining current height. TinyUIFix scale is applied automatically.
    /// </summary>
    /// <param name="width">Unscaled width.</param>
    public static void SetWidth(this WindowBase window, float width)
    {
        if (window == null) return;
        Rect area = window.Area;
        area.Width = width * TinyUIFix.Scale;
        window.Area = area;
    }

    /// <summary>
    /// Sets the transparency of the window by updating the Alpha channel of its ShadeColor.
    /// </summary>
    /// <remarks>
    /// This method preserves the existing Red, Green, and Blue values of the window's 
    /// ShadeColor, modifying only the opacity.
    /// </remarks>
    /// <param name="window">The <see cref="WindowBase"/> instance to modify.</param>
    /// <param name="opacity">The alpha value (0 for fully transparent, 255 for fully opaque).</param>
    public static void SetOpacity(this WindowBase window, int opacity)
    {
        if (window == null) return;
        var shadeColor = new Color(window.ShadeColor.Red, window.ShadeColor.Green, window.ShadeColor.Blue, opacity);
        window.ShadeColor = shadeColor;
    }
    
    public static void FadeIn(this WindowBase window, int durationMs = 300, FakeFade.EaseType ease = FakeFade.EaseType.EaseInOut)
    {
        if (window != null && !window.Disposed)
        {
            Simulator.AddObject(new FakeFade.FadeIn(window, durationMs, ease));
        }
    }

    public static void FadeOut(this WindowBase window, int durationMs = 300, FakeFade.EaseType ease = FakeFade.EaseType.EaseInOut, bool hide = true)
    {
        if (window != null && !window.Disposed)
        {
            Simulator.AddObject(new FakeFade.FadeOut(window, durationMs, ease, 0, hide));
        }
    }

    public static void CrossFadeTo(this WindowBase fadeOutWindow, WindowBase fadeInWindow, int durationMs = 500, FakeFade.EaseType ease = FakeFade.EaseType.EaseInOut)
    {
        if (fadeOutWindow != null && !fadeOutWindow.Disposed && fadeInWindow != null && !fadeInWindow.Disposed)
        {
            Simulator.AddObject(new FakeFade.CrossFade(fadeOutWindow, fadeInWindow, durationMs, ease));
        }
    }
}