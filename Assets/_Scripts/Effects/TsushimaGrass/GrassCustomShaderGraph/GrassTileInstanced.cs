using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine.Rendering;

namespace Effects.TsushimaGrass
{
	using UnityEngine;

	public class GrassTileInstanced : MonoBehaviour
	{
		// Tsushima divides single tile into smaller tiles based on lod, increasing sample (and blade count) near player
		// (samples of grass density texture is constant per tile, regardless of size)

		// The overall tile will be the terrain tile, making terrain tile size largest LOD
		// Tsushima divides tiles into 3 LODs, quarters, eighths, sixteenths. Can have combination of divisions to fill single tile


		[Header("Dependencies")]
		public WorldGenerator _worldGenerator;

		[Header("Global Config")]
		public bool _useGlobalConfig;

		[SerializeField] private GrassGlobalConfig _globalConfig;

		[Header("Global Set Dependencies")]
		[SerializeField] private Mesh _grassMesh;

		[SerializeField] private Material _renderingShaderMat;

		[Header("Settings")]
		[SerializeField] private float distToPlayerCutoff = 1000f;

		[SerializeField] [Range(0, 100)] private int _samplesX, _samplesZ;
		[SerializeField] private float _fallbackTileSizeX, _fallbackTileSizeZ;
		[SerializeField] private float _meshBoundsPadding;

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

		private void MeshDataToBuffers(
			Mesh mesh,
			out GraphicsBuffer vertexPosBuffer,
			out GraphicsBuffer trisIndexBuffer
		) {
			vertexPosBuffer = new GraphicsBuffer(
				GraphicsBuffer.Target.Structured,
				mesh.vertices.Length,
				sizeof(float) * 3
			);
			vertexPosBuffer.SetData(mesh.vertices);
			trisIndexBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, mesh.triangles.Length, sizeof(int));
			trisIndexBuffer.SetData(mesh.triangles);
		}

		private void GPUComputePositionsTo(
			out GraphicsBuffer outputBuffer,
			int samplesX,
			int samplesZ,
			float tileSizeX,
			float tileSizeZ,
			float padding
		) {
			int kernelIndex = _positionCompute.FindKernel("ComputePosition");
			// make the ids static later
			_positionCompute.SetFloat(Shader.PropertyToID("_samplesX"), samplesX);
			_positionCompute.SetFloat(Shader.PropertyToID("_samplesZ"), samplesZ);
			_positionCompute.SetFloat(Shader.PropertyToID("_sizeX"), tileSizeX);
			_positionCompute.SetFloat(Shader.PropertyToID("_sizeZ"), tileSizeZ);
			_positionCompute.SetFloat(Shader.PropertyToID("_padding"), padding);
			_positionCompute.SetFloat(Shader.PropertyToID("_worldX"), transform.position.x);
			_positionCompute.SetFloat(Shader.PropertyToID("_worldY"), transform.position.y);
			_positionCompute.SetFloat(Shader.PropertyToID("_worldZ"), transform.position.z);
			int grassCount = samplesX * samplesZ;
			outputBuffer = new GraphicsBuffer(GraphicsBuffer.Target.Structured, grassCount, sizeof(float) * 16);
			_positionCompute.SetBuffer(kernelIndex, Shader.PropertyToID("_positionOutputBuffer"), outputBuffer);
			_positionCompute.GetKernelThreadGroupSizes(kernelIndex, out uint threadX, out _, out _);
			_positionCompute.Dispatch(kernelIndex, Mathf.CeilToInt(grassCount / (float)threadX), 1, 1);
		}

		private void DebugFloatBuffer(GraphicsBuffer matrixBuffer, int limit = 10) {
			float[] debug = new float[limit];
			matrixBuffer.GetData(debug);
			DebugFloatArray(debug, limit);
		}

		private void DebugFloatArray(float[] array, int lim = 0) {
			string output = "";
			for (int i = 0; i < lim; i++) output += array[i] + ", ";
			Debug.Log(output);
		}

