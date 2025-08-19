// Custom shader for rainbow cabin effect + distance colorin
Shader "Custom/CabinRainbowDistanceShader"
{
    Properties
    {
        _PlayerPos ("Player Position", Vector) = (0,0,0,0) // pos of the playr, set by script
        _CabinPos ("Cabin Position", Vector) = (0,0,0,0) // pos of the cabbin
        _DanceActive ("Dance Active", Float) = 0 // 0 = off, 1 = on, toggles rainbow road
        _TimeSpeed ("Time Speed", Float) = 256 // how fast the colors anim8
        _RainbowAlpha ("Rainbow Transparency", Range(0,1)) = 0.45 // transparncy for rainbow mode
    }

    SubShader
    {
        Tags { "RenderType"="Opaque" } // unity render type tag
        LOD 100 // lod level, not really used here donmt think

        Pass
        {
            Blend SrcAlpha OneMinusSrcAlpha // enable alpha blending
            CGPROGRAM // start cgpgrm

            #pragma vertex vert // mark vertex functn
            #pragma fragment frag // mark fragment functn
            #include "UnityCG.cginc" // unity cg include helpers

            // struct for input vertex data
            struct appdata
            {
                float4 vertex : POSITION; // vertex position
            };

            // struct for data between vertex and fragment
            struct v2f
            {
                float4 vertex : SV_POSITION; // clip space pos
                float3 worldPos : TEXCOORD0; // world pos for fx calc
            };

            // shader property uniforms
            float3 _PlayerPos; // playr pos passed from script
            float3 _CabinPos; // cabin pos passed from script
            float _DanceActive; // toggle rainbow mode
            float _TimeSpeed; // rainbow anim speed
            float _RainbowAlpha; // rainbow alpha value

            // vertex functn
            v2f vert(appdata v)
            {
                v2f o; // output struct
                o.vertex = UnityObjectToClipPos(v.vertex); // transfrm to clip space
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz; // get world pos
                return o; // send to frag
            }

            // HSV 2 RGB conversion helper
            fixed3 hsv2rgb(fixed3 c)
            {
                fixed4 K = fixed4(1.0, 2.0/3.0, 1.0/3.0, 3.0); // constnts for hsv converz
                fixed3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www); // compute
                return c.z * lerp(K.xxx, saturate(p - K.xxx), c.y); // ret col
            }

            // fragment functn (pixel shader)
            fixed4 frag(v2f i) : SV_Target
            {
                fixed3 finalColor; // color to out
                float alpha = 1.0; // default alpha

                if (_DanceActive > 0.5) // if rainbow dance mode toggled
                {
                    // RainnnBowRoad Type bEat
                    float wave = sin(i.worldPos.x * 0.1 + _Time.y * _TimeSpeed) + // sin wave on x
                                 cos(i.worldPos.z * 0.02 + _Time.y * _TimeSpeed); // cos wave on z
                    float hue = frac(wave * 0.5 + _Time.y * 0.2); // hue from wave and time
                    finalColor = hsv2rgb(fixed3(hue, 1, 1)); // make rainbow color

                    //  transparency
                    alpha = _RainbowAlpha; // use rainbow alpha property
                }
                else // else not dancing, use distance colors
                {
                    // Distance-based bright green 2 dark red
                    float dist = distance(_PlayerPos, _CabinPos); // calc distance
                    float t = saturate(dist / 250); // norm dist to 0-1 (250 maxish)
                    fixed3 closeColor = fixed3(0.0, 1.0, 0.0); // bright green near
                    fixed3 farColor   = fixed3(0.5, 0.0, 0.0); // dark red far
                    finalColor = lerp(closeColor, farColor, t); // mix based on t
                }

                return fixed4(finalColor, alpha); // return color + alpha
            }
            ENDCG // end cgprogram
        }
    }
}
