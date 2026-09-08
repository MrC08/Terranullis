shader_type spatial;
render_mode unshaded;

global uniform vec4 near_fade_color;
global uniform vec3 sun_dir;

uniform sampler2D heightmap;
uniform vec2 offset;
uniform float near_limit;
uniform vec3 far_color;

varying vec2 world_pos;
varying float destroy;
varying vec3 angle;
varying float fade;

#include "sample_sky_color.gdshaderinc"

void vertex() {
	bool too_close = distance(VERTEX.xz, vec2(0, 0)) < near_limit;
	fade = (distance(VERTEX.xz, vec2(0, 0)) - near_limit) / (8192.0 - near_limit);
	angle = normalize(vec3(VERTEX.x, 0.0, VERTEX.z));
	
	world_pos = (MODEL_MATRIX * vec4(VERTEX, 1.0)).xz + offset;
	// It should really be `256.0 * texture...` but it's corrected for the fact that the UFLOD should never look higher than mountains
	VERTEX.y = too_close ? -1024.0 : 240.0 * texture(heightmap, world_pos / vec2(180000, 90000) + vec2(0.5)).r;
	
	destroy = too_close ? 1.0 : 0.0;

	// Pin to the far plane in clip space so this never gets frustum-clipped, regardless of
	// how far VERTEX actually is. Godot's Forward+ renderer uses reverse-Z, so far = 0.0 here
	// (not 1.0, like the classic non-reversed version of this trick).
	POSITION = PROJECTION_MATRIX * MODELVIEW_MATRIX * vec4(VERTEX, 1.0);
	POSITION.z = 0.0000000001;
}

void fragment() {
	if (destroy > 0.1) {
		discard;
	}

	//texture(heightmap, world_pos / vec2(180000, 90000) + vec2(0.5)).rgb;
	ALBEDO = sample_sky_color(angle, sun_dir);
	ALBEDO = mix(ALBEDO + near_fade_color.rgb, ALBEDO, fade);
	DEPTH = min(0.0001, FRAGCOORD.z / 2.0);
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
