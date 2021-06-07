Shader "Rendering/Character/Character-HighRender_New_ALL" {
	Properties 
	{
		_half("half lambert change",Range(0,1.5)) = 1
		[NoScaleOffset] _MainTex("RGB = Texture.RGB",2D) = "white" {}
		_MainTexPower ("texture power", Range(0,0.3)) = 0.3
		_changeColor("change area color",Color) = (0.5,0.5,0.5,1)
		_ambient("ambient strength",Range(0,1)) = 0.2
		[NoScaleOffset] _norBump("Noraml RG = normalMap B = Speacular Texture",2D) = "white" {}
		_bumpPower("Normal Map Weight",Range(-2,2)) = 1
		_specColor("specular color",Color) = (1,1,1,1)
		_specluarColorStrength("specular color strength",Range(1,2)) = 2
		_specPower("specular area",Range(0,5)) = 4

		//[NoScaleOffset] _MatRim("Side Light Tex(RGB)",2D) = "black" {}
		//_sideLight_color("side light color",Color) = (0,0,0,0)
		//_sideLight_Area("side light area",Range(1,3)) = 3

		[NoScaleOffset] _cube("cubemap",Cube) = "white" {}
		_reflPower("reflection power",Range(0,1)) = 0.65

		[NoScaleOffset] _mask("R = reflect G = glow B = sss area A = sss",2D) = "white" {}
		[NoscaleOffset] _ChangeColorMask("R = change color arena G = change color Gradient",2D) = "white" {}

		[Header(sss)]
		_sssColor("SSS Color",Color) = (0.69,0.69,0.69,0)
		_AmbinetColor("skin Ambient Color",Color) = (0.65,0.65,0.65,0)
		_SkinLUT("LUT",2D) = "" {}
		_SSSDepth("SSSDepth",Range(0,1)) = 0.15
		_LightWarp("Light Warp",Range(0,0.5)) = 0.19
		_LightOffset("Light Offset",Range(0,1)) = 0.45

		[Header(Rim)]
		_RimMask("Rim Mask",2D) = "black" {}
		_RimColor("Rim Color",color) = (0,0,0,0)
		_RimPower("Rim Power",Range(0,2)) = 0
		_RimOffset("Rim Offset",Range(0,9)) = 2

		//_backlight("背光固有色亮度",Range(0,1)) = 0.55
		//_BackLightStrength("背光范围",Range(0,4)) = 4
		//_backlightPower("背光高光强度(也会影响正面高光强度)",Range(0,2)) = 0.8

		[Header(Glow)]
		_glowColor("glow color",Color) = (0,0,0,0)
		_glowTimeSpeed("glow speed",Float) = 1

		[Header(Glow)]
		_glowColor("glow color",Color) = (0,0,0,0)
		_glowTimeSpeed("glow speed",Float) = 1

		//程序调用
		[Header(Program Call)]
		_hitColor("被击边光颜色",Color) = (1,0,0,1)
		_hitStrength("Hit Strength",Range(0,1)) = 0
		_hitArea("Hit Area",Range(0,1)) = 1

		_fresnelColor("边光颜色",Color) = (1,1,1,1)
		_fresnelScale("边光范围",Range(0,9)) = 4.5
		_fresnelPower("边光强度",Range(0,2)) = 0
		_hitcolor2("被击颜色",Color) = (1,1,1,1)
		_hitcolor2Power("被击衰减",Range(0.5,1.5)) = 1

		_Alpha("Transparent",Range(0,1)) = 1
		[Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("Src Blend Mode",Float) = 0
		[Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("Dst Blend Mode",Float) = 1
	}

	SubShader 
	{
		Tags { "RenderType"="Opaque" }
		Tags {"Queue" = "Geometry+11" }
		LOD 200

		Pass
		{
			Tags { "LightMode" = "ForwardBase" }
			Name "Base"

			stencil
			{
				Ref 1
				Comp always
				Pass replace
			}

			Blend[_SrcBlend][_DstBlend]

			CGPROGRAM

			#pragma fragmentoption ARB_prectision_hint_fastest
			#pragma multi_compile_fwdbase
			#pragma vertex vert
			#pragma fragment frag
			#pragma multi_compile_fog

			#define _SSS_ENABLED
			#define _USE_MASK2
			#define _Rim_ENABLED

			#pragma shader_freature _HIGH_QUALITY _MEDIUM_QUALITY _LOW_QUALITY
			//#pragma shader_feature _GUP_SKINNING
			#pragma skip_variants VERTEXLIGHT_ON DYNAMICLIGHTMAP_ON DIRLIGHTMAP_COMBINED LIGHTMAP_ON SHADOWS_SHADOWMASK LIGHTMAP_SHADOW_MIXING SHADOWS_SCREEN SPOT DIRECTIONAL_COOKIE POINT_COOKIE

			#define _FWD_BASE

			#include "CharacterCore_New.cginc"

			ENDCG
		}

		Pass
		{

			Tags {"LightMode" = "ForwardAdd"}

			Name "Add"

			//Blend One One
			Blend One One
			ZWrite Off
			
			CGPROGRAM

			#pragma fragmentoption ARB_precision_hint_fastest
			#pragma multi_compile_fwdadd
			#pragma vertex vert
			#pragma fragment frag

			#define _SSS_ENABLED
			#define _USE_MASK2
			#define _Rim_ENABLED

			#pragma shader_feature _HIGH_QUALITY _MEDIUM_QUALITY _LOW_QUALITY
			//#pragma shader_feature _GPU_SKINNING
			#pragma skip_variants VERTEXLIGHT_ON DYNAMICLIGHTMAP_ON DIRLIGHT_COMBINED LIGHTMAP_ON SHADOWS_SHADOWMASK LIGHTMAP_SHADOW_MIXING SHADOWS_SCREEN SPOT DIRECTIONAL_COOKIE POINT_COOKIE

			#include "CharacterCore_New.cginc"

			ENDCG
		}
	}

	FallBack "VertexLit"
}
