Shader "Custom/3DWorldBoxShader"
{
    Properties
    {
        _MainTex ("Render Texture", 2D) = "white" {}
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
                // Sample the render texture
                fixed4 color = tex2D(_MainTex, i.uv);

                // Use depth logic for clipping (adjust based on your needs)
                float depth = i.uv.x * _ClipDistance;  // Simulating depth (replace with actual depth logic)

                if (depth > _ClipDistance)
                {
                    discard; // Clip parts outside the radius
                }

                return color;
            }
            ENDCG
        }
    }
    FallBack "Diffuse"
}
