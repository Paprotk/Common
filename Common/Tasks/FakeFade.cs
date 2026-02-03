using System;
using Arro.Common;
using Sims3.SimIFace;
using Sims3.UI;

namespace Arro.MCR.Common.Tasks;

public class FakeFade
{
    public enum EaseType
    {
        Linear,
        EaseIn,
        EaseOut,
        EaseInOut,
        SmoothStep
    }

    private static float ApplyEasing(float t, EaseType easeType)
    {
        switch (easeType)
        {
            case EaseType.Linear: return t;
            case EaseType.EaseIn: return t * t;
            case EaseType.EaseOut: return 1f - (1f - t) * (1f - t);
            case EaseType.EaseInOut: return t < 0.5f ? 2f * t * t : 1f - (float)Math.Pow(-2f * t + 2f, 2) / 2f;
            case EaseType.SmoothStep: return t * t * (3f - 2f * t);
            default: return t;
        }
    }

    // Maksymalny skok czasu w milisekundach. 
    // Jeśli klatka trwała dłużej (lag), udajemy że trwała tylko tyle.
    // 50ms = minimalnie 20 FPS dla animacji.
    private const float MAX_DELTA_TIME = 50f; 

    public class FadeIn : Task
    {
        private WindowBase targetWindow;
        private bool isDisposed;
        private int targetOpacity;
        private EaseType easeType;
        private float durationMs;
        private float elapsedTime;
        private long lastTick;

        public FadeIn(WindowBase window, int durationMs = 300, EaseType ease = EaseType.EaseInOut, int target = 255)
        {
            targetWindow = window;
            this.durationMs = durationMs;
            easeType = ease;
            targetOpacity = target;
            elapsedTime = 0f;
            isDisposed = false;
            
            // Pobieramy czas startu
            lastTick = DateTime.UtcNow.Ticks;

            if (targetWindow != null && !targetWindow.Disposed)
            {
                targetWindow.SetOpacity(0);
                targetWindow.Visible = true;
            }

            Logger.Log($"FadeIn task created with {ease} easing, duration: {durationMs}ms");
        }

        public override void Simulate()
        {
            if (isDisposed || targetWindow == null || targetWindow.Disposed)
            {
                Dispose();
                return;
            }

            long currentTick = DateTime.UtcNow.Ticks;
            // Obliczamy ile ms minęło od ostatniej klatki
            float deltaMs = (currentTick - lastTick) / 10000f;
            lastTick = currentTick;

            // ZABEZPIECZENIE: Jeśli gra się zacięła na 1 sekundę (1000ms),
            // liczymy tylko 50ms. Animacja zwolni, ale nie przeskoczy.
            deltaMs = Math.Min(deltaMs, MAX_DELTA_TIME);

            elapsedTime += deltaMs;
            float progress = elapsedTime / durationMs;

            if (progress > 1f) progress = 1f;

            float easedT = ApplyEasing(progress, easeType);
            int opacity = (int)(easedT * targetOpacity);

            targetWindow.SetOpacity(opacity);

            if (progress >= 1f)
            {
                Dispose();
            }
        }

