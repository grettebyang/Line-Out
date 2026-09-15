shader_type spatial;
render_mode blend_mix, unshaded, cull_disabled;

uniform vec4 edge_color : source_color = vec4(1.0, 1.0, 1.0, 0.1);
uniform float fresnel_power : hint_range(1, 10) = 3.0;
uniform float edge_thickness : hint_range(0, 1) = 0.3;
uniform float fade_amount : hint_range(0, 1) = 1.0;
uniform float fade_progress : hint_range(0, 1) = 0.0;

void fragment() {
    float f = pow(1.0 - abs(dot(normalize(NORMAL), normalize(VIEW))), fresnel_power);
    float outline = smoothstep(edge_thickness, edge_thickness + 0.1, f);

    ALBEDO = edge_color.rgb * outline;
    ALPHA = outline * edge_color.a * fade_amount * (1.0 - fade_progress);
}