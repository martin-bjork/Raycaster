using UnityEngine;

namespace RaytracingEngine {

    public class Sphere : MonoBehaviour {

        [SerializeField]
        private Color albedo = new Color(0.8f, 0.8f, 0.8f);
        [SerializeField]
        private Color specular = new Color(0.05f, 0.05f, 0.05f);

        private Color previousAlbedo;
        private Color previousSpecular;

        private SphereData _data;
        public SphereData Data => IsDirty ? _data = CreateData() : _data;

        public bool IsDirty => transform.hasChanged || colorHasChanged;

        private bool colorHasChanged;

        private void OnValidate() {
            colorHasChanged = ColorHasChanged();
        }

        public void SetNotDirty() {
            transform.hasChanged = false;
            previousAlbedo = albedo;
            previousSpecular = specular;
            colorHasChanged = false;
        }

        public void SetValues(Vector3 position, float radius, Color albedo, Color specular) {
            transform.position = position;
            transform.localScale = Vector3.one * radius * 2;
            this.albedo = albedo;
            this.specular = specular;
            Renderer renderer = GetComponent<Renderer>();
            Material material = new Material(renderer.sharedMaterial);
            material.color = albedo != new Color(0, 0, 0) ? albedo : specular;
            renderer.material = material;
            colorHasChanged = ColorHasChanged();
        }

        private bool ColorHasChanged() {
            return albedo != previousAlbedo || specular != previousSpecular;
        }

        private SphereData CreateData() {
            Vector3 scale = transform.localScale;
            float averageScale = (Mathf.Abs(scale.x) + Mathf.Abs(scale.y) + Mathf.Abs(scale.z)) / 3;
            return new SphereData() {
                albedo = albedo,
                specular = specular,
                position = transform.position,
                radius = averageScale / 2
            };
        }

    }

}
