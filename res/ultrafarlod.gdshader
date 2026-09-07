shader_type spatial;

uniform sampler2D heightmap;
uniform vec2 offset;
uniform float near_limit;
uniform vec3 far_color;

varying vec2 world_pos;
varying float destroy;

void vertex() {
	bool too_far = distance(VERTEX.xz, vec2(0, 0)) < near_limit;
	
	world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xz + offset;
	// It should really be `256.0 * texture...` but it's corrected for the fact that the UFLOD should never look higher than mountains
	VERTEX.y = too_far ? -1024.0 : 240.0 * texture(heightmap, world_pos / vec2(180000, 90000) + vec2(0.5)).r;
	
	destroy = too_far ? 1.0 : 0.0;
}

void fragment() {
	if (destroy > 0.1) {
		discard;
	}
	
	ALBEDO = far_color;//texture(heightmap, world_pos / vec2(180000, 90000) + vec2(0.5)).rgb;
	DEPTH = min(0.0001, FRAGCOORD.z / 2.0);
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
