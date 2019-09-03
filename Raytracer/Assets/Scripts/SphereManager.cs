using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace RaytracingEngine {

    public class SphereManager : MonoBehaviour {

        [SerializeField]
        private float minimumRadius = 0.3f;
        [SerializeField]
        private float maximumRadius = 1.2f;
        [SerializeField]
        private float specularFactor = 0.04f;

        [SerializeField]
        private int maximumSpheres = 100;
        [SerializeField]
        private float placementRadius = 10;
        [SerializeField]
        private int maximumPlacementAttempts = 10;

        [SerializeField]
        private GameObject spherePrefab = default;

        public List<Sphere> Spheres { get; private set; } = new List<Sphere>();

        public bool IsDirty => Spheres.Any(sphere => sphere.IsDirty);

        private void OnEnable() {
            SpawnSpheres();
        }

        public void SetNotDirty() {
            foreach (Sphere sphere in Spheres) {
                sphere.SetNotDirty();
            }
        }

        private void SpawnSpheres() {

            DestroyAllSpheres();

            for (int i = 0; i < maximumSpheres; i++) {

                bool foundValidPosition = false;
                float radius = Random.Range(minimumRadius, maximumRadius);
                Vector3 position = Vector3.zero;

                for (int j = 0; j < maximumPlacementAttempts; j++) {

                    Vector2 position2D = Random.insideUnitCircle * placementRadius;
                    position = new Vector3(position2D.x, radius, position2D.y);

                    foundValidPosition = IsValidPosition(position, radius);

                    if (foundValidPosition) {
                        break;
                    }
                }

                if (!foundValidPosition) {
                    continue;
                }

                Color color = Random.ColorHSV();
                bool isMetallic = Random.value < 0.5f;

                Color albedo = isMetallic ? new Color(0, 0, 0) : color;
                Color specular = isMetallic ? color : color * specularFactor;

                GameObject sphereObject = Instantiate(spherePrefab);
                Sphere sphere = sphereObject.GetComponent<Sphere>();
                sphere.SetValues(position, radius, albedo, specular);

                Spheres.Add(sphere);
            }

        }

        private void DestroyAllSpheres() {
            foreach (Sphere sphere in Spheres) {
                Destroy(sphere.gameObject);
            }
            Spheres.Clear();
        }

        private bool IsValidPosition(Vector3 position, float radius) {
            foreach (Sphere sphere in Spheres) {
                if (Vector3.Distance(position, sphere.Data.position) < radius + sphere.Data.radius) {
                    return false;
                }
            }
            return true;
        }

    }

}
