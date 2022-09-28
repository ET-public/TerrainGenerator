Shader "Custom/StylizedWater"
{
    Properties
    {
        [Header(Colors)]
        [HDR]_Color ("Color", Color) = (1,1,1,1)
        [HDR]_FogColor("Fog Color", Color) = (1,1,1,1)
 
        [Header(Thresholds)]
        _FogThreshold("Fog threshold", float) = 0
 
        [Header(Normal maps)]
        [Normal]_NormalA("Normal A", 2D) = "bump" {} 
        [Normal]_NormalB("Normal B", 2D) = "bump" {}
        _NormalStrength("Normal strength", float) = 1
        _NormalSpeed("Normal panning speeds", Vector) = (0,0,0,0)

 
        [Header(Misc)]
        _Glossiness ("Smoothness", Range(0,1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        LOD 200
 
        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:premul
 
        #pragma target 3.0
 
 
        struct Input
        {
            float4 screenPos;
            float3 worldPos;
            float3 viewDir;
        };
 
        fixed4 _Color;
        fixed4 _FogColor;
        float _FogThreshold;
 
        sampler2D _NormalA;
        sampler2D _NormalB;
        float4 _NormalA_ST;
        float4 _NormalB_ST;
        float _NormalStrength;
        float4 _NormalSpeed;
 
        half _Glossiness;
 
        sampler2D _CameraDepthTexture;
 
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
 
 
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
 
            float depth = tex2Dproj(_CameraDepthTexture, UNITY_PROJ_COORD(IN.screenPos));
            depth = LinearEyeDepth(depth);
 
            float fogDiff = saturate((depth - IN.screenPos.w) / _FogThreshold);             
            fixed4 c = lerp(_Color, _FogColor, fogDiff);
            o.Albedo = c.rgb;
 
            
            float3 normalA = UnpackNormalWithScale(tex2D(_NormalA, IN.worldPos.xz * _NormalA_ST.xy + _Time.y * _NormalSpeed.xy), _NormalStrength);
            float3 normalB = UnpackNormalWithScale(tex2D(_NormalB, IN.worldPos.xz * _NormalB_ST.xy + _Time.y * _NormalSpeed.zw), _NormalStrength);
            o.Normal = normalA + normalB;
            o.Smoothness = _Glossiness;
            o.Alpha = lerp(0.05, _FogColor.a, fogDiff);
        }
        ENDCG
    }
    FallBack "Diffuse"
}