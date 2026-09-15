shader_type spatial;
render_mode blend_mix,depth_draw_opaque,cull_back,diffuse_burley,specular_schlick_ggx;

// Textures for material A, for Material B and for the transition noise
group_uniforms textures;
uniform sampler2D texture_albedo_a : source_color,filter_linear_mipmap,repeat_enable;
uniform sampler2D texture_orm_a : hint_roughness_g,filter_linear_mipmap,repeat_enable;
uniform sampler2D texture_normal_a : hint_roughness_normal,filter_linear_mipmap,repeat_enable;
uniform sampler2D texture_albedo_b : source_color,filter_linear_mipmap,repeat_enable;
uniform sampler2D texture_orm_b : hint_roughness_g,filter_linear_mipmap,repeat_enable;
uniform sampler2D texture_normal_b : hint_roughness_normal,filter_linear_mipmap,repeat_enable;
uniform sampler2D texture_noise : source_color,filter_linear_mipmap,repeat_enable;

group_uniforms main_settings;
uniform vec4 albedo_a : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform vec4 albedo_b : source_color = vec4(1.0, 1.0, 1.0, 1.0);
uniform float normal_scale : hint_range(-16,16) = 1.0;
uniform float ao_light_affect = 1.0;
uniform vec3 uv1_scale = vec3(1.0, 1.0, 1.0);
uniform vec3 uv1_offset;

group_uniforms blend_settings;
uniform float blend_threshold = 0.5;
uniform float blend_noise_intensity = 0.5;
uniform float blend_noise_scale = 5.0;
uniform float blend_softness = 0.05;
uniform vec4 blend_transition_color : source_color = vec4(0.0, 0.0, 0.0, 0.25);

void vertex() {
	UV=UV*uv1_scale.xy+uv1_offset.xy;
}

void fragment() {
	// UVs
	vec2 base_uv = UV;
	vec2 noise_uv = UV * blend_noise_scale;

    // get vertex color mask value
    float mask = COLOR.r;

    // compute noise value (RED channel only)
    float noise = texture(texture_noise, noise_uv).r * blend_noise_intensity;

    // compute threshold
    float threshold = clamp(blend_threshold + noise, 0.0, 1.0);

    // compute blend
    float blend = smoothstep(threshold - blend_softness, threshold + blend_softness, mask);

	// albedo
	vec4 albedo_tex_a = texture(texture_albedo_a, base_uv) * albedo_a.rgba;
	vec4 albedo_tex_b = texture(texture_albedo_b, base_uv) * albedo_b.rgba;
	vec3 albedo_mixed = mix(albedo_tex_a.rgb, albedo_tex_b.rgb, blend);
    float transition = clamp(1.0 - smoothstep(blend_softness * 0.5, blend_softness * 1.5, abs(mask - threshold)), 0.0, 1.0);
    vec3 transition_visual = mix(albedo_mixed.rgb, blend_transition_color.rgb, transition * blend_transition_color.a);
	ALBEDO = transition_visual;

	// rough + metal
	vec4 orm_tex_a = texture(texture_orm_a, base_uv);
	vec4 orm_tex_b = texture(texture_orm_b, base_uv);
	ROUGHNESS = mix(orm_tex_a.g, orm_tex_b.g, blend);
	METALLIC = mix(orm_tex_a.b, orm_tex_b.b, blend);

	// normal
	vec3 normal_map_a = texture(texture_normal_a, base_uv).rgb;
	vec3 normal_map_b = texture(texture_normal_b, base_uv).rgb;
	NORMAL_MAP = mix(normal_map_a, normal_map_b, blend);
	NORMAL_MAP_DEPTH = normal_scale;

	// ao
	AO = mix(orm_tex_a.r, orm_tex_b.r, blend);;
	AO_LIGHT_AFFECT = ao_light_affect;
}
