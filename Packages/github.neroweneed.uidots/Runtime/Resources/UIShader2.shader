Shader "UIShader"
    {
        Properties
        {
            [NoScaleOffset]_Images("Images", 2D) = "white" {}
            [NoScaleOffset]_Fonts("Fonts", 2D) = "white" {}
            [HideInInspector][NoScaleOffset]unity_Lightmaps("unity_Lightmaps", 2DArray) = "" {}
            [HideInInspector][NoScaleOffset]unity_LightmapsInd("unity_LightmapsInd", 2DArray) = "" {}
            [HideInInspector][NoScaleOffset]unity_ShadowMasks("unity_ShadowMasks", 2DArray) = "" {}
        }
        SubShader
        {
            Tags
            {
                "RenderPipeline"="UniversalPipeline"
                "RenderType"="Transparent"
                "UniversalMaterialType" = "Unlit"
                "Queue"="Transparent"
            }
            Pass
            {
                Name "Pass"
                Tags
                {
                    // LightMode: <None>
                }
    
                // Render State
                Cull Back
                Blend One One
                ZTest LEqual
                ZWrite Off
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 2.0
                #pragma only_renderers gles gles3 glcore
                #pragma multi_compile_instancing
                #pragma multi_compile_fog
                #pragma vertex vert
                #pragma fragment frag
    
                // DotsInstancingOptions: <None>
                // HybridV1InjectedBuiltinProperties: <None>
    
                // Keywords
                #pragma multi_compile _ LIGHTMAP_ON
                #pragma multi_compile _ DIRLIGHTMAP_COMBINED
                #pragma shader_feature _ _SAMPLE_GI
                // GraphKeywords: <None>
    
                // Defines
                #define _SURFACE_TYPE_TRANSPARENT 1
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_COLOR
                #define VARYINGS_NEED_TEXCOORD0
                #define VARYINGS_NEED_TEXCOORD1
                #define VARYINGS_NEED_COLOR
                #define FEATURES_GRAPH_VERTEX
                /* WARNING: $splice Could not find named fragment 'PassInstancing' */
                #define SHADERPASS SHADERPASS_UNLIT
                /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
    
                // Includes
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct Attributes
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv0 : TEXCOORD0;
                    float4 uv1 : TEXCOORD1;
                    float4 color : COLOR;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct Varyings
                {
                    float4 positionCS : SV_POSITION;
                    float4 texCoord0;
                    float4 texCoord1;
                    float4 color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float4 uv0;
                    float4 uv1;
                    float4 VertexColor;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryings
                {
                    float4 positionCS : SV_POSITION;
                    float4 interp0 : TEXCOORD0;
                    float4 interp1 : TEXCOORD1;
                    float4 interp2 : TEXCOORD2;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                };
    
                PackedVaryings PackVaryings (Varyings input)
                {
                    PackedVaryings output;
                    output.positionCS = input.positionCS;
                    output.interp0.xyzw =  input.texCoord0;
                    output.interp1.xyzw =  input.texCoord1;
                    output.interp2.xyzw =  input.color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    return output;
                }
                Varyings UnpackVaryings (PackedVaryings input)
                {
                    Varyings output;
                    output.positionCS = input.positionCS;
                    output.texCoord0 = input.interp0.xyzw;
                    output.texCoord1 = input.interp1.xyzw;
                    output.color = input.interp2.xyzw;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
                // Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _Images_TexelSize;
                float4 _Fonts_TexelSize;
                CBUFFER_END
                
                // Object and Global properties
                TEXTURE2D(_Images);
                SAMPLER(sampler_Images);
                TEXTURE2D(_Fonts);
                SAMPLER(sampler_Fonts);
                SAMPLER(_SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_Sampler_3_Linear_Repeat);
                SAMPLER(_SampleTexture2D_f8436224dd2048b39e22f12538081454_Sampler_3_Linear_Repeat);
    
                // Graph Functions
                
                void Unity_OneMinus_float(float In, out float Out)
                {
                    Out = 1 - In;
                }
                
                // fcf95dba3c4c4a006aaef6eb8d0cb248
                #include "Assets/ResourceData/Shaders/Custom/SDF.hlsl"
                
                struct Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13
                {
                };
                
                void SG_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13(float Vector1_91A76D05, float Vector1_60E8F76D, Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13 IN, out float OutVector1_1)
                {
                    float _Property_b3a491fd0b6b414ca535b385e1a1ef5c_Out_0 = Vector1_91A76D05;
                    float _Property_02a2e617dd994c4b9aa36e0586b3f563_Out_0 = Vector1_60E8F76D;
                    float _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2;
                    SDFSample_float(_Property_b3a491fd0b6b414ca535b385e1a1ef5c_Out_0, _Property_02a2e617dd994c4b9aa36e0586b3f563_Out_0, _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2);
                    OutVector1_1 = _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2;
                }
                
                void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
                {
                    Out = Base * Blend;
                    Out = lerp(Base, Out, Opacity);
                }
                
                void Unity_Blend_Overwrite_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
                {
                    Out = lerp(Base, Blend, Opacity);
                }
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float3 BaseColor;
                    float Alpha;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    float4 _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0 = SAMPLE_TEXTURE2D(_Images, sampler_Images, IN.uv0.xy);
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_R_4 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.r;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_G_5 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.g;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_B_6 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.b;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_A_7 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.a;
                    float4 _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0 = SAMPLE_TEXTURE2D(_Fonts, sampler_Fonts, IN.uv1.xy);
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_R_4 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.r;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_G_5 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.g;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_B_6 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.b;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_A_7 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.a;
                    float _OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1;
                    Unity_OneMinus_float(_SampleTexture2D_f8436224dd2048b39e22f12538081454_A_7, _OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1);
                    Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13 _SDFSample_dd64d16a6d0748b3817a20bf966205c1;
                    float _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1;
                    SG_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13(_OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1, 0.5, _SDFSample_dd64d16a6d0748b3817a20bf966205c1, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float4 _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2;
                    Unity_Blend_Multiply_float4(IN.VertexColor, (_SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1.xxxx), _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float4 _Blend_114a934b2c424e1196e0d299f95efca4_Out_2;
                    Unity_Blend_Overwrite_float4(_SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0, _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2, _Blend_114a934b2c424e1196e0d299f95efca4_Out_2, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_R_1 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[0];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_G_2 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[1];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_B_3 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[2];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_A_4 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[3];
                    surface.BaseColor = (_Blend_114a934b2c424e1196e0d299f95efca4_Out_2.xyz);
                    surface.Alpha = _Split_56c387a2a7c04cec92ebbd64fcca03ee_A_4;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                
                
                
                
                    output.uv0 =                         input.texCoord0;
                    output.uv1 =                         input.texCoord1;
                    output.VertexColor =                 input.color;
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
                #else
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                #endif
                #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                
                    return output;
                }
                
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "ShadowCaster"
                Tags
                {
                    "LightMode" = "ShadowCaster"
                }
    
                // Render State
                Cull Back
                Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
                ZTest LEqual
                ZWrite On
                ColorMask 0
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 2.0
                #pragma only_renderers gles gles3 glcore
                #pragma multi_compile_instancing
                #pragma vertex vert
                #pragma fragment frag
    
                // DotsInstancingOptions: <None>
                // HybridV1InjectedBuiltinProperties: <None>
    
                // Keywords
                // PassKeywords: <None>
                // GraphKeywords: <None>
    
                // Defines
                #define _SURFACE_TYPE_TRANSPARENT 1
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_COLOR
                #define VARYINGS_NEED_TEXCOORD0
                #define VARYINGS_NEED_TEXCOORD1
                #define VARYINGS_NEED_COLOR
                #define FEATURES_GRAPH_VERTEX
                /* WARNING: $splice Could not find named fragment 'PassInstancing' */
                #define SHADERPASS SHADERPASS_SHADOWCASTER
                /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
    
                // Includes
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct Attributes
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv0 : TEXCOORD0;
                    float4 uv1 : TEXCOORD1;
                    float4 color : COLOR;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct Varyings
                {
                    float4 positionCS : SV_POSITION;
                    float4 texCoord0;
                    float4 texCoord1;
                    float4 color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float4 uv0;
                    float4 uv1;
                    float4 VertexColor;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryings
                {
                    float4 positionCS : SV_POSITION;
                    float4 interp0 : TEXCOORD0;
                    float4 interp1 : TEXCOORD1;
                    float4 interp2 : TEXCOORD2;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                };
    
                PackedVaryings PackVaryings (Varyings input)
                {
                    PackedVaryings output;
                    output.positionCS = input.positionCS;
                    output.interp0.xyzw =  input.texCoord0;
                    output.interp1.xyzw =  input.texCoord1;
                    output.interp2.xyzw =  input.color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    return output;
                }
                Varyings UnpackVaryings (PackedVaryings input)
                {
                    Varyings output;
                    output.positionCS = input.positionCS;
                    output.texCoord0 = input.interp0.xyzw;
                    output.texCoord1 = input.interp1.xyzw;
                    output.color = input.interp2.xyzw;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
                // Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _Images_TexelSize;
                float4 _Fonts_TexelSize;
                CBUFFER_END
                
                // Object and Global properties
                TEXTURE2D(_Images);
                SAMPLER(sampler_Images);
                TEXTURE2D(_Fonts);
                SAMPLER(sampler_Fonts);
                SAMPLER(_SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_Sampler_3_Linear_Repeat);
                SAMPLER(_SampleTexture2D_f8436224dd2048b39e22f12538081454_Sampler_3_Linear_Repeat);
    
                // Graph Functions
                
                void Unity_OneMinus_float(float In, out float Out)
                {
                    Out = 1 - In;
                }
                
                // fcf95dba3c4c4a006aaef6eb8d0cb248
                #include "Assets/ResourceData/Shaders/Custom/SDF.hlsl"
                
                struct Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13
                {
                };
                
                void SG_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13(float Vector1_91A76D05, float Vector1_60E8F76D, Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13 IN, out float OutVector1_1)
                {
                    float _Property_b3a491fd0b6b414ca535b385e1a1ef5c_Out_0 = Vector1_91A76D05;
                    float _Property_02a2e617dd994c4b9aa36e0586b3f563_Out_0 = Vector1_60E8F76D;
                    float _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2;
                    SDFSample_float(_Property_b3a491fd0b6b414ca535b385e1a1ef5c_Out_0, _Property_02a2e617dd994c4b9aa36e0586b3f563_Out_0, _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2);
                    OutVector1_1 = _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2;
                }
                
                void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
                {
                    Out = Base * Blend;
                    Out = lerp(Base, Out, Opacity);
                }
                
                void Unity_Blend_Overwrite_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
                {
                    Out = lerp(Base, Blend, Opacity);
                }
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float Alpha;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    float4 _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0 = SAMPLE_TEXTURE2D(_Images, sampler_Images, IN.uv0.xy);
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_R_4 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.r;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_G_5 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.g;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_B_6 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.b;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_A_7 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.a;
                    float4 _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0 = SAMPLE_TEXTURE2D(_Fonts, sampler_Fonts, IN.uv1.xy);
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_R_4 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.r;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_G_5 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.g;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_B_6 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.b;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_A_7 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.a;
                    float _OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1;
                    Unity_OneMinus_float(_SampleTexture2D_f8436224dd2048b39e22f12538081454_A_7, _OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1);
                    Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13 _SDFSample_dd64d16a6d0748b3817a20bf966205c1;
                    float _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1;
                    SG_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13(_OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1, 0.5, _SDFSample_dd64d16a6d0748b3817a20bf966205c1, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float4 _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2;
                    Unity_Blend_Multiply_float4(IN.VertexColor, (_SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1.xxxx), _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float4 _Blend_114a934b2c424e1196e0d299f95efca4_Out_2;
                    Unity_Blend_Overwrite_float4(_SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0, _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2, _Blend_114a934b2c424e1196e0d299f95efca4_Out_2, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_R_1 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[0];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_G_2 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[1];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_B_3 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[2];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_A_4 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[3];
                    surface.Alpha = _Split_56c387a2a7c04cec92ebbd64fcca03ee_A_4;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                
                
                
                
                    output.uv0 =                         input.texCoord0;
                    output.uv1 =                         input.texCoord1;
                    output.VertexColor =                 input.color;
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
                #else
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                #endif
                #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                
                    return output;
                }
                
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "DepthOnly"
                Tags
                {
                    "LightMode" = "DepthOnly"
                }
    
                // Render State
                Cull Back
                Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
                ZTest LEqual
                ZWrite On
                ColorMask 0
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 2.0
                #pragma only_renderers gles gles3 glcore
                #pragma multi_compile_instancing
                #pragma vertex vert
                #pragma fragment frag
    
                // DotsInstancingOptions: <None>
                // HybridV1InjectedBuiltinProperties: <None>
    
                // Keywords
                // PassKeywords: <None>
                // GraphKeywords: <None>
    
                // Defines
                #define _SURFACE_TYPE_TRANSPARENT 1
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_COLOR
                #define VARYINGS_NEED_TEXCOORD0
                #define VARYINGS_NEED_TEXCOORD1
                #define VARYINGS_NEED_COLOR
                #define FEATURES_GRAPH_VERTEX
                /* WARNING: $splice Could not find named fragment 'PassInstancing' */
                #define SHADERPASS SHADERPASS_DEPTHONLY
                /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
    
                // Includes
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct Attributes
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv0 : TEXCOORD0;
                    float4 uv1 : TEXCOORD1;
                    float4 color : COLOR;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct Varyings
                {
                    float4 positionCS : SV_POSITION;
                    float4 texCoord0;
                    float4 texCoord1;
                    float4 color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float4 uv0;
                    float4 uv1;
                    float4 VertexColor;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryings
                {
                    float4 positionCS : SV_POSITION;
                    float4 interp0 : TEXCOORD0;
                    float4 interp1 : TEXCOORD1;
                    float4 interp2 : TEXCOORD2;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                };
    
                PackedVaryings PackVaryings (Varyings input)
                {
                    PackedVaryings output;
                    output.positionCS = input.positionCS;
                    output.interp0.xyzw =  input.texCoord0;
                    output.interp1.xyzw =  input.texCoord1;
                    output.interp2.xyzw =  input.color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    return output;
                }
                Varyings UnpackVaryings (PackedVaryings input)
                {
                    Varyings output;
                    output.positionCS = input.positionCS;
                    output.texCoord0 = input.interp0.xyzw;
                    output.texCoord1 = input.interp1.xyzw;
                    output.color = input.interp2.xyzw;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
                // Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _Images_TexelSize;
                float4 _Fonts_TexelSize;
                CBUFFER_END
                
                // Object and Global properties
                TEXTURE2D(_Images);
                SAMPLER(sampler_Images);
                TEXTURE2D(_Fonts);
                SAMPLER(sampler_Fonts);
                SAMPLER(_SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_Sampler_3_Linear_Repeat);
                SAMPLER(_SampleTexture2D_f8436224dd2048b39e22f12538081454_Sampler_3_Linear_Repeat);
    
                // Graph Functions
                
                void Unity_OneMinus_float(float In, out float Out)
                {
                    Out = 1 - In;
                }
                
                // fcf95dba3c4c4a006aaef6eb8d0cb248
                #include "Assets/ResourceData/Shaders/Custom/SDF.hlsl"
                
                struct Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13
                {
                };
                
                void SG_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13(float Vector1_91A76D05, float Vector1_60E8F76D, Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13 IN, out float OutVector1_1)
                {
                    float _Property_b3a491fd0b6b414ca535b385e1a1ef5c_Out_0 = Vector1_91A76D05;
                    float _Property_02a2e617dd994c4b9aa36e0586b3f563_Out_0 = Vector1_60E8F76D;
                    float _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2;
                    SDFSample_float(_Property_b3a491fd0b6b414ca535b385e1a1ef5c_Out_0, _Property_02a2e617dd994c4b9aa36e0586b3f563_Out_0, _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2);
                    OutVector1_1 = _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2;
                }
                
                void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
                {
                    Out = Base * Blend;
                    Out = lerp(Base, Out, Opacity);
                }
                
                void Unity_Blend_Overwrite_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
                {
                    Out = lerp(Base, Blend, Opacity);
                }
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float Alpha;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    float4 _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0 = SAMPLE_TEXTURE2D(_Images, sampler_Images, IN.uv0.xy);
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_R_4 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.r;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_G_5 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.g;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_B_6 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.b;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_A_7 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.a;
                    float4 _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0 = SAMPLE_TEXTURE2D(_Fonts, sampler_Fonts, IN.uv1.xy);
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_R_4 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.r;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_G_5 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.g;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_B_6 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.b;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_A_7 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.a;
                    float _OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1;
                    Unity_OneMinus_float(_SampleTexture2D_f8436224dd2048b39e22f12538081454_A_7, _OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1);
                    Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13 _SDFSample_dd64d16a6d0748b3817a20bf966205c1;
                    float _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1;
                    SG_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13(_OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1, 0.5, _SDFSample_dd64d16a6d0748b3817a20bf966205c1, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float4 _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2;
                    Unity_Blend_Multiply_float4(IN.VertexColor, (_SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1.xxxx), _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float4 _Blend_114a934b2c424e1196e0d299f95efca4_Out_2;
                    Unity_Blend_Overwrite_float4(_SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0, _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2, _Blend_114a934b2c424e1196e0d299f95efca4_Out_2, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_R_1 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[0];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_G_2 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[1];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_B_3 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[2];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_A_4 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[3];
                    surface.Alpha = _Split_56c387a2a7c04cec92ebbd64fcca03ee_A_4;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                
                
                
                
                    output.uv0 =                         input.texCoord0;
                    output.uv1 =                         input.texCoord1;
                    output.VertexColor =                 input.color;
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
                #else
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                #endif
                #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                
                    return output;
                }
                
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"
    
                ENDHLSL
            }
        }
        SubShader
        {
            Tags
            {
                "RenderPipeline"="UniversalPipeline"
                "RenderType"="Transparent"
                "UniversalMaterialType" = "Unlit"
                "Queue"="Transparent"
            }
            Pass
            {
                Name "Pass"
                Tags
                {
                    // LightMode: <None>
                }
    
                // Render State
                Cull Back
                Blend One One
                ZTest LEqual
                ZWrite Off
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 4.5
                #pragma exclude_renderers gles gles3 glcore
                #pragma multi_compile_instancing
                #pragma multi_compile_fog
                #pragma multi_compile _ DOTS_INSTANCING_ON
                #pragma vertex vert
                #pragma fragment frag
    
                // DotsInstancingOptions: <None>
                // HybridV1InjectedBuiltinProperties: <None>
    
                // Keywords
                #pragma multi_compile _ LIGHTMAP_ON
                #pragma multi_compile _ DIRLIGHTMAP_COMBINED
                #pragma shader_feature _ _SAMPLE_GI
                // GraphKeywords: <None>
    
                // Defines
                #define _SURFACE_TYPE_TRANSPARENT 1
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_COLOR
                #define VARYINGS_NEED_TEXCOORD0
                #define VARYINGS_NEED_TEXCOORD1
                #define VARYINGS_NEED_COLOR
                #define FEATURES_GRAPH_VERTEX
                /* WARNING: $splice Could not find named fragment 'PassInstancing' */
                #define SHADERPASS SHADERPASS_UNLIT
                /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
    
                // Includes
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct Attributes
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv0 : TEXCOORD0;
                    float4 uv1 : TEXCOORD1;
                    float4 color : COLOR;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct Varyings
                {
                    float4 positionCS : SV_POSITION;
                    float4 texCoord0;
                    float4 texCoord1;
                    float4 color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float4 uv0;
                    float4 uv1;
                    float4 VertexColor;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryings
                {
                    float4 positionCS : SV_POSITION;
                    float4 interp0 : TEXCOORD0;
                    float4 interp1 : TEXCOORD1;
                    float4 interp2 : TEXCOORD2;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                };
    
                PackedVaryings PackVaryings (Varyings input)
                {
                    PackedVaryings output;
                    output.positionCS = input.positionCS;
                    output.interp0.xyzw =  input.texCoord0;
                    output.interp1.xyzw =  input.texCoord1;
                    output.interp2.xyzw =  input.color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    return output;
                }
                Varyings UnpackVaryings (PackedVaryings input)
                {
                    Varyings output;
                    output.positionCS = input.positionCS;
                    output.texCoord0 = input.interp0.xyzw;
                    output.texCoord1 = input.interp1.xyzw;
                    output.color = input.interp2.xyzw;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
                // Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _Images_TexelSize;
                float4 _Fonts_TexelSize;
                CBUFFER_END
                
                // Object and Global properties
                TEXTURE2D(_Images);
                SAMPLER(sampler_Images);
                TEXTURE2D(_Fonts);
                SAMPLER(sampler_Fonts);
                SAMPLER(_SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_Sampler_3_Linear_Repeat);
                SAMPLER(_SampleTexture2D_f8436224dd2048b39e22f12538081454_Sampler_3_Linear_Repeat);
    
                // Graph Functions
                
                void Unity_OneMinus_float(float In, out float Out)
                {
                    Out = 1 - In;
                }
                
                // fcf95dba3c4c4a006aaef6eb8d0cb248
                #include "Assets/ResourceData/Shaders/Custom/SDF.hlsl"
                
                struct Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13
                {
                };
                
                void SG_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13(float Vector1_91A76D05, float Vector1_60E8F76D, Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13 IN, out float OutVector1_1)
                {
                    float _Property_b3a491fd0b6b414ca535b385e1a1ef5c_Out_0 = Vector1_91A76D05;
                    float _Property_02a2e617dd994c4b9aa36e0586b3f563_Out_0 = Vector1_60E8F76D;
                    float _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2;
                    SDFSample_float(_Property_b3a491fd0b6b414ca535b385e1a1ef5c_Out_0, _Property_02a2e617dd994c4b9aa36e0586b3f563_Out_0, _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2);
                    OutVector1_1 = _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2;
                }
                
                void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
                {
                    Out = Base * Blend;
                    Out = lerp(Base, Out, Opacity);
                }
                
                void Unity_Blend_Overwrite_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
                {
                    Out = lerp(Base, Blend, Opacity);
                }
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float3 BaseColor;
                    float Alpha;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    float4 _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0 = SAMPLE_TEXTURE2D(_Images, sampler_Images, IN.uv0.xy);
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_R_4 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.r;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_G_5 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.g;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_B_6 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.b;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_A_7 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.a;
                    float4 _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0 = SAMPLE_TEXTURE2D(_Fonts, sampler_Fonts, IN.uv1.xy);
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_R_4 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.r;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_G_5 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.g;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_B_6 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.b;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_A_7 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.a;
                    float _OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1;
                    Unity_OneMinus_float(_SampleTexture2D_f8436224dd2048b39e22f12538081454_A_7, _OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1);
                    Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13 _SDFSample_dd64d16a6d0748b3817a20bf966205c1;
                    float _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1;
                    SG_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13(_OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1, 0.5, _SDFSample_dd64d16a6d0748b3817a20bf966205c1, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float4 _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2;
                    Unity_Blend_Multiply_float4(IN.VertexColor, (_SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1.xxxx), _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float4 _Blend_114a934b2c424e1196e0d299f95efca4_Out_2;
                    Unity_Blend_Overwrite_float4(_SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0, _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2, _Blend_114a934b2c424e1196e0d299f95efca4_Out_2, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_R_1 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[0];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_G_2 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[1];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_B_3 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[2];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_A_4 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[3];
                    surface.BaseColor = (_Blend_114a934b2c424e1196e0d299f95efca4_Out_2.xyz);
                    surface.Alpha = _Split_56c387a2a7c04cec92ebbd64fcca03ee_A_4;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                
                
                
                
                    output.uv0 =                         input.texCoord0;
                    output.uv1 =                         input.texCoord1;
                    output.VertexColor =                 input.color;
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
                #else
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                #endif
                #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                
                    return output;
                }
                
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/UnlitPass.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "ShadowCaster"
                Tags
                {
                    "LightMode" = "ShadowCaster"
                }
    
                // Render State
                Cull Back
                Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
                ZTest LEqual
                ZWrite On
                ColorMask 0
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 4.5
                #pragma exclude_renderers gles gles3 glcore
                #pragma multi_compile_instancing
                #pragma multi_compile _ DOTS_INSTANCING_ON
                #pragma vertex vert
                #pragma fragment frag
    
                // DotsInstancingOptions: <None>
                // HybridV1InjectedBuiltinProperties: <None>
    
                // Keywords
                // PassKeywords: <None>
                // GraphKeywords: <None>
    
                // Defines
                #define _SURFACE_TYPE_TRANSPARENT 1
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_COLOR
                #define VARYINGS_NEED_TEXCOORD0
                #define VARYINGS_NEED_TEXCOORD1
                #define VARYINGS_NEED_COLOR
                #define FEATURES_GRAPH_VERTEX
                /* WARNING: $splice Could not find named fragment 'PassInstancing' */
                #define SHADERPASS SHADERPASS_SHADOWCASTER
                /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
    
                // Includes
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct Attributes
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv0 : TEXCOORD0;
                    float4 uv1 : TEXCOORD1;
                    float4 color : COLOR;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct Varyings
                {
                    float4 positionCS : SV_POSITION;
                    float4 texCoord0;
                    float4 texCoord1;
                    float4 color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float4 uv0;
                    float4 uv1;
                    float4 VertexColor;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryings
                {
                    float4 positionCS : SV_POSITION;
                    float4 interp0 : TEXCOORD0;
                    float4 interp1 : TEXCOORD1;
                    float4 interp2 : TEXCOORD2;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                };
    
                PackedVaryings PackVaryings (Varyings input)
                {
                    PackedVaryings output;
                    output.positionCS = input.positionCS;
                    output.interp0.xyzw =  input.texCoord0;
                    output.interp1.xyzw =  input.texCoord1;
                    output.interp2.xyzw =  input.color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    return output;
                }
                Varyings UnpackVaryings (PackedVaryings input)
                {
                    Varyings output;
                    output.positionCS = input.positionCS;
                    output.texCoord0 = input.interp0.xyzw;
                    output.texCoord1 = input.interp1.xyzw;
                    output.color = input.interp2.xyzw;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
                // Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _Images_TexelSize;
                float4 _Fonts_TexelSize;
                CBUFFER_END
                
                // Object and Global properties
                TEXTURE2D(_Images);
                SAMPLER(sampler_Images);
                TEXTURE2D(_Fonts);
                SAMPLER(sampler_Fonts);
                SAMPLER(_SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_Sampler_3_Linear_Repeat);
                SAMPLER(_SampleTexture2D_f8436224dd2048b39e22f12538081454_Sampler_3_Linear_Repeat);
    
                // Graph Functions
                
                void Unity_OneMinus_float(float In, out float Out)
                {
                    Out = 1 - In;
                }
                
                // fcf95dba3c4c4a006aaef6eb8d0cb248
                #include "Assets/ResourceData/Shaders/Custom/SDF.hlsl"
                
                struct Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13
                {
                };
                
                void SG_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13(float Vector1_91A76D05, float Vector1_60E8F76D, Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13 IN, out float OutVector1_1)
                {
                    float _Property_b3a491fd0b6b414ca535b385e1a1ef5c_Out_0 = Vector1_91A76D05;
                    float _Property_02a2e617dd994c4b9aa36e0586b3f563_Out_0 = Vector1_60E8F76D;
                    float _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2;
                    SDFSample_float(_Property_b3a491fd0b6b414ca535b385e1a1ef5c_Out_0, _Property_02a2e617dd994c4b9aa36e0586b3f563_Out_0, _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2);
                    OutVector1_1 = _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2;
                }
                
                void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
                {
                    Out = Base * Blend;
                    Out = lerp(Base, Out, Opacity);
                }
                
                void Unity_Blend_Overwrite_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
                {
                    Out = lerp(Base, Blend, Opacity);
                }
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float Alpha;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    float4 _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0 = SAMPLE_TEXTURE2D(_Images, sampler_Images, IN.uv0.xy);
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_R_4 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.r;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_G_5 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.g;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_B_6 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.b;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_A_7 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.a;
                    float4 _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0 = SAMPLE_TEXTURE2D(_Fonts, sampler_Fonts, IN.uv1.xy);
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_R_4 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.r;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_G_5 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.g;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_B_6 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.b;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_A_7 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.a;
                    float _OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1;
                    Unity_OneMinus_float(_SampleTexture2D_f8436224dd2048b39e22f12538081454_A_7, _OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1);
                    Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13 _SDFSample_dd64d16a6d0748b3817a20bf966205c1;
                    float _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1;
                    SG_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13(_OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1, 0.5, _SDFSample_dd64d16a6d0748b3817a20bf966205c1, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float4 _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2;
                    Unity_Blend_Multiply_float4(IN.VertexColor, (_SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1.xxxx), _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float4 _Blend_114a934b2c424e1196e0d299f95efca4_Out_2;
                    Unity_Blend_Overwrite_float4(_SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0, _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2, _Blend_114a934b2c424e1196e0d299f95efca4_Out_2, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_R_1 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[0];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_G_2 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[1];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_B_3 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[2];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_A_4 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[3];
                    surface.Alpha = _Split_56c387a2a7c04cec92ebbd64fcca03ee_A_4;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                
                
                
                
                    output.uv0 =                         input.texCoord0;
                    output.uv1 =                         input.texCoord1;
                    output.VertexColor =                 input.color;
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
                #else
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                #endif
                #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                
                    return output;
                }
                
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShadowCasterPass.hlsl"
    
                ENDHLSL
            }
            Pass
            {
                Name "DepthOnly"
                Tags
                {
                    "LightMode" = "DepthOnly"
                }
    
                // Render State
                Cull Back
                Blend SrcAlpha OneMinusSrcAlpha, One OneMinusSrcAlpha
                ZTest LEqual
                ZWrite On
                ColorMask 0
    
                // Debug
                // <None>
    
                // --------------------------------------------------
                // Pass
    
                HLSLPROGRAM
    
                // Pragmas
                #pragma target 4.5
                #pragma exclude_renderers gles gles3 glcore
                #pragma multi_compile_instancing
                #pragma multi_compile _ DOTS_INSTANCING_ON
                #pragma vertex vert
                #pragma fragment frag
    
                // DotsInstancingOptions: <None>
                // HybridV1InjectedBuiltinProperties: <None>
    
                // Keywords
                // PassKeywords: <None>
                // GraphKeywords: <None>
    
                // Defines
                #define _SURFACE_TYPE_TRANSPARENT 1
                #define ATTRIBUTES_NEED_NORMAL
                #define ATTRIBUTES_NEED_TANGENT
                #define ATTRIBUTES_NEED_TEXCOORD0
                #define ATTRIBUTES_NEED_TEXCOORD1
                #define ATTRIBUTES_NEED_COLOR
                #define VARYINGS_NEED_TEXCOORD0
                #define VARYINGS_NEED_TEXCOORD1
                #define VARYINGS_NEED_COLOR
                #define FEATURES_GRAPH_VERTEX
                /* WARNING: $splice Could not find named fragment 'PassInstancing' */
                #define SHADERPASS SHADERPASS_DEPTHONLY
                /* WARNING: $splice Could not find named fragment 'DotsInstancingVars' */
    
                // Includes
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
                #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/TextureStack.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/ShaderGraphFunctions.hlsl"
    
                // --------------------------------------------------
                // Structs and Packing
    
                struct Attributes
                {
                    float3 positionOS : POSITION;
                    float3 normalOS : NORMAL;
                    float4 tangentOS : TANGENT;
                    float4 uv0 : TEXCOORD0;
                    float4 uv1 : TEXCOORD1;
                    float4 color : COLOR;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : INSTANCEID_SEMANTIC;
                    #endif
                };
                struct Varyings
                {
                    float4 positionCS : SV_POSITION;
                    float4 texCoord0;
                    float4 texCoord1;
                    float4 color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                };
                struct SurfaceDescriptionInputs
                {
                    float4 uv0;
                    float4 uv1;
                    float4 VertexColor;
                };
                struct VertexDescriptionInputs
                {
                    float3 ObjectSpaceNormal;
                    float3 ObjectSpaceTangent;
                    float3 ObjectSpacePosition;
                };
                struct PackedVaryings
                {
                    float4 positionCS : SV_POSITION;
                    float4 interp0 : TEXCOORD0;
                    float4 interp1 : TEXCOORD1;
                    float4 interp2 : TEXCOORD2;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    uint instanceID : CUSTOM_INSTANCE_ID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    uint stereoTargetEyeIndexAsBlendIdx0 : BLENDINDICES0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    uint stereoTargetEyeIndexAsRTArrayIdx : SV_RenderTargetArrayIndex;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    FRONT_FACE_TYPE cullFace : FRONT_FACE_SEMANTIC;
                    #endif
                };
    
                PackedVaryings PackVaryings (Varyings input)
                {
                    PackedVaryings output;
                    output.positionCS = input.positionCS;
                    output.interp0.xyzw =  input.texCoord0;
                    output.interp1.xyzw =  input.texCoord1;
                    output.interp2.xyzw =  input.color;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    return output;
                }
                Varyings UnpackVaryings (PackedVaryings input)
                {
                    Varyings output;
                    output.positionCS = input.positionCS;
                    output.texCoord0 = input.interp0.xyzw;
                    output.texCoord1 = input.interp1.xyzw;
                    output.color = input.interp2.xyzw;
                    #if UNITY_ANY_INSTANCING_ENABLED
                    output.instanceID = input.instanceID;
                    #endif
                    #if (defined(UNITY_STEREO_MULTIVIEW_ENABLED)) || (defined(UNITY_STEREO_INSTANCING_ENABLED) && (defined(SHADER_API_GLES3) || defined(SHADER_API_GLCORE)))
                    output.stereoTargetEyeIndexAsBlendIdx0 = input.stereoTargetEyeIndexAsBlendIdx0;
                    #endif
                    #if (defined(UNITY_STEREO_INSTANCING_ENABLED))
                    output.stereoTargetEyeIndexAsRTArrayIdx = input.stereoTargetEyeIndexAsRTArrayIdx;
                    #endif
                    #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                    output.cullFace = input.cullFace;
                    #endif
                    return output;
                }
    
                // --------------------------------------------------
                // Graph
    
                // Graph Properties
                CBUFFER_START(UnityPerMaterial)
                float4 _Images_TexelSize;
                float4 _Fonts_TexelSize;
                CBUFFER_END
                
                // Object and Global properties
                TEXTURE2D(_Images);
                SAMPLER(sampler_Images);
                TEXTURE2D(_Fonts);
                SAMPLER(sampler_Fonts);
                SAMPLER(_SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_Sampler_3_Linear_Repeat);
                SAMPLER(_SampleTexture2D_f8436224dd2048b39e22f12538081454_Sampler_3_Linear_Repeat);
    
                // Graph Functions
                
                void Unity_OneMinus_float(float In, out float Out)
                {
                    Out = 1 - In;
                }
                
                // fcf95dba3c4c4a006aaef6eb8d0cb248
                #include "Assets/ResourceData/Shaders/Custom/SDF.hlsl"
                
                struct Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13
                {
                };
                
                void SG_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13(float Vector1_91A76D05, float Vector1_60E8F76D, Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13 IN, out float OutVector1_1)
                {
                    float _Property_b3a491fd0b6b414ca535b385e1a1ef5c_Out_0 = Vector1_91A76D05;
                    float _Property_02a2e617dd994c4b9aa36e0586b3f563_Out_0 = Vector1_60E8F76D;
                    float _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2;
                    SDFSample_float(_Property_b3a491fd0b6b414ca535b385e1a1ef5c_Out_0, _Property_02a2e617dd994c4b9aa36e0586b3f563_Out_0, _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2);
                    OutVector1_1 = _CustomFunction_4b7092e6965b4645958df8828a13e90c_Out_2;
                }
                
                void Unity_Blend_Multiply_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
                {
                    Out = Base * Blend;
                    Out = lerp(Base, Out, Opacity);
                }
                
                void Unity_Blend_Overwrite_float4(float4 Base, float4 Blend, out float4 Out, float Opacity)
                {
                    Out = lerp(Base, Blend, Opacity);
                }
    
                // Graph Vertex
                struct VertexDescription
                {
                    float3 Position;
                    float3 Normal;
                    float3 Tangent;
                };
                
                VertexDescription VertexDescriptionFunction(VertexDescriptionInputs IN)
                {
                    VertexDescription description = (VertexDescription)0;
                    description.Position = IN.ObjectSpacePosition;
                    description.Normal = IN.ObjectSpaceNormal;
                    description.Tangent = IN.ObjectSpaceTangent;
                    return description;
                }
    
                // Graph Pixel
                struct SurfaceDescription
                {
                    float Alpha;
                };
                
                SurfaceDescription SurfaceDescriptionFunction(SurfaceDescriptionInputs IN)
                {
                    SurfaceDescription surface = (SurfaceDescription)0;
                    float4 _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0 = SAMPLE_TEXTURE2D(_Images, sampler_Images, IN.uv0.xy);
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_R_4 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.r;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_G_5 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.g;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_B_6 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.b;
                    float _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_A_7 = _SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0.a;
                    float4 _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0 = SAMPLE_TEXTURE2D(_Fonts, sampler_Fonts, IN.uv1.xy);
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_R_4 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.r;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_G_5 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.g;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_B_6 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.b;
                    float _SampleTexture2D_f8436224dd2048b39e22f12538081454_A_7 = _SampleTexture2D_f8436224dd2048b39e22f12538081454_RGBA_0.a;
                    float _OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1;
                    Unity_OneMinus_float(_SampleTexture2D_f8436224dd2048b39e22f12538081454_A_7, _OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1);
                    Bindings_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13 _SDFSample_dd64d16a6d0748b3817a20bf966205c1;
                    float _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1;
                    SG_SDFSample_347bfd7dac9adda43833ba8a3b5e9b13(_OneMinus_fc6a980acb1b4e689a919117a5fdb4e6_Out_1, 0.5, _SDFSample_dd64d16a6d0748b3817a20bf966205c1, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float4 _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2;
                    Unity_Blend_Multiply_float4(IN.VertexColor, (_SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1.xxxx), _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float4 _Blend_114a934b2c424e1196e0d299f95efca4_Out_2;
                    Unity_Blend_Overwrite_float4(_SampleTexture2D_f63fccd73b7349fc931156c471f4ca2a_RGBA_0, _Blend_60494b7ebc7243bc8a267687e5116acb_Out_2, _Blend_114a934b2c424e1196e0d299f95efca4_Out_2, _SDFSample_dd64d16a6d0748b3817a20bf966205c1_OutVector1_1);
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_R_1 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[0];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_G_2 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[1];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_B_3 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[2];
                    float _Split_56c387a2a7c04cec92ebbd64fcca03ee_A_4 = _Blend_114a934b2c424e1196e0d299f95efca4_Out_2[3];
                    surface.Alpha = _Split_56c387a2a7c04cec92ebbd64fcca03ee_A_4;
                    return surface;
                }
    
                // --------------------------------------------------
                // Build Graph Inputs
    
                VertexDescriptionInputs BuildVertexDescriptionInputs(Attributes input)
                {
                    VertexDescriptionInputs output;
                    ZERO_INITIALIZE(VertexDescriptionInputs, output);
                
                    output.ObjectSpaceNormal =           input.normalOS;
                    output.ObjectSpaceTangent =          input.tangentOS;
                    output.ObjectSpacePosition =         input.positionOS;
                
                    return output;
                }
                
                SurfaceDescriptionInputs BuildSurfaceDescriptionInputs(Varyings input)
                {
                    SurfaceDescriptionInputs output;
                    ZERO_INITIALIZE(SurfaceDescriptionInputs, output);
                
                
                
                
                
                    output.uv0 =                         input.texCoord0;
                    output.uv1 =                         input.texCoord1;
                    output.VertexColor =                 input.color;
                #if defined(SHADER_STAGE_FRAGMENT) && defined(VARYINGS_NEED_CULLFACE)
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN output.FaceSign =                    IS_FRONT_VFACE(input.cullFace, true, false);
                #else
                #define BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                #endif
                #undef BUILD_SURFACE_DESCRIPTION_INPUTS_OUTPUT_FACESIGN
                
                    return output;
                }
                
    
                // --------------------------------------------------
                // Main
    
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/ShaderPass.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/Varyings.hlsl"
                #include "Packages/com.unity.render-pipelines.universal/Editor/ShaderGraph/Includes/DepthOnlyPass.hlsl"
    
                ENDHLSL
            }
        }
        FallBack "Hidden/Shader Graph/FallbackError"
    }