        public override void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            if (targetWindow != null && !targetWindow.Disposed)
            {
                targetWindow.SetOpacity(targetOpacity);
            }
            base.Dispose();
        }
    }

    public class FadeOut : Task
    {
        private WindowBase targetWindow;
        private bool isDisposed;
        private int targetOpacity;
        private EaseType easeType;
        private float durationMs;
        private float elapsedTime;
        private bool hideOnComplete;
        private long lastTick;

        public FadeOut(WindowBase window, int durationMs = 300, EaseType ease = EaseType.EaseInOut, int target = 0, bool hide = true)
        {
            targetWindow = window;
            this.durationMs = durationMs;
            easeType = ease;
            targetOpacity = target;
            hideOnComplete = hide;
            elapsedTime = 0f;
            isDisposed = false;
            
            lastTick = DateTime.UtcNow.Ticks;

            if (targetWindow != null && !targetWindow.Disposed)
            {
                targetWindow.SetOpacity(255);
            }

            Logger.Log($"FadeOut task created with {ease} easing, duration: {durationMs}ms");
        }

        public override void Simulate()
        {
            if (isDisposed || targetWindow == null || targetWindow.Disposed)
            {
                Dispose();
                return;
            }

            long currentTick = DateTime.UtcNow.Ticks;
            float deltaMs = (currentTick - lastTick) / 10000f;
            lastTick = currentTick;

            // Zabezpieczenie przed skokami
            deltaMs = Math.Min(deltaMs, MAX_DELTA_TIME);

            elapsedTime += deltaMs;
            float progress = elapsedTime / durationMs;

            if (progress > 1f) progress = 1f;

            float easedT = ApplyEasing(progress, easeType);
            int opacity = 255 - (int)(easedT * (255 - targetOpacity));

            targetWindow.SetOpacity(opacity);

            if (progress >= 1f)
            {
                Dispose();
            }
        }

        public override void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            if (targetWindow != null && !targetWindow.Disposed)
            {
                targetWindow.SetOpacity(targetOpacity);
                if (hideOnComplete && targetOpacity == 0)
                {
                    targetWindow.Visible = false;
                }
            }
            base.Dispose();
        }
    }

    public class CrossFade : Task
    {
        private WindowBase fadeOutWindow;
        private WindowBase fadeInWindow;
        private bool isDisposed;
        private EaseType easeType;
        private float durationMs;
        private float elapsedTime;
        private long lastTick;

        public CrossFade(WindowBase fadeOutWin, WindowBase fadeInWin, int durationMs = 500, EaseType ease = EaseType.EaseInOut)
        {
            fadeOutWindow = fadeOutWin;
            fadeInWindow = fadeInWin;
            this.durationMs = durationMs;
            easeType = ease;
            elapsedTime = 0f;
            isDisposed = false;
            
            lastTick = DateTime.UtcNow.Ticks;

            if (fadeOutWindow != null && !fadeOutWindow.Disposed)
            {
                fadeOutWindow.SetOpacity(255);
            }

            if (fadeInWindow != null && !fadeInWindow.Disposed)
            {
                fadeInWindow.SetOpacity(0);
                fadeInWindow.Visible = true;
            }

            Logger.Log($"CrossFade task created with {ease} easing, duration: {durationMs}ms");
        }

        public override void Simulate()
        {
            if (isDisposed || 
                (fadeOutWindow == null || fadeOutWindow.Disposed) && 
                (fadeInWindow == null || fadeInWindow.Disposed))
            {
                Dispose();
                return;
            }

            long currentTick = DateTime.UtcNow.Ticks;
            float deltaMs = (currentTick - lastTick) / 10000f;
            lastTick = currentTick;

            // Zabezpieczenie przed skokami
            deltaMs = Math.Min(deltaMs, MAX_DELTA_TIME);

            elapsedTime += deltaMs;
            float progress = elapsedTime / durationMs;

            if (progress > 1f) progress = 1f;

            float easedT = ApplyEasing(progress, easeType);
            int fadeOutOpacity = 255 - (int)(easedT * 255);
            int fadeInOpacity = (int)(easedT * 255);

            if (fadeOutWindow != null && !fadeOutWindow.Disposed)
            {
                fadeOutWindow.SetOpacity(fadeOutOpacity);
            }

            if (fadeInWindow != null && !fadeInWindow.Disposed)
            {
                fadeInWindow.SetOpacity(fadeInOpacity);
            }

            if (progress >= 1f)
            {
                if (fadeOutWindow != null && !fadeOutWindow.Disposed)
                {
                    fadeOutWindow.Visible = false;
                    fadeOutWindow.SetOpacity(0);
                }

                if (fadeInWindow != null && !fadeInWindow.Disposed)
                {
                    fadeInWindow.SetOpacity(255);
                }

                Dispose();
            }
        }

        public override void Dispose()
        {
            if (isDisposed) return;
            isDisposed = true;

            if (fadeOutWindow != null && !fadeOutWindow.Disposed)
            {
                fadeOutWindow.Visible = false;
                fadeOutWindow.SetOpacity(0);
            }

            if (fadeInWindow != null && !fadeInWindow.Disposed)
            {
                fadeInWindow.SetOpacity(255);
            }
            base.Dispose();
        }
    }
}