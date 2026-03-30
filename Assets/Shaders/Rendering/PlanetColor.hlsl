#pragma once

#include "Assets/Shaders/Displacement/FastNoiseLite.hlsl"

struct BiomeParameters
{
    float3 color;
    int noiseType;
    float frequency;
    int octaves;
    float maskThreshold;
};

StructuredBuffer<BiomeParameters> Biomes;
int BiomeCount;
float4 BaseColor;

void CalculateColor_float(float3 position, out float3 color)
{
    color = BaseColor.rgb;
    
    for (int i = 0; i < BiomeCount; ++i)
    {
        fnl_state state = fnlCreateState();
        state.noise_type = Biomes[i].noiseType;
        state.frequency = Biomes[i].frequency;
        state.fractal_type = 0;
        state.octaves = Biomes[i].octaves;
        
        float noiseValue = fnlGetNoise3D(state, position.x, position.y, position.z);

        if (noiseValue < Biomes[i].maskThreshold) continue;
        color = Biomes[i].color;
    }
}