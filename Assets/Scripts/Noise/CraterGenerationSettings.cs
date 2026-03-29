using UnityEngine;

namespace ProceduralPlanets.Noise
{
    [System.Serializable]
    public class CraterGenerationSettings
    {
        [field: SerializeField] public Vector2 StrengthRange { get; private set; } = new Vector2(0.05f, 0.3f);
        [field: SerializeField] public Vector2 RadiusRange{ get; private set; } = new Vector2(0.05f, 0.2f);
        [field: SerializeField] public Vector2 DepthRange { get; private set; } = new Vector2(0.9f, 1.1f);
        [field: SerializeField] public Vector2 RimWidthRange { get; private set; } = new Vector2(0.05f, 0.3f);
        [field: SerializeField] public Vector2 RimSteepnessRange { get; private set; } = new Vector2(1f, 3f);
        [field: SerializeField] public Vector2 NumberRange { get; private set; } = new Vector2(10, 50);
    }
    
}