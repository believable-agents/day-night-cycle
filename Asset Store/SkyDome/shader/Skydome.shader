Shader "RenderFX/Skydome" {
Properties {
	_Color ("Color", Color) = (.5, .5, .5, .5)
	_MainTex ("Base (RGB)", 2D) = "white" {}
}

SubShader {
	Tags { "Queue" = "Background" }
	Cull Off
	ZWrite On
	ZTest Always
	Fog { Mode Off }
	Lighting Off		
	Color [_Color]
	Pass {
			SetTexture [_MainTex] {constantColor [_Color] Combine texture * primary DOUBLE, texture * constant}
	}
}
}