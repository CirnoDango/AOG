shader_type canvas_item;

uniform vec4 beam_color : source_color = vec4(1.0, 0.3, 0.3, 1.0);
uniform float softness = 1.0; // 控制边缘过渡柔和度

void fragment() {
    // UV.y 从 0 → 1，中间 0.5 是光柱中心
    float center_falloff = 1.0 - abs(UV.y - 0.5) * 2.0;

    // 使用 smoothstep 让边缘过渡更柔和
    float fade = smoothstep(0.0, softness, center_falloff);

    COLOR = beam_color;
}