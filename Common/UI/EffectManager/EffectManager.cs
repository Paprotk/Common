using System.Collections.Generic;
using Sims3.SimIFace;
using Sims3.UI;

namespace Arro.Common;

/// <summary>
/// Provides utility methods to easily add and manage UI effects (animations) on windows.
/// </summary>
internal static class EffectManager
{
    /// <summary>
    /// Adds a fade-in or fade-out effect to the window.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="duration">Duration of the animation in seconds.</param>
    /// <param name="triggerType"><see cref="Sims3.UI.EffectBase.TriggerTypes"/></param>
    /// <param name="interpolationType"><see cref="Sims3.UI.EffectBase.InterpolationTypes"/></param>
    public static void AddFadeEffect(WindowBase window,
        float duration = 0.2f,
        EffectBase.TriggerTypes triggerType = EffectBase.TriggerTypes.Invisible,
        EffectBase.InterpolationTypes interpolationType = EffectBase.InterpolationTypes.EaseInOut)
    {
        FadeEffect fade = new FadeEffect();
        fade.Duration = duration;
        fade.TriggerType = triggerType;
        fade.InterpolationType = interpolationType;
        window.EffectList.Add(fade);
        window.Tag = fade;
    }

    /// <summary>
    /// Adds a scale/inflation effect, typically used for button hovers.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="scale">The target scale multiplier (e.g., 1.1f for 110%).</param>
    /// <param name="duration">Duration of the animation.</param>
    /// <param name="triggerType"><see cref="Sims3.UI.EffectBase.TriggerTypes"/></param>
    /// <param name="interpolationType"><see cref="Sims3.UI.EffectBase.InterpolationTypes"/></param>
    /// <param name="autoReverse">If true, the effect reverses when the trigger ends (e.g., mouse leaves).</param>
    public static void AddScaleEffect(WindowBase window,
        float scale = 1.1f,
        float duration = 0.2f,
        EffectBase.TriggerTypes triggerType = EffectBase.TriggerTypes.MouseFocus,
        EffectBase.InterpolationTypes interpolationType = EffectBase.InterpolationTypes.EaseInOut,
        bool autoReverse = true)
    {
        InflateEffect effect = new InflateEffect();
        effect.Scale = scale;
        effect.Duration = duration;
        effect.TriggerType = triggerType;
        if (autoReverse) effect.ResetEffect(true);
        effect.InterpolationType = interpolationType;
        window.EffectList.Add(effect);
        window.Tag = effect;
    }

    /// <summary>
    /// Adds a rotation effect around a specified axis.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="angle">The rotation angle in degrees.</param>
    /// <param name="axis">The axis of rotation (defaults to Z-axis).</param>
    /// <param name="duration">Duration of the animation.</param>
    /// <param name="triggerType"><see cref="Sims3.UI.EffectBase.TriggerTypes"/></param>
    /// <param name="interpolationType"><see cref="Sims3.UI.EffectBase.InterpolationTypes"/></param>
    /// <param name="autoReverse">If true, the effect reverses when the trigger ends.</param>
    public static void AddRotateEffect(WindowBase window,
        float angle = 10f,
        Vector3 axis = default,
        float duration = 0.2f,
        EffectBase.TriggerTypes triggerType = EffectBase.TriggerTypes.MouseFocus,
        EffectBase.InterpolationTypes interpolationType = EffectBase.InterpolationTypes.EaseInOut,
        bool autoReverse = true)
    {
        if (axis == default) axis = new Vector3(0, 0, 1);

        RotateEffect rotate = new RotateEffect();
        rotate.Angle = angle;
        rotate.RotationAxis = axis;
        rotate.Duration = duration;
        rotate.TriggerType = triggerType;
        if (autoReverse) rotate.ResetEffect(true);
        rotate.InterpolationType = interpolationType;
        window.EffectList.Add(rotate);
        window.Tag = rotate;
    }

    /// <summary>
    /// Adds a movement (sliding) effect. The offset is automatically scaled by TinyUIFix.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="offset">The unscaled distance to move.</param>
    /// <param name="duration">Duration of the animation.</param>
    /// <param name="triggerType"><see cref="Sims3.UI.EffectBase.TriggerTypes"/></param>
    /// <param name="interpolationType"><see cref="Sims3.UI.EffectBase.InterpolationTypes"/></param>
    /// <param name="autoReverse">If true, the effect reverses when the trigger ends.</param>
    public static void AddGlideEffect(WindowBase window,
        Vector2 offset,
        float duration = 0.2f,
        EffectBase.TriggerTypes triggerType = EffectBase.TriggerTypes.MouseFocus,
        EffectBase.InterpolationTypes interpolationType = EffectBase.InterpolationTypes.EaseInOut,
        bool autoReverse = true)
    {
        GlideEffect glide = new GlideEffect();
        glide.Offset = offset * TinyUIFix.Scale;
        glide.Duration = duration;
        glide.TriggerType = triggerType;
        if (autoReverse) glide.ResetEffect(true);
        glide.InterpolationType = interpolationType;
        window.EffectList.Add(glide);
        window.Tag = glide;
    }

    /// <summary>
    /// Adds an effect that grows or shrinks the window's boundaries. Values are automatically scaled <see cref="Arro.Common.TinyUIFix.Scale"/>.
    /// </summary>
    /// <param name="window">The target window.</param>
    /// <param name="leftChange">Unscaled change to the left boundary.</param>
    /// <param name="topChange">Unscaled change to the top boundary.</param>
    /// <param name="rightChange">Unscaled change to the right boundary.</param>
    /// <param name="bottomChange">Unscaled change to the bottom boundary.</param>
    /// <param name="duration">Duration of the animation.</param>
    /// <param name="triggerType"><see cref="Sims3.UI.EffectBase.TriggerTypes"/></param>
    /// <param name="interpolationType"><see cref="Sims3.UI.EffectBase.InterpolationTypes"/></param>
    public static void AddGrowEffect(WindowBase window,
        float leftChange = 0f,
        float topChange = 0f,
        float rightChange = 0f,
        float bottomChange = 0f,
        float duration = 0.2f,
        EffectBase.TriggerTypes triggerType = EffectBase.TriggerTypes.MouseFocus,
        EffectBase.InterpolationTypes interpolationType = EffectBase.InterpolationTypes.EaseInOut)
    {
        GrowEffect grow = new GrowEffect();
        grow.BoundChangeRect = new Rect(leftChange * TinyUIFix.Scale,
            topChange * TinyUIFix.Scale,
            rightChange * TinyUIFix.Scale,
            bottomChange * TinyUIFix.Scale);
        grow.Duration = duration;
        grow.TriggerType = triggerType;
        grow.InterpolationType = interpolationType;
        window.EffectList.Add(grow);
        window.Tag = grow;
    }

    /// <summary>
    /// Clears and disposes all currently added effects from the window..
    /// </summary>
    /// <param name="window">The window to clean.</param>
    public static void RemoveAllEffects(WindowBase window)
    {
        List<EffectBase> effectsToRemove = new List<EffectBase>();

        foreach (object obj in window.EffectList)
        {
            if (obj is EffectBase effect)
            {
                effectsToRemove.Add(effect);
            }
        }

        foreach (EffectBase effect in effectsToRemove)
        {
            window.EffectList.Remove(effect);
            effect.Dispose();
        }
    }
}