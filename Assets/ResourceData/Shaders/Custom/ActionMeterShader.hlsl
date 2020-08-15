//UNITY_SHADER_NO_UPGRADE
#ifndef PI
#define PI = 3.14159265359f
#endif
#ifndef ACTIONMETERSHADERINCLUDE_INCLUDED
#define ACTIONMETERSHADERINCLUDE_INCLUDED
float ToFullRotation(float angle) {
    return angle * (angle >= 0) + (angle+(2*PI)) * (angle < 0);
}
float Closest(float value,float lower,float upper,float otherLower) {
    float distLower = abs(value-lower);
    float distUpper = abs(value-upper);
    float distOtherLower = abs(value-otherLower);
    return lower*(distLower < distUpper || distOtherLower < distUpper)+upper*(distLower >= distUpper && distOtherLower >= distUpper);
}
void ProgressBar_float(float2 UV,float2 Center,float Percentage,float Radius,float Thickness,out float Out) {
    float2 delta = UV - Center;
    float2 polar = {length(delta)*2, ToFullRotation(atan2(delta.y, delta.x))};
    float stride = (Percentage*2*PI);
    int inRange = (stride > 0 && polar.y <= stride) || (stride < 0 && polar.y >= (2*PI)+stride);
    float targetAngle = ((polar.y)*inRange)+(!inRange*((Closest(polar.y,0,(2*PI*(stride < 0))+stride,2*PI)) ));
    Out = distance(UV,float2(Radius*cos(targetAngle)+Center.x, Radius*sin(targetAngle)+Center.y))*(stride != 0)+(stride == 0);
}



#endif