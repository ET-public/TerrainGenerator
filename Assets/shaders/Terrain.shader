Shader "Custom/Terrain"
{
    Properties
    {
        _RockEdge ("Rock lower limit", float) = 0.6
        _GrassEdge ("Grass lower limit", float) = 0.052
        _MainTex ("GrassTex", 2D) = "white" {}
        _RockTex ("RockTex", 2D) = "white" {}
        _SandTex ("SandTex", 2D) = "white" {}
        _NoiseTEx ("NoiseTex", 2D) = "white" {}
        _scaleKam ("float a", float) = 0.01
        _scaleTra ("float b", float) = 0.01
        _scaleSand ("Sand scale", float) = 0.01
        [Normal]_NormalA ("Trava normal", 2D) = "bumb" {}
        _NormalStrengthA ("Grass Normal", float) = 1
        [Normal]_NormalB ("Trava normal", 2D) = "bumb" {}
        _NormalStrengthB ("Grass Normal", float) = 1
        [Normal]_NormalSand ("Sand normal", 2D) = "bumb" {}
        _NormalStrengthSand ("Sand Normal", float) = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        // Physically based Standard lighting model, and enable shadows on all light types
        #pragma surface surf Standard fullforwardshadows

        // Use shader model 3.0 target, to get nicer looking lighting
        #pragma target 3.0


        struct Input
        {
            float3 worldPos;
            float2 uv_MainTex : TEXCOORD1;
            float2 uv_RockTex : TEXCOORD1;
            float2 uv_SandTex : TEXCOORD1;
        };

        float minHeight;
        float maxHeight;
        float inverseLerp(float a, float b, float value){
            return saturate((value-a)/(b-a));
        }
        // Add instancing support for this shader. You need to check 'Enable Instancing' on materials that use the shader.
        // See https://docs.unity3d.com/Manual/GPUInstancing.html for more information about instancing.
        // #pragma instancing_options assumeuniformscaling
        UNITY_INSTANCING_BUFFER_START(Props)
            // put more per-instance properties here
        UNITY_INSTANCING_BUFFER_END(Props)
        sampler2D _MainTex;
        sampler2D _RockTex;
        sampler2D _SandTex;
        sampler2D _NormalA;
        sampler2D _NormalB;
        sampler2D _NormalSand;
        float _scaleKam;
        float _scaleTra;
        float _scaleSand;
        float _NormalStrengthA;
        float _NormalStrengthB;
        float _NormalStrengthSand;
        float _RockEdge;
        float _GrassEdge;
        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float heightPercent = inverseLerp(minHeight, maxHeight, IN.worldPos.y);
            float temp = 1/(_GrassEdge - 0.048);
            float lerpNumRock = 5*(_RockEdge-heightPercent);
            float lerpNumSand = temp*(_GrassEdge-heightPercent);
            fixed4 tex1 = tex2D(_MainTex, IN.uv_MainTex*_scaleKam);
            fixed4 tex2 = tex2D(_RockTex, IN.uv_RockTex*_scaleTra);
            fixed4 sandTex = tex2D(_SandTex, IN.uv_SandTex*_scaleSand);
            o.Albedo = tex1 * (floor(-1*(heightPercent - _GrassEdge)*(heightPercent-0.4))+1) + tex2 * (floor((heightPercent-_RockEdge))+1) + sandTex * (floor((-heightPercent+0.048))+1);
            o.Albedo += (floor(-1*(heightPercent - 0.4)*(heightPercent-_RockEdge)) + 1) * lerp(tex2,tex1, lerpNumRock)
            + (floor(-1*(heightPercent - 0.048)*(heightPercent-_GrassEdge)) + 1) * lerp(tex1, sandTex, lerpNumSand);
            float3 normalA = UnpackNormalWithScale(tex2D(_NormalA, IN.uv_MainTex*_scaleKam), _NormalStrengthA);
            float3 normalB = UnpackNormalWithScale(tex2D(_NormalB, IN.uv_MainTex*_scaleTra), _NormalStrengthB);
            float3 normalSand = UnpackNormalWithScale(tex2D(_NormalSand, IN.uv_SandTex*_scaleSand), _NormalStrengthSand);
            o.Normal = normalA * (floor(-1*(heightPercent - _GrassEdge)*(heightPercent-0.4))+1) + normalB * (floor((heightPercent-_RockEdge))+1) + normalSand * (floor((-heightPercent+0.048))+1) 
            + (floor(-1*(heightPercent - 0.4)*(heightPercent-_RockEdge)) + 1) * lerp(normalB,normalA, lerpNumRock) 
            + (floor(-1*(heightPercent - 0.048)*(heightPercent-_GrassEdge)) + 1) * lerp(normalA,normalSand, lerpNumSand);
        }
        ENDCG
    }
    FallBack "Diffuse"
}
