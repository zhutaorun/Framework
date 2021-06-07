#ifndef Character_CORE_New
#define Character_CORE_New

#include "AutoLight.cginc"
#include "UnityCG.cginc"
#include "ColorUtils.cginc"

#if !defined(_HIGH_QUALITY) && !defined(_MEDIUM_QUALITY) && !defined(_LOW_QUALITY)
#define _HIGH_QUALITY
#endif

#ifdef _LOW_QUALITY
#define _GLOW_ENABLED
#endif

#ifdef _MEDIUM_QUALITY
#defin _GLOW_ENABLED
#endif

#ifdef _HIGH_QUALITY
# define _NORMAL_MAP_ENABLED
# define _SPECULAR_ENABLED
# define _REFLECT_ENABLED
# define _GLOW_ENABLED
#endif

sampler2D _MainTex;
sampler2D _mask; //r = reflect;g = glow;b = sss Area/change color Area; a = sssTex/change color Gradient

#ifdef _USE_MASK2
sampler2D _ChangeColorMask; //second mask //  r= change color Area; g = change color Gradient
half4 ChangeColMask;
#endif

half _halfL, _ambient, _hitArea, _backligth, _fresnelScale, _fresnelPower, _hitcolor2Power, _hitStrength, _MainTexPower, _Alpha;
half3 _LightColor0, _changeColor, _hitColor, _fresnelColor, _hitcolor2;

#ifdef _SPECULAR_ENABLED
half _specluarColorStrength, _specPower, _backlightPower, _BackLighttStrength;
half3 _specColor;
#endif

#ifdef _NORMAL_MAP_ENABLED
sampler2D _norBump;
half _bumpPower;
#endif

#ifdef _Rim_ENABLED
sampler2D _RimMask;
half3 _RimColor;
half _RimPower;
half _RimOffset;
half _RimMaskPower;
half _RimLightDir;
#endif

#ifdef _REFLECT_ENABLED
samplerCUBE _cube;
half _reflPower;
#endif

#ifdef _SSS_ENABLED
sampler2D _SkinLUT;
half _LightWarp, _LightOffset, _SSSDepth;
half3 _AmbientColor, _sssColor;
#endif

#ifdef _GLOW_ENABLED
half3 _glowColor;
half _glowColorGaient;
half _glowTimeSpeed;
#endif

#ifdef _HAIR_CHANGE_COLOR	//头发变色
half3 _HsvMain, _HsvVice;	//主色hsv,副色hsv
half _HsvPercent;			//主副色比例调整
#endif

#ifdef _Aniso_Hair
half3 _AnisoSpecCol;
half _AnisoSpecArea;
half _AnisoSpecPower;
half _AnisoOffset;
half3 _AnisoLightDir;
#endif

#ifdef _MODEL_DISSOLVE
sampler2D _DissolveTex;
half4 _DissolveTex_ST;
half3 _MainColor, _DissolveColor;
half _RAmount, _DissovleWidth, _Illuminate;
#endif

#ifdef _WING
half4 _Color;
sampler2D _MainTexture;
float4 _MainTexture_ST;
half _MainTexSpeedX;
half _MainTexSpeedY;

sampler2D _MaskTexture;
float4 _MaskTexture_ST;
#endif

#ifdef _3CHANNEL_COLOR
sampler2D _ChannelColor;
half3 _ChangeR;
half3 _ChangeG;
half3 _ChangeB;
#endif

