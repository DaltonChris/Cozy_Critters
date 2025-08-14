using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PSX
{
    public class DitheringRenderFeature : ScriptableRendererFeature
    {
        private DitheringPass ditheringPass;

        public override void Create()
        {
            ditheringPass = new DitheringPass(RenderPassEvent.BeforeRenderingPostProcessing);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // Just enqueue the pass, DO NOT access cameraColorTarget here
            renderer.EnqueuePass(ditheringPass);
        }
    }

    public class DitheringPass : ScriptableRenderPass
    {
        private static readonly string shaderPath = "PostEffect/Dithering";
        private static readonly string k_RenderTag = "Render Dithering Effects";
        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int TempTargetId = Shader.PropertyToID("_TempTargetDithering");

        // Shader property IDs
        private static readonly int PatternIndex = Shader.PropertyToID("_PatternIndex");
        private static readonly int DitherThreshold = Shader.PropertyToID("_DitherThreshold");
        private static readonly int DitherStrength = Shader.PropertyToID("_DitherStrength");
        private static readonly int DitherScale = Shader.PropertyToID("_DitherScale");

        private Material ditheringMaterial;
        private Dithering dithering;
        private RenderTargetIdentifier currentTarget;

        public DitheringPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;

            var shader = Shader.Find(shaderPath);
            if (shader == null)
            {
                Debug.LogError($"Shader {shaderPath} not found.");
                return;
            }

            ditheringMaterial = CoreUtils.CreateEngineMaterial(shader);
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (ditheringMaterial == null) return;
            if (!renderingData.cameraData.postProcessEnabled) return;

            var stack = VolumeManager.instance.stack;
            dithering = stack.GetComponent<Dithering>();
            if (dithering == null || !dithering.IsActive()) return;

            // Assign the camera target here, inside Execute
            currentTarget = renderingData.cameraData.renderer.cameraColorTarget;

            var cmd = CommandBufferPool.Get(k_RenderTag);
            Render(cmd, ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }

        private void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;

            int w = cameraData.camera.scaledPixelWidth;
            int h = cameraData.camera.scaledPixelHeight;

            cameraData.camera.depthTextureMode |= DepthTextureMode.Depth;

            // Set shader parameters
            ditheringMaterial.SetInt(PatternIndex, dithering.patternIndex.value);
            ditheringMaterial.SetFloat(DitherThreshold, dithering.ditherThreshold.value);
            ditheringMaterial.SetFloat(DitherStrength, dithering.ditherStrength.value);
            ditheringMaterial.SetFloat(DitherScale, dithering.ditherScale.value);

            int shaderPass = 0;

            cmd.SetGlobalTexture(MainTexId, currentTarget);
            cmd.GetTemporaryRT(TempTargetId, w, h, 0, FilterMode.Point, RenderTextureFormat.Default);
            cmd.Blit(currentTarget, TempTargetId);
            cmd.Blit(TempTargetId, currentTarget, ditheringMaterial, shaderPass);
        }
    }
}
