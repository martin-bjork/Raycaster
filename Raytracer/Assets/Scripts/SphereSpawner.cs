using System.Collections.Generic;
using UnityEngine;

namespace RaytracingEngine {

    public class SphereSpawner {

        private float minimumRadius;
        private float maximumRadius;
        private float specularFactor;

        private int maximumNumberOfSpheres;
        private float placementRadius;
        private int maximumPlacementAttempts;

        private GameObject spherePrefab;

        private List<Sphere> spheres = new List<Sphere>();

        public SphereSpawner() { }

        public SphereSpawner(float minimumRadius, float maximumRadius, float specularFactor,
                     int maximumNumberOfSpheres, float placementRadius, int maximumPlacementAttempts,
                     GameObject spherePrefab) {
            UpdateValues(minimumRadius, maximumRadius, specularFactor, maximumNumberOfSpheres,
                         placementRadius, maximumPlacementAttempts, spherePrefab);
        }

        public void UpdateValues(float minimumRadius, float maximumRadius, float specularFactor,
                                 int maximumNumberOfSpheres, float placementRadius, int maximumPlacementAttempts,
                                 GameObject spherePrefab) {

            this.minimumRadius = minimumRadius;
            this.maximumRadius = maximumRadius;
            this.specularFactor = specularFactor;
            this.maximumNumberOfSpheres = maximumNumberOfSpheres;
            this.placementRadius = placementRadius;
            this.maximumPlacementAttempts = maximumPlacementAttempts;
            this.spherePrefab = spherePrefab;
        }

        public List<Sphere> SpawnSpheres() {

            GameObject sphereParent = new GameObject("Spheres");

            for (int i = 0; i < maximumNumberOfSpheres; i++) {

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
                bool isMetallic = Random.value < 0;//0.5f;

                Color albedo = isMetallic ? new Color(0, 0, 0) : color;
                Color specular = isMetallic ? color : color * specularFactor;

                GameObject sphereObject = Object.Instantiate(spherePrefab, sphereParent.transform);
                Sphere sphere = sphereObject.GetComponent<Sphere>();
                sphere.SetValues(position, radius, albedo, specular);

                spheres.Add(sphere);
            }

            return spheres;
        }

        public void DestroyAllSpheres() {
            foreach (Sphere sphere in spheres) {
                Object.Destroy(sphere.gameObject);
            }
            spheres.Clear();
        }

        private bool IsValidPosition(Vector3 position, float radius) {
            foreach (Sphere sphere in spheres) {
                if (Vector3.Distance(position, sphere.Data.position) < radius + sphere.Data.radius) {
                    return false;
                }
            }
            return true;
        }

    }

}
