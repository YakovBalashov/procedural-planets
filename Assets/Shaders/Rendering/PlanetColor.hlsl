#pragma once

#include "Assets/Shaders/Displacement/FastNoiseLite.hlsl"
#include "Assets/Shaders/Displacement/SurfaceNoise.hlsl"

static const int DefaultBiome = 0;

struct BiomeParameters
{
    int biomeType;
    float3 color;
    int noiseType;
    float frequency;
    int octaves;
    float maskThreshold;
    float blendFactor;
};


StructuredBuffer<BiomeParameters> _Biomes;
int _BiomeCount;
float4 _BaseColor;

float3 ApplyDefaultBiome(float3 position, float3 color, BiomeParameters biome)
{
    fnl_state state = fnlCreateState();
    state.noise_type = biome.noiseType;
    state.frequency = biome.frequency / _PlanetRadius;
    state.fractal_type = FNL_FRACTAL_FBM;
    state.octaves = biome.octaves;

    float noiseValue = fnlGetNoise3D(state, position.x, position.y, position.z);
        
    if (noiseValue < biome.maskThreshold - biome.blendFactor) return color;
    float blend = smoothstep(biome.maskThreshold - biome.blendFactor, biome.maskThreshold, noiseValue);
    return lerp(color, biome.color, blend);
}

void CalculateColor_float(float3 position, out float3 color)
{
    color = _BaseColor.rgb;
    
    for (int i = 0; i < _BiomeCount; ++i)
    {
        [flatten]
        switch (_Biomes[i].biomeType)
        {
            case DefaultBiome:
                color = ApplyDefaultBiome(position, color, _Biomes[i]);
                break;
            default:
                color = ApplyDefaultBiome(position, color, _Biomes[i]);
                break;
        }
    }
}