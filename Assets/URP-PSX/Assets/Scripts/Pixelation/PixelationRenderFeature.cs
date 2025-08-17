using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

namespace PSX
{
    public class PixelationRenderFeature : ScriptableRendererFeature
    {
        private PixelationPass pixelationPass;

        public override void Create()
        {
            pixelationPass = new PixelationPass(RenderPassEvent.BeforeRenderingPostProcessing);
        }

        public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
        {
            // Just enqueue the pass
            renderer.EnqueuePass(pixelationPass);
        }

    }

    public class PixelationPass : ScriptableRenderPass
    {
        private static readonly string shaderPath = "PostEffect/Pixelation";
        private static readonly string k_RenderTag = "Render Pixelation Effects";

        private static readonly int MainTexId = Shader.PropertyToID("_MainTex");
        private static readonly int TempTargetId = Shader.PropertyToID("_TempTargetPixelation");

        // Shader property IDs
        private static readonly int WidthPixelation = Shader.PropertyToID("_WidthPixelation");
        private static readonly int HeightPixelation = Shader.PropertyToID("_HeightPixelation");
        private static readonly int ColorPrecison = Shader.PropertyToID("_ColorPrecision");

        private Material pixelationMaterial;
        private RenderTargetIdentifier currentTarget;
        private Pixelation pixelation;

        public PixelationPass(RenderPassEvent evt)
        {
            renderPassEvent = evt;

            var shader = Shader.Find(shaderPath);
            if (shader == null)
            {
                Debug.LogError($"Shader {shaderPath} not found.");
                return;
            }

            pixelationMaterial = CoreUtils.CreateEngineMaterial(shader);
        }

        public void Setup(RenderTargetIdentifier currentTarget)
        {
            this.currentTarget = currentTarget;
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            if (pixelationMaterial == null) return;
            if (!renderingData.cameraData.postProcessEnabled) return;

            var stack = VolumeManager.instance.stack;
            pixelation = stack.GetComponent<Pixelation>();
            if (pixelation == null || !pixelation.IsActive()) return;

            // Set currentTarget here instead of in AddRenderPasses
            currentTarget = renderingData.cameraData.renderer.cameraColorTarget;

            var cmd = CommandBufferPool.Get(k_RenderTag);
            Render(cmd, ref renderingData);
            context.ExecuteCommandBuffer(cmd);
            CommandBufferPool.Release(cmd);
        }


        private void Render(CommandBuffer cmd, ref RenderingData renderingData)
        {
            ref var cameraData = ref renderingData.cameraData;
            var source = currentTarget;
            int destination = TempTargetId;

            int w = cameraData.camera.scaledPixelWidth;
            int h = cameraData.camera.scaledPixelHeight;

            cameraData.camera.depthTextureMode |= DepthTextureMode.Depth;

            // Set shader properties
            pixelationMaterial.SetFloat(WidthPixelation, pixelation.widthPixelation.value);
            pixelationMaterial.SetFloat(HeightPixelation, pixelation.heightPixelation.value);
            pixelationMaterial.SetFloat(ColorPrecison, pixelation.colorPrecision.value);

            int shaderPass = 0;

            cmd.SetGlobalTexture(MainTexId, source);
            cmd.GetTemporaryRT(destination, w, h, 0, FilterMode.Point, RenderTextureFormat.Default);
            cmd.Blit(source, destination);
            cmd.Blit(destination, source, pixelationMaterial, shaderPass);
        }
    }
}
