shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx, skip_vertex_transform;


uniform sampler2D texture_albedo : source_color, filter_nearest, repeat_enable;
uniform sampler2D smoothness_map : source_color, filter_nearest, repeat_enable;

uniform bool lod;

const mat4 dither_matrix = mat4(
	vec4(0, 8, 2, 10),
	vec4(12, 4, 14, 6),
	vec4(3, 11, 1, 9),
	vec4(15, 7, 13, 5)
);

varying float dist;
varying vec2 uv;
varying float vertex_id_interp;


void vertex() {
	//VERTEX.y += random(vec2(float(VERTEX_ID) + VERTEX.x, VERTEX.z)) * 0.0001f;

	VERTEX = (MODELVIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
	NORMAL = normalize((MODELVIEW_MATRIX * vec4(NORMAL, 0.0)).xyz);
	BINORMAL = normalize((MODELVIEW_MATRIX * vec4(BINORMAL, 0.0)).xyz);
	TANGENT = normalize((MODELVIEW_MATRIX * vec4(TANGENT, 0.0)).xyz);

	vec3 world_camera = (INV_VIEW_MATRIX * vec4(vec3(0.0), 1.0)).xyz;
	vec4 a = INV_VIEW_MATRIX * vec4(VERTEX, 1.0);

	dist = distance(a.xz, world_camera.xz);

	// Extract UV coordinates
	if (!lod) {
		uv = BONE_WEIGHTS.xy;
		uv.x /= BONE_WEIGHTS.z; // Correct for greedy meshing
	} else {
		uv = UV;
	}

	vertex_id_interp = float(VERTEX_ID);
}


void fragment() {
	// Get linear depth
	float depth_raw = FRAGCOORD.z; 

	vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth_raw); 
	vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0); 
	view.xyz /= view.w; 

	// Fade between chunks and LODs
	if (lod) {
		if (dither_matrix[int(mod(FRAGCOORD.x, 4.0))][int(mod(FRAGCOORD.y, 4.0))] > dist - 240.0) {
			discard;
		}
	} else {
		if (dither_matrix[int(mod(FRAGCOORD.x, 4.0))][int(mod(FRAGCOORD.y, 4.0))] < dist - 240.0) {
			discard;
		}
	}

	if (!lod && dist < 32.0) {
		DEPTH = FRAGCOORD.z;
	} else {
		DEPTH = FRAGCOORD.z;
	}

	// Default material shader
	vec4 albedo_tex = texture(texture_albedo, uv);
	ALBEDO = albedo_tex.rgb;

	ALPHA *= albedo_tex.a;
	ALPHA_SCISSOR_THRESHOLD = 0.5;

	float smoothness = texture(smoothness_map, uv).r;
	ROUGHNESS = 1.0 - smoothness;
	SPECULAR = smoothness;
	METALLIC = smoothness;


	if (lod) {
		ALPHA = 1.0;
	}


	// Pixelate shadows; credit: https://forum.godotengine.org/t/shader-for-pixelated-shadows-3d/115691/7
	vec2 tex_size = vec2(textureSize(texture_albedo, 0));
	vec2 nearest_texel = (floor(uv * tex_size) + 0.5) / tex_size;
	vec2 offset = nearest_texel - uv;
	vec2 x_derivative = dFdx(uv);
	vec2 y_derivative = dFdy(uv);

	offset /= x_derivative.s*y_derivative.t - y_derivative.s*x_derivative.t;
	vec2 frag_delta = vec2(
		offset.x*y_derivative.t - offset.y*y_derivative.s,
		offset.y*x_derivative.s - offset.x*x_derivative.t
	);
	vec3 view_delta = dFdx(VERTEX) * frag_delta.s + dFdy(VERTEX) * frag_delta.t;
	LIGHT_VERTEX += view_delta;
}