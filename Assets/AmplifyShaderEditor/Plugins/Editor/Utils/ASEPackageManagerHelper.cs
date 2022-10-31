// Amplify Shader Editor - Visual Shader Editing Tool
// Copyright (c) Amplify Creations, Lda <info@amplify.pt>

using System;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEditor;
using UnityEditor.PackageManager.Requests;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;

namespace AmplifyShaderEditor
{
	public enum ASESRPVersions
	{
		ASE_SRP_10_0_0 =	100000,
		ASE_SRP_10_1_0 =	100100,
		ASE_SRP_10_2_2 =	100202,
		ASE_SRP_10_3_1 =	100301,
		ASE_SRP_10_3_2 =	100302,
		ASE_SRP_10_4_0 =	100400,
		ASE_SRP_10_5_0 =	100500,
		ASE_SRP_10_5_1 =	100501,
		ASE_SRP_10_6_0 =	100600,
		ASE_SRP_10_7_0 =	100700,
		ASE_SRP_10_8_0 =	100800,
		ASE_SRP_10_8_1 =	100801,
		ASE_SRP_10_9_0 =    100901,
		ASE_SRP_10_10_0 =   101000,
		ASE_SRP_11_0_0 =	110000,
		ASE_SRP_12_0_0 =	120000,
		ASE_SRP_12_1_0 =	120100,
		ASE_SRP_12_1_1 =	120101,
		ASE_SRP_12_1_2 =	120102,
		ASE_SRP_12_1_3 =    120103,
		ASE_SRP_12_1_4 =    120104,
		ASE_SRP_12_1_5 =    120105,
		ASE_SRP_12_1_6 =    120106,
		ASE_SRP_12_1_7 =    120107,
		ASE_SRP_13_1_8 =    130108,
		ASE_SRP_14_0_3 =    140003,
		ASE_SRP_RECENT =	999999
	}

	public enum ASEImportState
	{
		None,
		Lightweight,
		HD,
		Both
	}

	public static class AssetDatabaseEX
	{
		private static System.Type type = null;
		public static System.Type Type { get { return ( type == null ) ? type = System.Type.GetType( "UnityEditor.AssetDatabase, UnityEditor" ) : type; } }

		public static void ImportPackageImmediately( string packagePath )
		{
			AssetDatabaseEX.Type.InvokeMember( "ImportPackageImmediately", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.InvokeMethod, null, null, new object[] { packagePath } );
		}
	}


	[Serializable]
	[InitializeOnLoad]
	public static class ASEPackageManagerHelper
	{
		private static string URPNewVersionDetected =	"A new Universal RP version was detected and new templates are being imported.\n" +
																"Please hit the Update button on your ASE canvas to recompile your shader under the newest version.";

		private static string HDNewVersionDetected =	"A new HD RP version was detected and new templates are being imported.\n" +
														"Please hit the Update button on your ASE canvas to recompile your shader under the newest version.";

		private static string HDPackageId = "com.unity.render-pipelines.high-definition";
		private static string UniversalPackageId = "com.unity.render-pipelines.universal";
		private static string HDEditorPrefsId = "ASEHDEditorPrefsId";
		private static string LWEditorPrefsId = "ASELightweigthEditorPrefsId ";

		private static string URPTemplateVersionPrefix = "ASEURPtemplate";
		private static string HDRPTemplateVersionPrefix = "ASEHDRPtemplate";

		private static string SPKeywordFormat = "ASE_SRP_VERSION {0}";
		private static ListRequest m_packageListRequest = null;
		private static UnityEditor.PackageManager.PackageInfo m_lwPackageInfo;
		private static UnityEditor.PackageManager.PackageInfo m_hdPackageInfo;

		// V4.8.0 and bellow
		// HD 
		private static readonly string[] GetNormalWSFunc =
		{
			"inline void GetNormalWS( FragInputs input, float3 normalTS, out float3 normalWS, float3 doubleSidedConstants )\n",
			"{\n",
			"\tGetNormalWS( input, normalTS, normalWS );\n",
			"}\n"
		};

