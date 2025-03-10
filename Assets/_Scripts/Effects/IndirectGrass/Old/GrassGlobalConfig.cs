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
		public float _LOD1Distance;
		public bool _castShadowsOn;
		public bool _recieveShadowsOn;
		[Range(0, 20)] public int _chunkSplitFactor;
		[Header("Dependencies")] 
		public Camera _mainCamera;
		public ComputeShader _positionCompute;
		public ComputeShader _bufferClearCompute;
		public Material _renderingShaderMat;
		public Mesh _LOD0GrassMesh;
    public Light _mainLight;

    private void Update() {
      Shader.SetGlobalVector("_MainLightDir", _mainLight.transform.forward);
    }
	}
}