struct appdata
{
	half4 vertex : POSITION;
	half3 normal :NORMAL;
	half2 uv :TEXCOORD0;

#ifdef _NORMAL_MAP_ENABLED
half4 tangent:TANGENT;
#endif

#ifdef _WING
half4 color:COLOR;
#endif

#ifdef _GPU_SKINNING
half4 boneIndex:TEXCOORD2;
float4 boneWeight : TEXCOORD3;
#endif
};
#ifdef _WING
struct v2f
{
	float4 pos : SV_POSITION;
	half2 uv : TEXCOORD0;
	float3 worldPos :TEXCOORD1;
	half3 color :COLOR;
	#ifdef _NORMAL_MAP_ENABLED
		half3 tSpace0 : TEXCOORD2;
		half3 tSpace1 : TEXCOORD3;
		half4 tSpace2 : TEXCOORD4;
		UNITY_SHADOW_COORDS(5)
		UNITY_FOG_COORDS(6)
		float4 winguv : TEXCOORD7;//xy--main texture uv, zw--mask texture uv
	#else
		half3 normal : TEXCOORD2;
		UNITY_SHADOW_COORDS(3)
		UNITY_FOG_COORDS(4)
		float4 winguv : TEXCOORD5;//xy--main texture uv, zw--mask texture uv
	#endif
};
#else
struct v2f
{
	float4 pos : SV_POSITION;
	half2 uv : TEXCOORD0;
	float3 worldPos : TEXCOORD1;
	half3 color : COLOR;
	#ifdef _NORMAL_MAP_ENABLED
		half3 tSpace0 : TEXCOORD2;
		half3 tSpace1 : TEXCOORD3;
		half3 tSpace2 : TEXCOORD4;
		UNITY_SHADOW_COORDS(5)
		UNITY_FOG_COORDS(6)
		#ifdef _MODEL_DISSOLVE
			half2 uv1 : TEXCOORD7;
		#endif
	#else
		half3 normal : TEXCOORD2;
		UNITY_SHADOW_COORDS(3)
		UNITY_FOG_COORDS(4)
		#ifdef _MODEL_DISSOLVE
			half2 uv1 : TEXCOORD5;
		#endif
	#endif
};
#endif

#ifdef _GPU_SKINNING
uniform float4 _MatrixPalette[52*3]
inline float4x4 GetMatrix(int idx)
{
	idx *= 3;
	return float4x4(_MatrixPalette[idx + 0], _MatrixPalette[idx + 1], _MatrixPalette[idx + 2], float4(0, 0, 0, 1));
}

float4 skin4(appdata v)
{
	float _dummy = _MatrixPalette[0].x;
	float4 ret = v.vertex;
	ret.x += _dummy;
	return ret;
}
#endif

v2f vert(appdata v)
{
	v2f o = (v2f)0;
	o.uv = v.uv;

#ifdef _MODEL_DISSOLVE
	o.uv1 = TRANSFORM_TEX(u.uv, _DissolveTex);
#endif

#ifdef _GUP_SKINNING
	float4 vpos = skin4(v)
#else
	float4 vpos = v.vertex;
#endif

	o.worldPos = mul(unity_ObjectToWorld, vpos).xyz;
	o.pos = UnityObjectToClipPos(vpos);

#ifdef _NORMAL_MAP_ENABLED
	half3 worldNormal = normalize(UnityObjectToWorldNormal(v.normal));
	half3 worldTangent = normalize(UnityObjectToWorldDir(v.tangent.xyz));
	half tangentSign = v.tangent.w * unity_WorldTransformParams.w;
	half3 worldBinormal = cross(worldNormal, worldTangent) * tangentSign;
	//world tangent space
	o.tSpace0 = half3(worldTangent.x, worldBinormal.x, worldNormal.x);
	o.tSpace1 = half3(worldTangent.y, worldBinormal.y, worldNormal.y);
	o.tSpace2 = half3(worldTangent.z, worldBinormal.z, worldNormal.z);

	o.color = ShadeSH9(half4(worldNormal, 1));
#else
	//o.normal = UnityObjectToWorldNormal(v.normal);
	o.normal = normalize(mul(v.normal, (float3x3)unity_WorldToObject));
#endif

#ifdef _WING
	half2 uvMain = v.uv * _MainTexture_ST.xy + _MainTexture_ST.zw;
	half2 uvMask = v.uv * _MaskTexture_ST.xy + _MaskTexture_ST.zw;
	o.winguv.xy = float2(uvMain.x + _MainTexSpeedX * _Time.x, uvMain.y + _MainTexSpeedY * _Time.x);
	o.winguv.zw = uvMask;
#endif

	UNITY_TRANSFER_SHADOW(o, v.uv);
	UNITY_TRANSFER_FOG(o, o.pos);

	return o;
}

