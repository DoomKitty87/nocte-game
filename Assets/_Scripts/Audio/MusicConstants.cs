public static class Musicants
{

  public static  float[] notes = {
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

  public static int[][] scales = new int[][] {
    new int[] { 0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11 },
    new int[] { 0, 2, 4, 6, 7, 9, 10 },
    new int[] { 0, 2, 3, 5, 7, 8, 10 },
    new int[] { 0, 1, 3, 4, 6, 8, 10 },
    new int[] { 0, 3, 4, 7, 8, 11 },
    new int[] { 0, 2, 4, 5, 7, 9, 10, 11 },
    new int[] { 0, 3, 5, 6, 7, 10 },
    new int[] { 0, 2, 3, 5, 7, 9, 10 },
    new int[] { 0, 1, 4, 5, 7, 8, 11 },
    new int[] { 0, 1, 4, 6, 8, 10, 11 },
    new int[] { 0, 1, 4, 5, 7, 8, 11 },
    new int[] { 0, 2, 3, 6, 7, 8, 10 },
    new int[] { 0, 2, 3, 5, 6, 8, 10 },
    new int[] { 0, 2, 4, 5, 7, 8, 11 },
    new int[] { 0, 2, 3, 5, 7, 8, 11 },
    new int[] { 0, 4, 6, 7, 11 },
    new int[] { 0, 2, 3, 6, 7, 8, 11 },
    new int[] { 0, 3, 4, 6, 7, 9, 10 },
    new int[] { 0, 1, 5, 7, 8 },
    new int[] { 0, 1, 5, 7, 10 },
    new int[] { 0, 2, 4, 5, 7, 9, 11 },
    new int[] { 0, 1, 3, 4, 6, 7 },
    new int[] { 0, 1, 5, 6, 10 },
    new int[] { 0, 1, 3, 5, 6, 8, 10 },
    new int[] { 0, 1, 3, 5, 6, 8, 10 },
    new int[] { 0, 2, 4, 6, 8, 9, 11 },
    new int[] { 0, 2, 3, 6, 7, 9, 11 },
    new int[] { 0, 2, 4, 6, 7, 9, 11 },
    new int[] { 0, 2, 4, 5, 7, 8, 9, 11 },
    new int[] { 0, 2, 4, 5, 6, 8, 10 },
    new int[] { 0, 2, 4, 7, 9 },
    new int[] { 0, 2, 3, 5, 7, 9, 11 },
    new int[] { 0, 2, 3, 5, 7, 8, 10 },
    new int[] { 0, 3, 5, 7, 10 },
    new int[] { 0, 2, 4, 5, 7, 9, 10 },
    new int[] { 0, 1, 3, 5, 7, 9, 11 },
    new int[] { 0, 1, 3, 5, 7, 8, 11 },
    new int[] { 0, 2, 3, 5, 6, 8, 9, 11 },
    new int[] { 0, 1, 3, 4, 6, 7, 9, 10 },
    new int[] { 0, 1, 4, 5, 6, 8, 11 },
    new int[] { 0, 1, 4, 5, 7, 8, 10 },
    new int[] { 0, 1, 3, 5, 7, 8, 10 },
    new int[] { 0, 2, 4, 6, 9, 10 },
    new int[] { 0, 3, 4, 5, 7, 9 },
    new int[] { 0, 1, 4, 6, 7, 10 },
    new int[] { 0, 1, 2, 6, 7, 8 },
    new int[] { 0, 2, 3, 6, 7, 9, 10 },
    new int[] { 0, 2, 4, 6, 8, 10 },
    new int[] { 0, 3, 5, 7, 10 }
  };

  public static class Scales
  {
    public static int[] chromatic = {
        0, 1, 2, 3, 4, 5, 6, 7, 8, 9, 10, 11
      };

    public static int[] acoustic = {
      0, 2, 4, 6, 7, 9, 10
    };

    public static int[] naturalMinor = {
      0, 2, 3, 5, 7, 8, 10
    };

    public static int[] altered = {
      0, 1, 3, 4, 6, 8, 10
    };

    public static int[] augmented = {
      0, 3, 4, 7, 8, 11
    };

    public static int[] bebopDominant = {
      0, 2, 4, 5, 7, 9, 10, 11
    };

    public static int[] blues = {
      0, 3, 5, 6, 7, 10
    };

    public static int[] dorian = {
      0, 2, 3, 5, 7, 9, 10
    };

    public static int[] doubleHarmonic = {
      0, 1, 4, 5, 7, 8, 11
    };

    public static int[] enigmatic = {
      0, 1, 4, 6, 8, 10, 11
    };

    public static int[] flamenco = {
      0, 1, 4, 5, 7, 8, 11
    };

    public static int[] doubleHarmonicMinor = {
      0, 2, 3, 6, 7, 8, 10
    };

    public static int[] halfDiminished = {
      0, 2, 3, 5, 6, 8, 10
    };

    public static int[] harmonicMajor = {
      0, 2, 4, 5, 7, 8, 11
    };

    public static int[] harmonicMinor = {
      0, 2, 3, 5, 7, 8, 11
    };

    public static int[] hirajoshi = {
      0, 4, 6, 7, 11
    };

    public static int[] hungarianMinor = {
      0, 2, 3, 6, 7, 8, 11
    };

    public static int[] hungarianMajor = {
      0, 3, 4, 6, 7, 9, 10
    };

    public static int[] inScale = {
      0, 1, 5, 7, 8
    };

    public static int[] insen = {
      0, 1, 5, 7, 10
    };

    public static int[] major = {
      0, 2, 4, 5, 7, 9, 11
    };

    public static int[] istrian = {
      0, 1, 3, 4, 6, 7
    };

    public static int[] iwato = {
      0, 1, 5, 6, 10
    };

    public static int[] locrian = {
      0, 1, 3, 5, 6, 8, 10
    };

    public static int[] lydianAugmented = {
      0, 2, 4, 6, 8, 9, 11
    };

    public static int[] lydianDiminished = {
      0, 2, 3, 6, 7, 9, 11
    };

    public static int[] lydian = {
      0, 2, 4, 6, 7, 9, 11
    };

    public static int[] majorBebop = {
      0, 2, 4, 5, 7, 8, 9, 11
    };

    public static int[] majorLocrian = {
      0, 2, 4, 5, 6, 8, 10
    };

    public static int[] majorPentatonic = {
      0, 2, 4, 7, 9
    };

    public static int[] melodicMinorAscending = {
      0, 2, 3, 5, 7, 9, 11
    };

    public static int[] melodicMinorDescending = {
      0, 2, 3, 5, 7, 8, 10
    };

    public static int[] minorPentatonic = {
      0, 3, 5, 7, 10
    };

    public static int[] mixolydian = {
      0, 2, 4, 5, 7, 9, 10
    };

    public static int[] neapolitanMajor = {
      0, 1, 3, 5, 7, 9, 11
    };

    public static int[] neapolitanMinor = {
      0, 1, 3, 5, 7, 8, 11
    };

    public static int[] octatonicPrimary = {
      0, 2, 3, 5, 6, 8, 9, 11
    };

    public static int[] octatonicSecondary = {
      0, 1, 3, 4, 6, 7, 9, 10
    };

    public static int[] persian = {
      0, 1, 4, 5, 6, 8, 11
    };

    public static int[] phrygianDominant = {
      0, 1, 4, 5, 7, 8, 10
    };

    public static int[] phryigan = {
      0, 1, 3, 5, 7, 8, 10
    };

    public static int[] prometheus = {
      0, 2, 4, 6, 9, 10
    };

    public static int[] harmonics = {
      0, 3, 4, 5, 7, 9
    };

    public static int[] tritone = {
      0, 1, 4, 6, 7, 10
    };

    public static int[] twoSemitoneTritone = {
      0, 1, 2, 6, 7, 8
    };

    public static int[] ukranianDominant = {
      0, 2, 3, 6, 7, 9, 10
    };

    public static int[] wholeTone = {
      0, 2, 4, 6, 8, 10
    };

    public static int[] yo = {
      0, 3, 5, 7, 10
    };

  }

}