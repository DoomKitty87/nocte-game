using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Effects.TsushimaGrass
{
	public class GrassGlobalConfig : MonoBehaviour
	{
		[Header("Settings")]
		public int _tileSplitFactor;
		[Range(1, 100)] public int _samplesX, _samplesY;
		public Material _renderingMaterial;
		public float _meshBoundsPadding;
		public float _distToPlayerCutoff;
		[Header("Dependencies")]
		public Mesh _grassMesh;
	}
}