Shader "Custom/ClippingMask"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _CarPosition ("Car Position", Vector) = (0,0,0,0)
        _Radius ("Visible Radius", Float) = 5.0
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
                float3 worldPos : TEXCOORD1;
            };

            sampler2D _MainTex;
            float4 _CarPosition;
            float _Radius;

            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
                o.uv = v.uv;
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                float distanceFromCar = distance(i.worldPos, _CarPosition.xyz);

                if (distanceFromCar > _Radius)
                {
                    discard; // Clip the pixel
                }

                // Create a fading effect near the radius limit
                float fade = saturate((_Radius - distanceFromCar) / _Radius);
                fixed4 color = tex2D(_MainTex, i.uv);
                color.a = fade;  // Adjust transparency
                return color;
            }

            ENDCG
        }
    }
    FallBack "Diffuse"
}
