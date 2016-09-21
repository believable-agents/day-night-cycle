Shader "Particles/Additive (Soft) NoFog" {
Properties {
	_MainTex ("Particle Texture", 2D) = "white" {}
}

Category {
	Tags { "Queue" = "Transparent" }
	Blend One OneMinusSrcColor
	ColorMask RGB
	Cull Off Lighting Off ZWrite Off Fog { Color (0,0,0,0) }
	BindChannels {
		Bind "Color", color
		Bind "Vertex", vertex
		Bind "TexCoord", texcoord
	}
	
	// ---- Dual texture cards
	SubShader {
		Pass {
			Fog { Mode Off }
			SetTexture [_MainTex] {
				combine texture * primary
			}
			SetTexture [_MainTex] {
				combine previous * previous alpha, previous
			}
		}
	}
	
	// ---- Single texture cards (does not do particle colors)
	SubShader {
		Pass {
			Fog { Mode Off }
			SetTexture [_MainTex] {
				combine texture * texture alpha, texture
			}
		}
	}
}
}