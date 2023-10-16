public static class MusicConstants
{

  public static final float[] notes = {
    261.626f, 
    277.183f,
    293.665f,
    311.127f,
    329.628f,
    349.228f,
    369.994f,
    391.995f,
    415.305f,
    440f,
    466.164f,
    493.883f
  }; // Notes start at middle C - 261.626hz.

  public static final int[][] scales = {
    { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
    { 0, 2, 4, 6, 7, 9, 10 },
    { 0, 2, 3, 5, 7, 8, 10 },
    { 0, 1, 3, 4, 6, 8, 10 },
    { 0, 3, 4, 7, 8, 11 },
    { 0, 2, 4, 5, 7, 9, 10, 11 },
    { 0, 3, 5, 6, 7, 10 },
    { 0, 2, 3, 5, 7, 9, 10 },
    { 0, 1, 4, 5, 7, 8, 11 },
    { 0, 1, 4, 6, 8, 10, 11 },
    { 0, 1, 4, 5, 7, 8, 11 },
    { 0, 2, 3, 6, 7, 8, 10 },
    { 0, 2, 3, 5, 6, 8, 10 },
    { 0, 2, 4, 5, 7, 8, 11 },
    { 0, 2, 3, 5, 7, 8, 11 },
    { 0, 4, 6, 7, 11 },
    { 0, 2, 3, 6, 7, 8, 11 },
    { 0, 3, 4, 6, 7, 9, 10 },
    { 0, 1, 5, 7, 8 },
    { 0, 1, 5, 7, 10 },
    { 0, 2, 4, 5, 7, 9, 11 },
    { 0, 1, 3, 4, 6, 7 },
    { 0, 1, 5, 6, 10 },
    { 0, 1, 3, 5, 6, 8, 10 },
    { 0, 1, 3, 5, 6, 8, 10 },
    { 0, 2, 4, 6, 8, 9, 11 },
    { 0, 2, 3, 6, 7, 9, 11 },
    { 0, 2, 4, 6, 7, 9, 11 },
    { 0, 2, 4, 5, 7, 8, 9, 11 },
    { 0, 2, 4, 5, 6, 8, 10 },
    { 0, 2, 4, 7, 9},
    { 0, 2, 3, 5, 7, 9, 11 },
    { 0, 2, 3, 5, 7, 8, 10 },
    { 0, 3, 5, 7, 10 },
    { 0, 2, 4, 5, 7, 9, 10 },
    { 0, 1, 3, 5, 7, 9, 11 },
    { 0, 1, 3, 5, 7, 8, 11 },
    { 0, 2, 3, 5, 6, 8, 9, 11 },
    { 0, 1, 3, 4, 6, 7, 9, 10 },
    { 0, 1, 4, 5, 6, 8, 11 },
    { 0, 1, 4, 5, 7, 8, 10 },
    { 0, 1, 3, 5, 7, 8, 10 },
    { 0, 2, 4, 6, 9, 10 },
    { 0, 3, 4, 5, 7, 9 },
    { 0, 1, 4, 6, 7, 10 },
    { 0, 1, 2, 6, 7, 8 },
    { 0, 2, 3, 6, 7, 9, 10 },
    { 0, 2, 4, 6, 8, 10 },
    { 0, 3, 5, 7, 10 }

  }

  public static class Scales
  {
    public static final int[] chromatic = {
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11
      };

    public static final int[] acoustic = {
      0, 2, 4, 6, 7, 9, 10
    };

    public static final int[] naturalMinor = {
      0, 2, 3, 5, 7, 8, 10
    };

    public static final int[] altered = {
      0, 1, 3, 4, 6, 8, 10
    };

    public static final int[] augmented = {
      0, 3, 4, 7, 8, 11
    };

    public static final int[] bebopDominant = {
      0, 2, 4, 5, 7, 9, 10, 11
    };

    public static final int[] blues = {
      0, 3, 5, 6, 7, 10
    };

    public static final int[] dorian = {
      0, 2, 3, 5, 7, 9, 10
    };

    public static final int[] doubleHarmonic = {
      0, 1, 4, 5, 7, 8, 11
    };

    public static final int[] enigmatic = {
      0, 1, 4, 6, 8, 10, 11
    };

    public static final int[] flamenco = {
      0, 1, 4, 5, 7, 8, 11
    };

    public static final int[] doubleHarmonicMinor = {
      0, 2, 3, 6, 7, 8, 10
    };

    public static final int[] halfDiminished = {
      0, 2, 3, 5, 6, 8, 10
    };

    public static final int[] harmonicMajor = {
      0, 2, 4, 5, 7, 8, 11
    };

    public static final int[] harmonicMinor = {
      0, 2, 3, 5, 7, 8, 11
    };

    public static final int[] hirajoshi = {
      0, 4, 6, 7, 11
    };

    public static final int[] hungarianMinor = {
      0, 2, 3, 6, 7, 8, 11
    };

    public static final int[] hungarianMajor = {
      0, 3, 4, 6, 7, 9, 10
    };

    public static final int[] in = {
      0, 1, 5, 7, 8
    };

    public static final int[] insen = {
      0, 1, 5, 7, 10
    };

    public static final int[] major = {
      0, 2, 4, 5, 7, 9, 11
    };

    public static final int[] istrian = {
      0, 1, 3, 4, 6, 7
    };

    public static final int[] iwato = {
      0, 1, 5, 6, 10
    };

    public static final int[] locrian = {
      0, 1, 3, 5, 6, 8, 10
    };

    public static final int[] lydianAugmented = {
      0, 2, 4, 6, 8, 9, 11
    };

    public static final int[] lydianDiminished = {
      0, 2, 3, 6, 7, 9, 11
    };

    public static final int[] lydian = {
      0, 2, 4, 6, 7, 9, 11
    };

    public static final int[] majorBebop = {
      0, 2, 4, 5, 7, 8, 9, 11
    };

    public static final int[] majorLocrian = {
      0, 2, 4, 5, 6, 8, 10
    };

    public static final int[] majorPentatonic = {
      0, 2, 4, 7, 9
    };

    public static final int[] melodicMinorAscending = {
      0, 2, 3, 5, 7, 9, 11
    };

    public static final int[] melodicMinorDescending = {
      0, 2, 3, 5, 7, 8, 10
    };

    public static final int[] minorPentatonic = {
      0, 3, 5, 7, 10
    };

    public static final int[] mixolydian = {
      0, 2, 4, 5, 7, 9, 10
    };

    public static final int[] neapolitanMajor = {
      0, 1, 3, 5, 7, 9, 11
    };

    public static final int[] neapolitanMinor = {
      0, 1, 3, 5, 7, 8, 11
    };

    public static final int[] octatonicPrimary = {
      0, 2, 3, 5, 6, 8, 9, 11
    };

    public static final int[] octatonicSecondary = {
      0, 1, 3, 4, 6, 7, 9, 10
    };

    public static final int[] persian = {
      0, 1, 4, 5, 6, 8, 11
    };

    public static final int[] phrygianDominant = {
      0, 1, 4, 5, 7, 8, 10
    };

    public static final int[] phryigan = {
      0, 1, 3, 5, 7, 8, 10
    };

    public static final int[] prometheus = {
      0, 2, 4, 6, 9, 10
    };

    public static final int[] harmonics = {
      0, 3, 4, 5, 7, 9
    };

    public static final int[] tritone = {
      0, 1, 4, 6, 7, 10
    };

    public static final int[] twoSemitoneTritone = {
      0, 1, 2, 6, 7, 8
    };

    public static final int[] ukranianDominant = {
      0, 2, 3, 6, 7, 9, 10
    };

    public static final int[] wholeTone = {
      0, 2, 4, 6, 8, 10
    };

    public static final int[] yo = {
      0, 3, 5, 7, 10
    };

  }

}