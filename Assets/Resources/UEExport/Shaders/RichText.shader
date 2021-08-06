Shader "Unique/UI/RichText"
{
    Properties
    {
        _MainTex("Font Texture", 2D) = "white" {}
        _SpriteText("Sprite Texture", 2D) = "white" {}
        _Color("Text Color",Color) = (1,1,1,1)

            //如果需要mask裁剪请复制下述代码 Begin
             _StencilComp("Stencil Comparsison",Float) = 8
             _Stencil("Stencil ID",Float) = 0
             _StencilOp("Stencil Operation",Float) = 0
             _StencilWriteMask("Stencil Write Mask",Float) = 255
             _StencilReadMask("Stencil Read Mask",Float) = 255

             _ColorMask("Color Mask",Float) = 15
             [Toggle(UNITY_UI_ALPHACLIP)] _UseUIAlphaClip("Use Alpha Clip",Float) = 0
            //如果需要mask裁剪请复制上述代码 End
    }
    SubShader
    {
        Tags
        {
            "Queue" = "Transparent"
            "IgnoreProjector" = "True"
            "RenderType" = "Transparent"
            "PreviewType" = "Plane"
        }

            //如果需要mask裁剪请复制下述代码 Begin
            Stencil
        {
            Ref[_Stencil]
            Comp[_StencilComp]
            Pass[_StencilOp]
            ReadMask[_StencilReadMask]
            WriteMask[_StencilWriteMask]
        }

            Cull Off
            Lighting Off
            ZWrite Off
            ZTest[unity_GUIZTestMode]
            Blend SrcAlpha OneMinusSrcAlpha
            ColorMask[_ColorMask]

            //如果需要mask裁剪请复制上述代码 End
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile _ UNITY_SINGLE_PASS_STEREO STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
       

            //如果需要mask裁剪请复制下述代码 Begin
            #pragma multi_compile __ UNITY_UI_CLIP_RECT
            #pragma multi_compile __ UNITY_UI_ALPHACLIP
            //如果需要mask裁剪请复制上述代码 End

            #include "UnityCG.cginc"
            //如果需要mask裁剪请复制下述代码 Begin
            #include "UnityUI.cginc"
            //如果需要mask裁剪请复制上述代码 End

            struct appdata_t
            {
                float4 vertex : POSITION;
                half4 color : COLOR;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 vertex : POSITION;
                half color : COLOR;
                float2 uv0 : TEXCOORD0;
                float2 uv1 : TEXCOORD1;
                //如果需要mask裁剪请复制下述代码 Begin
                float3 worldPosition : TEXCOORD2;//此处按需
                //如果需要mask裁剪请复制上述代码 End
            };


            sampler2D _MainTex;
            float4 _MainTex_ST;

            sampler2D _SpriteTex;
            float4 _SpriteTex_ST;
            float4 _ClipRect;
            uniform fixed _Color;

            v2f vert(appdata_t v)
            {
                v2f o;
                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv0 = TRANSFORM_TEX(v.uv0, _MainTex);
                o.uv1 = TRANSFORM_TEX(v.uv1, _SpriteTex);

                //如果需要mask裁剪请复制下述代码 Begin
                o.worldPosition = v.vertex;
                //如果需要mask裁剪请复制上述代码 End
                o.color = v.color * _Color;

                return o;
            }


            fixed4  frag(v2f i) : SV_Target
            {
                half4 result = i.color * i.uv1.x;
                result.a *= (tex2D(_MainTex, i.uv0)).a;
                result += i.uv1.y * tex2D(_SpriteTex, i.uv0);


                //如果需要mask裁剪请复制下述代码 Begin
        #ifdef UNITY_UI_CLIP_RECT
                result.a *= UnityGet2DClipping(i.worldPosition.xy, _ClipRect);
        #endif

        #ifdef UNITY_UI_ALPHACLIP
                clip(result.a - 0.001);
        #endif
                //如果需要mask裁剪请复制上述代码 End

                return result;
            }
            ENDCG
        }
    }
}