// Shader from http://wiki.unity3d.com/index.php?title=Blend_2_Textures

Shader "Blend 3 Textures" { 
 
Properties {
	_BlendNight ("BlendNight", Range (0, 1) ) = 0.5 
	_BlendDusk ("BlendDusk", Range (0, 1) ) = 0.5 
	_MainTex ("Texture 1", 2D) = "" 
	_Texture2 ("Texture 2", 2D) = ""
	_Texture3 ("Texture 3", 2D) = ""
}
 
SubShader {	
	//float bl;
	//float bl = 2 * (0.5 - abs(0.5 - _Blend));
	Pass {
		SetTexture[_MainTex]
		SetTexture[_Texture2] { 
			ConstantColor (0,0,0, [_BlendDusk]) 
			Combine texture Lerp(constant) previous
		}	
		SetTexture[_Texture3] { 
			ConstantColor (0,0,0, [_BlendNight]) 
			Combine texture Lerp(constant) previous
		}
	}
} 
 
}