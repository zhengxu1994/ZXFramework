Shader "taecg/SkillIndicator/Arrow"
{
	Properties
	{
		_Color("Color",color) = (1,1,1,1)
		_MainTex ("Texture", 2D) = "white" {}
		_Intensity("Intensity", float) = 1

		[Header(Flow)]
		_FlowColor("Flow Color",color) = (1,1,1,1)
		_Duration ("Duration",range(0,1)) = 0
		
		[Space]
		[Header(Blend)]
		[Enum(UnityEngine.Rendering.BlendMode)] _SrcBlend("Src Blend Mode", Float) = 1
		[Enum(UnityEngine.Rendering.BlendMode)] _DstBlend("Dst Blend Mode", Float) = 1
	}

	SubShader
	{
		Tags { "Queue" = "Transparent" "RenderType"="Transparent" "RenderPipeline" = "UniversalPipeline" }

		Pass
		{
			Blend [_SrcBlend] [_DstBlend]
			ZWrite Off

			Name "Unlit"
			HLSLPROGRAM
			// Required to compile gles 2.0 with standard srp library
			#pragma prefer_hlslcc gles
			#pragma exclude_renderers d3d11_9x
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog
			#include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
			#include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"

			struct Attributes
			{
				float4 positionOS       : POSITION;
				float2 uv               : TEXCOORD0;
			};

			struct Varyings
			{
				float4 positionCS       : SV_POSITION;
				float2 uv 				: TEXCOORD0;
				float2 uv_mask			: TEXCOORD1;
				float2 uv_flow			: TEXCOORD2;
				float fogCoord      	: TEXCOORD3;
			};

			CBUFFER_START(UnityPerMaterial)
			half4 _Color;
			half _Intensity;
			half4 _FlowColor;
			half _Duration;
			CBUFFER_END
			TEXTURE2D (_MainTex);SAMPLER(sampler_MainTex);float4 _MainTex_ST;
			// #define smp _linear_clampU_mirrorV
			// SAMPLER(smp);

			Varyings vert(Attributes v)
			{
				Varyings o = (Varyings)0;

				o.positionCS = TransformObjectToHClip(v.positionOS.xyz);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				o.uv_mask = v.uv;
				o.uv_flow = float2(v.uv.x, v.uv.y + (1 - _Duration));
				o.fogCoord = ComputeFogFactor(o.positionCS.z);
				return o;
			}

			half4 frag(Varyings i) : SV_Target
			{
				//尾部透明遮罩
				half mask = smoothstep(0, 0.3, i.uv_mask.y);
				half2 uv = i.uv;

				//主纹理
				half4 mainTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, uv);
				half4 mainCol = mainTex.r * mask* _Color * _Intensity;

				//扫光
				half4 flow = 0;
				half4 flowTex = SAMPLE_TEXTURE2D(_MainTex, sampler_MainTex, i.uv_flow);
				flow = flowTex.g * mainTex.b * mask* _FlowColor;

				half4 col = mainCol + flow;
				// col.rgb = MixFog(col.rgb, i.fogCoord);
				return col;
			}
			ENDHLSL
		}
	}
	
	SubShader
	{
		Tags {"Queue" = "Transparent" "RenderType"="Transparent" }
		Blend [_SrcBlend] [_DstBlend]
		ZWrite Off

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex : SV_POSITION;
				float2 uv : TEXCOORD0;
				float2 uv_mask: TEXCOORD1;
				float2 uv_flow: TEXCOORD2;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			fixed4 _Color;
			fixed _Intensity;
			fixed4 _FlowColor;
			fixed _Duration;

			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = v.uv.xy * _MainTex_ST.xy + _MainTex_ST.zw;
				o.uv_mask = v.uv;
				o.uv_flow = float2(v.uv.x, v.uv.y + (1-_Duration));
				return o;
			}

			fixed4 frag (v2f i) : SV_Target
			{
				//尾部透明遮罩
				fixed mask = smoothstep(0, 0.3, i.uv_mask.y);
				half2 uv = i.uv;

				//主纹理
				fixed4 mainTex = tex2D(_MainTex, uv);
				fixed4 mainCol = mainTex.r * mask* _Color * _Intensity;

				//扫光
				fixed4 flow = 0;
				fixed4 flowTex = tex2D(_MainTex, i.uv_flow);
				flow = flowTex.g * mainTex.b * mask* _FlowColor;

				fixed4 col = mainCol + flow;
				return col;
			}
			ENDCG
		}
	}
}
