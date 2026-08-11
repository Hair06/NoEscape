Shader "Custom/XRaySeal"
{
    Properties
    {
        _MainTex ("Seal Texture", 2D) = "white" {}
        [HDR] _Color ("Seal Color", Color) = (1, 0, 0, 1) // Thêm [HDR] để kích hoạt thanh Intensity
    }
    SubShader
    {
        Tags { "Queue"="Overlay" "RenderType"="Transparent" }
        
        Cull Off       // Vẽ cả 2 mặt (không bị mất hình khi hạ camera thấp)
        ZTest Always   // Luôn nhìn xuyên tường/bệ đá
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

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
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _Color;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : COLOR
            {
                fixed4 col = tex2D(_MainTex, i.uv) * _Color;
                return col;
            }
            ENDCG
        }
    }
}