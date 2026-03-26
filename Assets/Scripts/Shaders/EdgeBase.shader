Shader "Unlit/EdgeBase"
{
    Properties
    {
        _BackgroundColor ("Background", Color) = (0, 0, 1, 1)
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma target 4.5

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                //UNITY_FOG_COORDS(1)
                float4 vertex : SV_POSITION;
            };

            fixed4 _BackgroundColor;

            StructuredBuffer<float4> _Colors;
            StructuredBuffer<float> _Thresholds;
            uint _ColorsCount;

            v2f vert (appdata v)
            {
                v2f o;

                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                return o;
            }

            fixed4 frag (v2f input) : SV_Target
            {
                if (_ColorsCount == 0) return _BackgroundColor;

                for (uint i = 0; i < _ColorsCount; ++i)
                    if (abs(1 - input.uv.x) < _Thresholds[i])
                        return _Colors[i];

                return _BackgroundColor;
            }
            ENDCG
        }
    }
}
