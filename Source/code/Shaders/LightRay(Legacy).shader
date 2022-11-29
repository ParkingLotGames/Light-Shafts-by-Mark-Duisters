// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "LightShafts/LightShaftAdditive(Legacy)" {
Properties {

	_Intensity ("Shaft Intensity", Range(0.01,255.0)) = 1.0
	_ShaftColor ("Tint Color", Color) = (0.5,0.5,0.5,255)

	[HideInInspector]_SampleColor("Do not edit this",Color)= (255,255,255,255)

	_MainTex ("Particle Texture", 2D) = "white" {}

	_InvFade ("Soft Particles Factor", Range(0.01,3.0)) = 1.0

	
}

Category {
	Tags { "Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent" }


	SubShader {

		Pass {

			Blend SrcColor One
			ColorMask RGB
			Cull Off
			Lighting Off
			ZWrite Off


			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag

			#include "UnityCG.cginc"
			
			float _Intensity;
			fixed4 _ShaftColor;
			fixed4 _SampleColor;
			sampler2D _MainTex;
			
			struct appdata_t {
				float4 vertex : POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};

			struct v2f {
				float4 vertex : SV_POSITION;
				fixed4 color : COLOR;
				float2 texcoord : TEXCOORD0;
			};
			
			float4 _MainTex_ST;

			v2f vert (appdata_t v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.color = v.color;
				o.texcoord = TRANSFORM_TEX(v.texcoord,_MainTex);
				return o;
			}

			float _Fade;
			
			fixed4 frag (v2f i) : SV_Target
			{
				//this is the color we receive
				fixed4 col = 2.0f * i.color * _ShaftColor * _SampleColor * tex2D(_MainTex, i.texcoord)*(_Intensity * 0.01f);
				return col;
			}
			ENDCG 
		}
	}	
}
}
