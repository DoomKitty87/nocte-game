using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Effects.TsushimaGrass
{
	public class GrassGlobalConfig : MonoBehaviour
	{
		[Header("Settings")]
		[Range(1, 5000)] public int _samplesX; 
		[Range(1, 5000)] public int _samplesY;
		public float _fallbackTileSizeX, _fallbackTileSizeZ;
		public float _tileBoundsPadding;
		public float _distToPlayerCutoff;
		public bool _castShadowsOn;
		public bool _recieveShadowsOn;
		[Range(0, 8)] public int _chunkSplitFactor; 
		[Header("Dependencies")]
		public ComputeShader _positionCompute;
		public Material _renderingShaderMat;
		public Mesh _grassMesh;
	}
}