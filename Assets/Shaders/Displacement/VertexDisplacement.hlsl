#pragma once

#include "SurfaceNoise.hlsl"
#include "SurfaceCraters.hlsl"

float PlanetRadius;

float3 ApplyVertexDisplacement(float3 position)
{
    position = normalize(position);

    float elevation = 0.0;

    elevation += EvaluateMultilayeredNoise(position);
    elevation += EvaluateCrater(position);

    float distanceFromCenter = PlanetRadius * (1.0 + elevation);
    
    return position * distanceFromCenter;
}