		// v4.6.0 and below
		private static readonly string[] BuildWordTangentFunc =
		{
			"float3x3 BuildWorldToTangent(float4 tangentWS, float3 normalWS)\n",
			"{\n",
			"\tfloat3 unnormalizedNormalWS = normalWS;\n",
			"\tfloat renormFactor = 1.0 / length(unnormalizedNormalWS);\n",
			"\tfloat3x3 worldToTangent = CreateWorldToTangent(unnormalizedNormalWS, tangentWS.xyz, tangentWS.w > 0.0 ? 1.0 : -1.0);\n",
			"\tworldToTangent[0] = worldToTangent[0] * renormFactor;\n",
			"\tworldToTangent[1] = worldToTangent[1] * renormFactor;\n",
			"\tworldToTangent[2] = worldToTangent[2] * renormFactor;\n",
			"\treturn worldToTangent;\n",
			"}\n"
		};

		private static bool m_lateImport = false;
		private static string m_latePackageToImport;

		private static bool m_requireUpdateList = false;
		private static ASEImportState m_importingPackage = ASEImportState.None;
		
		private static int m_packageHDVersion = -1; // @diogo: starts as missing
		private static int m_packageLWVersion = -1;

		private static RenderPipelineAsset m_currentRenderPipelineAsset = null;

		private static ASESRPVersions m_currentHDVersion = ASESRPVersions.ASE_SRP_RECENT;
		private static ASESRPVersions m_currentLWVersion = ASESRPVersions.ASE_SRP_RECENT;

		private static int m_urpTemplateVersion = 26;
		private static int m_hdrpTemplateVersion = 20;

		private static Dictionary<string, ASESRPVersions> m_srpVersionConverter = new Dictionary<string, ASESRPVersions>()
		{
			{"10.0.0-preview.26",   ASESRPVersions.ASE_SRP_10_0_0},
			{"10.0.0-preview.27",	ASESRPVersions.ASE_SRP_10_0_0},
			{"10.1.0",				ASESRPVersions.ASE_SRP_10_1_0},
			{"10.2.2",              ASESRPVersions.ASE_SRP_10_2_2},
			{"10.3.1",              ASESRPVersions.ASE_SRP_10_3_1},
			{"10.3.2",              ASESRPVersions.ASE_SRP_10_3_2},
			{"10.4.0",              ASESRPVersions.ASE_SRP_10_4_0},
			{"10.5.0",              ASESRPVersions.ASE_SRP_10_5_0},
			{"10.5.1",              ASESRPVersions.ASE_SRP_10_5_1},
			{"10.6.0",              ASESRPVersions.ASE_SRP_10_6_0},
			{"10.7.0",              ASESRPVersions.ASE_SRP_10_7_0},
			{"10.8.0",              ASESRPVersions.ASE_SRP_10_8_0},
			{"10.8.1",              ASESRPVersions.ASE_SRP_10_8_1},
			{"10.9.0",              ASESRPVersions.ASE_SRP_10_9_0},
			{"10.10.0",             ASESRPVersions.ASE_SRP_10_10_0},
			{"11.0.0",              ASESRPVersions.ASE_SRP_11_0_0},
			{"12.0.0",              ASESRPVersions.ASE_SRP_12_0_0},
			{"12.1.0",              ASESRPVersions.ASE_SRP_12_1_0},
			{"12.1.1",              ASESRPVersions.ASE_SRP_12_1_1},
			{"12.1.2",              ASESRPVersions.ASE_SRP_12_1_2},
			{"12.1.3",              ASESRPVersions.ASE_SRP_12_1_3},
			{"12.1.4",              ASESRPVersions.ASE_SRP_12_1_4},
			{"12.1.5",              ASESRPVersions.ASE_SRP_12_1_5},
			{"12.1.6",              ASESRPVersions.ASE_SRP_12_1_6},
			{"12.1.7",              ASESRPVersions.ASE_SRP_12_1_7},
			{"13.1.8",              ASESRPVersions.ASE_SRP_13_1_8},
			{"14.0.3",              ASESRPVersions.ASE_SRP_14_0_3}
        };

