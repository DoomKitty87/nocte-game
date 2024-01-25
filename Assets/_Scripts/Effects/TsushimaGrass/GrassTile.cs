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

		[Header("Auto Assigned Dependencies")]
		[SerializeField] private MeshRenderer _meshRenderer;
		[SerializeField] private MeshFilter _terrainTileMeshFilter;
		[Header("Dependencies")]
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
		private Matrix4x4[] _worldPosTransformMatrices;
		
		// Working up to GPU position calculation:
		// 1 - CPU evenly spaced positions ----
		// 2 - CPU jittered posiitons
		// 3 - GPU of 1
		// 4 - GPU of 2

		private Matrix4x4[] WorldPositionsToMatrix(Vector3[] worldPositions) {
			Matrix4x4[] objectToWorld = new Matrix4x4[worldPositions.Length];
			for (int i = 0; i < worldPositions.Length; i++) {
				Vector3 worldPosition = worldPositions[i];
				objectToWorld[i] = Matrix4x4.TRS(worldPosition, Quaternion.identity, Vector3.one);
			}
			return objectToWorld;
		}

		private float FindMeshHeightAtLocalXZ(Mesh mesh, float x, float z) {
			Vector3[] meshVerts = mesh.vertices;
			int[] triangles = mesh.triangles;
			float closestX = Single.PositiveInfinity;
			float closestZ = Single.PositiveInfinity;
			int closestTriangleIndexStart = 0;
			for (int i = 0; i < triangles.Length / 3; i++) {
				int triStart = triangles[i * 3];
			}
		}
		
		private Vector3[] GetPositions(int samplesX, int samplesZ) {
			// left to right, forward to back
			// lets hope the terrain tile is never rotated when instanced
			float sizeX = _meshRenderer.bounds.size.x - _meshBoundsPadding;
			float sizeZ = _meshRenderer.bounds.size.z - _meshBoundsPadding;
			float xSpacing = sizeX / samplesX;
			float zSpacing = sizeZ / samplesZ;
			Vector3[] output = new Vector3[samplesZ * samplesX];
			for (int z = 0; z < samplesZ; z++) {
				for (int x = 0; x < samplesX; x++) {
					float xOut = (xSpacing * x - xSpacing / 2) - sizeX / 2;
					float yOut = 0;
					float zOut = (zSpacing * z - zSpacing / 2) - sizeZ / 2;
					output[samplesX * z + x] = new Vector3(xOut, yOut, zOut);
				}
			}
			return output;
		}

		private float MinOfVector3Components(Vector3 vector) {
			float x = vector.x;
			float y = vector.y;
			float z = vector.z;
			return x < y ? (x < z ? x : z) : (y < z ? y : z);
		}
		
		//----------------------------------------------------------------------------------
		
		private void OnValidate() {
			if (_meshBoundsPadding > MinOfVector3Components(_meshRenderer.bounds.extents)) {
				_meshBoundsPadding = MinOfVector3Components(_meshRenderer.bounds.extents);
			}
			if (_terrainTileMeshFilter == null) {
				_terrainTileMeshFilter = gameObject.GetComponent<MeshFilter>();
			}
			if (_meshRenderer == null) {
				_meshRenderer = gameObject.GetComponent<MeshRenderer>();
			}
		}

		private void Start() {
			_renderParams = new RenderParams(_renderingMaterial);
		}

		private void Update() {
			_worldPosTransformMatrices = WorldPositionsToMatrix(GetPositions(_samplesX, _samplesY));
			Graphics.RenderMeshInstanced(_renderParams, _grassMesh, 0, _worldPosTransformMatrices);
		}
	}
}