		private void AssignGlobalConfig() {
			_samplesX = _globalConfig._samplesX;
			_samplesZ = _globalConfig._samplesY;
			_fallbackTileSizeX = _globalConfig._fallbackTileSizeX;
			_fallbackTileSizeZ = _globalConfig._fallbackTileSizeZ;
			_grassMesh = _globalConfig._grassMesh;
			_renderingShaderMat = _globalConfig._renderingShaderMat;
			_meshBoundsPadding = _globalConfig._tileBoundsPadding;
			distToPlayerCutoff = _globalConfig._distToPlayerCutoff;
			_positionCompute = _globalConfig._positionCompute;
		}

		private Matrix4x4[] FillPlaceholderArrayWithIdent(int count) {
			Matrix4x4[] output = new Matrix4x4[count];
			for (int i = 0; i < count; i++) output[i] = Matrix4x4.identity;
			// this doesn't work, look into more later
			output[0] = new Matrix4x4(
				new Vector4(1, 0, 0, 10),
				new Vector4(0, 1, 0, 1000),
				new Vector4(0, 0, 1, 10),
				new Vector4(0, 0, 0, 1)
			);
			output[1] = new Matrix4x4(
				new Vector4(1, 0, 0, -10),
				new Vector4(0, 1, 0, -1000),
				new Vector4(0, 0, 1, -10),
				new Vector4(0, 0, 0, 1)
			);
			return output;
		}

		private bool _renderingGrass = false;
		private const int instanceGroupSize = 500;

		private IEnumerator RenderGrassMesh() {
			_renderingGrass = true;
			int grassCount = _samplesX * _samplesZ;
			int instanceGroupCount = Mathf.CeilToInt(grassCount / (float)instanceGroupSize);
			_renderParams.matProps.SetBuffer("_instancePositionMatrices", _grassPositionsBuffer);
			for (int i = 0; i < instanceGroupCount; i++) {
				_renderParams.matProps.SetFloat("_instanceGroupID", i);
				_renderParams.matProps.SetFloat("_instancesPerGroup", instanceGroupSize);
				int instanceCount = Mathf.Min(instanceGroupSize, grassCount - i * instanceGroupSize);
				// bounds are calculated based off of matrix positions - see documentation
				Bounds grassBounds = new Bounds(transform.position, new Vector3(10000, 1000000, 10000));
				Graphics.DrawMeshInstancedIndirect(_grassMesh, 0, _renderingShaderMat, grassBounds, );
				yield return null;
			}
			_renderingGrass = false;
		}

		//----------------------------------------------------------------------------------

		private void Start() {
			#region Script Dep Null Checks

			if (_globalConfig == null) {
				GameObject gameObj = GameObject.FindWithTag("GlobalObject");
				if (gameObj != null) {
					_globalConfig = gameObj.GetComponent<GrassGlobalConfig>();
					if (_globalConfig == null) {
						Debug.LogWarning(
							"GrassTile: No object tagged GlobalObject with GrassGlobalConfig found. Disabling global configuration."
						);
						_useGlobalConfig = false;
					}
				}
				else {
					Debug.LogWarning(
						"GrassTile: No object tagged GlobalObject with GrassGlobalConfig found. Disabling global configuration."
					);
					_useGlobalConfig = false;
				}
			}

			if (_globalConfig != null) AssignGlobalConfig();
			if (_worldGenerator == null) {
				Debug.LogWarning(
					"GrassTile: No WorldGenerator found. Height sampling will return zero. Using fallback tile size values."
				);
				_tileSizeX = _fallbackTileSizeX;
				_tileSizeZ = _fallbackTileSizeZ;
			}
			else {
				_tileSizeX = _worldGenerator._size * _worldGenerator._resolution;
				_tileSizeZ = _tileSizeX;
			}

			#endregion
			// Compute Shader
			GPUComputePositionsTo(out _grassPositionsBuffer, _samplesX, _samplesZ, _tileSizeX, _tileSizeZ, _meshBoundsPadding);
			// Render Material
			_renderParams = new RenderParams(_renderingShaderMat);
			_renderParams.matProps = new MaterialPropertyBlock();
		}

		private void Update() {
			if (_renderingGrass) return;
			StartCoroutine(RenderGrassMesh());
		}

		private void OnDrawGizmosSelected() {
			if (_worldPos == null) return;
			foreach (Vector3 pos in _worldPos) Gizmos.DrawSphere(pos, 1);
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