Shader "Custom/WaterPlaneShader"
{
    Properties
    {
    }
    SubShader
    {
        Tags { "RenderType" = "Opaque" "DisableBatching" = "True" }
        LOD 100

        Pass
        {
            CGPROGRAM

            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 vertex : SV_POSITION;
            };

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                return o;
            }

            fixed4 frag() : SV_Target
            {
                return fixed4(1, 1, 1, 1);
            }

            ENDCG
        }
    }

    SubShader
    {
        Pass
        {
            Name "RTPass"

            HLSLPROGRAM

            #include "UnityRayTracingMeshUtils.cginc"


            #pragma raytracing some_name

            // Use INSTANCING_ON shader keyword for supporting instanced and non-instanced geometries.
            // Unity will setup SH coeffiecients - unity_SHAArray, unity_SHBArray, etc when RayTracingAccelerationStructure.AddInstances is used.
            #pragma multi_compile _ INSTANCING_ON

            #if INSTANCING_ON
                // Unity built-in shader property and represents the index of the fist ray tracing Mesh instance in the TLAS.
                uint unity_BaseInstanceID;

                // How many ray tracing instances were added using RayTracingAccelerationStructure.AddInstances is used. Not used here.
                uint unity_InstanceCount;
            #endif

            struct RayPayload
            {
                float3 hitIndex;
            };

            TextureCube<float4>				g_EnvTexture : register(t0, space1);
            RaytracingAccelerationStructure g_AccelStruct : register(t1, space1);

            StructuredBuffer<float3> g_Colors;

            struct AttributeData
            {
                float2 barycentrics;
            };

            struct Vertex
            {
                float3 position;
                float3 normal;
            };

            Vertex FetchVertex(uint vertexIndex)
            {
                Vertex v;
                v.position = UnityRayTracingFetchVertexAttribute3(vertexIndex, kVertexAttributePosition);
                v.normal = UnityRayTracingFetchVertexAttribute3(vertexIndex, kVertexAttributeNormal);
                return v;
            }

            Vertex InterpolateVertices(Vertex v0, Vertex v1, Vertex v2, float3 barycentrics)
            {
                Vertex v;
                #define INTERPOLATE_ATTRIBUTE(attr) v.attr = v0.attr * barycentrics.x + v1.attr * barycentrics.y + v2.attr * barycentrics.z
                INTERPOLATE_ATTRIBUTE(position);
                INTERPOLATE_ATTRIBUTE(normal);
                return v;
            }

            [shader("closesthit")]
            void ClosestHitMain(inout RayPayload payload : SV_RayPayload, AttributeData attribs : SV_IntersectionAttributes)
            {
                payload.hitIndex = float3(0, 0, 1);
            }
            ENDHLSL
        }
    }
}
