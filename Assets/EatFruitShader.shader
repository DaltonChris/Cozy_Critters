Shader "Custom/EatFruitShader_Transparent"
{
    Properties
    {
        _MainTex("Texture", 2D) = "white" {}
        _TimeSinceEat("Time Since Eat", Float) = 0
        _DistortionStrength("Distortion Strength", Float) = 0.2
        _Alpha("Overlay Alpha", Range(0,1)) = 0.3
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float _TimeSinceEat;
            float _DistortionStrength;
            float _Alpha;

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float2 ripple(float2 uv, float time)
            {
                float2 center = float2(0.5, 0.5);
                float2 diff = uv - center;
                float dist = length(diff);
                float ripple = sin(dist * 20 - time * 10) * 0.03;
                return uv + normalize(diff) * ripple * _DistortionStrength;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 uv = ripple(i.uv, _TimeSinceEat);

                // RGB shift
                float shift = sin(_TimeSinceEat * 10) * 0.02;
                float r = tex2D(_MainTex, uv + float2(shift, 0)).r;
                float g = tex2D(_MainTex, uv).g;
                float b = tex2D(_MainTex, uv - float2(shift, 0)).b;

                fixed4 col = fixed4(r, g, b, _Alpha);
                return col;
            }
            ENDCG
        }
    }
}
