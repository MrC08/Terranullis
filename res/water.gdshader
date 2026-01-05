shader_type spatial;

uniform sampler2D noisemap : source_color;

varying vec3 pos;

void vertex() {
	pos = (INV_VIEW_MATRIX * (MODELVIEW_MATRIX * vec4(VERTEX, 1.0))).xyz;
}

void fragment() {
	ALBEDO = vec3(0.2, 0.4, 0.8) * (texture(noisemap, pos.xz) * 0.1 + 0.9).r;
	NORMAL = vec3(0.0, 1.0, 0.0);

	ROUGHNESS = 0.0;
	SPECULAR = 1.0;

	ALPHA = 0.5;
}