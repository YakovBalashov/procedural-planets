#pragma once

#include "PlanetColor.hlsl"
#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

CBUFFER_START(UnityPerMaterial)
    float4 _RingColor;
    int _RingNoiseType;
    int _RingNoiseOctaves;
    float _RingNoiseFrequency;
CBUFFER_END


void CalculateColor_float(float3 position, out float4 color)
{
    color.rgb = _RingColor.rgb;

    fnl_state state = fnlCreateState();
    state.noise_type = _RingNoiseType;
    state.frequency = _RingNoiseFrequency;
    state.fractal_type = FNL_FRACTAL_FBM;
    state.octaves = _RingNoiseOctaves;

    float distanceFromCenter = length(position);
    float noiseValue = fnlGetNoise3D(state, distanceFromCenter, 0, 0);
    float alpha = InverseLerp(-1, 1, noiseValue);

    color.a = alpha;
}