		private static Dictionary<ASESRPVersions, string> m_srpToASEPackageLW = new Dictionary<ASESRPVersions, string>()
		{
			{ASESRPVersions.ASE_SRP_10_0_0, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_1_0, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_2_2, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_3_1, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_3_2, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_4_0, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_5_0, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_5_1, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_6_0, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_7_0, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_8_0, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_10_8_1, "b460b52e6c1feae45b70b7ddc2c45bd6"},
            {ASESRPVersions.ASE_SRP_10_9_0, "b460b52e6c1feae45b70b7ddc2c45bd6"},
            {ASESRPVersions.ASE_SRP_10_10_0, "b460b52e6c1feae45b70b7ddc2c45bd6"},
            {ASESRPVersions.ASE_SRP_11_0_0, "b460b52e6c1feae45b70b7ddc2c45bd6"},
			{ASESRPVersions.ASE_SRP_12_0_0, "57fcea0ed8b5eb347923c4c21fa31b57"},
			{ASESRPVersions.ASE_SRP_12_1_0, "57fcea0ed8b5eb347923c4c21fa31b57"},
			{ASESRPVersions.ASE_SRP_12_1_1, "57fcea0ed8b5eb347923c4c21fa31b57"},
			{ASESRPVersions.ASE_SRP_12_1_2, "57fcea0ed8b5eb347923c4c21fa31b57"},
            {ASESRPVersions.ASE_SRP_12_1_3, "57fcea0ed8b5eb347923c4c21fa31b57"},
            {ASESRPVersions.ASE_SRP_12_1_4, "57fcea0ed8b5eb347923c4c21fa31b57"},
            {ASESRPVersions.ASE_SRP_12_1_5, "57fcea0ed8b5eb347923c4c21fa31b57"},
            {ASESRPVersions.ASE_SRP_12_1_6, "57fcea0ed8b5eb347923c4c21fa31b57"},
            {ASESRPVersions.ASE_SRP_12_1_7, "57fcea0ed8b5eb347923c4c21fa31b57"},
            {ASESRPVersions.ASE_SRP_13_1_8, "57fcea0ed8b5eb347923c4c21fa31b57"},
            {ASESRPVersions.ASE_SRP_RECENT, "57fcea0ed8b5eb347923c4c21fa31b57"}
		};

		private static Dictionary<ASESRPVersions, string> m_srpToASEPackageHD = new Dictionary<ASESRPVersions, string>()
		{
			{ASESRPVersions.ASE_SRP_10_0_0, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_1_0, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_2_2, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_3_1, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_3_2, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_4_0, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_5_0, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_5_1, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_6_0, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_7_0, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_8_0, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_10_8_1, "2243c8b4e1ab6914995699133f67ab5a"},
            {ASESRPVersions.ASE_SRP_10_9_0, "2243c8b4e1ab6914995699133f67ab5a"},
            {ASESRPVersions.ASE_SRP_10_10_0, "2243c8b4e1ab6914995699133f67ab5a"},
            {ASESRPVersions.ASE_SRP_11_0_0, "2243c8b4e1ab6914995699133f67ab5a"},
			{ASESRPVersions.ASE_SRP_12_0_0, "9a5e61a8b3421b944863d0946e32da0a"},
			{ASESRPVersions.ASE_SRP_12_1_0, "9a5e61a8b3421b944863d0946e32da0a"},
			{ASESRPVersions.ASE_SRP_12_1_1, "9a5e61a8b3421b944863d0946e32da0a"},
			{ASESRPVersions.ASE_SRP_12_1_2, "9a5e61a8b3421b944863d0946e32da0a"},
            {ASESRPVersions.ASE_SRP_12_1_3, "9a5e61a8b3421b944863d0946e32da0a"},
            {ASESRPVersions.ASE_SRP_12_1_4, "9a5e61a8b3421b944863d0946e32da0a"},
            {ASESRPVersions.ASE_SRP_12_1_5, "9a5e61a8b3421b944863d0946e32da0a"},
            {ASESRPVersions.ASE_SRP_12_1_6, "9a5e61a8b3421b944863d0946e32da0a"},
            {ASESRPVersions.ASE_SRP_12_1_7, "9a5e61a8b3421b944863d0946e32da0a"},
            {ASESRPVersions.ASE_SRP_13_1_8, "9a5e61a8b3421b944863d0946e32da0a"},
            {ASESRPVersions.ASE_SRP_RECENT, "9a5e61a8b3421b944863d0946e32da0a"}

		};

		private static Shader m_lateShader;
		private static Material m_lateMaterial;
		private static AmplifyShaderFunction m_lateShaderFunction;

		static ASEPackageManagerHelper()
		{
			RequestInfo( true );
		}

		static void WaitForPackageListBeforeUpdating()
		{
			if ( m_packageListRequest.IsCompleted )
			{
				Update();
				EditorApplication.update -= WaitForPackageListBeforeUpdating;
			}
		}

		public static void RequestInfo( bool updateWhileWaiting = false )
		{
			if ( !m_requireUpdateList && m_importingPackage == ASEImportState.None )
			{
				m_requireUpdateList = true;
				m_packageListRequest = UnityEditor.PackageManager.Client.List( true );
				if ( updateWhileWaiting )
				{
					EditorApplication.update += WaitForPackageListBeforeUpdating;
				}
			}
		}

