Shader "FX/Hand Shader" {
	Properties {
		_Color ("Empty Color", Color) = (0.5, 0.5, 0.5, 1)
	}

	SubShader {
		Tags { "Queue"="Transparent" "RenderType"="Transparent" }
		Blend SrcAlpha One
		Cull Off
		ZWrite Off

		Pass {
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			fixed4 _Color;
			sampler2D _CameraDepthTexture;

			struct appdata_t {
				float4 vertex : POSITION;
				float3 normal : NORMAL;
			};
			
			struct v2f {
				float4 vertex : POSITION;
				half fresnel : TEXCOORD3;
			};
			
			v2f vert(appdata_t v) {
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);				
				half3 viewDir = normalize(ObjSpaceViewDir(v.vertex));
				o.fresnel = (1.0 - abs(dot(v.normal, viewDir)));
				return o;
			}

			fixed4 frag(v2f i) : COLOR {
				fixed4 color = _Color;
				color.rgb = lerp(color.rgb, 1.0, i.fresnel * i.fresnel); // White rim lighting.
				color.a *= i.fresnel;
				return color;
			}
			ENDCG
		}
	}

	Fallback Off
}