Shader "Custom/EatFruitShader_Transparent"
{
    // Define shader propperties, for in inspector
    Properties
    {
        _MainTex("Texture", 2D) = "white" {} // Main textture, defalt is white
        _TimeSinceEat("Time Since Eat", Float) = 0 // Float value, time passed since "eat" triggred
        _DistortionStrength("Distortion Strength", Float) = 0.2 // Controls how storng the ripple distortions is
        _Alpha("Overlay Alpha", Range(0,1)) = 0.3 // Alpha value, control transparancy (0 = invisible, 1 = fully visible)
    }
    SubShader
    {
        // Set this shader as transparent, so Unity draw it in the transparent queue
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha // Alpha blending, uses source * alpha + background * (1 - alpha)
        ZWrite Off // Disable depth write so overlay wont block othr objects

        Pass
        {
            CGPROGRAM // Begin the GPU shader programm
            #pragma vertex vert // Tells Unity "vert" is the vertex shader func
            #pragma fragment frag // Tells Unity "frag" is the fragment (pixel) shader func
            #include "UnityCG.cginc" // Include Unity's common shader helpers

            sampler2D _MainTex; // The main texutre, passed into shader
            float4 _MainTex_ST; // Used for scaling/tiling uv coords
            float _TimeSinceEat; // Timer to animate ripple efffect
            float _DistortionStrength; // How storng the displacement warpple is
            float _Alpha; // Transparancy factor

            // appdata = input struct for vertex shader
            struct appdata
            {
                float4 vertex : POSITION; // Vertex position in object spcae
                float2 uv : TEXCOORD0; // Texture coordinates
            };

            // v2f = data we send from vertex shader to fragment shader
            struct v2f
            {
                float2 uv : TEXCOORD0; // Pass uv coords
                float4 vertex : SV_POSITION; // Screen spcae position
            };

            // The vertex shader = runs once per vertex
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex); // Convert obj pos to screen clip pos
                o.uv = TRANSFORM_TEX(v.uv, _MainTex); // Apply scaling/offset to uv coords
                return o; // Send results to fragment shader
            }

            // Ripple function that displases UVs around center
            float2 ripple(float2 uv, float time)
            {
                float2 center = float2(0.5, 0.5); // Set center point at middle of texture
                float2 diff = uv - center; // Diffrence from center to uv coord
                float dist = length(diff); // How far uv is from center
                float ripple = sin(dist * 20 - time * 10) * 0.03; // Create sine wavve based ripple pattern
                return uv + normalize(diff) * ripple * _DistortionStrength; // Push uv outward/inward to create ripple
            }

            // The fragment shader = runs once per pixel
            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = ripple(i.uv, _TimeSinceEat); // Apply ripple distortion to uv

                // RGB channel shifting effect for glitchy look
                float shift = sin(_TimeSinceEat * 10) * 0.02; // Animated horizontal shift
                float r = tex2D(_MainTex, uv + float2(shift, 0)).r; // Sample red from slightly shifted uv
                float g = tex2D(_MainTex, uv).g; // Sample green normally
                float b = tex2D(_MainTex, uv - float2(shift, 0)).b; // Sample blue from shifted opposite side

                fixed4 col = fixed4(r, g, b, _Alpha); // Combine RGB + alpha into final color
                return col; // Return pixel color to screen
            }
            ENDCG // End shader code block
        }
    }
}
