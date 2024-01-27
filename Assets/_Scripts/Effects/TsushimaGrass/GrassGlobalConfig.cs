using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Effects.TsushimaGrass
{
	public class GrassGlobalConfig : MonoBehaviour
	{
		[Header("Settings")]
		[Range(1, 100)] public int _samplesX, _samplesY;
		public float _fallbackTileSizeX, _fallbackTileSizeZ;
		public Material _renderingMaterial;
		public float _tileBoundsPadding;
		public float _distToPlayerCutoff;
		[Header("Dependencies")]
		public Mesh _grassMesh;
	}
}