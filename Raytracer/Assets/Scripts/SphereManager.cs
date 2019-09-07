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

        [SerializeField]
        private bool spawnSpheresIfSceneIsEmpty = true;

        public List<Sphere> Spheres { get; private set; } = new List<Sphere>();

        public bool IsDirty => Spheres.Any(sphere => sphere.IsDirty);

        private void Awake() {

            Sphere[] spheresInScene = FindObjectsOfType<Sphere>();

            if (spheresInScene.Length > 0) {
                Spheres.AddRange(spheresInScene);
            } else if (spawnSpheresIfSceneIsEmpty) {
                SpawnSpheres();
            }

        }

        public void SetNotDirty() {
            foreach (Sphere sphere in Spheres) {
                sphere.SetNotDirty();
            }
        }

        public void SpawnSpheres() {
            SphereSpawner spawner = new SphereSpawner(minimumRadius, maximumRadius, specularFactor,
                                          maximumSpheres, placementRadius, maximumPlacementAttempts,
                                          spherePrefab);
            Spheres = spawner.SpawnSpheres();
        }

    }

}
