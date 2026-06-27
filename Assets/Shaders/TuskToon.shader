Shader "Tusk/Toon"
{
    Properties
    {
        _BaseMap        ("Base Map", 2D) = "white" {}
        _BaseColor      ("Base Color", Color) = (1,1,1,1)
        _ShadowTint     ("Shadow Tint", Color) = (0.55, 0.55, 0.65, 1)
        _Steps          ("Cel Steps", Range(2,6)) = 3
        _SpecColor      ("Spec Color", Color) = (1,1,1,1)
        _SpecSmoothness ("Spec Smoothness", Range(0,1)) = 0.6
        _SpecThreshold  ("Spec Threshold", Range(0,1)) = 0.5
        _RimColor       ("Rim Color", Color) = (1.0, 0.85, 0.55, 1)
        _RimPower       ("Rim Power", Range(0.5,8)) = 3.0
        _RimIntensity   ("Rim Intensity", Range(0,3)) = 1.0
        _OutlineColor   ("Outline Color", Color) = (0,0,0,1)
        _OutlineWidth   ("Outline Width", Range(0,0.03)) = 0.004
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline"="UniversalPipeline" "Queue"="Geometry" }
        LOD 200

        // === Outline pass (inverted hull) ===
        Pass
        {
            Name "Outline"
            Cull Front
            ZWrite On
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _ShadowTint;
                float  _Steps;
                float4 _SpecColor;
                float  _SpecSmoothness;
                float  _SpecThreshold;
                float4 _RimColor;
                float  _RimPower;
                float  _RimIntensity;
                float4 _OutlineColor;
                float  _OutlineWidth;
            CBUFFER_END

            struct Attributes { float4 positionOS:POSITION; float3 normalOS:NORMAL; };
            struct Varyings   { float4 positionHCS:SV_POSITION; };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                float3 expandedOS = IN.positionOS.xyz + IN.normalOS * _OutlineWidth;
                OUT.positionHCS = TransformObjectToHClip(expandedOS);
                return OUT;
            }
            half4 frag(Varyings IN) : SV_Target { return _OutlineColor; }
            ENDHLSL
        }

        // === Main toon pass ===
        Pass
        {
            Name "Toon"
            Tags { "LightMode"="UniversalForward" }
            Cull Back
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #pragma multi_compile _ _MAIN_LIGHT_SHADOWS _MAIN_LIGHT_SHADOWS_CASCADE
            #pragma multi_compile _ _SHADOWS_SOFT
            #pragma multi_compile_fragment _ _ADDITIONAL_LIGHTS

            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            CBUFFER_START(UnityPerMaterial)
                float4 _BaseMap_ST;
                float4 _BaseColor;
                float4 _ShadowTint;
                float  _Steps;
                float4 _SpecColor;
                float  _SpecSmoothness;
                float  _SpecThreshold;
                float4 _RimColor;
                float  _RimPower;
                float  _RimIntensity;
                float4 _OutlineColor;
                float  _OutlineWidth;
            CBUFFER_END

            TEXTURE2D(_BaseMap); SAMPLER(sampler_BaseMap);

            struct Attributes
            {
                float4 positionOS:POSITION;
                float3 normalOS:NORMAL;
                float2 uv:TEXCOORD0;
            };
            struct Varyings
            {
                float4 positionHCS:SV_POSITION;
                float3 positionWS:TEXCOORD0;
                float3 normalWS:TEXCOORD1;
                float2 uv:TEXCOORD2;
                float4 shadowCoord:TEXCOORD3;
            };

            Varyings vert(Attributes IN)
            {
                Varyings OUT;
                OUT.positionWS = TransformObjectToWorld(IN.positionOS.xyz);
                OUT.positionHCS = TransformWorldToHClip(OUT.positionWS);
                OUT.normalWS = TransformObjectToWorldNormal(IN.normalOS);
                OUT.uv = TRANSFORM_TEX(IN.uv, _BaseMap);
                OUT.shadowCoord = TransformWorldToShadowCoord(OUT.positionWS);
                return OUT;
            }

            half4 frag(Varyings IN) : SV_Target
            {
                Light mainLight = GetMainLight(IN.shadowCoord);
                float3 N = normalize(IN.normalWS);
                float3 L = normalize(mainLight.direction);
                float3 V = normalize(_WorldSpaceCameraPos - IN.positionWS);
                float3 H = normalize(L + V);

                // Albedo
                half4 baseMap = SAMPLE_TEXTURE2D(_BaseMap, sampler_BaseMap, IN.uv);
                half3 albedo = baseMap.rgb * _BaseColor.rgb;

                // Cel-stepped lambert
                float NdotL = saturate(dot(N, L));
                float steps = max(2.0, _Steps);
                float celLight = floor(NdotL * steps) / steps;
                // Shadowed parts get tinted, not just darker
                half3 shadowed = lerp(_ShadowTint.rgb, half3(1,1,1), celLight);
                half3 diffuse = albedo * shadowed * mainLight.color * mainLight.shadowAttenuation;

                // Hard spec highlight (cel-thresholded)
                float NdotH = saturate(dot(N, H));
                float spec = pow(NdotH, _SpecSmoothness * 128.0 + 1.0);
                half3 specular = (spec > _SpecThreshold) ? _SpecColor.rgb : half3(0,0,0);

                // Rim — biggest "intentional AI mesh" trick
                float NdotV = saturate(dot(N, V));
                float rim = pow(1.0 - NdotV, _RimPower);
                half3 rimCol = _RimColor.rgb * rim * _RimIntensity;

                half3 final = diffuse + specular * 0.4 + rimCol;
                return half4(final, baseMap.a);
            }
            ENDHLSL
        }

        // Shadow caster pass — required for receiving shadows in URP
        Pass
        {
            Name "ShadowCaster"
            Tags { "LightMode"="ShadowCaster" }
            ColorMask 0
            HLSLPROGRAM
            #pragma vertex shadowVert
            #pragma fragment shadowFrag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Shadows.hlsl"

            struct A { float4 pos:POSITION; float3 n:NORMAL; };
            struct V { float4 pos:SV_POSITION; };

            float3 _LightDirection;

            V shadowVert(A IN)
            {
                V o;
                float3 wp = TransformObjectToWorld(IN.pos.xyz);
                float3 wn = TransformObjectToWorldNormal(IN.n);
                float4 cp = TransformWorldToHClip(ApplyShadowBias(wp, wn, _LightDirection));
                #if UNITY_REVERSED_Z
                cp.z = min(cp.z, UNITY_NEAR_CLIP_VALUE);
                #else
                cp.z = max(cp.z, UNITY_NEAR_CLIP_VALUE);
                #endif
                o.pos = cp;
                return o;
            }
            half4 shadowFrag(V IN):SV_Target { return 0; }
            ENDHLSL
        }
    }
    Fallback "Universal Render Pipeline/Lit"
}
