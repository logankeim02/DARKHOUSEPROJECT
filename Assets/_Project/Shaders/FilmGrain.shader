Shader "DarkHouse/FilmGrain"
{
    Properties
    {
        _Intensity  ("Intensity",   Range(0, 1))    = 0.08
        _GrainSize  ("Grain Size",  Range(0.5, 4))  = 1.0
        _Speed      ("Speed",       Range(0, 30))   = 15.0
        _Luminance  ("Luminance",   Range(0, 1))    = 0.5
    }

    SubShader
    {
        Tags
        {
            "Queue"           = "Overlay+100"
            "RenderType"      = "Transparent"
            "IgnoreProjector" = "True"
            "PreviewType"     = "Plane"
        }

        Cull Off
        Lighting Off
        ZWrite Off
        ZTest Always
        Blend SrcAlpha OneMinusSrcAlpha

        Pass
        {
            CGPROGRAM
            #pragma vertex   vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv     : TEXCOORD0;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
                float2 uv     : TEXCOORD0;
            };

            float _Intensity;
            float _GrainSize;
            float _Speed;
            float _Luminance;

            float Hash(float2 p)
            {
                p  = frac(p * float2(127.1, 311.7));
                p += dot(p, p + 74.27);
                return frac(p.x * p.y);
            }

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv     = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float  t          = floor(_Time.y * _Speed);
                float2 grainCoord = floor(i.uv * _ScreenParams.xy / _GrainSize);
                float  noise      = Hash(grainCoord + float2(t * 13.7, t * 7.3));

                float centered = noise - 0.5;
                float bright   = centered > 0 ? _Luminance + (1.0 - _Luminance) * noise : _Luminance * noise * 2.0;
                float alpha    = abs(centered) * 2.0 * _Intensity;

                return fixed4(bright, bright, bright, alpha);
            }
            ENDCG
        }
    }
}
