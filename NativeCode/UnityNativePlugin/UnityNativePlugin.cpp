#include "UnityNativePlugin.hpp"
#include "earcut.hpp"
#include <iostream>
#include <vector>

extern "C" {
    using Coord = float;
    using N = int;
    using Point = std::array<Coord, 2>;
    using PolygonArray = std::vector<std::vector<Point>>;
    using EarcutContext = std::vector<mapbox::Earcut<Coord, N>>;
    
    static EarcutContext earcuts;
    
    unsigned int CreateTesselationContext()
    {
        earcuts.emplace_back();
        return (unsigned int)earcuts.size() - 1;
    }
    
    void ReleaseTesselationContext(unsigned int context)
    {
        earcuts[context].indices.clear();
        earcuts[context].vertices.clear();
    }
 
    void TesselatePolygon(unsigned int context, char* pointsBuffer, char* ringsBuffer,
                          int nRings, unsigned int& nIndices, unsigned int& nVertices)
    {
        auto& earcut = earcuts[context];
        PolygonArray polygon;
        
        float* points = reinterpret_cast<float*>(pointsBuffer);
        int* rings = reinterpret_cast<int*>(ringsBuffer);

        int point = 0;

        for (int i = 0; i < nRings; ++i) {
            int ring = rings[i];

            polygon.emplace_back();

            for (int j = 0; j < ring; ++j) {
                polygon.back().push_back({points[point], points[point+1]});
                point += 2;
            }
        }

        earcut(polygon);

        nIndices = (unsigned int)earcut.indices.size();
        nVertices = (unsigned int)earcut.vertices.size();
    }
    
    void GetIndices(unsigned int context, char* indices)
    {
        std::memcpy(indices, earcuts[context].indices.data(),
                    earcuts[context].indices.size() * sizeof(N));
    }
    
    void GetVertices(unsigned int context, char* vertices)
    {
        std::memcpy(vertices, earcuts[context].vertices.data(),
                    earcuts[context].vertices.size() * sizeof(Point));
    }
}
