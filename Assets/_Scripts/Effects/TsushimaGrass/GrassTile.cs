using System;
using System.Numerics;

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
		[SerializeField] private Mesh _terrainTileMesh;
		[SerializeField] private Mesh _grassMesh;
		[SerializeField] private Material _renderingMaterial;

		[Header("Settings")]
		[Tooltip("Samples density texture this many times per tile, regardless of tile size. This is the amount of samples at LOD0.")]
		[SerializeField, Range(0, 100)] private int _samplesX, _samplesY;
		[Tooltip("Amount of splits in one tile for LOD consideration and minimum tile size. 0 = 0, 1 = 4, 2 = 16, 3 = 32")]
		[SerializeField, Range(0, 3)] private int _tileSplitFactor;

		[Tooltip("Treats mesh bounds as if they were smaller on all sides by this value. Useful for when terrain tiles overlap.")]
		[SerializeField] private float _meshBoundsPadding;
		
		private RenderParams _renderParams;

		// Compute this in compute shader when using RenderPrimitivesIndexed
		private WorldPositionTransformations _worldPosTransformMatrix;

		private struct WorldPositionTransformations
		{
			public Matrix4x4[] objectToWorld;

			public WorldPositionTransformations(Vector3[] worldPositions) {
				objectToWorld = new Matrix4x4[worldPositions.Length - 1];
				for (int i = 0; i < worldPositions.Length; i++) {
					Vector3 worldPosition = worldPositions[i];
					objectToWorld[i] = Matrix4x4.TRS(worldPosition, Quaternion.identity, Vector3.one);
				}
			}
		}
		
		// Working up to GPU position calculation:
		// 1 - CPU evenly spaced positions ----
		// 2 - CPU jittered posiitons
		// 3 - GPU of 1
		// 4 - GPU of 2

		private Vector3 GetPositionsAtTerrainHeight(int samplesX, int samplesY) {
			// left to right, top to bottom
			// lets hope the terrain tile is never rotated when instnaced
			float xExtent = _terrainTileMesh.bounds.extents.x;
			float zExtent = _terrainTileMesh.bounds.extents.z;
			for (int i = 0; i < samplesY; i++) {
				for (int j = 0; j < samplesX; j++) {
					// TODO: write this
					throw new NotImplementedException();
				}
			}
			// temp
			return Vector3.zero;
		}

		private float MinOfVector3Components(Vector3 vector) {
			float x = vector.x;
			float y = vector.y;
			float z = vector.z;
			return x < y ? (x < z ? x : z) : (y < z ? y : z);
		}
		
		private void OnValidate() {
			if (_meshBoundsPadding > MinOfVector3Components(_terrainTileMesh.bounds.extents)) {
				_meshBoundsPadding = MinOfVector3Components(_terrainTileMesh.bounds.extents);
			}
		}

		private void Start() {
			_renderParams = new RenderParams(_renderingMaterial);
			_worldPosTransformMatrix = new WorldPositionTransformations();
		}

		private void Update() {
			// Graphics.RenderMeshInstanced(_renderParams, _grassMesh, 0,,);
		}
	}
}