		static void FailedPackageImport( string packageName, string errorMessage )
		{
			FinishImporter();
		}

		static void CancelledPackageImport( string packageName )
		{
			FinishImporter();
		}

		static void CompletedPackageImport( string packageName )
		{
			FinishImporter();
		}

		public static void CheckLatePackageImport()
		{
			if( !Application.isPlaying && m_lateImport && !string.IsNullOrEmpty( m_latePackageToImport ) )
			{
				m_lateImport = false;
				StartImporting( m_latePackageToImport );
				m_latePackageToImport = string.Empty;	
			}
		}

		public static void StartImporting( string packagePath )
		{
			if( !Preferences.GlobalAutoSRP )
			{
				m_importingPackage = ASEImportState.None;
				return;
			}

			if( Application.isPlaying )
			{
				if( !m_lateImport )
				{
					m_lateImport = true;
					m_latePackageToImport = packagePath;
					Debug.LogWarning( "Amplify Shader Editor requires the \""+ packagePath +"\" package to be installed in order to continue. Please exit Play mode to proceed." );
				}
				return;
			}

			AssetDatabase.importPackageCancelled += CancelledPackageImport;
			AssetDatabase.importPackageCompleted += CompletedPackageImport;
			AssetDatabase.importPackageFailed += FailedPackageImport;
			AssetDatabase.ImportPackage( packagePath, false );
			//AssetDatabaseEX.ImportPackageImmediately( packagePath );
		}

		public static void FinishImporter()
		{
			m_importingPackage = ASEImportState.None;
			AssetDatabase.importPackageCancelled -= CancelledPackageImport;
			AssetDatabase.importPackageCompleted -= CompletedPackageImport;
			AssetDatabase.importPackageFailed -= FailedPackageImport;
		}

		public static void SetupLateShader( Shader shader )
		{
			if( shader == null )
				return;

			//If a previous delayed object is pending discard it and register the new one
			// So the last selection will be the choice of opening
			//This can happen when trying to open an ASE canvas while importing templates or in play mode
			if( m_lateShader != null )
			{
				EditorApplication.delayCall -= LateShaderOpener;
			}

			RequestInfo();
			m_lateShader = shader;
			EditorApplication.delayCall += LateShaderOpener;
		}

		public static void LateShaderOpener()
		{			
			Update();
			if( IsProcessing )
			{
				EditorApplication.delayCall += LateShaderOpener;
			}
			else
			{
				AmplifyShaderEditorWindow.ConvertShaderToASE( m_lateShader );
				m_lateShader = null;
			}
		}

		public static void SetupLateMaterial( Material material )
		{
			if( material == null )
				return;
			
			//If a previous delayed object is pending discard it and register the new one
			// So the last selection will be the choice of opening
			//This can happen when trying to open an ASE canvas while importing templates or in play mode
			if( m_lateMaterial != null )
			{
				EditorApplication.delayCall -= LateMaterialOpener;
			}

			RequestInfo();
			m_lateMaterial = material;
			EditorApplication.delayCall += LateMaterialOpener;
		}

		public static void LateMaterialOpener()
		{
			Update();
			if( IsProcessing )
			{
				EditorApplication.delayCall += LateMaterialOpener;
			}
			else
			{
				AmplifyShaderEditorWindow.LoadMaterialToASE( m_lateMaterial );
				m_lateMaterial = null;
			}
		}

		public static void SetupLateShaderFunction( AmplifyShaderFunction shaderFunction )
		{
			if( shaderFunction == null )
				return;

			//If a previous delayed object is pending discard it and register the new one
			// So the last selection will be the choice of opening
			//This can happen when trying to open an ASE canvas while importing templates or in play mode
			if( m_lateShaderFunction != null )
			{
				EditorApplication.delayCall -= LateShaderFunctionOpener;
			}

			RequestInfo();
			m_lateShaderFunction = shaderFunction;
			EditorApplication.delayCall += LateShaderFunctionOpener;
		}

		public static void LateShaderFunctionOpener()
		{
			Update();
			if( IsProcessing )
			{
				EditorApplication.delayCall += LateShaderFunctionOpener;
			}
			else
			{
				AmplifyShaderEditorWindow.LoadShaderFunctionToASE( m_lateShaderFunction, false );
				m_lateShaderFunction = null;
			}
		}

