// Made with Amplify Shader Editor v1.9.0.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "SkillCD"
{
	Properties
	{
		_Float0("Float 0", Range( 0 , 1)) = 0.2
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
				float2 texCoord1 = i.ase_texcoord1.xy * float2( 1,1 ) + float2( 0,0 );
				float2 temp_output_4_0 = ( texCoord1 - float2( 0.5,0.5 ) );
				float smoothstepResult24 = smoothstep( 0.0 , 0.05 , ( 1.0 - ( length( temp_output_4_0 ) * 2.0 ) ));
				clip( smoothstepResult24 - 0.5);
				float2 break6 = temp_output_4_0;
				float smoothstepResult9 = smoothstep( 0.0 , 0.005 , ( (0.0 + (atan2( break6.x , ( break6.y * -1.0 ) ) - -3.16) * (1.0 - 0.0) / (3.15 - -3.16)) - ( 1.0 - _Float0 ) ));
				
				
				finalColor = ( ( tex2D( _Main_Tex, uv_Main_Tex ) * smoothstepResult24 ) * (0.5 + (smoothstepResult9 - 0.0) * (1.0 - 0.5) / (1.0 - 0.0)) );
				return finalColor;
			}
			ENDCG
		}
	}
	CustomEditor "ASEMaterialInspector"
	
	Fallback Off
}
/*ASEBEGIN
Version=19002
0;45;1440;779;602.5643;460.5079;1.010001;True;True
Node;AmplifyShaderEditor.TextureCoordinatesNode;1;-564.6094,-135.0685;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;4;-518.9093,1.641605;Inherit;True;2;0;FLOAT2;0,0;False;1;FLOAT2;0.5,0.5;False;1;FLOAT2;0
Node;AmplifyShaderEditor.BreakToComponentsNode;6;-311.3492,48.72158;Inherit;False;FLOAT2;1;0;FLOAT2;0,0;False;16;FLOAT;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4;FLOAT;5;FLOAT;6;FLOAT;7;FLOAT;8;FLOAT;9;FLOAT;10;FLOAT;11;FLOAT;12;FLOAT;13;FLOAT;14;FLOAT;15
Node;AmplifyShaderEditor.LengthOpNode;22;-264.25,-233.8618;Inherit;False;1;0;FLOAT2;0,0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;8;-285.3594,174.6605;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;-1;False;1;FLOAT;0
Node;AmplifyShaderEditor.RangedFloatNode;14;-56.34366,87.39358;Inherit;False;Property;_Float0;Float 0;0;0;Create;True;0;0;0;False;0;False;0.2;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.ATan2OpNode;3;-134.7591,-26.31848;Inherit;False;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;25;-79.90595,-344.5266;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;2;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;23;131.89,-381.5818;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.OneMinusNode;15;214.0923,91.03314;Inherit;False;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;7;57.39276,-135.3571;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;-3.16;False;2;FLOAT;3.15;False;3;FLOAT;0;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SmoothstepOpNode;24;358.0904,-395.7619;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleSubtractOpNode;13;263.6077,-21.83435;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.SamplerNode;17;313.3605,-618.3011;Inherit;True;Property;_Main_Tex;Main_Tex;1;0;Create;True;0;0;0;False;0;False;-1;847ba9c94d7cd4ff397751150abbca21;847ba9c94d7cd4ff397751150abbca21;True;0;False;white;Auto;False;Object;-1;Auto;Texture2D;8;0;SAMPLER2D;;False;1;FLOAT2;0,0;False;2;FLOAT;0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT;1;False;6;FLOAT;0;False;7;SAMPLERSTATE;;False;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SmoothstepOpNode;9;276.6744,-208.8851;Inherit;True;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.005;False;1;FLOAT;0
Node;AmplifyShaderEditor.ClipNode;28;487.482,-281.4542;Inherit;False;3;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;26;632.6928,-411.1692;Inherit;False;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TFHCRemapNode;21;482.6805,-74.05306;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;0.5;False;4;FLOAT;1;False;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;20;781.6389,-407.1679;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;1137.682,-249.5797;Float;False;True;-1;2;ASEMaterialInspector;100;3;SkillCD;0770190933193b94aaa3065e307002fa;True;Unlit;0;0;Unlit;2;False;True;0;1;False;;0;False;;0;1;False;;0;False;;True;0;False;;0;False;;False;False;False;False;False;False;False;False;False;True;0;False;;False;True;0;False;;False;True;True;True;True;True;0;False;;False;False;False;False;False;False;False;True;False;255;False;;255;False;;255;False;;7;False;;1;False;;1;False;;1;False;;7;False;;1;False;;1;False;;1;False;;False;True;1;False;;True;3;False;;True;True;0;False;;0;False;;True;1;RenderType=Opaque=RenderType;True;2;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;1;LightMode=ForwardBase;False;False;0;;0;0;Standard;1;Vertex Position,InvertActionOnDeselection;1;0;0;1;True;False;;False;0
WireConnection;4;0;1;0
WireConnection;6;0;4;0
WireConnection;22;0;4;0
WireConnection;8;0;6;1
WireConnection;3;0;6;0
WireConnection;3;1;8;0
WireConnection;25;0;22;0
WireConnection;23;0;25;0
WireConnection;15;0;14;0
WireConnection;7;0;3;0
WireConnection;24;0;23;0
WireConnection;13;0;7;0
WireConnection;13;1;15;0
WireConnection;9;0;13;0
WireConnection;28;0;24;0
WireConnection;28;1;24;0
WireConnection;26;0;17;0
WireConnection;26;1;28;0
WireConnection;21;0;9;0
WireConnection;20;0;26;0
WireConnection;20;1;21;0
WireConnection;0;0;20;0
ASEEND*/
//CHKSM=AA3E6BD20D9D2DEF285DD2C8CC4E3BE3C9831E4F