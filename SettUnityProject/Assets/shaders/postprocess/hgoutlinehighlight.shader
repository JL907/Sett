Shader "Stubbed Hopoo Games/Internal/Outline Highlight" {
	Properties {
		_Color ("Color", Vector) = (1,1,1,1)
		_MainTex ("Main Texture", 2D) = "black" {}
		_OutlineMap ("Occlusion Map", 2D) = "black" {}
	}
	
	Fallback "Diffuse"
}