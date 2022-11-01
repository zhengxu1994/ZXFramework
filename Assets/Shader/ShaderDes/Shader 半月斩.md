### Shader 半月斩

### 实现流程：

- 使用Particle Alpha Blend 类型的Shader 因为这个效果需要使用到粒子。
  - Vertex Color 顶点色
  - Tint Color  自定颜色  这些都是Particle Alpha Blend 类型的Shader 的公用变量.
  - Float 颜色值乘2 ，因为Tint Color 默认值是（0.5,0.5,0.50.5）所以需要乘2 来归一

- 第一个圆 OutSide Circle ，半月斩显示的月牙部分。
- 第二个圆InSide Circle, 半月斩遮罩部分 用于显示出月牙缺失部分 ，将uv偏移使部分圆被遮挡 形成月牙状。 
- 第三圆 Color Circle，用于显示月牙颜色 ，以及月牙渐变色。
  - Color1  小于差值部分的颜色
  - Color2 大于差值部分的颜色
  - Lerp 过渡，从color1 过度到color2 形成渐变的效果



![image-20221101113614418](/Users/zhengzhengxu/Desktop/ZXFramework/Assets/Shader/ShaderDes/Shader 半月斩.assets/image-20221101113614418.png)

```c#
Shader "HalfCut"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		
	}


	Category 
	{
		SubShader
		{
		LOD 0

			Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" "PreviewType"="Plane" }
			Blend SrcAlpha OneMinusSrcAlpha
			ColorMask RGB
			Cull Off
			Lighting Off 
			ZWrite Off
			ZTest LEqual
			
			Pass {
			
				CGPROGRAM
				
				#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
				#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
				#endif
				
				#pragma vertex vert
				#pragma fragment frag
				#pragma target 2.0
				#pragma multi_compile_instancing
				#pragma multi_compile_particles
				#pragma multi_compile_fog
				#define ASE_NEEDS_FRAG_COLOR


				#include "UnityCG.cginc"

				struct appdata_t 
				{
					float4 vertex : POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_VERTEX_INPUT_INSTANCE_ID
					
				};

				struct v2f 
				{
					float4 vertex : SV_POSITION;
					fixed4 color : COLOR;
					float4 texcoord : TEXCOORD0;
					UNITY_FOG_COORDS(1)
					#ifdef SOFTPARTICLES_ON
					float4 projPos : TEXCOORD2;
					#endif
					UNITY_VERTEX_INPUT_INSTANCE_ID
					UNITY_VERTEX_OUTPUT_STEREO
					
				};
				
				
				#if UNITY_VERSION >= 560
				UNITY_DECLARE_DEPTH_TEXTURE( _CameraDepthTexture );
				#else
				uniform sampler2D_float _CameraDepthTexture;
				#endif

				//Don't delete this comment
				// uniform sampler2D_float _CameraDepthTexture;

				uniform sampler2D _MainTex;
				uniform fixed4 _TintColor;
				uniform float4 _MainTex_ST;
				uniform float _InvFade;
				

				v2f vert ( appdata_t v  )
				{
					v2f o;
					UNITY_SETUP_INSTANCE_ID(v);
					UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
					UNITY_TRANSFER_INSTANCE_ID(v, o);
					

					v.vertex.xyz +=  float3( 0, 0, 0 ) ;
					o.vertex = UnityObjectToClipPos(v.vertex);
					#ifdef SOFTPARTICLES_ON
						o.projPos = ComputeScreenPos (o.vertex);
						COMPUTE_EYEDEPTH(o.projPos.z);
					#endif
					o.color = v.color;
					o.texcoord = v.texcoord;
					UNITY_TRANSFER_FOG(o,o.vertex);
					return o;
				}

				fixed4 frag ( v2f i  ) : SV_Target
				{
					UNITY_SETUP_INSTANCE_ID( i );
					UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX( i );

					#ifdef SOFTPARTICLES_ON
						float sceneZ = LinearEyeDepth (SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.projPos)));
						float partZ = i.projPos.z;
						float fade = saturate (_InvFade * (sceneZ-partZ));
						i.color.a *= fade;
					#endif

					float4 color65 = IsGammaSpace() ? float4(0.5637683,0.9716981,0.9134225,1) : float4(0.2778826,0.9368213,0.814235,1);
					float4 color66 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
					float2 texCoord64 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float smoothstepResult62 = smoothstep( 0.0 , 0.3 , ( 1.0 - ( length( ( ( texCoord64 - float2( 0.5,0.5 ) ) / float2( 1,1 ) ) ) * 2.0 ) ));
					float4 lerpResult68 = lerp( color65 , color66 , smoothstepResult62);
					float2 texCoord1 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float smoothstepResult6 = smoothstep( 0.0 , 0.05 , ( 1.0 - ( length( ( ( texCoord1 - float2( 0.5,0.5 ) ) / float2( 1,1 ) ) ) * 2.0 ) ));
					float smoothstepResult24 = smoothstep( 0.0 , 0.05 , ( 1.0 - ( length( ( float2( 0,0 ) / float2( -1,-1 ) ) ) * 2.0 ) ));
					

					fixed4 col = ( i.color * _TintColor * 2.0 * ( lerpResult68 * saturate( ( smoothstepResult6 - smoothstepResult24 ) ) ) );
					UNITY_APPLY_FOG(i.fogCoord, col);
					return col;
				}
				ENDCG 
			}
		}	
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
```

