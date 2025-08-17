Shader "Custom/CabinRainbowDistanceShader"
{
    Properties
    {
        _PlayerPos ("Player Position", Vector) = (0,0,0,0)
        _CabinPos ("Cabin Position", Vector) = (0,0,0,0)
        _DanceActive ("Dance Active", Float) = 0 // 0 = off, 1 = on
        _TimeSpeed ("Time Speed", Float) = 256
        _RainbowAlpha ("Rainbow Transparency", Range(0,1)) = 0.45
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // allow transparency
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD0;
            };

            float3 _PlayerPos;
            float3 _CabinPos;
            float _DanceActive;
            float _TimeSpeed;
            float _RainbowAlpha;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                return o;
            }

            // HSV 2 RGB conversion
            fixed3 hsv2rgb(fixed3 c)
            {
                fixed4 K = fixed4(1.0, 2.0/3.0, 1.0/3.0, 3.0);
                fixed3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y);
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 finalColor;
                float alpha = 1.0;

                if (_DanceActive > 0.5)
                {
                    // RainnnBowRoad Type bEat
                    float wave = sin(i.worldPos.x * 0.1 + _Time.y * _TimeSpeed) +
                                 cos(i.worldPos.z * 0.02 + _Time.y * _TimeSpeed);
                    float hue = frac(wave * 0.5 + _Time.y * 0.2);
                    finalColor = hsv2rgb(fixed3(hue, 1, 1));

                    //  transparency
                    alpha = _RainbowAlpha;
                }
                else
                {
                    // Distance-based bright green 2 dark red
                    float dist = distance(_PlayerPos, _CabinPos);
                    float t = saturate(dist / 250); // Adjusted based on cabin/player pos given by CabinSHaderController
                    fixed3 closeColor = fixed3(0.0, 1.0, 0.0); // bright green
                    fixed3 farColor   = fixed3(0.5, 0.0, 0.0); // dark red
                    finalColor = lerp(closeColor, farColor, t);
                }

                return fixed4(finalColor, alpha);
            }
            ENDCG
        }
    }
}
