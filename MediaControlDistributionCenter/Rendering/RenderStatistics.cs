using System;

namespace MediaControlDistributionCenter.Rendering
{
    public class RenderStatistics
    {
        public int DrawCallsPerFrame { get; set; }
        public int LayerSavesPerFrame { get; set; }
        public int AnimatedElements { get; set; }
        public float FrameTimeMs { get; set; }
        public int PoolHitsPerFrame { get; set; }
        public int PoolMissesPerFrame { get; set; }

        public void ResetFrame()
        {
            DrawCallsPerFrame = 0;
            LayerSavesPerFrame = 0;
            AnimatedElements = 0;
            FrameTimeMs = 0;
            PoolHitsPerFrame = 0;
            PoolMissesPerFrame = 0;
        }

        public override string ToString()
        {
            return $"Draw:{DrawCallsPerFrame} Layer:{LayerSavesPerFrame} PoolH:{PoolHitsPerFrame} PoolM:{PoolMissesPerFrame} " +
                   $"Anim:{AnimatedElements} {FrameTimeMs:F1}ms";
        }
    }
}
