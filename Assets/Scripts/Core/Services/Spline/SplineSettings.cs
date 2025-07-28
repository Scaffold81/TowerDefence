using UnityEngine;

namespace Core.Services.Spline
{
    /// <summary>
    /// Configuration settings for spline generation
    /// </summary>
    [System.Serializable]
    public class SplineSettings
    {
        [Header("Sampling")]
        [Tooltip("Distance between reference points in meters")]
        [Range(0.1f, 2f)]
        public float sampleDistance = 0.5f;
        
        [Tooltip("Resolution per segment for length calculation")]
        [Range(10, 100)]
        public int lengthCalculationSamples = 20;
        
        [Header("Designer Markers")]
        [Tooltip("Distance between designer markers in meters")]
        [Range(0.5f, 5f)]
        public float markerSpacing = 2f;
        
        [Tooltip("Create markers at sharp turns automatically")]
        public bool autoMarkSharpTurns = true;
        
        [Tooltip("Minimum curvature to consider a turn 'sharp'")]
        [Range(0.1f, 2f)]
        public float sharpTurnThreshold = 0.7f;
        
        [Header("Speed Control (Disabled)")]
        [Tooltip("Speed control is disabled - enemies maintain constant speed")]
        [Range(0.1f, 1f)]
        public float minSpeedMultiplier = 1f; // Always 1.0 now
        
        [Tooltip("Speed control is disabled - enemies maintain constant speed")]
        [Range(0.5f, 2f)]
        public float maxSpeedMultiplier = 1f; // Always 1.0 now
        
        [Header("Quality")]
        [Tooltip("Higher values create smoother paths but use more memory")]
        public SplineQuality quality = SplineQuality.Medium;
        
        /// <summary>
        /// Gets the resolution per segment based on quality setting
        /// </summary>
        public int GetResolutionPerSegment()
        {
            return (int)quality;
        }
        
        /// <summary>
        /// Creates default settings for typical tower defense usage
        /// </summary>
        public static SplineSettings CreateDefault()
        {
            return new SplineSettings
            {
                sampleDistance = 0.5f,
                lengthCalculationSamples = 20,
                markerSpacing = 2f,
                autoMarkSharpTurns = true,
                sharpTurnThreshold = 0.7f,
                minSpeedMultiplier = 1f, // No slowdown
                maxSpeedMultiplier = 1f, // No slowdown
                quality = SplineQuality.Medium
            };
        }
        
        /// <summary>
        /// Creates high-quality settings for detailed paths
        /// </summary>
        public static SplineSettings CreateHighQuality()
        {
            var settings = CreateDefault();
            settings.sampleDistance = 0.25f;
            settings.lengthCalculationSamples = 50;
            settings.markerSpacing = 1f;
            settings.quality = SplineQuality.High;
            return settings;
        }
        
        /// <summary>
        /// Creates performance-optimized settings
        /// </summary>
        public static SplineSettings CreatePerformant()
        {
            var settings = CreateDefault();
            settings.sampleDistance = 1f;
            settings.lengthCalculationSamples = 10;
            settings.markerSpacing = 4f;
            settings.quality = SplineQuality.Low;
            return settings;
        }
    }
    
    /// <summary>
    /// Quality levels for spline generation
    /// Values represent samples per segment
    /// </summary>
    public enum SplineQuality
    {
        Low = 20,       // 20 samples per segment - good for mobile/performance
        Medium = 50,    // 50 samples per segment - balanced quality/performance
        High = 100,     // 100 samples per segment - high quality
        Ultra = 200     // 200 samples per segment - maximum quality
    }
}