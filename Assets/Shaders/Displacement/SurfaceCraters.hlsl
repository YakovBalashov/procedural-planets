#pragma once

#define CAVITY_SHAPE_DEGREE 4

struct CraterParameters
{
    float3 position;
    float radius;
    float depth;
    float rimWidth;
    float rimSteepness;
    float strength;
};

StructuredBuffer<CraterParameters> Craters;
int CraterCount;

float GetCavity(float normalizedDistanceFromCenter, float depth)
{
    float parabolaPoint = pow(normalizedDistanceFromCenter, CAVITY_SHAPE_DEGREE) - 1.0;
    return parabolaPoint * depth;
}

float GetRim(float normalizedDistanceFromCenter, float rimWidth, float rimSteepness)
{
    float movedDistance = normalizedDistanceFromCenter - (1.0 + rimWidth);
    return movedDistance * movedDistance * rimSteepness;
}

float EvaluateCrater(float3 position)
{
    float craterValue = 0.0;

    for (int i = 0; i < CraterCount; ++i)
    {
        float normalizedDistanceFromCenter = distance(position, Craters[i].position) / Craters[i].
            radius;

        if (normalizedDistanceFromCenter > 1.0 + Craters[i].rimWidth) continue;

        float currentCraterValue = (normalizedDistanceFromCenter > 1.0)
                                       ? GetRim(normalizedDistanceFromCenter, Craters[i].rimWidth,
                                                Craters[i].rimSteepness)
                                       : GetCavity(normalizedDistanceFromCenter, Craters[i].depth);

        craterValue += currentCraterValue * Craters[i].strength;
    }

    return craterValue;
}
