using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEditor;

namespace Effects.TsushimaGrass
{
	using UnityEngine;

	public class GrassTilePrimitives : MonoBehaviour
	{
		// Tsushima divides single tile into smaller tiles based on lod, increasing sample (and blade count) near player
		// (samples of grass density texture is constant per tile, regardless of size)

		// The overall tile will be the terrain tile, making terrain tile size largest LOD
		// Tsushima divides tiles into 3 LODs, quarters, eighths, sixteenths. Can have combination of divisions to fill single tile


		// For now, lets implement this on CPU because I wanna solidify overall method
		[Header("Dependencies")]
		public WorldGenerator _worldGenerator;
		public RenderTexture _tileHeightmap;
		[Header("Global Config")]
		public bool _useGlobalConfig;
		[SerializeField] private GrassGlobalConfig _globalConfig;
		[Header("Global Set Dependencies")]
		[SerializeField] private Mesh _grassMesh;
		[SerializeField] private Material _renderingShaderMat;
		
		[Header("Settings")]
		[SerializeField] private float distToPlayerCutoff = 1000f;
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
		private GraphicsBuffer _meshVertsBuffer;
		private GraphicsBuffer _meshTrisBuffer;
		// =====

		private void MeshDataToBuffers(Mesh mesh, out GraphicsBuffer vertexPosBuffer, out GraphicsBuffer trisIndexBuffer) {
			vertexPosBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.vertices.Length, sizeof(float) * 3);
			vertexPosBuffer.SetData(mesh.vertices);
			trisIndexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.triangles.Length, sizeof(int));
			trisIndexBuffer.SetData(mesh.triangles);
		}

		private void GPUComputePositionsTo(GraphicsBuffer outputBuffer, int indexOffset, Vector3 center, int samplesX, int samplesZ, float tileSizeX, float tileSizeZ, float padding) {
			int kernelIndex = _positionCompute.FindKernel("ComputePosition");
			// make the ids static later
			_positionCompute.SetFloat(Shader.PropertyToID("_samplesX"), samplesX);
			_positionCompute.SetFloat(Shader.PropertyToID("_samplesZ"), samplesZ);
			_positionCompute.SetFloat(Shader.PropertyToID("_sizeX"), tileSizeX);
			_positionCompute.SetFloat(Shader.PropertyToID("_sizeZ"), tileSizeZ);
			_positionCompute.SetFloat(Shader.PropertyToID("_padding"), padding);
			_positionCompute.SetFloat(Shader.PropertyToID("_worldX"), center.x);
			_positionCompute.SetFloat(Shader.PropertyToID("_worldY"), center.y);
			_positionCompute.SetFloat(Shader.PropertyToID("_worldZ"), center.z);
			_positionCompute.SetFloat(Shader.PropertyToID("_indexOffset"), (uint)indexOffset);
			_positionCompute.SetTexture(kernelIndex, Shader.PropertyToID("_tileHeightmapTexture"), _tileHeightmap);
			_positionCompute.SetInt(Shader.PropertyToID("_tileHeightmapTextureWidth"), _tileHeightmap.width);
			_positionCompute.SetInt(Shader.PropertyToID("_chunkSplitFactor"), _chunkSplitFactor);
			int grassCount = samplesX * samplesZ;
			_positionCompute.SetBuffer(kernelIndex, Shader.PropertyToID("_positionOutputBuffer"), outputBuffer);
			GraphicsBuffer debugBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, samplesX * samplesZ, sizeof(float));
			_positionCompute.SetBuffer(kernelIndex, Shader.PropertyToID("_debugCPUReadbackBuffer"), debugBuffer);
			_positionCompute.GetKernelThreadGroupSizes(kernelIndex, out uint threadX, out _, out _);
			_positionCompute.Dispatch(kernelIndex, Mathf.CeilToInt(grassCount/(float)threadX), 1, 1);
			DebugFloatBuffer(debugBuffer);
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
		
		private void SendChunksToComputeShader(GraphicsBuffer outputBuffer, int chunkSplitFactor, int samplesX, int samplesZ, float tileSizeX, float tileSizeZ, float padding) {
			if (chunkSplitFactor == 0) {
				GPUComputePositionsTo(outputBuffer, 0, transform.position, samplesX, samplesZ, tileSizeX, tileSizeZ, padding);
				return;
			}
			float sizeXNoPadding = tileSizeX - (padding * 2);
			float sizeZNoPadding = tileSizeZ - (padding * 2);
			int indexOffset = 0;
			float floatChunkSplitFactor = (float)chunkSplitFactor;
			for (int z = 0; z < chunkSplitFactor * 2; z++) {
				for (int x = 0; x < chunkSplitFactor * 2; x++) {
					// Vector2 topLeft = new Vector2(sizeXNoPadding * (x / (chunkSplitFactor * 2)) + padding, sizeZNoPadding * (z / (chunkSplitFactor * 2)) + padding);
					// Vector2 topRight = new Vector2(sizeXNoPadding * (x + 1 / (chunkSplitFactor * 2)) + padding, sizeZNoPadding * (z / (chunkSplitFactor * 2)) + padding);
					// Vector2 bottomLeft = new Vector2(sizeXNoPadding * (x / (chunkSplitFactor * 2)) + padding, sizeZNoPadding * (z + 1 / (chunkSplitFactor * 2)) + padding);
					// Vector2 bottomRight = new Vector2(sizeXNoPadding * (x + 1 / (chunkSplitFactor * 2)) + padding, sizeZNoPadding * (z + 1 / (chunkSplitFactor * 2)) + padding);
					Vector2 topLeft = new Vector2(sizeXNoPadding * (x / (floatChunkSplitFactor * 2)) + padding, sizeZNoPadding * (z / (floatChunkSplitFactor * 2)) + padding);
					Vector2 topRight = new Vector2(sizeXNoPadding * ((x + 1) / (floatChunkSplitFactor * 2)) + padding, sizeZNoPadding * (z / (floatChunkSplitFactor * 2)) + padding);
					Vector2 bottomLeft = new Vector2(sizeXNoPadding * (x / (floatChunkSplitFactor * 2)) + padding, sizeZNoPadding * ((z + 1) / (floatChunkSplitFactor * 2)) + padding);
					Vector2 bottomRight = new Vector2(sizeXNoPadding * ((x + 1) / (floatChunkSplitFactor * 2)) + padding, sizeZNoPadding * ((z + 1) / (floatChunkSplitFactor * 2)) + padding);
					Vector2 chunkCenter = AverageElements(new List<Vector2> { topLeft, topRight, bottomLeft, bottomRight });
					int chunkSamplesX = samplesX / (chunkSplitFactor * 2);
					int chunkSamplesZ = samplesZ / (chunkSplitFactor * 2);
					float chunkSizeX = sizeXNoPadding / (floatChunkSplitFactor * 2);
					float chunkSizeZ = sizeZNoPadding / (floatChunkSplitFactor * 2);
					GPUComputePositionsTo(outputBuffer, indexOffset, new Vector3(chunkCenter.x, 0, chunkCenter.y) + transform.position, chunkSamplesX, chunkSamplesZ, chunkSizeX, chunkSizeZ, 0);
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
		
		//----------------------------------------------------------------------------------

		private void Start() {
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
			#region Globalconfig Assignment
			if (_useGlobalConfig) {
				_samplesX = _globalConfig._samplesX;
				_samplesZ = _globalConfig._samplesY;
				_fallbackTileSizeX = _globalConfig._fallbackTileSizeX;
				_fallbackTileSizeZ = _globalConfig._fallbackTileSizeZ;
				_grassMesh = _globalConfig._grassMesh;
				_renderingShaderMat = _globalConfig._renderingShaderMat;
				_meshBoundsPadding = _globalConfig._tileBoundsPadding;
				distToPlayerCutoff = _globalConfig._distToPlayerCutoff;
				_positionCompute = _globalConfig._positionCompute;
				_chunkSplitFactor = _globalConfig._chunkSplitFactor;
			}
			#endregion
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
			SendChunksToComputeShader(_grassPositionsBuffer, _chunkSplitFactor, _samplesX, _samplesZ, _tileSizeX, _tileSizeZ, _meshBoundsPadding);
			MeshDataToBuffers(_grassMesh, out _meshVertsBuffer, out _meshTrisBuffer);
			_renderParams = new RenderParams(_renderingShaderMat);
			_renderParams.matProps = new MaterialPropertyBlock();
			_renderParams.matProps.SetBuffer("_meshVertPositions", _meshVertsBuffer);
			_renderParams.matProps.SetBuffer("_instancePositionMatrices", _grassPositionsBuffer);
			_renderParams.worldBounds = new Bounds(transform.position, new Vector3(_tileSizeX, 1000000, _tileSizeZ));
			
		}
		
		private void Update() {
			// if (Vector2.Distance(new Vector2(transform.position.x, transform.position.z), new Vector2(Camera.main.transform.position.x, Camera.main.transform.position.z)) > distToPlayerCutoff) return;
			Graphics.RenderPrimitivesIndexed(_renderParams, MeshTopology.Triangles, _meshTrisBuffer, _meshTrisBuffer.count, instanceCount: _samplesX * _samplesZ);
		}

		private void OnDrawGizmosSelected() {
			if (_worldPos == null) return;
			foreach (Vector3 pos in _worldPos) {
				Gizmos.DrawSphere(pos, 1);
			}
		}

		private void OnDestroy() {
			_grassPositionsBuffer?.Dispose();
			_grassPositionsBuffer = null;
			_meshVertsBuffer?.Dispose();
			_meshVertsBuffer = null;
			_meshTrisBuffer?.Dispose();
			_meshTrisBuffer = null;
		}
	}
}