using UnityEngine;

namespace ProceduralPlanets
{
    public class PrimaryStarLight : MonoBehaviour
    {
        [SerializeField] private Transform target;
        
        private void Update()
        {
            if (!target) return;
            transform.LookAt(target);
        }
    }
}
