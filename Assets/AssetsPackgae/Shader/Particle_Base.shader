Shader "Effects_ArtCenter/Particle_Base"
{
    //配合DepthMask使用
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _FixColor("FixColor",COLOR) = (1,1,1,1)
        _Brightness("Brightness",Float) = 1
        _MainTexPannerX("Main Tex Panner X",Float) = 0
        _MainTexPannerY("Main Tex Panner Y",Float) = 0

        _StencilComp("Stencil Comparison",Float) = 8
        _Stencil("Stencil ID",Float) = 0
        _StencilOp("Stencil Operation",Float) = 0
        _StencilWriteMask("Stencil Write Mask",Float) = 255
        _StencilReadMask("Stencil Read Mask",Float) = 255
        [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest",Float) = 4
        [Enum(UnityEngine.Rendering.BlendMode)] SrcBlend("SrcBlend",Float) = 5
        [Enum(UnityEngine.Rendering.BlendMode)] DstBlend("DstBlend",Float) = 1

        _globeDayNightUse("_globeDayNightUse",Range(0,1)) = 0

        _useAreaMask("_useAreaMask",Range(0,1)) = 0

            //-----------add-----------
            _MinX("Min X",Float) = -10
            _MaxX("Max X",Float) = 10
            _MinY("Min Y",Float) = -10
            _MaxY("Max Y",Float) = 10
            //-----------add-----------

    }
        SubShader
        {
            Tags {
                "IgnoreProjector" = "true"
                "Queue" = "Transparent+100"
                "RenderType" = "Transparent"
            }

            Pass
            {
                Name "FORWARD"

                Tags {
                    "LightMode" = "ForwardBase"
                }

                Stencil
                {
                    Ref[_Stencil]
                    Comp[_StencilComp]
                    Pass[_StencilOp]
                    ReadMask[_StencilReadMask]
                    WriteMask[_StencilWriteMask]
                }

                Blend[SrcBlend][DstBlend]
                Cull Off
                ZWrite Off
                ZTest[_ZTest]

                CGPROGRAM
                #pragma vertex vert
                #pragma fragment frag
                #define UNITY_PASS_FORWARDBASE
                #include "UnityCG.cginc"
                #pragma multi_compile_fwdbase 

                #pragma skip_variants DIRLIGHTMAP_COMBINED DYNAMICLIGHTMAP_ON LIGHTMAP_ON LIGHTMAP_SHADOW_MXIING SHADOWS_SCREEN SHADOWS_SHADOWMASK VERTEXLIGHT_ON LIGHTPROBE_SH DIRECTIONAL

                uniform sampler2D _MainTex;
                uniform float4 _MainTex_ST;
                uniform fixed4 _FixColor;
                uniform float _Brightness;
                uniform float _MainTexPannerX;
                uniform float _MainTexPannerY;

                uniform fixed4 globeEffectBright;
                fixed _globeDayNightUse;
                fixed _useAreaMask;

                //---------------add----------------
                float _MIinX;
                float _MaxX;
                float _MinY;
                float _MaxY;
                //---------------add----------------
                
                struct VertexInput {
                    float4 vertex : POSITION;
                    float2 texcoord0 : TEXCOORD0;
                    float4 vertexColor : COLOR;
                };

                struct VertexOutput {
                    float4 pos : SV_POSITION
                    float2 uv0 : TEXCOORD0;
                    float4 vertexColor :COLOR ;
                    //------------add------------
                    float3 vpos: TEXCOORD2;
                    //------------add------------
                };

                VertexOutput vert (VertexInput v) {
                   VertexOutput o = (VertexOutput)0;
                   //------------add------------
                   if (_useAreaMask > 0)
                   {
                       o.vpos = mul(unity_ObjectToWorld,V.vertex);
                   }
                   //------------add------------

                    o.uv0 = (TRANSFORM_TEX(v.texcoord0, _MainTex)+half2(_MainTexPannerX,_MainTexPannerY)* _Time.y);
                    o.vertexColor = v.vertexColor;
                    o.pos = UnityObjectToClipPos(v.vertex);
                    return o;
                }

                fixed4 frag (VertexOutput i) : COLOR {
                    fixed4 _MainTex_var = tex2D(_MainTex, i.uv0);
                    float3 emissive = _Brightness * _MainTex_var.rgb * i.vertexColor.rgb * _FixColor.rgb;
                    fixed4 c = fixed4(emissive * lerp(1, globeEffectBright, _globeDayNightUse), i.vertexColor.a * _MainTex_var.a * _FixColor.a);
                    //---------------add--------------
                    if (_useAreaMask > 0)
                    {
                        float _temp = 1.0;
                        _temp *= (i.vpos.x >= _MinX);
                        _temp *= (i.vpos.x <= _MaxX);
                        _temp *= (i.vpos.y >= _MinY);
                        _temp * = (i.vpos.y<=  _MaxY);
                        clip(_temp - 0.0000100);
                    }
                    //---------------add--------------
                    return c;
                }
                ENDCG
        }
    }
    //
}
