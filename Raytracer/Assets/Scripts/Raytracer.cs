using UnityEngine;

namespace RaytracingEngine {

    public class Raytracer : MonoBehaviour {

        private const int GROUP_SIZE_X = 8;
        private const int GROUP_SIZE_Y = 8;

        private const string KERNEL_NAME = "Raytrace";
        private const string RESULT_TEXTURE_NAME = "Result";

        [SerializeField]
        private ComputeShader raytracingShader;

        private RenderTexture renderTexture;

        private int kernelId;
        private int resultTextureId;

        private int threadGroupsX;
        private int threadGroupsY;

        private void OnRenderImage(RenderTexture source, RenderTexture destination) {
            Render(destination);
        }

        private void Render(RenderTexture destination) {

            if (RenderTextureNeedsUpdate()) {
                SetupRenderTexture();
                SetupShader();
            }

            raytracingShader.Dispatch(kernelId, threadGroupsX, threadGroupsY, 1);
            Graphics.Blit(renderTexture, destination);
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

            raytracingShader.SetTexture(kernelId, resultTextureId, renderTexture);

            threadGroupsX = Mathf.CeilToInt((float)Screen.width / GROUP_SIZE_X);
            threadGroupsY = Mathf.CeilToInt((float)Screen.height / GROUP_SIZE_Y);
        }

    }

}