//BlinnPhong
inline half SpecFunction(half3 lightDir, half3 viewDir, half3 normDir, half ss)
{
#ifdef _SPECULAR_ENABLED
	half3 h = normalize(lightDir + viewDir);
	float nh = saturate(dot(normDir, h));
	half spec = pow(nh, exp(ss));
#else
	half spec = 0;
#endif 
	return spec;
}

inline half AnisoSpec(half anisoMask, half3 lightDir, half viewDir, half3 Normal,half3 tangent, half specPow, half AnisoOffset)
{
	half NdotL = saturate(dot(Normal, lightDir));

	half3 T1 = -normalize(cross(Normal, tangent));
	half3 T2 = -normalize(cross(Normal, T1));
	half3 T = lerp(T1, T2, anisoMask * AnisoOffset);
	half3 L = normalize(lightDir);
	half3 V = normalize(-viewDir);

	half TL = dot(T, L);
	half TV = dot(T, V);

	half sq1 = sqrt(1 - pow(TL, 2));
	half sq2 = sqrt(1 - pow(TV, 2));

	half aniso = TL * TV;
	aniso += sq1 * sq2;
	aniso = pow(aniso, specPow * 64);

	return aniso;
}

inline half AnisoSpecFun(half3 lightDir, half viewDir, half3 normalDir, half AnisoSpecArea, half offset, half anisoMask, half3 anisoLightDir)
{
	half3 halfDir = normalize(anisoLightDir + viewDir * 0.5);
	half HdotN = saturate(dot(halfDir, normalDir));
	float aniso = saturate(sin(radians(HdotN + offset) * 180));
	float s = saturate(pow(lerp(HdotN, aniso, anisoMask), AnisoSpecArea * 64));

	return s;
}

