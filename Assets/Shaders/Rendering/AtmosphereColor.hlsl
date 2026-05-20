#pragma once

#include "PlanetColor.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _AtmosphereColor;
    int _AtmosphereNoiseType;
    float _AtmosphereNoiseFrequency;
    int _AtmosphereNoiseOctaves;
    int _AtmosphereNoiseWarpType;
    float _AtmosphereNoiseWarpAmplitude;
    float2 _AtmosphereNoiseRange;
    float _AtmosphereMinAlpha;
    float3 _AtmosphereOffset;
CBUFFER_END

void CalculateColor_float(float3 position, out float4 color)
{
    color.rgb = _AtmosphereColor.rgb;

    fnl_state state = fnlCreateState();
    state.noise_type = _AtmosphereNoiseType;
    state.frequency = _AtmosphereNoiseFrequency;
    state.fractal_type = FNL_FRACTAL_FBM;
    state.octaves = _AtmosphereNoiseOctaves;
    state.domain_warp_type = _AtmosphereNoiseWarpType;
    state.domain_warp_amp = _AtmosphereNoiseWarpAmplitude;

    position += _AtmosphereOffset;
    
    if (_AtmosphereNoiseWarpAmplitude > 0) fnlDomainWarp3D(state, position.x, position.y, position.z);

    float noiseValue = fnlGetNoise3D(state, position.x, position.y, position.z);
    float alpha = InverseLerp(_AtmosphereNoiseRange.x, _AtmosphereNoiseRange.y, noiseValue);
    color.a = max(alpha, _AtmosphereMinAlpha);
}
