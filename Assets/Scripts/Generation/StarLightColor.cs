using UnityEngine;

namespace ProceduralPlanets.Generation
{
    [RequireComponent(typeof(Light))]
    public class StarLightColor : MonoBehaviour
    {
        [SerializeField] private SystemGenerator systemGenerator;
        private Light _starLight;

        private void Awake()
        {
            _starLight = GetComponent<Light>();
        }

        private void Start()
        {
            var starData = systemGenerator.GetBodyByIndex(Vector2.zero).GetBodyData();

            _starLight.color = starData.BaseColor;
        }
    }
}