		private static readonly string SemVerPattern = @"^(0|[1-9]\d*)\.(0|[1-9]\d*)\.(0|[1-9]\d*)(?:-((?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*)(?:\.(?:0|[1-9]\d*|\d*[a-zA-Z-][0-9a-zA-Z-]*))*))?(?:\+([0-9a-zA-Z-]+(?:\.[0-9a-zA-Z-]+)*))?$";

		private static int PackageVersionStringToCode( string version )
		{			
			int versionCode = 999999;
			MatchCollection matches = Regex.Matches( version, SemVerPattern, RegexOptions.Multiline );
			if ( matches.Count > 0 && matches[ 0 ].Groups.Count >= 4 )
			{
				versionCode  = int.Parse( matches[ 0 ].Groups[ 1 ].Value ) * 10000;
				versionCode += int.Parse( matches[ 0 ].Groups[ 2 ].Value ) * 100;
				versionCode += int.Parse( matches[ 0 ].Groups[ 3 ].Value );
			}
			return versionCode;
		}		

		public static void Update()
		{
			//if( Application.isPlaying )
			//	return;

			CheckLatePackageImport();
			//if( m_lwPackageInfo != null )
			//{
			//	if( m_srpVersionConverter[ m_lwPackageInfo.version ] != m_currentLWVersion )
			//	{
			//		m_currentLWVersion = m_srpVersionConverter[ m_lwPackageInfo.version ];
			//		EditorPrefs.SetInt( LWEditorPrefsId, (int)m_currentLWVersion );
			//		m_importingPackage = ASEImportState.Lightweight;
			//		string packagePath = AssetDatabase.GUIDToAssetPath( m_srpToASEPackageLW[ m_currentLWVersion ] );
			//		StartImporting( packagePath );
			//	}
			//}

			//if( m_hdPackageInfo != null )
			//{
			//	if( m_srpVersionConverter[ m_hdPackageInfo.version ] != m_currentHDVersion )
			//	{
			//		m_currentHDVersion = m_srpVersionConverter[ m_hdPackageInfo.version ];
			//		EditorPrefs.SetInt( HDEditorPrefsId, (int)m_currentHDVersion );
			//		m_importingPackage = ASEImportState.HD;
			//		string packagePath = AssetDatabase.GUIDToAssetPath( m_srpToASEPackageHD[ m_currentHDVersion ] );
			//		StartImporting( packagePath );
			//	}
			//}

			m_currentRenderPipelineAsset = GraphicsSettings.currentRenderPipeline;

			if( m_requireUpdateList && m_importingPackage == ASEImportState.None )
			{
				if( m_packageListRequest != null && m_packageListRequest.IsCompleted )
				{
					m_requireUpdateList = false;
					foreach( UnityEditor.PackageManager.PackageInfo pi in m_packageListRequest.Result )
					{
						if( pi.name.Equals( UniversalPackageId ) )
						{
							m_currentLWVersion = ASESRPVersions.ASE_SRP_RECENT;
							m_lwPackageInfo = pi;
							ASESRPVersions oldVersion = (ASESRPVersions)EditorPrefs.GetInt( LWEditorPrefsId );
							if( m_srpVersionConverter.ContainsKey( pi.version ) )
							{
								m_currentLWVersion = m_srpVersionConverter[ pi.version ];
							}
							else
							{
								m_currentLWVersion = ASESRPVersions.ASE_SRP_RECENT;
							}
							m_packageLWVersion = PackageVersionStringToCode( pi.version );

							EditorPrefs.SetInt( LWEditorPrefsId, (int)m_currentLWVersion );
							bool foundNewVersion = oldVersion != m_currentLWVersion;

							string URPTemplateVersion = URPTemplateVersionPrefix + Application.productName;
							int urpVersion = EditorPrefs.GetInt( URPTemplateVersion, m_urpTemplateVersion );
							if( urpVersion < m_urpTemplateVersion )
								foundNewVersion = true;
							EditorPrefs.SetInt( URPTemplateVersion, m_urpTemplateVersion );

							if( !File.Exists( AssetDatabase.GUIDToAssetPath( TemplatesManager.URPLitGUID ) ) ||
								!File.Exists( AssetDatabase.GUIDToAssetPath( TemplatesManager.URPUnlitGUID ) ) ||
								foundNewVersion
								)
							{
								if( foundNewVersion )
									Debug.Log( URPNewVersionDetected );

								m_importingPackage = ASEImportState.Lightweight;
								string guid = m_srpToASEPackageLW.ContainsKey( m_currentLWVersion ) ? m_srpToASEPackageLW[ m_currentLWVersion ] : m_srpToASEPackageLW[ ASESRPVersions.ASE_SRP_RECENT ];
								string packagePath = AssetDatabase.GUIDToAssetPath( guid );
								StartImporting( packagePath );
							}
							
						}

						if( pi.name.Equals( HDPackageId ) )
						{
							m_currentHDVersion = ASESRPVersions.ASE_SRP_RECENT;
							m_hdPackageInfo = pi;
							ASESRPVersions oldVersion = (ASESRPVersions)EditorPrefs.GetInt( HDEditorPrefsId );
							if( m_srpVersionConverter.ContainsKey( pi.version ) )
							{
								m_currentHDVersion = m_srpVersionConverter[ pi.version ];
							}
							else
							{
								m_currentHDVersion = ASESRPVersions.ASE_SRP_RECENT;
							}
							m_packageHDVersion = PackageVersionStringToCode( pi.version );

							EditorPrefs.SetInt( HDEditorPrefsId, (int)m_currentHDVersion );
							bool foundNewVersion = oldVersion != m_currentHDVersion;

							string HDRPTemplateVersion = HDRPTemplateVersionPrefix + Application.productName;
							int hdrpVersion = EditorPrefs.GetInt( HDRPTemplateVersion, m_hdrpTemplateVersion );
							if( hdrpVersion < m_hdrpTemplateVersion )
								foundNewVersion = true;
							EditorPrefs.SetInt( HDRPTemplateVersion, m_hdrpTemplateVersion );

							if( !File.Exists( AssetDatabase.GUIDToAssetPath( TemplatesManager.HDRPLitGUID ) ) ||
								!File.Exists( AssetDatabase.GUIDToAssetPath( TemplatesManager.HDRPUnlitGUID ) ) ||
								foundNewVersion
								)
							{
								if( foundNewVersion )
									Debug.Log( HDNewVersionDetected );

								m_importingPackage = m_importingPackage == ASEImportState.Lightweight ? ASEImportState.Both : ASEImportState.HD;
								string guid = m_srpToASEPackageHD.ContainsKey( m_currentHDVersion ) ? m_srpToASEPackageHD[ m_currentHDVersion ] : m_srpToASEPackageHD[ ASESRPVersions.ASE_SRP_RECENT ];
								string packagePath = AssetDatabase.GUIDToAssetPath( guid );
								StartImporting( packagePath );
							}
							
						}
					}
				}
			}
		}

