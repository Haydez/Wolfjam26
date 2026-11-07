// ShapeGradient.hlsl
// Shader Graph Custom Function (File mode), function name: ShapeGradient
//
// Procedural replacement for the gradient PNGs. Builds a signed distance field (SDF)
// for a shape and returns:
//   D    : 0 at the shape's center, 1 at its edge, keeps rising (>1) outside it
//   Mask : 1 inside the shape, 0 outside, with an anti-aliased edge
//
// Shape: 0 = circle, 1 = square, 2 = triangle, 3 = heart
// Size : how much of the quad the shape fills (0.8 = 80%)
//
// SDF formulas adapted from Inigo Quilez, https://iquilezles.org/articles/distfunctions2d/

#ifndef SHAPE_GRADIENT_INCLUDED
#define SHAPE_GRADIENT_INCLUDED

float SG_Box(float2 p, float b)
{
    float2 q = abs(p) - b;
    return length(max(q, 0.0)) + min(max(q.x, q.y), 0.0);
}

float SG_Triangle(float2 p, float r)
{
    const float k = 1.7320508; // sqrt(3)
    p.x = abs(p.x) - r;
    p.y = p.y + r / k;
    if (p.x + k * p.y > 0.0) p = float2(p.x - k * p.y, -k * p.x - p.y) / 2.0;
    p.x -= clamp(p.x, -2.0 * r, 0.0);
    return -length(p) * sign(p.y);
}

float SG_Heart(float2 p)
{
    p.x = abs(p.x);
    if (p.y + p.x > 1.0)
        return length(p - float2(0.25, 0.75)) - 0.35355339; // sqrt(2)/4
    float2 a = p - float2(0.0, 1.0);
    float2 b = p - 0.5 * max(p.x + p.y, 0.0);
    return sqrt(min(dot(a, a), dot(b, b))) * sign(p.x - p.y);
}

void ShapeGradient_float(float2 UV, float Shape, float Size, out float D, out float Mask)
{
    // Centered coords: -1..1 across the shape when Size = 1.
    float2 p = (UV - 0.5) * 2.0 / max(Size, 1e-4);

    int   s = (int)round(Shape);
    float sd;      // signed distance: negative inside
    float depth;   // distance from edge to deepest point, used to normalize D to 0..1

    if (s == 1)      { sd = SG_Box(p, 0.8);                         depth = 0.8; }
    else if (s == 2) { sd = SG_Triangle(p + float2(0, 0.15), 1.0);  depth = 0.57735027; } // r / sqrt(3)
    else if (s == 3) { sd = SG_Heart(p * 0.62 + float2(0, 0.52));   depth = 0.41365747; } // heart's inner depth
    else             { sd = length(p) - 1.0;                        depth = 1.0; }

    D = 1.0 + sd / depth;

    // Soft 1-pixel edge so the outline doesn't stair-step.
    float aa = max(fwidth(sd), 1e-5);
    Mask = 1.0 - smoothstep(-aa, aa, sd);
}

void ShapeGradient_half(half2 UV, half Shape, half Size, out half D, out half Mask)
{
    float d, m;
    ShapeGradient_float(UV, Shape, Size, d, m);
    D = d; Mask = m;
}

#endif
