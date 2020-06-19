//UNITY_SHADER_NO_UPGRADE
#ifndef SDFSAMPLEINCLUDE_INCLUDED
#define SDFSAMPLEINCLUDE_INCLUDED

void SDFSample_float(float SDF, float Offset,out float Out) {
Out = SDF - Offset;
Out = saturate(-Out / fwidth(Out));
}

#endif //SDFSAMPLEINCLUDE_INCLUDED