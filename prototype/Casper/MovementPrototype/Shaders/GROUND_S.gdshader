shader_type spatial;

void vertex() {
	// Called for every vertex the material is visible on.
}

void fragment() {
    float square_size = 4.0; // Adjust this to set the checker size
    // Get world-space position of the fragment
    vec3 world_pos = (INV_VIEW_MATRIX * vec4(VERTEX, 1.0)).xyz;

    // Use XZ plane for the checkerboard pattern
    vec2 grid_pos = world_pos.xz;

    // Generate the checkerboard pattern
    vec2 grid = floor(grid_pos / square_size);
    float checker = mod(grid.x + grid.y, 2.0);

    // Mix between two colors (dark and light squares)
    vec3 color = mix(vec3(0.2), vec3(0.8), checker);
    ALBEDO = color;
}

//void light() {
//	// Called for every pixel for every light affecting the material.
//	// Uncomment to replace the default light processing function with this one.
//}
