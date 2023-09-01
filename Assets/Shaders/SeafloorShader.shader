Shader "Custom/SeafloorShader"
{
    Properties
    {        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "DisableBatching"="True" }
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
                /*int nrOfInteractions;
                float3 startingPoint;
                float3 direction;
                float3 intersectionPoint;
                bool spawnNewRay;
                float3 newDirection;*/
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
                /*uint3 triangleIndices = UnityRayTracingFetchTriangleIndices(PrimitiveIndex());

                Vertex v0, v1, v2;
                v0 = FetchVertex(triangleIndices.x);
                v1 = FetchVertex(triangleIndices.y);
                v2 = FetchVertex(triangleIndices.z);

                float3 barycentricCoords = float3(1.0 - attribs.barycentrics.x - attribs.barycentrics.y, attribs.barycentrics.x, attribs.barycentrics.y);
                Vertex v = InterpolateVertices(v0, v1, v2, barycentricCoords);

                float3 worldPosition = mul(ObjectToWorld(), float4(v.position, 1));
                float3 worldNormal = normalize(mul(v.normal, (float3x3)WorldToObject()));

                payload.hitIndex = float3(1, 0, 0);
                payload.intersectionPoint = worldPosition;

                if (payload.nrOfInteractions < 3) {
                    // spawn new ray, setup data first
                    payload.newDirection = float3(payload.direction.x, -payload.direction.y, payload.direction.z); // bounce of seafloor, simply flip vertical direction, for now

                    // define new ray
                    RayDesc ray;
                    ray.Origin = worldPosition + worldNormal * 0.005f;
                    ray.Direction = payload.newDirection;
                    ray.TMin = 0;
                    ray.TMax = 1e20f;

                    // define the payload of the new ray
                    RayPayload newRayPayload;
                    newRayPayload.nrOfInteractions = payload.nrOfInteractions + 1;
                    newRayPayload.startingPoint = ray.Origin;
                    newRayPayload.direction = ray.Direction;

                    //spawn the new ray
                    uint missShaderIndex = 0;
                    //TraceRay(g_AccelStruct, 0, 0xFF, 0, 1, missShaderIndex, ray, newRayPayload);*/
                payload.hitIndex = float3(1, 0, 0);
                //}
            }
            ENDHLSL
        }
    }
}
