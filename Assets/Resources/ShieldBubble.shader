Shader "PrettyTanks/ShieldBubble"
{
    Properties
    {
        [PerRendererData] _MainTex ("Sprite", 2D) = "white" {}
        _ShieldColor ("Shield color", Color) = (1, 0.55, 0.82, 0.42)
    }
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" "RenderPipeline"="UniversalPipeline" }
        Cull Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha
        Pass
        {
            Tags { "LightMode"="Universal2D" }
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            CBUFFER_START(UnityPerMaterial)
            float4 _ShieldColor;
            CBUFFER_END
            struct appdata { float4 vertex : POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            struct v2f { float4 vertex : SV_POSITION; float2 uv : TEXCOORD0; half4 color : COLOR; };
            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = TransformObjectToHClip(v.vertex.xyz);
                o.uv = v.uv;
                o.color = v.color;
                return o;
            }
            half4 frag(v2f i) : SV_Target
            {
                float2 p = i.uv * 2.0 - 1.0;
                float radius = length(p);
                clip(1.0 - radius);
                // Имитация френеля сферы: центр прозрачный, край заметнее.
                float normalZ = sqrt(saturate(1.0 - radius * radius));
                float rim = pow(1.0 - normalZ, 2.0);
                float edge = smoothstep(0.88, 0.95, radius) * (1.0 - smoothstep(0.98, 1.0, radius));
                float shine = exp(-dot(p - float2(-0.4, 0.55), p - float2(-0.4, 0.55)) * 65.0);
                float alpha = _ShieldColor.a * (0.10 + rim * 0.5 + edge * 0.6 + shine * 0.5);
                return half4(lerp(_ShieldColor.rgb, half3(1, 0.95, 1), shine), alpha);
            }
            ENDHLSL
        }
    }
}
