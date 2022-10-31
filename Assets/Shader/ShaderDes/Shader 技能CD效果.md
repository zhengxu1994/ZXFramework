### Shader 技能CD效果

![image-20221028101713308](/Users/zhengzhengxu/Library/Application Support/typora-user-images/image-20221028101713308.png)

总计分为三部分

- MainTex 获取技能图标

  - Texture Simple 
  - multiply 圆形遮罩 multiply 圆形角度

- 画圆 作为技能图标的遮罩

  - subtract - length - multiply - oneminus - smoothstep - clip （将大于设置alpha值的阈值部分给裁剪掉）

- 画出圆形角度 0-360度

  - Subtract uv偏移
  - Split 将uv输出成x y值
  - multiply -1 翻转
  - Atan2 获取360-0的角度。这也是为什么上面要乘以-1的原因
  - remap 映射 将-pi 到 pi 映射到0 - 1 这样变成了从白变黑
  - float 得到0-1的滑动条 可以设置进度 oneminus 翻转
  - subtract 映射的值
  - smoothstep 将超过某个值的部分设置为白色 可以将值设置的很小
  - remap 将小于0的部分 映射为0.3 或者其他值 这样cd外部分就不会是全黑

  

```c#
Shader "SkillCD"
{
	Properties
	{
		_Float0("Float 0", Range( 0 , 1)) = 0.5488718
		_Main_Tex("Main_Tex", 2D) = "white" {}
		[HideInInspector] _texcoord( "", 2D ) = "white" {}

	}
	
	SubShader
	{
		
		
		Tags { "RenderType"="Opaque" }
	LOD 100

		CGINCLUDE
		#pragma target 3.0
		ENDCG
		Blend Off
		AlphaToMask Off
		Cull Back
		ColorMask RGBA
		ZWrite On
		ZTest LEqual
		Offset 0 , 0
		
		
		
		Pass
		{
			Name "Unlit"
			Tags { "LightMode"="ForwardBase" }
			CGPROGRAM

			

			#ifndef UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX
			//only defining to not throw compilation error over Unity 5.5
			#define UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input)
			#endif
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_instancing
			#include "UnityCG.cginc"
			

			struct appdata
			{
				float4 vertex : POSITION;
				float4 color : COLOR;
				float4 ase_texcoord : TEXCOORD0;
				UNITY_VERTEX_INPUT_INSTANCE_ID
			};
			
			struct v2f
			{
				float4 vertex : SV_POSITION;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 worldPos : TEXCOORD0;
				#endif
				float4 ase_texcoord1 : TEXCOORD1;
				UNITY_VERTEX_INPUT_INSTANCE_ID
				UNITY_VERTEX_OUTPUT_STEREO
			};

			uniform sampler2D _Main_Tex;
			uniform float4 _Main_Tex_ST;
			uniform float _Float0;

			
			v2f vert ( appdata v )
			{
				v2f o;
				UNITY_SETUP_INSTANCE_ID(v);
				UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
				UNITY_TRANSFER_INSTANCE_ID(v, o);

				o.ase_texcoord1.xy = v.ase_texcoord.xy;
				
				//setting value to unused interpolator channels and avoid initialization warnings
				o.ase_texcoord1.zw = 0;
				float3 vertexValue = float3(0, 0, 0);
				#if ASE_ABSOLUTE_VERTEX_POS
				vertexValue = v.vertex.xyz;
				#endif
				vertexValue = vertexValue;
				#if ASE_ABSOLUTE_VERTEX_POS
				v.vertex.xyz = vertexValue;
				#else
				v.vertex.xyz += vertexValue;
				#endif
				o.vertex = UnityObjectToClipPos(v.vertex);

				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
				#endif
				return o;
			}
			
			fixed4 frag (v2f i ) : SV_Target
			{
				UNITY_SETUP_INSTANCE_ID(i);
				UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
				fixed4 finalColor;
				#ifdef ASE_NEEDS_FRAG_WORLD_POSITION
				float3 WorldPosition = i.worldPos;
				#endif
				float2 uv_Main_Tex = i.ase_texcoord1.xy * _Main_Tex_ST.xy + _Main_Tex_ST.zw;
				float2 texCoord1 = i.ase_texcoord1.xy;
				float2 tex_offset = ( texCoord1 - float2( 0.5,0.5 ) );
				float circle_mask = smoothstep( 0.0 , 0.05 , ( 1.0 - ( length( tex_offset ) * 2.0 ) ));
				clip( circle_mask - 0.5);
				float2 break6 = tex_offset;
				float skillCd = smoothstep( 0.0 , 0.005 , ( (0.0 + (atan2( break6.x , ( break6.y * -1.0 ) ) - -3.16) * (1.0 - 0.0) / (3.15 - -3.16)) - ( 1.0 - _Float0 ) ));
				
				
				finalColor = ( ( tex2D( _Main_Tex, uv_Main_Tex ) * circle_mask ) * (0.5 + (skillCd - 0.0) * (1.0 - 0.5) / (1.0 - 0.0)) );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
```

