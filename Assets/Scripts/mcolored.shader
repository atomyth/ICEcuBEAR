// Upgrade NOTE: replaced 'UNITY_INSTANCE_ID' with 'UNITY_VERTEX_INPUT_INSTANCE_ID'

// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Simplified Diffuse shader. Differences from regular Diffuse one:
// - no Main Color
// - fully supports only 1 directional light. Other lights can affect it, but it will be per-vertex/SH.

Shader "Mobile/Colored" {
Properties {
	_color("color",Color) = (1,1,1,1)
}
SubShader {
	Tags { "RenderType"="Opaque" }
	LOD 150

CGPROGRAM
#pragma surface surf Lambert noforwardadd
#pragma multi_compile_instancing
UNITY_INSTANCING_CBUFFER_START (MyProperties)
UNITY_DEFINE_INSTANCED_PROP(fixed4,_color)
UNITY_INSTANCING_CBUFFER_END

//ed4 _color;

struct Input {
	float2 uv_MainTex;
	UNITY_VERTEX_INPUT_INSTANCE_ID
};

void surf (Input IN, inout SurfaceOutput o) {
	UNITY_SETUP_INSTANCE_ID(IN);
	fixed4 c = UNITY_ACCESS_INSTANCED_PROP(_color);
	o.Albedo = c.rgb;
	o.Alpha = c.a;
}
ENDCG
}

Fallback "Mobile/VertexLit"
}
