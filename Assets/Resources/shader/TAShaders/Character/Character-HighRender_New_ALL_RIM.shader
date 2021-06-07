Shader "Rendering/Character/Character-HighRender_New_ALL_RIM"
{
	Properties
	{
		_halfL("half lambert change",Range(0,1.5)) = 1
		[NoScaleOffset]_MainTex ("RGB = Texture.RGB", 2D) = "white" {}
		_MainTexPower("texture power",Range(0,0.3)) = 0.3
		_changeColor("change area color",Color) = (0.5,0.5,0.5,1)
		_ambient("ambient strength",Range(0,1)) = 0.2
		[NoScaleOffset]_norBump("Normal RG = normalMap B = Specular Texture",2D) = "white"{}
		_bumpPower("Normal Map Weight",Range(-2,2)) = 1
		_specColor("specular color",Color) = (1,1,1,1)
		_specluarColorStrength("specular color strength",Range(1,2)) = 2
		_specPower("specular area",Range(0,5)) = 4

		//[NoScaleOffset] _MatRim("Side Light Tex(RGB)",2D) = "black" {}
		//_sideLight_color("side light color",Color) = (0,0,0,0)
		//_sideLight_Area("side light area",Range(1,3)) = 3

		[NoScaleOffset]_cube("cubemap",Cube) = "white" {}
		_reflPower("reflection power",Range(0,1)) = 0.65

		[NoScaleOffset] _mask("R = reflect G = glow B = sss area A = sss",2D) = "white" {}
		[NoScaleOffset] _ChangeColorMask("R = change color area G = change color Gradient",2D) = "" {}

		[Header(SSS)]
		_sssColor("SSS Color",Color) = (0.69,0.69,0.69,0)
		_AmbientColor("Skin Ambient Color",Color) =(0.65,0.65,0.65,0)
		_SkinLUT("LUT",2D) = "" {}
		_SSSDepth("SSSDepth",Range(0,1)) = 0.15
		_LightWarp("Light Warp",Range(0,0.5)) = 0.19
		_LightOffset("Light Offset",Range(0,1)) = 0.45

		[Header(Rim)]
		_RimMask("RIM Mask",2D) = "black" {}
		_RimColor("RIM Color",color) = (1,0.36,0,1)
		_RimPower("RIM Power",Range(0,5)) = 0.5
		_RimOffset("RIM Offset",Range(0,9)) = 2
		[Enum(UnityEngine.Rendering.CompareFunction)]_RimStencilComp("Rim Stencil Compare",Float) = 3
		[Enum(UnityEngine.Rendering.CompareFunction)]_RimDepthComp("Rim Depth Compare",Float) = 5
		_RimZWrite("Rim Z Write",float) = 0

		//_backlight("背光固有色亮度",Range(0,1)) = 0.55
		//_BackLightStrength("背光范围",Range(0,4)) = 3
		//_backlightPower("背光高光强度(也会影响正面高光强度)",Range(0,2)) = 0.8

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
		Tags { "Queue" = "AlphaTest+1"}
		LOD 200

		Pass
		{
			Name "Rim"
			ZWrite [_RimZWrite]
			ZTest [_RimDepthComp]
			Blend SrcAlpha OneMinusSrcAlpha

			stencil
			{
				Ref 0
				Comp [_RimStencilComp]
				Pass keep
			}

			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			
			#pragma skip_variants VERTEXLIGHT_ON DYNAMICLIGHTMAP_ON DIRLIGHTMAP_COMBINED LIGHTMAP_ON SHADOWS_SHADOWMASK LIGHTMAP_SHADOW_MIXING SHADOWS_SCREEN SPOT DIRECTIONAL_COOKIE POINT_COOKIE
			
			#include "UnityCG.cginc"

			struct vertexInput
			{
				float4 vertex : POSITION;
				half3 normal : NORMAL;
			};

			struct vertexOutput
			{
				float4 pos : SV_POSITION;
				float4 worldpos : TEXCOORD0;
				half3 normal : TEXCOORD1;

			};

			float4 _RimColor;
			float _RimPower;

			vertexOutput vert(vertexInput input)
			{
				vertexOutput o = (vertexOutput)0;
				o.pos = UnityObjectToClipPos(input.vertex);
				o.worldpos = mul(unity_ObjectToWorld,input.vertex);
				o.normal = UnityObjectToWorldNormal(input.normal);
				return o;
			}

			half4 frag(vertexOutput output) : COLOR
			{
				half3 normalDir = normalize(output.normal);
				half3 viewDir = normalize(UnityWorldSpaceViewDir(output.worldpos));
				half rim = 1.0 - saturate(dot(viewDir, normalDir));
				half4 finalColor = _RimColor * pow(rim, _RimPower);
				return finalColor;
			}
			
			ENDCG
		}

		UsePass "Rendering/Character/Character-HightRender_New_ALL/BASE"
		UsePass "Rendering/Character/Character-HightRender_New_ALL/ADD"
	}

	FallBack "VertexLit"
}
