using System.Collections.Generic;
using ProceduralPlanets.Extensions;
using ProceduralPlanets.Generation;
using ProceduralPlanets.Noise;
using UnityEngine;

namespace ProceduralPlanets.ScriptableObjects.Generation
{
    [CreateAssetMenu(fileName = "RingType", menuName = "Planetary Systems/Generation Types/Ring Type")]
    public class RingType : ScriptableObject
    {
        [field: SerializeField] public Vector2 InnerRadiusRange { get; private set; }
        [field: SerializeField] public Vector2 OuterRadiusRange { get; private set; }
        [field: SerializeField] public int SegmentCount { get; private set; }
        [field: SerializeField] public FnlParameters NoiseParameters { get; private set; }
        [field: SerializeField] public List<Color> Colors { get; private set; }
        [field: SerializeField] public Vector2 NoiseRange { get; private set; } = new Vector2(-1f, 1f);

        public RingParameters GenerateRingParameters(int seed)
        {
            var random = new System.Random(seed);
            var innerRadius = random.Range(InnerRadiusRange);
            var outerRadius = random.Range(OuterRadiusRange);
            var color = Colors[random.Range(0, Colors.Count)];
            NoiseParameters.SetOffset(SystemGenerator.GetOffset(random));
            return new RingParameters(true, innerRadius, outerRadius, SegmentCount, NoiseParameters, color, NoiseRange);
        }
    }
}