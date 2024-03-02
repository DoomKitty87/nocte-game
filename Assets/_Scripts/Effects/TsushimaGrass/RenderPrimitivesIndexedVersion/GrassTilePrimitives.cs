using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Numerics;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor;

namespace Effects.TsushimaGrass
{
	using UnityEngine;

	public class GrassTilePrimitives : MonoBehaviour
	{
		
		// Function flow ---
		// Start() -> SendChunksToComputeShader -> GPUComputePositionsTo -> Update -> Graphics.SomeRenderFunction
		// File flow ---
		// this -> CalculateGrassParams.compute -> this -> GrassCustomShader.shader
		
		[Header("Dependencies")]
		public WorldGenerator _worldGenerator;
		public Texture2D _tileHeightmap;
		[Header("Global Config")]
		public bool _useGlobalConfig;
		[SerializeField] private GrassGlobalConfig _globalConfig;
		[Header("Global Set Dependencies")]
		public Camera _mainCamera;
		[SerializeField] private Mesh _grassMesh;
		[SerializeField] private Material _renderingShaderMat;
		
		[Header("Settings")]
		[SerializeField] private float _distToPlayerCutoff = 1000f;
		[SerializeField] private float _LOD1Distance;
		[SerializeField] [Range(0, 100)]private int _samplesX, _samplesZ;
		[SerializeField] private float _fallbackTileSizeX, _fallbackTileSizeZ;
		[SerializeField] private float _meshBoundsPadding;
		[SerializeField][Range(0, 10)] private int _chunkSplitFactor = 1;
		
		private float _tileSizeX, _tileSizeZ;
		private RenderParams _renderParams;
		
		private Vector3[] _worldPos;
		private Matrix4x4[] _worldPosTransformMatrices;
		
		// ===== Compute Shader + Buffers
		[SerializeField] private ComputeShader _positionCompute;
		// ===== Passed between (re-referenced)
		private GraphicsBuffer _grassPositionsBuffer;
		// ===== Material Shader + Buffers =====
		private GraphicsBuffer _grassMeshVertexBuffer;
		private GraphicsBuffer _grassMeshTrisBuffer;
		private GraphicsBuffer _lodIndexBuffer;
		// =====

		private List<Vector3> _chunkCenters;
		
		private void DisposeOfBuffers() {
			_grassPositionsBuffer?.Dispose();
			_grassPositionsBuffer = null;
			_grassMeshVertexBuffer?.Dispose();
			_grassMeshVertexBuffer = null;
			_grassMeshTrisBuffer?.Dispose();
			_grassMeshTrisBuffer = null;
			_lodIndexBuffer?.Dispose();
			_lodIndexBuffer = null;
		}
		
