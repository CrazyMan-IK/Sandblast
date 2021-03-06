Shader "Unlit/MarkIlsands"
{
	SubShader
	{
		Tags{ "RenderType" = "Opaque" }
		LOD	   100
		ZTest  Off
		ZWrite Off
		Cull   Off

		Pass
		{
			CGPROGRAM
			#pragma vertex   vert
			#pragma fragment frag


			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex   : POSITION;
				float2 uv	    : TEXCOORD0;
			};

			struct v2f
			{
				float4 vertex   : SV_POSITION;
			};

			v2f vert(appdata v)
			{
				v2f o;

				float2 uvRemapped   = v.uv.xy;
					   uvRemapped.y = 1.0 - uvRemapped.y;
					   uvRemapped   = uvRemapped * 2.0 - 1.0;

					   o.vertex		= float4(uvRemapped.xy, 0.0, 1.0);

				return o;
			}

			fixed4 frag(v2f i) : SV_Target
			{
				return float4(1.0, 0.0, 0.0, 1.0);
			}
			ENDCG
		}
	}
}
