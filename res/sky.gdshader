shader_type sky;

uniform vec3 sky_zenith_color : source_color; 	// 61738c
uniform vec3 sky_horizon_color : source_color; 	// a8a6a9
uniform vec3 sky_nadir_color : source_color; 	// 707994

uniform sampler2D star_noise;

void sky() {
	vec3 sky_col = mix(sky_zenith_color, sky_horizon_color, clamp(pow(SKY_COORDS.y * 2.0, 5.0), 0.0, 1.0));
	vec3 ground_col = mix(sky_nadir_color, sky_horizon_color, clamp(pow(SKY_COORDS.y * 2.0, 5.0), 0.0, 1.0) - 1.0);
	
	float mix_value = (sin(PI * (4.0 * clamp(SKY_COORDS.y, 0.375, 0.625) - 2.0)) / 2.0) + 0.5;
	
	COLOR = mix(sky_col, ground_col, mix_value);
	
	float sun = clamp(pow(dot(EYEDIR, LIGHT0_DIRECTION), 300.0), 0.0, 1.0);
	float sunset = pow(1.0 - abs(LIGHT0_DIRECTION.y), 2.0) * clamp(pow(dot(EYEDIR, LIGHT0_DIRECTION), 3.0), 0.0, 1.0);
	
	COLOR = mix(COLOR, sky_horizon_color + vec3(0.2, -0.125, -0.125) * clamp(-0.5 * LIGHT0_DIRECTION.y, 0.0, 1.0), sunset);
	
	COLOR += sun;

	vec2 star_coords = (SKY_COORDS * 4.0);
	COLOR = mix(COLOR, COLOR + texture(star_noise, star_coords).rgb, clamp(
		pow(1.0 - max(COLOR.r, max(COLOR.g, COLOR.b)), 16.0)
		, 0.0, 1.0));
}
