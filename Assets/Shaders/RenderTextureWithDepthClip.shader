Shader "Custom/RenderTextureWithDepthClip"
{
    Properties
    {
        _MainTex ("Render Texture", 2D) = "white" {}
        _DepthTex ("Depth Texture", 2D) = "white" {}  // Depth texture from the secondary camera
        _ClipDistance ("Clip Distance", Float) = 5.0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 200

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _DepthTex;
            float _ClipDistance;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Sample the main render texture (from the secondary camera)
                fixed4 color = tex2D(_MainTex, i.uv);

                // Sample the depth texture
                float depth = tex2D(_DepthTex, i.uv).r;

                // Clip based on depth
                if (depth > _ClipDistance)
                {
                    discard;
                }

                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
