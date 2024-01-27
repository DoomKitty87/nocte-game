using System;
using System.Numerics;
using UnityEditor;

namespace Effects.TsushimaGrass
{
	using UnityEngine;

	public class GrassTile : MonoBehaviour
	{
		// Tsushima divides single tile into smaller tiles based on lod, increasing sample (and blade count) near player
		// (samples of grass density texture is constant per tile, regardless of size)

		// The overall tile will be the terrain tile, making terrain tile size largest LOD
		// Tsushima divides tiles into 3 LODs, quarters, eighths, sixteenths. Can have combination of divisions to fill single tile


		// For now, lets implement this on CPU because I wanna solidify overall method
		[Header("Dependencies")]
		public WorldGenerator _worldGenerator;
		[Header("Global Config")]
		public bool _useGlobalConfig;
		[SerializeField] private GrassGlobalConfig _globalConfig;
		[Header("Global Set Dependencies")]
		[SerializeField] private Mesh _grassMesh;
		[SerializeField] private Material _renderingMaterial;
		
		[Header("Settings")]
		[SerializeField] private float distToPlayerCutoff = 1000f;
		[Tooltip("Samples density texture this many times per tile, regardless of tile size. This is the amount of samples at LOD0.")]
		[SerializeField] [Range(0, 100)]private int _samplesX, _samplesY;
		[Tooltip("Amount of splits in one tile for LOD consideration and minimum tile size. 0 = 0, 1 = 4, 2 = 16, 3 = 32")]
		[SerializeField] private float _fallbackTileSizeX, _fallbackTileSizeZ;
		[Tooltip("Treats mesh bounds as if they were smaller on all sides by this value. Useful for when terrain tiles overlap.")]
		[SerializeField] private float _meshBoundsPadding;
		
		private float _tileSizeX, _tileSizeZ;
		private RenderParams _renderParams;

		// Compute this in compute shader when using RenderPrimitivesIndexed
		// RenderPrimitivesIndexed requires:
		// custom RenderParams.bounds
		// RenderParams.matProps = MaterialPropertyBlock with 

		// for gizmo debugging
		private Vector3[] _worldPos;
		private Matrix4x4[] _worldPosTransformMatrices;

		private Matrix4x4[] WorldPositionsToMatrix(Vector3[] worldPositions) {
			Matrix4x4[] objectToWorld = new Matrix4x4[worldPositions.Length];
			for (int i = 0; i < worldPositions.Length; i++) {
				Vector3 worldPosition = worldPositions[i];
				objectToWorld[i] = Matrix4x4.TRS(worldPosition, Quaternion.Euler(0, Random.Range(0, 365), 0), Vector3.one);
			}

			return objectToWorld;
		}

		// GetHeightValue inside WorldGenerator.cs, follow for base function

		private float FindMeshHeightAtWorldXZ(float x, float z) {
			return _worldGenerator.GetHeightValue(new Vector2(x, z));
		}

		private Vector3[] GetGrassPositionsWorld(int samplesX, int samplesZ) {
			// left to right, forward to back
			// lets hope the terrain tile is never rotated when instanced
			if (samplesX <= 0 || samplesZ <= 0) {
				Debug.LogWarning("SamplesX or SamplesZ cannot be zero!");
				samplesX = 1;
				samplesZ = 1;
			}
			Vector3 origin = transform.position;
			float sizeX = _tileSizeX - _meshBoundsPadding;
			float sizeZ = _tileSizeZ - _meshBoundsPadding;
			float xSpacing = sizeX / samplesX;
			float zSpacing = sizeZ / samplesZ;
			Vector3[] output = new Vector3[samplesZ * samplesX];
			for (int z = 0; z < samplesZ; z++) {
				for (int x = 0; x < samplesX; x++) {
					float xOut = xSpacing * x + xSpacing / 2 + Random.Range(-0.1f, 0.1f);
					float zOut = zSpacing * z + zSpacing / 2 + Random.Range(-0.1f, 0.1f);
					float yOut = FindMeshHeightAtWorldXZ(xOut + origin.x, zOut + origin.z);
					// use if origin in center
					// xOut -= sizeX / 2;
					// zOut -= sizeZ / 2;
					Vector3 localOut = new Vector3(xOut, yOut, zOut);
					output[samplesX * z + x] = localOut + gameObject.transform.position;
				}
			}
			return output;
		}

		//----------------------------------------------------------------------------------

		private void Start() {
			#region Script Dep Null Checks 
			if (_worldGenerator == null) {
				Debug.LogWarning("GrassTile: No WorldGenerator found. Height sampling will return zero. Using fallback tile size values.");
				_tileSizeX = _fallbackTileSizeX;
				_tileSizeZ = _fallbackTileSizeZ;
			}
			else {
				_tileSizeX = _worldGenerator._size * _worldGenerator._resolution;
				_tileSizeZ = _tileSizeX;
			}
			if (_globalConfig == null) {
				GameObject gameObj = GameObject.FindWithTag("GlobalObject");
				if (gameObj != null) {
					_globalConfig = gameObj.GetComponent<GrassGlobalConfig>();
					if (_globalConfig == null) {
						Debug.LogWarning("GrassTile: No object tagged GlobalObject with GrassGlobalConfig found. Disabling global configuration.");
						_useGlobalConfig = false;
					}
				}
				else {
					Debug.LogWarning("GrassTile: No object tagged GlobalObject with GrassGlobalConfig found. Disabling global configuration.");
					_useGlobalConfig = false;
				}
			}
			#endregion
			if (_useGlobalConfig) {
				_samplesX = _globalConfig._samplesX;
				_samplesY = _globalConfig._samplesY;
				_fallbackTileSizeX = _globalConfig._fallbackTileSizeX;
				_fallbackTileSizeZ = _globalConfig._fallbackTileSizeZ;
				_grassMesh = _globalConfig._grassMesh;
				_renderingMaterial = _globalConfig._renderingMaterial;
				_meshBoundsPadding = _globalConfig._tileBoundsPadding;
				distToPlayerCutoff = _globalConfig._distToPlayerCutoff;
			}
			_renderParams = new RenderParams(_renderingMaterial);
			_worldPos = GetGrassPositionsWorld(_samplesX, _samplesY);
			_worldPosTransformMatrices = WorldPositionsToMatrix(_worldPos);
		}

		private void Update() {

			if (Vector3.Distance(transform.position, Camera.main.transform.position) > distToPlayerCutoff) {
				return;
			}
			if (_useGlobalConfig) {
				_samplesX = _globalConfig._samplesX;
				_samplesY = _globalConfig._samplesY;
				_fallbackTileSizeX = _globalConfig._fallbackTileSizeX;
				_fallbackTileSizeZ = _globalConfig._fallbackTileSizeZ;
				_grassMesh = _globalConfig._grassMesh;
				_renderingMaterial = _globalConfig._renderingMaterial;
				_meshBoundsPadding = _globalConfig._tileBoundsPadding;
				distToPlayerCutoff = _globalConfig._distToPlayerCutoff;
			}
			Graphics.RenderMeshInstanced(_renderParams, _grassMesh, 0, _worldPosTransformMatrices);
		}

		private void OnDrawGizmosSelected() {
			if (_worldPos == null) return;
			foreach (Vector3 pos in _worldPos) {
				Gizmos.DrawSphere(pos, 1);
			}
		}
	}
}