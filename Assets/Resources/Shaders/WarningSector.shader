Shader "taecg/SkillIndicator/Circle" 
{
    Properties 
    {
        [Header(Base)]
        _Color("Color", Color) = (1,1,1,1)
        _MainTex("MainTex", 2D) = "white" {}
        _Intensity("Intensity", float) = 1

        [Header(Sector)]
        [MaterialToggle] _Sector("Sector", Float) = 1
        _Angle ("Angle", Range(0, 360)) = 60
        _Outline ("Outline", Range(0, 5)) = 0.35
        _OutlineAlpha("Outline Alpha",Range(0,1))=0.5
        [MaterialToggle] _Indicator("Indicator", Float) = 1	//Ԥ����Բ�δ�Χ��ͼ

        [Header(Flow)]
        _FlowColor("Flow Color",color) = (1,1,1,1)
        _FlowFade("Fade",range(0,1)) = 1
        _Duration("Duration",range(0,1)) = 0

        [Header(Blend)]
        //��Ϸ�ʽ
        [Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 1
        [Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 1
    }

    SubShader 
    {
        Tags { "Queue"="Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" }

        Pass 
        {
            Blend [_SrcBlend][_DstBlend]
            ZWrite [_ZWrite]
            
            HLSLPROGRAM
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma vertex vert
            #pragma fragment frag  
            #pragma target 2.5
            #pragma multi_compile __ _INDICATOR_ON
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

            struct Attributes 
            {
                float4 positionOS : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct Varyings 
            {
                float4 positionCS : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            CBUFFER_START(UnityPerMaterial)
            half4 _Color;
            half _Intensity;
            float _Angle;
            half _Sector;
            half _Outline;
            half _OutlineAlpha;
            half4 _FlowColor;
            half _FlowFade;
            half _Duration;
            CBUFFER_END
            TEXTURE2D(_MainTex);SAMPLER(sampler_MainTex);float4 _MainTex_ST;

            Varyings vert (Attributes v) 
            {
                Varyings o = (Varyings)0;
                o.uv = v.texcoord;
                o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
                return o;
            }

            float4 frag(Varyings i) : SV_Target 
            {
                half4 col = 0;
                half2 uv = i.uv;
                half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
                mainTex *= _Intensity;

                #if _INDICATOR_ON
                    return mainTex.b * 0.6 * _Color;
                #endif

                float2 centerUV = (uv * 2 - 1);
                float atan2UV = 1-abs(atan2(centerUV.g, centerUV.r)/3.14);

                half sector = lerp(1.0, 1.0 - ceil(atan2UV - _Angle*0.002777778), _Sector);
                half sectorBig = lerp(1.0, 1.0 - ceil(atan2UV - (_Angle+ _Outline) * 0.002777778), _Sector);
                half outline = (sectorBig -sector) * mainTex.g * _OutlineAlpha;

                half needOutline = 1 - step(359, _Angle);
                outline *= needOutline;
                col = mainTex.r * _Color * sector + outline * _Color;

                half flowCircleInner = smoothstep(_Duration - _FlowFade, _Duration, length(centerUV));
                half flowCircleMask = step(length(centerUV), _Duration);
                half4 flow = flowCircleInner * flowCircleMask * _FlowColor *mainTex.g * sector;

                col += flow;
                return col;
            }
            ENDHLSL
        }
    }

    SubShader 
    {
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" "IgnoreProjector" = "True" }

        Pass 
        {
            Blend [_SrcBlend][_DstBlend]
            ZWrite [_ZWrite]
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag  
            #include "UnityCG.cginc"
            #pragma target 2.5
            #pragma multi_compile __ _INDICATOR_ON

            fixed4 _Color;
            sampler2D _MainTex; uniform float4 _MainTex_ST;
            half _Intensity;
            float _Angle;
            fixed _Sector;
            fixed _Outline;
            fixed _OutlineAlpha;

            fixed4 _FlowColor;
            fixed _FlowFade;
            fixed _Duration;

            struct appdata 
            {
                float4 vertex : POSITION;
                float2 texcoord : TEXCOORD0;
            };

            struct v2f 
            {
                float4 pos : SV_POSITION;
                float2 uv : TEXCOORD0;
            };

            v2f vert (appdata v) 
            {
                v2f o;
                UNITY_INITIALIZE_OUTPUT(v2f,o);

                o.uv = v.texcoord;
                o.pos = UnityObjectToClipPos(v.vertex);
                return o;
            }

            float4 frag(v2f i) : SV_Target 
            {
                fixed4 col = 0;
                fixed2 uv = i.uv;
                fixed4 mainTex = tex2D(_MainTex, uv);
                mainTex *= _Intensity;

                #if _INDICATOR_ON
                    return mainTex.b * 0.6 * _Color;
                #endif

                //������
                float2 centerUV = (uv * 2 - 1);
                float atan2UV = 1-abs(atan2(centerUV.g, centerUV.r)/3.14);

                //�����и�
                fixed sector = lerp(1.0, 1.0 - ceil(atan2UV - _Angle*0.002777778), _Sector);
                //��һ����������������ߵ�����
                fixed sectorBig = lerp(1.0, 1.0 - ceil(atan2UV - (_Angle+ _Outline) * 0.002777778), _Sector);
                fixed outline = (sectorBig -sector) * mainTex.g * _OutlineAlpha;

                fixed needOutline = 1 - step(359, _Angle);
                outline *= needOutline;
                col = mainTex.r * _Color * sector + outline * _Color;

                //Բ�ε�����
                fixed flowCircleInner = smoothstep(_Duration - _FlowFade, _Duration, length(centerUV));	//�������Ȧ
                fixed flowCircleMask = step(length(centerUV), _Duration);	//Ӳ������
                fixed4 flow = flowCircleInner * flowCircleMask * _FlowColor *mainTex.g * sector;

                col += flow;
                return col;
            }
            ENDCG
        }
    }
}
