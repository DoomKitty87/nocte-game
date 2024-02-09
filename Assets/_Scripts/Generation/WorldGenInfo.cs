using UnityEngine;

[System.Serializable]
public static class WorldGenInfo
{

  public static int _seed;
  public static float _maxUpdatesPerFrame;
  public static float _lakePlaneHeight;
  public static float _tileEdgeSize;
  
  [System.Serializable]
  public struct AmalgamNoiseParams
  {
    public static int NOISEPARAMS_octaves;
    public static float NOISEPARAMS_lacunarity;
    public static float NOISEPARAMS_persistence;
    public static float NOISEPARAMS_sharpnessScale;        
    public static float NOISEPARAMS_sharpnessAmplitude;        
    public static float NOISEPARAMS_sharpnessMean;        
    public static float NOISEPARAMS_scaleScale;        
    public static float NOISEPARAMS_scaleAmplitude;        
    public static float NOISEPARAMS_scaleMean;
    public static float NOISEPARAMS_amplitudeScale;        
    public static float NOISEPARAMS_amplitudeAmplitude;         
    public static float NOISEPARAMS_amplitudeMean;       
    public static float NOISEPARAMS_warpStrengthScale;         
    public static float NOISEPARAMS_warpStrengthAmplitude;       
    public static float NOISEPARAMS_warpStrengthMean;        
    public static float NOISEPARAMS_warpScaleScale;      
    public static float NOISEPARAMS_warpScaleAmplitude;
    public static float NOISEPARAMS_warpScaleMean;
    public static float NOISEPARAMS_amplitudePower;

    public static float NOISEPARAMS_scaleMeanAmplitude;
    public static float NOISEPARAMS_sharpnessScaleAmplitude; 
    public static float NOISEPARAMS_sharpnessAmplitudeAmplitude;
    public static float NOISEPARAMS_sharpnessMeanAmplitude;
    public static float NOISEPARAMS_amplitudeScaleAmplitude;
    public static float NOISEPARAMS_amplitudeAmplitudeAmplitude;
    public static float NOISEPARAMS_amplitudeMeanAmplitude;
    public static float NOISEPARAMS_warpStrengthAmplitudeAmplitude; 
    public static float NOISEPARAMS_warpStrengthMeanAmplitude;
    public static float NOISEPARAMS_warpScaleMeanAmplitude;
    public static float NOISEPARAMS_amplitudePowerAmplitude;

    public static void Perturb(int seed) {
      NOISEPARAMS_scaleMean += (Mathf.PerlinNoise(seed % 296.13f, seed % 906.13f)) * NOISEPARAMS_scaleMeanAmplitude;
      NOISEPARAMS_sharpnessScale += (Mathf.PerlinNoise(seed % 751.92f, seed % 601.93f)) * NOISEPARAMS_sharpnessScaleAmplitude;
      NOISEPARAMS_sharpnessAmplitude += (Mathf.PerlinNoise(seed % 968.01f, seed % 981.24f) - 0.5f) * NOISEPARAMS_sharpnessAmplitudeAmplitude;
      NOISEPARAMS_sharpnessMean += (Mathf.PerlinNoise(seed % 214.25f, seed % 591.85f)) * NOISEPARAMS_sharpnessMeanAmplitude;
      NOISEPARAMS_amplitudeScale += (Mathf.PerlinNoise(seed % 172.82f, seed % 918.96f)) * NOISEPARAMS_amplitudeScaleAmplitude;
      NOISEPARAMS_amplitudeAmplitude += (Mathf.PerlinNoise(seed % 619.34f, seed % 729.14f) - 0.5f) * NOISEPARAMS_amplitudeAmplitudeAmplitude;
      NOISEPARAMS_amplitudeMean += (Mathf.PerlinNoise(seed % 481.83f, seed % 389.06f)) * NOISEPARAMS_amplitudeMeanAmplitude;
      NOISEPARAMS_warpStrengthAmplitude += (Mathf.PerlinNoise(seed % 195.12f, seed % 702.18f) - 0.5f) * NOISEPARAMS_warpStrengthAmplitudeAmplitude;
      NOISEPARAMS_warpStrengthMean += (Mathf.PerlinNoise(seed % 810.53f, seed % 109.52f) - 0.5f) * NOISEPARAMS_warpStrengthMeanAmplitude;
      NOISEPARAMS_warpScaleMeanAmplitude += (Mathf.PerlinNoise(seed % 639.14f, seed % 561.92f)) * NOISEPARAMS_warpScaleMeanAmplitude;
      NOISEPARAMS_amplitudePower += (Mathf.PerlinNoise(seed % 591.03f, seed % 329.51f) - 0.5f) * NOISEPARAMS_amplitudePowerAmplitude;
    }
  }
  
}