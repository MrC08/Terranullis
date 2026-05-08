shader_type spatial;

uniform sampler2D heightmap;
varying vec2 world_pos;

void vertex() {
	world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xz;
	VERTEX.y = 256.0 * texture(heightmap, world_pos / vec2(180000, 90000) + vec2(0.5)).r;
}

void fragment() {
	ALBEDO = texture(heightmap, world_pos / vec2(180000, 90000) + vec2(0.5)).rgb;
	DEPTH = min(0.0001, FRAGCOORD.z / 2.0);
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
