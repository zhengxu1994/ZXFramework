// Made with Amplify Shader Editor v1.9.0.2
// Available at the Unity Asset Store - http://u3d.as/y3X 
Shader "HalfCut"
{
	Properties
	{
		_TintColor ("Tint Color", Color) = (0.5,0.5,0.5,0.5)
		_MainTex ("Particle Texture", 2D) = "white" {}
		_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0
		_Progress("Progress", Range( 0 , 1)) = 0.7100435

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
				uniform float _Progress;


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

					float4 color65 = IsGammaSpace() ? float4(0.5637683,0.9716981,0.9134225,1) : float4(0.2778827,0.9368213,0.8142352,1);
					float4 color66 = IsGammaSpace() ? float4(1,1,1,1) : float4(1,1,1,1);
					float2 break16_g4 = float2( 0,0.5 );
					float2 texCoord64 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float smoothstepResult12_g4 = smoothstep( break16_g4.x , break16_g4.y , ( 1.0 - ( length( ( ( texCoord64 - float2( 0.5,0.5 ) ) / float2( 1,1 ) ) ) * 2.0 ) ));
					float4 lerpResult68 = lerp( color65 , color66 , smoothstepResult12_g4);
					float2 break16_g2 = float2( 0,0.05 );
					float2 texCoord1 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float smoothstepResult12_g2 = smoothstep( break16_g2.x , break16_g2.y , ( 1.0 - ( length( ( ( texCoord1 - float2( 0.5,0.5 ) ) / float2( 1,1 ) ) ) * 2.0 ) ));
					float2 break16_g3 = float2( 0,0.05 );
					float2 texCoord28 = i.texcoord.xy * float2( 1,1 ) + float2( 0,0 );
					float2 appendResult43 = (float2((-0.5 + (( i.texcoord.xyz.z + _Progress ) - 0.0) * (0.5 - -0.5) / (1.0 - 0.0)) , 0.5));
					float smoothstepResult12_g3 = smoothstep( break16_g3.x , break16_g3.y , ( 1.0 - ( length( ( ( texCoord28 - appendResult43 ) / float2( 1,1 ) ) ) * 2.0 ) ));
					

					fixed4 col = ( i.color * _TintColor * 2.0 * ( lerpResult68 * saturate( ( smoothstepResult12_g2 - smoothstepResult12_g3 ) ) ) );
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
/*ASEBEGIN
Version=19002
26;45;1440;779;1196.794;703.6913;2.407836;True;True
Node;AmplifyShaderEditor.CommentaryNode;81;-358.4027,79.31905;Inherit;False;1006.966;442.9058;遮罩圆;7;80;28;44;43;55;71;72;;1,1,1,1;0;0
Node;AmplifyShaderEditor.TexCoordVertexDataNode;55;-308.4027,302.3563;Inherit;False;0;3;0;5;FLOAT3;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.RangedFloatNode;72;86.27461,406.2248;Inherit;False;Property;_Progress;Progress;0;0;Create;True;0;0;0;False;0;False;0.7100435;0;0;1;0;1;FLOAT;0
Node;AmplifyShaderEditor.SimpleAddOpNode;71;-108.3215,373.9239;Inherit;False;2;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.TFHCRemapNode;44;0.7694407,232.8019;Inherit;False;5;0;FLOAT;0;False;1;FLOAT;0;False;2;FLOAT;1;False;3;FLOAT;-0.5;False;4;FLOAT;0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;79;135.2267,-288.8531;Inherit;False;525.932;332.1429;月牙;3;8;1;78;;1,1,1,1;0;0
Node;AmplifyShaderEditor.CommentaryNode;85;26.29412,559.7028;Inherit;False;1031.949;439.5319;渐变色;6;65;68;66;57;64;84;;1,1,1,1;0;0
Node;AmplifyShaderEditor.Vector2Node;8;203.5298,-120.7102;Inherit;False;Constant;_offset1;offset1;0;0;Create;True;0;0;0;False;0;False;0.5,0.5;0.5,0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.TextureCoordinatesNode;28;-248.794,129.3191;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.DynamicAppendNode;43;250.8728,279.3875;Inherit;False;FLOAT2;4;0;FLOAT;0;False;1;FLOAT;0.5;False;2;FLOAT;0;False;3;FLOAT;0;False;1;FLOAT2;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;1;185.2267,-238.8531;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.FunctionNode;80;395.5635,198.4477;Inherit;False;DrawCircle;-1;;3;3ca3ee9d4c6494c06ac6b66aa8c4714e;0;4;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.FunctionNode;78;408.1587,-152.8891;Inherit;False;DrawCircle;-1;;2;3ca3ee9d4c6494c06ac6b66aa8c4714e;0;4;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0.05;False;1;FLOAT;0
Node;AmplifyShaderEditor.TextureCoordinatesNode;64;76.40472,639.2755;Inherit;False;0;-1;2;3;2;SAMPLER2D;;False;0;FLOAT2;1,1;False;1;FLOAT2;0,0;False;5;FLOAT2;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.Vector2Node;57;76.2941,817.7961;Inherit;False;Constant;_Vector0;Vector 0;0;0;Create;True;0;0;0;False;0;False;0.5,0.5;0.5,0.5;0;3;FLOAT2;0;FLOAT;1;FLOAT;2
Node;AmplifyShaderEditor.FunctionNode;84;476.4935,804.7877;Inherit;False;DrawCircle;-1;;4;3ca3ee9d4c6494c06ac6b66aa8c4714e;0;4;2;FLOAT2;0,0;False;3;FLOAT2;0,0;False;4;FLOAT2;0,0;False;5;FLOAT2;0,0.5;False;1;FLOAT;0
Node;AmplifyShaderEditor.ColorNode;66;371.8649,609.7028;Inherit;False;Constant;_InSideColor;InSideColor;0;0;Create;True;0;0;0;False;0;False;1,1,1,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.ColorNode;65;582.0494,612.1031;Inherit;False;Constant;_OutSideColor;OutSideColor;0;0;Create;True;0;0;0;False;0;False;0.5637683,0.9716981,0.9134225,1;0,0,0,0;True;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleSubtractOpNode;17;769.9745,224.9519;Inherit;True;2;0;FLOAT;0;False;1;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.LerpOp;68;793.243,745.2347;Inherit;True;3;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SaturateNode;53;1031.985,231.0714;Inherit;True;1;0;FLOAT;0;False;1;FLOAT;0
Node;AmplifyShaderEditor.CommentaryNode;76;875.4377,-427.2774;Inherit;False;574.5253;603.2454;ParticleInfo;4;48;47;54;50;;1,1,1,1;0;0
Node;AmplifyShaderEditor.RangedFloatNode;54;1048.659,-7.964569;Inherit;False;Constant;_Float0;Float 0;1;0;Create;True;0;0;0;False;0;False;2;0;0;0;0;1;FLOAT;0
Node;AmplifyShaderEditor.TemplateShaderPropertyNode;48;925.4377,-280.8737;Inherit;False;0;0;_TintColor;Shader;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.VertexColorNode;47;1112.01,-377.2774;Inherit;False;0;5;COLOR;0;FLOAT;1;FLOAT;2;FLOAT;3;FLOAT;4
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;70;1259.23,223.3593;Inherit;True;2;2;0;COLOR;0,0,0,0;False;1;FLOAT;0;False;1;COLOR;0
Node;AmplifyShaderEditor.SimpleMultiplyOpNode;50;1287.963,-7.032082;Inherit;False;4;4;0;COLOR;0,0,0,0;False;1;COLOR;0,0,0,0;False;2;FLOAT;0;False;3;COLOR;0,0,0,0;False;1;COLOR;0
Node;AmplifyShaderEditor.TemplateMultiPassMasterNode;0;1489.906,-119.6037;Float;False;True;-1;2;ASEMaterialInspector;0;9;HalfCut;0b6a9f8b4f707c74ca64c0be8e590de0;True;SubShader 0 Pass 0;0;0;SubShader 0 Pass 0;2;False;True;2;5;False;;10;False;;0;1;False;;0;False;;False;False;False;False;False;False;False;False;False;False;False;False;True;2;False;;False;True;True;True;True;False;0;False;;False;False;False;False;False;False;False;False;False;True;2;False;;True;3;False;;False;True;4;Queue=Transparent=Queue=0;IgnoreProjector=True;RenderType=Transparent=RenderType;PreviewType=Plane;False;False;0;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;False;True;0;False;0;;0;0;Standard;0;0;1;True;False;;False;0
WireConnection;71;0;55;3
WireConnection;71;1;72;0
WireConnection;44;0;71;0
WireConnection;43;0;44;0
WireConnection;80;2;28;0
WireConnection;80;3;43;0
WireConnection;78;2;1;0
WireConnection;78;3;8;0
WireConnection;84;2;64;0
WireConnection;84;3;57;0
WireConnection;17;0;78;0
WireConnection;17;1;80;0
WireConnection;68;0;65;0
WireConnection;68;1;66;0
WireConnection;68;2;84;0
WireConnection;53;0;17;0
WireConnection;70;0;68;0
WireConnection;70;1;53;0
WireConnection;50;0;47;0
WireConnection;50;1;48;0
WireConnection;50;2;54;0
WireConnection;50;3;70;0
WireConnection;0;0;50;0
ASEEND*/
//CHKSM=39BA8AAFE652803FE3B27C5E869327F61C2695EB