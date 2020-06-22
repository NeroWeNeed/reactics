//UNITY_SHADER_NO_UPGRADE
#ifndef ACTIONMETERINCLUDE_INCLUDED
#define ACTIONMETERINCLUDE_INCLUDED
void ActionMeter_float(float Radius,float Thickness, float StartingAngle,float3 OutlineColor,float3 PositiveChargeColor,float3 NegativeChargeColor, float Charge,float Cost,out float3 Out) {
Out = PositiveChargeColor;
}

#endif