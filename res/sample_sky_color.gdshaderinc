global uniform vec4 sky_zenith_color : source_color; 	// 61738c
global uniform vec4 sky_horizon_color : source_color; 	// a8a6a9
global uniform vec4 sky_nadir_color : source_color; 	// 707994

vec3 sample_sky_color(vec3 dir, vec3 light_dir) {
	float sky_y = acos(dir.y) / PI;
	
	vec3 sky_col = mix(sky_zenith_color.rgb, sky_horizon_color.rgb, clamp(pow(sky_y * 2.0, 5.0), 0.0, 1.0));
	vec3 ground_col = mix(sky_nadir_color.rgb, sky_horizon_color.rgb, clamp(pow(sky_y * 2.0, 5.0), 0.0, 1.0) - 1.0);
	
	float mix_value = (sin(PI * (4.0 * clamp(sky_y, 0.375, 0.625) - 2.0)) / 2.0) + 0.5;
	
	vec3 color = mix(sky_col, ground_col, mix_value);
	
	float sun = clamp(pow(dot(dir, light_dir), 300.0), 0.0, 1.0);
	float sunset = pow(1.0 - abs(light_dir.y), 2.0) * clamp(pow(dot(dir, light_dir), 3.0), 0.0, 1.0);
	
	color = mix(color, sky_horizon_color.rgb + vec3(0.2, -0.125, -0.125) * clamp(-0.5 * light_dir.y, 0.0, 1.0), sunset);
	
	color += sun;
	
	return color;
}