		public static int CurrentSRPVersion()
		{
			int srpVersion = -1;
			if ( m_currentRenderPipelineAsset != null )
			{			
				bool isHD = ( m_currentRenderPipelineAsset.GetType().Name == "HDRenderPipelineAsset" );
				bool isLW = ( m_currentRenderPipelineAsset.GetType().Name == "UniversalRenderPipelineAsset" );
				if ( isHD || isLW )
				{
					srpVersion = isHD ? ASEPackageManagerHelper.PackageHDVersion : ASEPackageManagerHelper.PackageLWVersion;
				}
			}			
			return srpVersion;
		}			

		public static void SetSRPInfoOnDataCollector( ref MasterNodeDataCollector dataCollector )
		{
			if( m_requireUpdateList )
				Update();

			if( dataCollector.CurrentSRPType == TemplateSRPType.HD )
			{
				dataCollector.AddToDirectives( string.Format( SPKeywordFormat, m_packageHDVersion ) ,-1, AdditionalLineType.Define );
			}

			if( dataCollector.CurrentSRPType == TemplateSRPType.Lightweight )
				dataCollector.AddToDirectives( string.Format( SPKeywordFormat, m_packageLWVersion ), -1, AdditionalLineType.Define );
		}
		public static ASESRPVersions CurrentHDVersion { get { return m_currentHDVersion; } }
		public static ASESRPVersions CurrentLWVersion { get { return m_currentLWVersion; } }

		public static int PackageHDVersion { get { return m_packageHDVersion; } }
		public static int PackageLWVersion { get { return m_packageLWVersion; } }

		public static bool FoundHDVersion { get { return m_hdPackageInfo != null; } }
		public static bool FoundLWVersion { get { return m_lwPackageInfo != null; } }

		public static bool CheckImporter { get { return m_importingPackage != ASEImportState.None; } }
		public static bool IsProcessing { get { return m_requireUpdateList && m_importingPackage == ASEImportState.None; } }
	}
}
