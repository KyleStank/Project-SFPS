Shader "Hidden/AstarPathfindingProject/Navmesh Outline" {
Properties {
	_Color ("Main Color", Color) = (1,1,1,0.5)
	_MainTex ("Texture", 2D) = "white" { }
	_Scale ("Scale", float) = 1
	_FadeColor ("Fade Color", Color) = (1,1,1,0.3)
}
SubShader {

	Pass {
		ZTest LEqual
		Lighting Off
		Blend SrcAlpha OneMinusSrcAlpha
		ZWrite Off
		Offset -2, -50

		CGPROGRAM
		#pragma vertex vert
		#pragma fragment frag

		float4 _Color;
		sampler2D _MainTex;
		float _Scale;

		struct appdata_color {
			float4 vertex : POSITION;
			fixed4 color : COLOR;
			float4 normal : NORMAL;
		};

		struct v2f {
			float4  pos : SV_POSITION;
			float2  uv : TEXCOORD0;
			half4 col : COLOR;
		};

		v2f vert (appdata_color v) {
			v2f o;
			float4 p1 = mul(UNITY_MATRIX_MVP, v.vertex);
			float4 p2 = mul(UNITY_MATRIX_MVP, v.vertex + v.normal);
			float4 p1s = p1/p1.w;
			float4 p2s = p2/p2.w;

			float4 delta = p2s - p1s;
			float2 screenSpaceNormal = float2(-delta.y, delta.x);
			screenSpaceNormal = normalize(screenSpaceNormal) / _ScreenParams.xy;
			float4 sn = float4(screenSpaceNormal.x, screenSpaceNormal.y, 0, 0) * (v.color.a-0.5);
			o.pos = p1s + sn*8;

			// Multiply by w because homogeneous coordinates
			o.pos *= p1.w;

			o.col = v.color;
			o.col.a = 1;
			o.uv = float2(v.color.a, 0);
			return o;
		}

		half4 frag (v2f i) : COLOR {
			return tex2D(_MainTex, i.uv) * i.col;
		}
		ENDCG

		}


	}
Fallback "None"
}