		private void MeshDataToBuffers(Mesh mesh, out GraphicsBuffer vertexPosBuffer, out GraphicsBuffer trisIndexBuffer) {
			vertexPosBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.vertices.Length, sizeof(float) * 3);
			vertexPosBuffer.SetData(mesh.vertices);
			trisIndexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.triangles.Length, sizeof(int));
			trisIndexBuffer.SetData(mesh.triangles);
		}

		private void GPUComputePositionsTo(Vector3 cameraPosition, GraphicsBuffer outputBuffer, GraphicsBuffer lodIndexBuffer, int indexOffset, Vector3 pivot, int samplesX, int samplesZ, float tileSizeX, float tileSizeZ, float padding) {
			int kernelIndex = _positionCompute.FindKernel("ComputePosition");
			// make the ids static later
			_positionCompute.SetFloat(Shader.PropertyToID("_samplesX"), samplesX);
			_positionCompute.SetFloat(Shader.PropertyToID("_samplesZ"), samplesZ);
			_positionCompute.SetFloat(Shader.PropertyToID("_sizeX"), tileSizeX);
			_positionCompute.SetFloat(Shader.PropertyToID("_sizeZ"), tileSizeZ);
			_positionCompute.SetFloat(Shader.PropertyToID("_padding"), padding);
			_positionCompute.SetVector(Shader.PropertyToID("_tilePivotWorldPosition"), transform.position);
			_positionCompute.SetVector(Shader.PropertyToID("_chunkPivotWorldPosition"), pivot);
			// _positionCompute.SetTexture(kernelIndex, Shader.PropertyToID("_tileHeightmapTexture"), _tileHeightmap);
			// _positionCompute.SetInt(Shader.PropertyToID("_tileHeightmapTextureWidth"), _tileHeightmap.width);
			_positionCompute.SetInt(Shader.PropertyToID("_chunkSplitFactor"), _chunkSplitFactor);
			_positionCompute.SetBuffer(kernelIndex, Shader.PropertyToID("_lodIndexBuffer"), lodIndexBuffer);
			_positionCompute.SetVector(Shader.PropertyToID("_cameraWorldPosition"), cameraPosition);
			_positionCompute.SetFloat(Shader.PropertyToID("_LOD1Distance"), _LOD1Distance);
			int grassCount = samplesX * samplesZ;
			_positionCompute.SetBuffer(kernelIndex, Shader.PropertyToID("_positionOutputBuffer"), outputBuffer);
			_positionCompute.SetFloat(Shader.PropertyToID("_indexOffset"), (uint)indexOffset);
			// GraphicsBuffer debugBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, samplesX * samplesZ, sizeof(float));
			// _positionCompute.SetBuffer(kernelIndex, Shader.PropertyToID("_debugCPUReadbackBuffer"), debugBuffer);
			_positionCompute.GetKernelThreadGroupSizes(kernelIndex, out uint threadX, out _, out _);
			_positionCompute.Dispatch(kernelIndex, Mathf.CeilToInt(grassCount/(float)threadX), 1, 1);
			// DebugFloatBuffer(debugBuffer);
		}

		// this sucks, will fix later
		private Vector2 AverageElements(List<Vector2> vectors) {
			List<float> xComps = new();
			List<float> yComps = new();
			foreach (Vector2 vector in vectors) {
				xComps.Add(vector.x);
				yComps.Add(vector.y);
			}
			float xAdded = 0;
			foreach (float x in xComps) {
				xAdded += x;
			}
			float yAdded = 0;
			foreach (float y in yComps) {
				yAdded += y;
			}
			return new Vector2(xAdded / xComps.Count, yAdded / yComps.Count);
		}
		
		private void SendChunksToComputeShader(Camera mainCamera, GraphicsBuffer outputBuffer, GraphicsBuffer lodIndexBuffer, int chunkSplitFactor, int samplesX, int samplesZ, float tileSizeX, float tileSizeZ, float padding) {
			_chunkCenters = new List<Vector3>();
			if (chunkSplitFactor == 0) {
				GPUComputePositionsTo(mainCamera.transform.position, outputBuffer, lodIndexBuffer, 0, transform.position, samplesX, samplesZ, tileSizeX, tileSizeZ, padding);
				return;
			}
			float sizeXNoPadding = tileSizeX - (padding * 2);
			float sizeZNoPadding = tileSizeZ - (padding * 2);
			int indexOffset = 0;
			float floatChunkSplitFactor = (float)chunkSplitFactor;
			for (int z = 0; z < chunkSplitFactor * 2; z++) {
				for (int x = 0; x < chunkSplitFactor * 2; x++) {
					Vector2 topLeft = new Vector2(sizeXNoPadding * (x / (floatChunkSplitFactor * 2)) + padding, sizeZNoPadding * (z / (floatChunkSplitFactor * 2)) + padding);
					Vector2 topRight = new Vector2(sizeXNoPadding * ((x + 1) / (floatChunkSplitFactor * 2)) + padding, sizeZNoPadding * (z / (floatChunkSplitFactor * 2)) + padding);
					Vector2 bottomLeft = new Vector2(sizeXNoPadding * (x / (floatChunkSplitFactor * 2)) + padding, sizeZNoPadding * ((z + 1) / (floatChunkSplitFactor * 2)) + padding);
					Vector2 bottomRight = new Vector2(sizeXNoPadding * ((x + 1) / (floatChunkSplitFactor * 2)) + padding, sizeZNoPadding * ((z + 1) / (floatChunkSplitFactor * 2)) + padding);
					Vector2 chunkCenter = AverageElements(new List<Vector2> { topLeft, topRight, bottomLeft, bottomRight });
					_chunkCenters.Add(chunkCenter);
					if (Vector3.Distance(chunkCenter, mainCamera.transform.position) > _distToPlayerCutoff) {
						continue;
					}
					int chunkSamplesX = samplesX / (chunkSplitFactor * 2);
					int chunkSamplesZ = samplesZ / (chunkSplitFactor * 2);
					float chunkSizeX = sizeXNoPadding / (floatChunkSplitFactor * 2);
					float chunkSizeZ = sizeZNoPadding / (floatChunkSplitFactor * 2);
					GPUComputePositionsTo(mainCamera.transform.position, outputBuffer, _lodIndexBuffer, indexOffset, new Vector3(topLeft.x, 0, topLeft.y) + transform.position, chunkSamplesX, chunkSamplesZ, chunkSizeX, chunkSizeZ, 0);
					// print($"Index Offset: {indexOffset}, ABCD: [{topLeft},{topRight},{bottomLeft},{bottomRight}], Chunk Center: {chunkCenter}, Chunk Samples: {chunkSamplesX}x{chunkSamplesZ}, Chunk Size: {chunkSizeX}x{chunkSizeZ}");
					indexOffset += chunkSamplesX * chunkSamplesZ;
				}
			}
		}

		private void DebugFloatBuffer(GraphicsBuffer matrixBuffer, int limit = 10) {
			float[] debug = new float[limit];
			matrixBuffer.GetData(debug);
			DebugFloatArray(debug, limit);
		}
		
		private void DebugFloatArray(float[] array, int lim = 0) {
			string output = "";
			for (int i = 0; i < lim; i++) {
				output += array[i] + ", ";
			}
			Debug.Log(output);
		}

		public void GenerateGrassHook() {
			AssignGlobalConfigValues();
			GenerateGrass(_grassPositionsBuffer, _lodIndexBuffer, _renderParams);
		}
		
		private void GenerateGrass(GraphicsBuffer positionsBuffer, GraphicsBuffer lodBuffer, RenderParams renderParams) {
			SendChunksToComputeShader(_mainCamera, positionsBuffer, lodBuffer, _chunkSplitFactor, _samplesX, _samplesZ, _tileSizeX, _tileSizeZ, _meshBoundsPadding);
			MeshDataToBuffers(_grassMesh, out _grassMeshVertexBuffer, out _grassMeshTrisBuffer);
			renderParams.matProps.SetBuffer("_vertexPositions", _grassMeshVertexBuffer);
			renderParams.matProps.SetBuffer("_instancePositionMatrices", positionsBuffer);
			renderParams.matProps.SetBuffer("_lodIndexBuffer", lodBuffer);
			
		}
		
		private void AssignGlobalConfigValues() {
			_samplesX = _globalConfig._samplesX;
			_samplesZ = _globalConfig._samplesY;
			_fallbackTileSizeX = _globalConfig._fallbackTileSizeX;
			_fallbackTileSizeZ = _globalConfig._fallbackTileSizeZ;
			_grassMesh = _globalConfig._LOD0GrassMesh;
			_renderingShaderMat = _globalConfig._renderingShaderMat;
			_meshBoundsPadding = _globalConfig._tileBoundsPadding;
			_distToPlayerCutoff = _globalConfig._distToPlayerCutoff;
			_positionCompute = _globalConfig._positionCompute;
			_chunkSplitFactor = _globalConfig._chunkSplitFactor;
			_LOD1Distance = _globalConfig._LOD1Distance;
			_mainCamera = _globalConfig._mainCamera;
		}
		
		//----------------------------------------------------------------------------------

		public void Start() {
			#region Script Dep Null Checks 
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
			if (_useGlobalConfig) {
				AssignGlobalConfigValues();
			}
			if (_worldGenerator == null) {
				Debug.LogWarning("GrassTile: No WorldGenerator found. Height sampling will return zero. Using fallback tile size values.");
				_tileSizeX = _fallbackTileSizeX;
				_tileSizeZ = _fallbackTileSizeZ;
			}
			else {
				_tileSizeX = _worldGenerator._size * _worldGenerator._resolution;
				_tileSizeZ = _tileSizeX;
			}
			
			#endregion
			_grassPositionsBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _samplesX * _samplesZ, sizeof(float) * 16);
			_lodIndexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, _samplesX * _samplesZ, sizeof(uint));
			_renderParams = new RenderParams(_renderingShaderMat);
			_renderParams.matProps = new MaterialPropertyBlock();
			_renderParams.worldBounds = new Bounds(transform.position, new Vector3(_tileSizeX, 1000000, _tileSizeZ));
			GenerateGrass(_grassPositionsBuffer, _lodIndexBuffer, _renderParams);
		}

		
		
		private void Update() {
			Graphics.RenderPrimitivesIndexed(_renderParams, MeshTopology.Triangles, _grassMeshTrisBuffer, _grassMeshTrisBuffer.count, instanceCount: _samplesX * _samplesZ);
		}

		private void OnDrawGizmos() {
			if (_chunkCenters == null) return;
			foreach (Vector3 pos in _chunkCenters) {
				Gizmos.DrawSphere(pos, 10);
			}
		}

		private void OnDestroy() {
			DisposeOfBuffers();
		}
	}
}