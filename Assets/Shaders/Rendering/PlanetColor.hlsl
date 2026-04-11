#pragma once

#include "Assets/Shaders/Displacement/FastNoiseLite.hlsl"
#include "Assets/Shaders/Displacement/SurfaceNoise.hlsl"

struct BiomeParameters
{
    float3 color;
    int noiseType;
    float frequency;
    int octaves;
    float maskThreshold;
    float blendFactor;
    float2 heightRange;
};

StructuredBuffer<BiomeParameters> _Biomes;
int _BiomeCount;
float4 _BaseColor;

float _LowestVertexHeight;
float _HighestVertexHeight;

float InverseLerp(float lowerBound, float upperBound, float value)
{
    return (value - lowerBound) / (upperBound - lowerBound);
}

float GetBaseBlend(float3 position, BiomeParameters biome)
{
    fnl_state state = fnlCreateState();
    state.noise_type = biome.noiseType;
    state.frequency = biome.frequency / _PlanetRadius;
    state.fractal_type = FNL_FRACTAL_FBM;
    state.octaves = biome.octaves;

    float noiseValue = fnlGetNoise3D(state, position.x, position.y, position.z);
        
    if (noiseValue < biome.maskThreshold - biome.blendFactor) return 0.0;
    float blend = smoothstep(biome.maskThreshold - biome.blendFactor, biome.maskThreshold, noiseValue);
    return blend;
}

float GetHeightBlend(float3 position, BiomeParameters biome)
{
    float2 heightRange = biome.heightRange;
    
    float height = InverseLerp(_LowestVertexHeight, _HighestVertexHeight, length(position));

    float bottomFade = smoothstep(heightRange.x - biome.blendFactor, heightRange.x, height);

    float topFade = 1.0 - smoothstep(heightRange.y, heightRange.y + biome.blendFactor, height);

    return bottomFade * topFade;
}

float GetBiomeBlend(float3 position, BiomeParameters biome)
{
    float baseBlend = GetBaseBlend(position, biome);
    float heightBlend = GetHeightBlend(position, biome);
    return baseBlend * heightBlend;
}

void CalculateColor_float(float3 position, out float3 color)
{
    color = _BaseColor.rgb;
    
    for (int i = 0; i < _BiomeCount; ++i)
    {
        float blend = GetBiomeBlend(position, _Biomes[i]);
        color = lerp(color, _Biomes[i].color, blend);
    }
}