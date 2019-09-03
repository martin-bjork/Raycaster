﻿using System.Linq;
using UnityEngine;

namespace RaytracingEngine {

    public class Raytracer : MonoBehaviour {

        private const int GROUP_SIZE_X = 8;
        private const int GROUP_SIZE_Y = 8;

        private const string ADD_SHADER_NAME = "Hidden/AddShader";
        private const string SAMPLE_NAME = "_Sample";

        private const string KERNEL_NAME = "TraceRays";
        private const string RESULT_TEXTURE_NAME = "Result";
        private const string CAMERA_TO_WORLD_NAME = "CameraToWorld";
        private const string CAMERA_INVERSE_NAME = "CameraInverseProjection";
        private const string SKYBOX_NAME = "SkyboxTexture";
        private const string PIXEL_OFFSET_NAME = "PixelOffset";
        private const string LIGHT_NAME = "DirectionalLight";
        private const string SPHERES_NAME = "Spheres";

        [SerializeField]
        private ComputeShader raytracingShader = default;

        [SerializeField]
        private Texture skyboxTexture = default;

        [SerializeField]
        private Light directionalLigth = default;

        [SerializeField]
        private SphereManager sphereManager = default;

        private RenderTexture renderTexture;
        private Camera renderCamera;

        private int kernelId;
        private int resultTextureId;
        private int cameraToWorldId;
        private int cameraInverseId;
        private int skyboxTextureId;
        private int pixelOffsetId;
        private int lightId;
        private int spheresId;

        private ComputeBuffer sphereBuffer;
        private int sphereBufferStride = sizeof(float) * (3 + 1 + 4 + 4);   // Size of SphereData

        private int threadGroupsX;
        private int threadGroupsY;

        private int currentSample = 0;
        private Material addMaterial;
        private int sampleId;

        private void Awake() {
            renderCamera = GetComponent<Camera>();
            SetupShader();
            SetupRenderTexture();
            SetupComputeShader();
            SetupComputeBuffer();
        }

        private void OnRenderImage(RenderTexture source, RenderTexture destination) {
            Render(destination);
        }

        private void Update() {
            bool objectsHaveChanged = sphereManager?.IsDirty ?? false;
            if (objectsHaveChanged) {
                SetupComputeBuffer();
                sphereManager.SetNotDirty();
            }
            if (objectsHaveChanged || transform.hasChanged || directionalLigth.transform.hasChanged) {
                currentSample = 0;
                transform.hasChanged = false;
                directionalLigth.transform.hasChanged = false;
            }
        }

        private void OnDestroy() {
            DisposeComputeBuffer();
        }

        private void Render(RenderTexture destination) {

            if (RenderTextureNeedsUpdate()) {
                SetupRenderTexture();
                SetupComputeShader();
                currentSample = 0;
            }

            UpdateComputeShaderParameters();
            UpdateShaderParameters();
            raytracingShader.Dispatch(kernelId, threadGroupsX, threadGroupsY, 1);
            Graphics.Blit(renderTexture, destination, addMaterial);
        }

        private void UpdateComputeShaderParameters() {
            Vector2 pixelOffset = new Vector2(Random.value, Random.value);
            raytracingShader.SetVector(pixelOffsetId, pixelOffset);
            raytracingShader.SetMatrix(cameraToWorldId, renderCamera.cameraToWorldMatrix);
            raytracingShader.SetMatrix(cameraInverseId, renderCamera.projectionMatrix.inverse);
            Vector3 lightDirection = directionalLigth.transform.forward;
            Vector4 lightData = new Vector4(lightDirection.x, lightDirection.y, lightDirection.z, directionalLigth.intensity);
            raytracingShader.SetVector(lightId, lightData);
        }

        private void UpdateShaderParameters() {
            addMaterial.SetInt(sampleId, currentSample);
            currentSample++;
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

        private void SetupComputeShader() {

            kernelId = raytracingShader.FindKernel(KERNEL_NAME);
            resultTextureId = Shader.PropertyToID(RESULT_TEXTURE_NAME);
            cameraToWorldId = Shader.PropertyToID(CAMERA_TO_WORLD_NAME);
            cameraInverseId = Shader.PropertyToID(CAMERA_INVERSE_NAME);
            skyboxTextureId = Shader.PropertyToID(SKYBOX_NAME);
            pixelOffsetId = Shader.PropertyToID(PIXEL_OFFSET_NAME);
            lightId = Shader.PropertyToID(LIGHT_NAME);
            spheresId = Shader.PropertyToID(SPHERES_NAME);

            raytracingShader.SetTexture(kernelId, resultTextureId, renderTexture);
            raytracingShader.SetTexture(kernelId, skyboxTextureId, skyboxTexture);

            threadGroupsX = Mathf.CeilToInt((float)Screen.width / GROUP_SIZE_X);
            threadGroupsY = Mathf.CeilToInt((float)Screen.height / GROUP_SIZE_Y);
        }

        private void SetupComputeBuffer() {
            DisposeComputeBuffer();
            sphereBuffer = new ComputeBuffer(sphereManager.Spheres.Count, sphereBufferStride);
            sphereBuffer.SetData(sphereManager.Spheres.Select(sphere => sphere.Data).ToArray());
            raytracingShader.SetBuffer(kernelId, spheresId, sphereBuffer);
        }

        private void DisposeComputeBuffer() {
            sphereBuffer?.Dispose();
        }

        private void SetupShader() {
            addMaterial = new Material(Shader.Find(ADD_SHADER_NAME));
            sampleId = Shader.PropertyToID(SAMPLE_NAME);
        }

    }

}