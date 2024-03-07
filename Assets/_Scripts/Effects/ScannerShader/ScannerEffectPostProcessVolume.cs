using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using System;
using UnityEngine.Splines.Interpolators;

[Serializable, VolumeComponentMenu("Post-processing/Custom/ScannerEffectPostProcessVolume")]
public sealed class ScannerEffectPostProcessVolume : CustomPostProcessVolumeComponent, IPostProcessComponent
{
    [Tooltip("Controls the intensity of the effect.")]
    public ClampedFloatParameter _intensity = new ClampedFloatParameter(0f, 0f, 1f);
    public Vector3Parameter _scannerCenterPosition = new Vector3Parameter(new Vector3(0, 0, 0));
    public Vector2Parameter _scanDirectionXZ = new Vector2Parameter(new Vector2(0, 1));
    public ClampedFloatParameter _scanDegrees = new ClampedFloatParameter(120, 0, 360);
    public FloatParameter _scanDistance = new FloatParameter(10f);
    public ColorParameter _scanLineColor = new ColorParameter(Color.cyan, true, false, true);
    public ColorParameter _lastScanLineColor = new ColorParameter(Color.cyan, true, false, true);
    public FloatParameter _scanLineWidth = new FloatParameter(0.02f);
    public FloatParameter _scanLineDistBetween = new FloatParameter(1);
    public ColorParameter _edgeGlowColor = new ColorParameter(Color.cyan, true, false, true);
    public ColorParameter _edgeGlowAccentColor = new ColorParameter(Color.black, true, false, true);
    public FloatParameter _edgeGlowWidth = new FloatParameter(2);
    public ClampedFloatParameter _darkenOpacity = new ClampedFloatParameter(1, 0, 1);
    public ClampedFloatParameter _darkenBaseValue = new ClampedFloatParameter(1, 0, 1);
    public ClampedFloatParameter _darkenWidth = new ClampedFloatParameter(0.85f, 0, 1);
    public ClampedFloatParameter _sideFadeMagnitude = new ClampedFloatParameter(0.1f, 0, 1);
    
    // uniform float3 _scanCenterPos;
    // uniform float2 _scanDirectionXZ;
    // uniform float _scanDegrees;
    // uniform float _scanDistance;
    // uniform float4 _scanLineColor;
    // uniform float4 _lastScanLineColor;
    // uniform float _scanLineWidth;
    // uniform float _scanLineDistBetween;
    // uniform float4 _edgeGlowColor;
    // uniform float4 _edgeGlowAccentColor;
    // uniform float _edgeGlowWidth;
    // uniform float _darkenOpacity;
    // uniform float _darkenWidth;
    // uniform float _sideFadeMagnitude;
    
    Material m_Material;

    public bool IsActive() => m_Material != null && _intensity.value > 0f;

    // Do not forget to add this post process in the Custom Post Process Orders list (Project Settings > Graphics > HDRP Global Settings).
    public override CustomPostProcessInjectionPoint injectionPoint => CustomPostProcessInjectionPoint.BeforePostProcess;

    const string kShaderName = "FullScreen/ScannerEffectFullscreen";

    public override void Setup()
    {
        if (Shader.Find(kShaderName) != null)
            m_Material = new Material(Shader.Find(kShaderName));
        else
            Debug.LogError($"Unable to find shader '{kShaderName}'. Post Process Volume ScannerEffectPostProcessVolume is unable to load.");
    }

    public override void Render(CommandBuffer cmd, HDCamera camera, RTHandle source, RTHandle destination)
    {
        if (m_Material == null)
            return;

        m_Material.SetFloat("_intensity", _intensity.value);
        m_Material.SetVector("_scanCenterPos", _scannerCenterPosition.value);
        m_Material.SetVector("_scanDirectionXZ", _scanDirectionXZ.value);
        m_Material.SetFloat("_scanDegrees", _scanDegrees.value);
        m_Material.SetFloat("_scanDistance", _scanDistance.value);
        m_Material.SetColor("_scanLineColor", _scanLineColor.value);
        m_Material.SetColor("_lastScanLineColor", _lastScanLineColor.value);
        m_Material.SetFloat("_scanLineWidth", _scanLineWidth.value);
        m_Material.SetFloat("_scanLineDistBetween", _scanLineDistBetween.value);
        m_Material.SetColor("_edgeGlowColor", _edgeGlowColor.value);
        m_Material.SetColor("_edgeGlowAccentColor", _edgeGlowAccentColor.value);
        m_Material.SetFloat("_edgeGlowWidth", _edgeGlowWidth.value);
        m_Material.SetFloat("_darkenOpacity", _darkenOpacity.value);
        m_Material.SetFloat("_darkenBaseValue", _darkenBaseValue.value);
        m_Material.SetFloat("_darkenWidth", _darkenWidth.value);
        m_Material.SetFloat("_sideFadeMagnitude", _sideFadeMagnitude.value);
        HDUtils.DrawFullScreen(cmd, m_Material, destination, shaderPassId: 0);
    }

    public override void Cleanup()
    {
        CoreUtils.Destroy(m_Material);
    }
}
