//UNITY_SHADER_NO_UPGRADE
#ifndef SDFINCLUDE_INCLUDED
#define SDFINCLUDE_INCLUDED
#define PI 3.1415926535897932384626433832795
#define CIRCLE PI*2
float floormod(float x, float y)
{
  return x - y * floor(x/y);
}
float normalizeAngleCounterClockwise(float angle,float start) {
if (angle >= start)
return angle-start;
else
return CIRCLE-(start-angle);
}
float normalizeAngleClockwise(float angle,float start) {
if (start >= angle)
return start-angle;
else
return CIRCLE-(angle-start);
}

float closest(float a,float b, float value) {
    float2 distance = { abs(a-value),abs(b-value)};
    return distance.x < distance.y ? a : b;
}
float closest(float2 values, float value) {
    return abs(abs(values.x)-abs(value)) < abs(abs(values.y)-abs(value)) ? values.x : values.y;
}
float radianDistance(float alpha,float beta) {
    
    float phi = abs(abs(beta)-abs(alpha)) % CIRCLE;
    return phi;
    //return phi > PI ? CIRCLE - phi : phi; 
}
float closest(float3 values, float value) {
    float3 distances = { abs(abs(values.x)-abs(value)),abs(abs(values.y)-abs(value)),abs(abs(values.z)-abs(value)) };
    if (distances.x < distances.y) {
        if (distances.x < distances.z)
            return values.x;
        else 
            return values.z;
    }
    else {
        if (distances.y < distances.z)
            return values.y;
        else 
            return values.z;
    }
}
void SDFSample_float(float SDF, float Offset,out float Out) {
    Out = SDF - Offset;
    Out = saturate(-Out / fwidth(Out));
}
void SDFSampleStrip_float(float SDF, float2 Offset,out float Out) {
    Out = max(-(SDF - Offset.x), SDF - Offset.y);
    Out = saturate(-Out / fwidth(Out));
}
void SDFLine_float(float2 UV, float Position,float Width,out float SDF) {
    SDF = abs(UV.x - Position) - 0.5*Width;
}
void SDFLineSegment_float(float2 UV,float2 PositionStart,float2 PositionEnd,out float SDF) {
    float2 heading = (PositionEnd-PositionStart);
    float magnitudeMax = length(heading);
    heading = normalize(heading);
    float dotP = clamp(dot(UV - PositionStart,heading),0,magnitudeMax);
    float2 closest = PositionStart + heading * dotP;
    SDF = distance(UV,closest);
}
void SDFCircle_float(float2 UV, float2 Position,float Radius,out float SDF) {
    SDF = length(UV - Position) - Radius;
}
void SDFRing_float(float2 UV, float2 Position,float Radius,out float SDF) {
    float2 delta = UV - Position;
    float angle = atan2(delta.y,delta.x);
    float2 target;
    target.x = Radius*cos(angle)+Position.x;
    target.y = Radius*sin(angle)+Position.y;
    SDF = distance(UV,target);
}
void SDFArc_float(float2 UV, float2 Position,float Radius,float Angle,float Length,out float SDF) {
    float2 delta = UV - Position;
    float angle = atan2(delta.y,delta.x);
    float2 range;
    if (Length > 0)
        range = float2(Angle, Angle+Length);
    else 
        range = float2(Angle+Length, Angle);
    if (!(angle >= range.x && angle <= range.y) && !(angle >= range.x-CIRCLE && angle <= range.y-CIRCLE) && !(angle >= range.x+CIRCLE && angle <= range.y+CIRCLE)) {
        float3 distances = { closest(range,angle), closest(range-CIRCLE,angle), closest(range+CIRCLE,angle) };
        angle = closest(distances,angle);
    }
    SDF = distance(UV,float2(Radius*cos(angle)+Position.x, Radius*sin(angle)+Position.y));
}
void SDFRectangle_float(float2 UV, float2 Position,float Width, float Height, float CornerRadius,out float SDF) {
    float2 d = abs(UV-Position) - 0.5*float2(Width,Height);
    SDF = min(max(d.x,d.y),0) + length(max(d,0));
    SDF = SDF - CornerRadius;
}
void SDFPolygon_float(float2 UV, float2 Position,float Radius,float Sides,float CornerRadius,out float SDF) {
    float2 f = UV - Position;
	float theta = atan2(f.y, f.x);
	float angle = 6.2831853071/Sides;
	float SinSide, CosSide;
	sincos(round(theta / angle) * angle, SinSide, CosSide); 
    float2 d = float2(SinSide, -CosSide); 
	float2 n = float2(CosSide, SinSide);
    float t = dot(d, f);
    float sideLength = Radius * tan(0.5*angle);
    SDF = abs(t) < Radius * tan(0.5*angle) ? dot(f, n) - Radius : length(f - (Radius * n + d * clamp(dot(d, f), -sideLength, sideLength)));
    SDF = SDF - CornerRadius;
}

void SDFBooleanUnion_float(float A,float B,float Out) {
    Out = min(A, B);
}
void SDFBooleanIntersection_float(float A,float B,float Out) {
    Out = max(A, B);  
}
void SDFBooleanDifference_float(float A,float B,float Out) {
    Out = max(A, -B);
}
void SDFBooleanSoftUnion_float(float A,float B,float Smoothing,float Out) {
    float t = clamp(0.5 * (1 + (B - A) / Smoothing), 0, 1);
    Out = lerp(B, A, t) - Smoothing * t * (1 - t);
}
void SDFBooleanSoftIntersection_float(float A,float B,float Smoothing,float Out) {
    float t = clamp(0.5 * (1 + (A - B) / Smoothing), 0, 1);
    Out = -(lerp(-B, -A, t) - Smoothing * t * (1 - t));
}
void SDFBooleanSoftDifference_float(float A,float B,float Smoothing,float Out) {
    float t = clamp(0.5 * (1 + (A + B) / Smoothing), 0, 1);
    Out = -(lerp(B, -A, t) - Smoothing * t * (1 - t));
}

#endif