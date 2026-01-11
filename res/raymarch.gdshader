shader_type spatial;
// Prevent the quad from being affected by lighting and fog. This also improves performance.
render_mode unshaded, fog_disabled;

uniform sampler2D global_noise;

void vertex() {
	POSITION = vec4(VERTEX.xy, 1.0, 1.0);
}

vec2 pos_to_noise_uv(vec2 v) {
	return ((v + vec2(180000.0, 90000.)) / vec2(360000.0, 180000.0)).yx;
}

float get_height(vec2 pos) {
	vec2 uv = pos_to_noise_uv(pos);
	return texture(global_noise, uv).r;
}

vec3 rotateX(vec3 v, float angle) {
	return mat3(
		vec3(1.0, 0.0, 0.0),
		vec3(0.0, cos(angle), sin(angle)),
		vec3(0.0, -sin(angle), cos(angle))
	) * v;
}

vec3 rotateY(vec3 v, float angle) {
	return mat3(
		vec3(cos(angle), 0.0, -sin(angle)),
		vec3(0.0, 1.0, 0.0),
		vec3(sin(angle), 0.0, cos(angle))
	) * v;
}

vec3 rotateZ(vec3 v, float angle) {
	return mat3(
		vec3(cos(angle), sin(angle), 0.0),
		vec3(-sin(angle), cos(angle), 0.0),
		vec3(0.0, 0.0, 1.0)
	) * v;
}

void fragment() {
	ALBEDO = vec3(0.0);//ALBEDO = vec3(0.3, 0.5, 0.7);
	ALPHA = 0.0;
	vec3 pos = CAMERA_POSITION_WORLD;

	float aspect = VIEWPORT_SIZE.x / VIEWPORT_SIZE.y;
	float fov2 = radians(80.0) / 2.0;

	// convert coordinates from [0, 1] to [-1, 1]
	// and invert y axis to flow from bottom to top
	vec2 screenCoord = (SCREEN_UV - 0.5) * 2.0;
	screenCoord.x *= aspect;
	screenCoord.y = -screenCoord.y;

	vec2 offsets = screenCoord * tan(fov2);

	vec3 rayFront = vec3(1.0, 0.0, 0.0);
	vec3 rayRight = normalize(cross(rayFront, vec3(0.0, 1.0, 0.0)));
	vec3 rayUp = cross(rayRight, rayFront);
	vec3 dir = rayFront + rayRight * offsets.x + rayUp * offsets.y;
	
	dir = rotateZ(dir, tan(-CAMERA_DIRECTION_WORLD.y));
	dir = mat3(
		vec3(-CAMERA_DIRECTION_WORLD.z, 0.0, CAMERA_DIRECTION_WORLD.x),
		vec3(0.0, 1.0, 0.0),
		vec3(-CAMERA_DIRECTION_WORLD.x, 0.0, -CAMERA_DIRECTION_WORLD.z)
	) * dir;
	//rotateY(dir, (CAMERA_DIRECTION_WORLD.z));

	//vec3 dir = CAMERA_DIRECTION_WORLD * vec3(1.0, -1.0, 1.0);
	for (float step = 0.0; step < 256.0; step++) {
		if (pos.y < get_height(pos.xz)) {
			ALPHA = 1.0;
			break;
		}

		pos += dir * 32.0;
		ALBEDO += (1.0 / 256.0);
	}

	DEPTH = 0.0;
	//ALPHA = texture(global_noise, pos_to_noise_uv(CAMERA_POSITION_WORLD.xz)).r / 128.0;
}