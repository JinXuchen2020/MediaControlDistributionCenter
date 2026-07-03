using SkiaSharp;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace MediaControlDistributionCenter.Rendering
{
    public class AnimationEngine : IDisposable
    {
        private readonly Dictionary<IRenderable, List<IAnimation>> _animations = new();
        private readonly object _animLock = new();
        private readonly ConcurrentQueue<IAnimation> _completedQueue = new();
        private Timer? _timer;
        private volatile int _animationCount;
        private int _activeFadeInCount;
        private float _maxFadeInAlpha = 1f;
        private readonly List<IRenderable> _emptyTargetBuffer = new();
        private readonly List<IAnimation> _completedBuffer = new();
        private readonly Stopwatch _tickWatch = Stopwatch.StartNew();

        public bool HasActiveAnimations => _animationCount > 0;

        public int ActiveFadeInCount => _activeFadeInCount;

        public float MaxFadeInAlpha => _maxFadeInAlpha;

        public AnimationEngine()
        {
            _timer = new Timer(_ =>
            {
                try { Tick(); }
                catch { }
            }, null, 10, 10);
        }

        private void Tick()
        {
            float dt = (float)_tickWatch.Elapsed.TotalSeconds;
            _tickWatch.Restart();
            if (dt > 0.1f) dt = 0.016f;
            _emptyTargetBuffer.Clear();

            lock (_animLock)
            {
                foreach (var (target, anims) in _animations)
                {
                    _completedBuffer.Clear();
                    anims.RemoveAll(a =>
                    {
                        a.Update(dt);
                        if (a.IsCompleted)
                            _completedBuffer.Add(a);
                        return a.IsCompleted;
                    });
                    foreach (var c in _completedBuffer)
                    {
                        _completedQueue.Enqueue(c);
                        Interlocked.Decrement(ref _animationCount);
                    }
                    if (anims.Count == 0)
                        _emptyTargetBuffer.Add(target);
                }

                foreach (var target in _emptyTargetBuffer)
                    _animations.Remove(target);

                RecalcFadeInAlpha();
            }
        }

        public void DrainCompleted()
        {
            while (_completedQueue.TryDequeue(out var anim))
                anim.Dispose();
        }

        public void Play(IRenderable target, IAnimation animation)
        {
            lock (_animLock)
            {
                if (!_animations.ContainsKey(target))
                    _animations[target] = new List<IAnimation>();
                _animations[target].Add(animation);
                Interlocked.Increment(ref _animationCount);
                RecalcFadeInAlpha();
            }
        }

        public void Stop(IRenderable target)
        {
            lock (_animLock)
            {
                if (_animations.TryGetValue(target, out var anims))
                {
                    foreach (var anim in anims)
                    {
                        _completedQueue.Enqueue(anim);
                        Interlocked.Decrement(ref _animationCount);
                    }
                    _animations.Remove(target);
                }
                RecalcFadeInAlpha();
            }
        }

        public void StopAll()
        {
            lock (_animLock)
            {
                foreach (var anims in _animations.Values)
                    foreach (var anim in anims)
                    {
                        _completedQueue.Enqueue(anim);
                        Interlocked.Decrement(ref _animationCount);
                    }
                _animations.Clear();
                RecalcFadeInAlpha();
            }
        }

        private void RecalcFadeInAlpha()
        {
            _activeFadeInCount = 0;
            _maxFadeInAlpha = 1f;
            foreach (var (_, animList) in _animations)
            {
                foreach (var a in animList)
                {
                    if (a is FadeInAnimation fade && !fade.IsCompleted)
                    {
                        _activeFadeInCount++;
                        float alpha = Math.Clamp(fade.Elapsed / fade.Duration, 0f, 1f);
                        if (alpha < _maxFadeInAlpha) _maxFadeInAlpha = alpha;
                    }
                }
            }
        }

        public void ApplyAnimations(SKCanvas canvas, IRenderable target)
        {
            List<IAnimation>? anims;
            lock (_animLock)
            {
                if (!_animations.TryGetValue(target, out anims))
                    return;
            }

            foreach (var anim in anims)
            {
                try { anim.Apply(canvas); }
                catch (Exception ex) { Serilog.Log.Error(ex, "ApplyAnimations: exception in animation for {Type}", target.Type); }
            }
        }

        public void Dispose()
        {
            _timer?.Dispose();
            _timer = null;
            StopAll();
            DrainCompleted();
        }
    }
}
