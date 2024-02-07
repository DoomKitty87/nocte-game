using UnityEngine;

static class WorldGenInfo
{

  public static int _seed;
  public static float _maxUpdatesPerFrame;
  public static float _lakePlaneHeight;
  public static float _tileEdgeSize;
  
  [System.Serializable]
  public struct AmalgamNoiseParams
  {

    public static int octaves;
    public float lacunarity;
    public float persistence;
    public float sharpnessScale;        
    public float sharpnessAmplitude;        
    public float sharpnessMean;        
    public float scaleScale;        
    public float scaleAmplitude;        
    public float scaleMean;
    public float amplitudeScale;        
    public float amplitudeAmplitude;         
    public float amplitudeMean;       
    public float warpStrengthScale;         
    public float warpStrengthAmplitude;       
    public float warpStrengthMean;        
    public float warpScaleScale;      
    public float warpScaleAmplitude;
    public float warpScaleMean;
    public float amplitudePower;

    public float scaleMeanAmplitude;
    public float sharpnessScaleAmplitude; 
    public float sharpnessAmplitudeAmplitude;
    public float sharpnessMeanAmplitude;
    public float amplitudeScaleAmplitude;
    public float amplitudeAmplitudeAmplitude;
    public float amplitudeMeanAmplitude;
    public float warpStrengthAmplitudeAmplitude; 
    public float warpStrengthMeanAmplitude;
    public float warpScaleMeanAmplitude;
    public float amplitudePowerAmplitude;

    public void Perturb(int seed) {
      scaleMean += (Mathf.PerlinNoise(seed % 296.13f, seed % 906.13f)) * scaleMeanAmplitude;
      sharpnessScale += (Mathf.PerlinNoise(seed % 751.92f, seed % 601.93f)) * sharpnessScaleAmplitude;
      sharpnessAmplitude += (Mathf.PerlinNoise(seed % 968.01f, seed % 981.24f) - 0.5f) * sharpnessAmplitudeAmplitude;
      sharpnessMean += (Mathf.PerlinNoise(seed % 214.25f, seed % 591.85f)) * sharpnessMeanAmplitude;
      amplitudeScale += (Mathf.PerlinNoise(seed % 172.82f, seed % 918.96f)) * amplitudeScaleAmplitude;
      amplitudeAmplitude += (Mathf.PerlinNoise(seed % 619.34f, seed % 729.14f) - 0.5f) * amplitudeAmplitudeAmplitude;
      amplitudeMean += (Mathf.PerlinNoise(seed % 481.83f, seed % 389.06f)) * amplitudeMeanAmplitude;
      warpStrengthAmplitude += (Mathf.PerlinNoise(seed % 195.12f, seed % 702.18f) - 0.5f) * warpStrengthAmplitudeAmplitude;
      warpStrengthMean += (Mathf.PerlinNoise(seed % 810.53f, seed % 109.52f) - 0.5f) * warpStrengthMeanAmplitude;
      warpScaleMeanAmplitude += (Mathf.PerlinNoise(seed % 639.14f, seed % 561.92f)) * warpScaleMeanAmplitude;
      amplitudePower += (Mathf.PerlinNoise(seed % 591.03f, seed % 329.51f) - 0.5f) * amplitudePowerAmplitude;
    }
  }
  
}