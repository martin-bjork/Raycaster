using UnityEngine;

namespace RaytracingEngine {

    public class Raytracer : MonoBehaviour {

        private const int GROUP_SIZE_X = 8;
        private const int GROUP_SIZE_Y = 8;

        private const string KERNEL_NAME = "TraceRays";
        private const string RESULT_TEXTURE_NAME = "Result";
        private const string CAMERA_TO_WORLD_NAME = "CameraToWorld";
        private const string CAMERA_INVERSE_NAME = "CameraInverseProjection";

        [SerializeField]
        private ComputeShader raytracingShader = default(ComputeShader);    // TODO: Figure out why default literal isn't allowed. Some wrong setting?

        private RenderTexture renderTexture;
        private Camera renderCamera;

        private int kernelId;
        private int resultTextureId;
        private int cameraToWorldId;
        private int cameraInverseId;

        private int threadGroupsX;
        private int threadGroupsY;

        private void Awake() {
            renderCamera = GetComponent<Camera>();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination) {
            Render(destination);
        }

        private void Render(RenderTexture destination) {

            if (RenderTextureNeedsUpdate()) {
                SetupRenderTexture();
                SetupShader();
            }

            UpdateShaderParameters();
            raytracingShader.Dispatch(kernelId, threadGroupsX, threadGroupsY, 1);
            Graphics.Blit(renderTexture, destination);
        }

        private void UpdateShaderParameters() {
            raytracingShader.SetMatrix(cameraToWorldId, renderCamera.cameraToWorldMatrix);
            raytracingShader.SetMatrix(cameraInverseId, renderCamera.projectionMatrix.inverse);
        }

        private bool RenderTextureNeedsUpdate() {
            return renderTexture == null || renderTexture.width != Screen.width || renderTexture.height != Screen.height;
        }

        private void SetupRenderTexture() {
            renderTexture?.Release();
            renderTexture = new RenderTexture(Screen.width, Screen.height, 0, 
                                              RenderTextureFormat.ARGBFloat, 
                                              RenderTextureReadWrite.Linear);
            renderTexture.enableRandomWrite = true;
            renderTexture.Create();
        }

        private void SetupShader() {

            kernelId = raytracingShader.FindKernel(KERNEL_NAME);
            resultTextureId = Shader.PropertyToID(RESULT_TEXTURE_NAME);
            cameraToWorldId = Shader.PropertyToID(CAMERA_TO_WORLD_NAME);
            cameraInverseId = Shader.PropertyToID(CAMERA_INVERSE_NAME);

            raytracingShader.SetTexture(kernelId, resultTextureId, renderTexture);

            threadGroupsX = Mathf.CeilToInt((float)Screen.width / GROUP_SIZE_X);
            threadGroupsY = Mathf.CeilToInt((float)Screen.height / GROUP_SIZE_Y);
        }

    }

}