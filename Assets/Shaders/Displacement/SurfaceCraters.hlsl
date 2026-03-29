#pragma once

struct CraterSettings
{
    float strength;
    float radius;
    float floorDepth;
    float rimSteepness;
    float rimWidth;
    int fnlSeed;
};

StructuredBuffer<CraterSettings> _CraterSettings;

float EvaluateCrater(float3 position)
{
    CraterSettings settings = _CraterSettings[0];
    float3 craterCenter = float3(1, 0, 0);
    float fractionFromCenter = abs(distance(position, craterCenter) / settings.radius);

    if (fractionFromCenter > 1.0) return 0.0;

    float cavity = fractionFromCenter * fractionFromCenter - 1.0;

    return cavity *  settings.strength;  
}