half4 frag(v2f i) :SV_Target
{
	half3 main = tex2D(_MainTex,i.uv);
	//r = reflect; g = glow; b = sss Area/change color Area; a = sss/change color Gradient;
	half4 mask = tex2D(_mask, i.uv);

#ifdef _HAIR_CHANGE_COLOR
	half changeCOlorArea = mask.b;
	half changeColorGradient = mask.a;
#else
	half changeColorArea = mask.b;
#endif

#ifdef _USE_MASk2
	//r  = change color Area; g = change Color Gradient
	ChangeColMask = tex2D(_ChangeColorMask, i.uv);
	changeColorArea = ChangeColMask.r;
#endif


#ifdef _SSS_ENABLED
	half sssArea = mask.b;
	half sss = mask.b;
#endif

#ifndef USING_DIRECTIONAL_LIGHT
	half3 worldlightDir = normalize(UnityWorldSpaceLightDir(i.worldPos));
#else
	half3 worldlightDir = _WorldSpaceLightPos0.xyz;
#endif
	half3 worldViewDir = normalize(_WorldSpaceCameraPos.xyz - i.worldPos);

#ifdef _NORMAL_MAP_ENABLED
	half3 com = tex2D(_norBump,i.uv);
	half3 tsn = half3(0, 0, 0);
	tsn.xy = com.xy * 2.0 - 1.0;
	tsn.xy *= _bumpPower;
	tsn.z = sqrt(saturate(1.0 - dot(tsn.xy, tsn.xy)));
	half3 norm = normalize(mul(half3x3(i.tSpace0, i.tSpace1, i.tSpace2), tsn));
#else
	half3 norm = i.normal;
#endif

	//unity shadowmap
	UNITY_LIGHT_ATTENUATION(atten, i, i.worldPos)

	//change color
	half3 color;
#ifndef _HAIR_CHANGE_COLOR
		//����˵ȫ�ҵ�����²���ɫ(liner�ռ���ʱȥ����ɫ����Ȼ��ɫ�ܻ�)
	#ifdef UNITY_COLORSPACE_GAMA
		if (_changeColor.r != 0.5 || _changeColor.g != 0.5 || _changeColor.b != 0.5)
			color = ChangeColor(main.rgb, _changeColor.rgb, changeColorArea);
		else
			color = main.rgb;
	#else
		color = main.rgb;
	#endif
#else
	half3 hsv=lerp(_HsvMain, _HsvVice,changeColorGradient);
	color = ApplyHSV(main.rgb, hsv);
	color = saturate(color);

	changeColorArea = min(changeColorArea/0.3999,1);//maskͼĿǰ�ɱ�ɫ�����ֵһ������0.4
	color = lerp(main.rgb,color,changeColorArea);
#endif

#ifdef _3CHANNEL_COLOR
		half3 ChannelColor = tex2D(_ChannelColor, i.uv);
		color = main.rgb;
		if (any(_ChangeR.rgb))
			color = ChangeColor(main.rgb, _ChangeR, ChannelColor.r);
		if (any(_ChangeG.rgb))
			color = ChangeColor(main.rgb, _ChangeG, ChannelColor.g);
		if (any(_ChangeB.rgb))
			color = ChangeColor(main.rgb, _ChangeeB, ChannelColor.b);
#endif

#ifdef _MODEL_DISSOLVE
		half4 DissolveTexCol = tex2D(_DissolveTex, i.uv);
		half clipValueR = DissolveTexCol.r - _RAmount;
		if (clipVauleR <= 0)
		{
			if (clipVauleR > -_DissolveWidth)
			{
				half t = clipVauleR / -_DissolveWidth;
				color = lerp(color, _DissolbeColor, t);
			}
			else
			{
				discard;// Michael note:bad for mobile performance!
			}
		}
		else
		{
			discard;// Michael note:bad for mobile performance!
		}
		color* = _Illuminate;
#endif

		half nl = saturate(dot(worldlightDir, norm));
		half nv = saturate(dot(worldViewDir, norm));
		//half dimBottom = (norm.y * 0.5+0.5);

		#ifdef _CHA_SELCET
		half nlv = lerp(nl, nv, _backlight);
		#else
		half nlv = lerp(nl , nv, 0.55);
		#endif

		half3 l = lerp(nlv, nlv * 0.5 + 0.5, _halfL);
		half3 LightColor0 = lerp(_LightColor0, 1.0f, 0.7f);
		half3 Ambient = i.color * _ambient * 0.4;

#ifdef _SSS_ENABLED
		float2 BRDF = float2(0, 0);
		BRDF.x = nlv * _LightWarp + _LightOffset;
		BRDF.y = sss + _SSSDepth;
		half3 skinSSS = tex2D(_SkinLUT, BRDF);
		//return half4(skinSSS,1);
		half skinDiff = _sssColor.rgb * (skinSSS + _AmbientColor.rgb);
		color *= (lerp(1, skinDiff, 1 - sssArea)) * LightColor0 * atten;
#else
		color *= 1 * LightColor0.rgb * atten;
#endif

#ifdef _SPECULAR_ENABLED
	
	#ifndef _Aniso_Hair
		half specTex = com.b;
		
		half specPow = lerp(3, _specPower, nl);
		half SpecFunc = SpecFunction(worldViewDir, worldViewDir, norm, specPow);
		SpecFunc = lerp(SpecFunc * 0.8, SpecFunc, nl);//extra control for back specular

		// Add little value to mask for skin specular,because skin area in the mask is black.
		//half specMask = max(main.a,0.0);
		half3 specMask = max(specTex, 0.0);
		half3 spec = max(0, (SpecFunc * specMask * _specluarColorStrength) * _specColor.rgb);
	#else
		half specTex = com.b;

		half specPow = lerp(3,_specPower,nl);
		half SpecFunc = SpecFunction(worldViewDir,worldViewDir,norm,specPow);
		SpecFunc = lerp(SpecFunc * 0.8, SpecFunc, nl);//extra control for back specular

		//Add little value to mask for skin specular,because skin area in the mask is black
		//half3 specMask= max(main.a,0.0);
		half3 specMask = max(specTex,0.0);
		half3 spec = max(0, (SpecFunc * specMask * _specluarColorStrength) * _specColor, rgb);

		half anisoMask = step(0.1, changeColorArea);
		half3 hairspec = AnisoSpecFun(worldlightDir,worldViewDir,norm,_AnisoSpeccArea,_AnisoOffset,anisoMask,_AnisoLightDir)* 1* color* _AnisoSpecPower* _AnisoSpecCol;

		spec = max(0,(lerp(spec,hairspec,anisoMask));
	#endif
#endif


#ifdef _REFLECT_ENABLED
		half reflectArea = mask.r;

		half worldRelf = reflect(-worldViewDir, norm);
		half reflectCol = texCUBE(_cube,worldRelf).rgb;
		reflectCol = lerp(reflectCol,color,_reflPower)* reflectArea;
		half3 rc = reflectCol + color * (1.0-reflectArea);
		//half3 rc = lerp(color, reflectCol, mm.g * _reflPower); //simplfied

#else
		half3 rc = color;
#endif

#ifdef _GLOW_ENABLED
		half glowArea = mask.g;
		half3 glow = sin(max(_glowColorGaient, abs(frac((_Time.x) * _glowTimeSpeed)))) * _glowColor * glowArea;
#else
		half3 glow = 0;
#endif
		half fresnel = (1.0 - nv);
		half f4 = fresnel * fresnel * fresnel * fresnel;
		half3 hc = lerp(fresnel,f4,_hitArea)* _hitColor* _hitStrength;

		half3 fresnelColor = pow(fresnel,_fresnelScale)* _fresnelColor* _fresnelPower;
		half3 jyColor = _hitcolor2 * _hitcolor2Power;

#if defined (_Rim_ENABLED) && !defined(_DISABLE_RIM)
		half rm = tex2D(_RimMask,i.uv);

		half RimMask = lerp(rm,saturate(norm.y* 0.5),_RimMaskPower);
		half3 worldNormal = normalize(UnityObjectToWorldNormal(norm));

		half dotrimlightDir = dot(worldNormal, _RimLightDir) * atten;
		half3 rim = saturate(dotrimlightDir * pow(saturate(fresnel + _RimOffset), _RimPower));
		rim *= _RimColor * RimMask;
#else
		half3 rim = 0;
#endif



#ifdef _FWD_BASE
	#ifdef _SPECULAR_ENABLED
		half3 c = rc * _MainTexPower * jyColor + rc * jyColor + i.color * 0.4 * _ambient + spec + glow + hc + fresnelColor + rim;//+ sideLight;
	#else
		half3 c = rc * _MainTexPower * jyColor + rc * jyColor + i.color * 0.4 * _ambient + glow + hc + fresnelColor + rim;//+sideLigth;
	#endif
#else
	#ifdef _SPECULAR_ENABLED
		half3 c = (rc * _MainTexPower * jyColor + rc * jyColor + spec) * atten;
	#else
		half3 c = (rc * _MainTexPower * jyColor + rc * jyColor) * atten;
	#endif
#endif
	UNITY_APPLY_FOG(i.fogCoord,c);
	c = saturate(c);

#ifdef _WING
		half4 wingColor = tex2D(_MainTexture, i.winguv.xy) * _Color * 2.0;
		half4 wingMask = tex2D(_MaskTexture,i.winguv.zw);
		wingColor.rgb *= wingMask.rgb;
		c + = wingColor.rgb * _Color.a;
		return half4(c, 1.0);
#else
	#ifdef _USE_MASK2
		return half4(c, _Alpha * ChangeColMask.a);
	#else
		return half4(c, _Alpha);
	#endif
#endif
}
#endif