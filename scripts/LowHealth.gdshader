shader_type canvas_item;

uniform float alpha = 0.1;
uniform float inner_radius = 0.0;
uniform float outer_radius = 4;

void fragment() {
	float x = abs(UV.r-.5) * 1.5;
	float y = abs(UV.g-.5)*2.0;
	float q = 1.0-(1.0-sqrt(x*x+y*y)/outer_radius)/(1.0-inner_radius);
	COLOR = vec4(0.5, 0, 0, q*(alpha * sin(3.0*TIME) + 0.9));
}