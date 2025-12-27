shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_burley, specular_schlick_ggx, skip_vertex_transform;

uniform sampler2D texture_albedo : source_color, filter_nearest, repeat_enable;
uniform float alpha_scissor_threshold : hint_range(0.0, 1.0, 0.001);

uniform float roughness : hint_range(0.0, 1.0);
uniform sampler2D texture_metallic : hint_default_white, filter_nearest, repeat_enable;
uniform vec4 metallic_texture_channel;
uniform sampler2D texture_roughness : hint_roughness_r, filter_nearest, repeat_enable;

uniform float specular : hint_range(0.0, 1.0, 0.01);
uniform float metallic : hint_range(0.0, 1.0, 0.01);

uniform vec3 sky_color : source_color;

uniform bool lod;

const float cam_near = 0.05; 
const float cam_far = 4000.0;

const mat4 dither_matrix = mat4(
	vec4(0, 8, 2, 10),
	vec4(12, 4, 14, 6),
	vec4(3, 11, 1, 9),
	vec4(15, 7, 13, 5)
);

varying float dist;

void vertex() {
	VERTEX = (MODELVIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;
	NORMAL = normalize((MODELVIEW_MATRIX * vec4(NORMAL, 0.0)).xyz);
	BINORMAL = normalize((MODELVIEW_MATRIX * vec4(BINORMAL, 0.0)).xyz);
	TANGENT = normalize((MODELVIEW_MATRIX * vec4(TANGENT, 0.0)).xyz);

	vec3 world_camera = (INV_VIEW_MATRIX * vec4(vec3(0.0), 1.0)).xyz;
	vec4 a = INV_VIEW_MATRIX * vec4(VERTEX, 1.0);

	dist = distance(a.xz, world_camera.xz);
}


void fragment() {
	// Get linear depth
	float depth_raw = FRAGCOORD.z; 

	vec3 ndc = vec3(SCREEN_UV * 2.0 - 1.0, depth_raw); 
	vec4 view = INV_PROJECTION_MATRIX * vec4(ndc, 1.0); 
	view.xyz /= view.w; 
	
	float depth_linear = -view.z; 
	
	float depth_normalized = (depth_linear - cam_near) / (cam_far - cam_near);

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


	// Default material shader
	vec4 albedo_tex = texture(texture_albedo, UV);
	ALBEDO = albedo_tex.rgb;

	float metallic_tex = dot(texture(texture_metallic, UV), metallic_texture_channel);
	METALLIC = metallic_tex * metallic;
	SPECULAR = specular;

	vec4 roughness_texture_channel = vec4(1.0, 0.0, 0.0, 0.0);
	float roughness_tex = dot(texture(texture_roughness, UV), roughness_texture_channel);
	ROUGHNESS = roughness_tex * roughness;
	ALPHA *= albedo_tex.a;
	ALPHA_SCISSOR_THRESHOLD = alpha_scissor_threshold;


	// Fade with distance
	ALBEDO.rgb = mix(ALBEDO.rgb, sky_color, pow(depth_normalized, 1.25));


	// Pixelate shadows; credit: https://forum.godotengine.org/t/shader-for-pixelated-shadows-3d/115691/7
	vec2 tex_size = vec2(textureSize(texture_albedo, 0));
	vec2 nearest_texel = (floor(UV * tex_size) + 0.5) / tex_size;
	vec2 offset = nearest_texel - UV;
	vec2 x_derivative = dFdx(UV);
	vec2 y_derivative = dFdy(UV);

	offset /= x_derivative.s*y_derivative.t - y_derivative.s*x_derivative.t;
	vec2 frag_delta = vec2(
		offset.x*y_derivative.t - offset.y*y_derivative.s,
		offset.y*x_derivative.s - offset.x*x_derivative.t
	);
	vec3 view_delta = dFdx(VERTEX) * frag_delta.s + dFdy(VERTEX) * frag_delta.t;
	LIGHT_VERTEX += view_delta;
}