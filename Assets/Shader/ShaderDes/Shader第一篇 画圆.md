### Shader第一篇 画圆

![image-20221027155402949](/Users/zhengzhengxu/Library/Application Support/typora-user-images/image-20221027155402949.png)

**流程**

-  获取uv面板
- 将uv进行偏移 uv是（0-1）从左下角由黑变白，如果想黑色在中心位置则需要将uv偏移（0.5，0.5），得到的就是中间的位置length值接近0然后向外越来越接近1. 比如左下角是0 减去0.5得到-0.5 length等于0.5 他的颜色就偏白色，但是得到最大值也只到0.5所以才有后面的乘以2 归一。
- 将uv除以一个值 达到缩放效果 （除以一个值那么uv值就会越来越大 白色的部分就越来越多）
- length 求取uv长度 得到0-1的值
- 将length取得值乘以2 归一化 因为之前得到的uv是偏移（0.5,0.5）后的结果
- 到这一步 得到的结果是中间黑外边白的结果，圆则是中间白外边黑 则需要翻转 使用oneminus 1 - length值 得到圆
- 使用smoothstep 可以使大于或者小于某个值的部分变为设置的值 ，比如可以使大于0.5的值为1这样圆就变得更加实心 （1就是白色翻转下就是黑色）
  - 将得到的圆乘以一个颜色得到一个带颜色的圆



```c#
Shader "Circle01"
{
	Properties
	{
		_CirlceColor("CirlceColor", Color) = (1,0,0,0)
		_Offset2Scale2("Offset2Scale2", Vector) = (0.5,0.5,1,1)

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

			uniform float4 _CirlceColor;
			uniform float4 _Offset2Scale2;

			
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
				float2 _Vector1 = float2(0,0.5);
				float2 texCoord1 = i.ase_texcoord1.xy ;
				float2 offset = (float2(_Offset2Scale2.x , _Offset2Scale2.y));
				float2 scale = (float2(_Offset2Scale2.z , _Offset2Scale2.w));
                float2 val = ( texCoord1 - offset ) / scale;
				float color = smoothstep( _Vector1.x , _Vector1.y , ( 1.0 - ( length( val ) * 2.0 ) ));
				
				
				finalColor = ( _CirlceColor * color );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
```

