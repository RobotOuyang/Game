Shader "MirrorUI/Blur"{
	Properties{
		_Color("Color", Color) = (1,1,1,1)
		_blurSizeXY("BlurSizeXY", Range(1,20)) = 2
	}
		SubShader{

		// Draw ourselves after all opaque geometry
		Tags{ "Queue" = "Transparent" }

		// Grab the screen behind the object into _GrabTexture
		GrabPass{}

		// Render the object with the texture generated above
		Pass{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag 
			#pragma target 3.0
			#include "UnityCG.cginc"

			sampler2D _GrabTexture : register(s0);
			float _blurSizeXY;
			float4 _Color;

			uniform half4 _GrabTexture_TexelSize;

			static const half4 curve4[7] = { half4(0.0205,0.0205,0.0205,0), half4(0.0855,0.0855,0.0855,0), half4(0.232,0.232,0.232,0),
				half4(0.324,0.324,0.324,1), half4(0.232,0.232,0.232,0), half4(0.0855,0.0855,0.0855,0), half4(0.0205,0.0205,0.0205,0) };

			struct appdata_t
			{
				float4 vertex   : POSITION;
			};

			struct v2f {
				float4 position : SV_POSITION;
				float4 screenPos : TEXCOORD0;
			};

			v2f vert(appdata_t v) {
				v2f o;
				o.position = UnityObjectToClipPos(v.vertex);

				o.screenPos = ComputeScreenPos(o.position);
				return o;
			}

			half4 frag(v2f i) : COLOR
			{
				half2 screenPos = i.screenPos.xy / i.screenPos.w;
				screenPos.x = screenPos.x - 3.0 * _GrabTexture_TexelSize.x;
				
#ifdef UNITY_UV_STARTS_AT_TOP
				screenPos.y = 1.0 - screenPos.y;
#endif

				half4 sum = half4(0.0h, 0.0h, 0.0h, 0.0h);

				for (int i = 0; i < 7; i++)
				{
					sum += tex2D(_GrabTexture, screenPos) * curve4[i];
					screenPos.x += _GrabTexture_TexelSize.x;
				}

				screenPos.x -= 3 * _GrabTexture_TexelSize.x;
				screenPos.y -= 3 * _GrabTexture_TexelSize.y;
				for (int i = 0; i < 7; i++)
				{
					sum += tex2D(_GrabTexture, screenPos) * curve4[i];
					screenPos.y += _GrabTexture_TexelSize.y;
				}
				sum *= _Color;

				return sum / 2;
			}
			ENDCG
		}
	}
	Fallback Off
}