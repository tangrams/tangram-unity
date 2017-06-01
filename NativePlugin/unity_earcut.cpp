#include "unity_earcut.hpp"
#include "earcut.hpp"
#include <array>
#include <mutex>

#define CAPACITY 32

extern "C" {
    using Coord = float;
    using N = int;
    using Point = std::array<Coord, 2>;
    using PolygonArray = std::vector<std::vector<Point>>;
    using Earcut = mapbox::Earcut<Coord, N>;

    struct EarcutContext {
        std::array<bool, CAPACITY> used;
        std::array<Earcut, CAPACITY> earcuts;
    };

    EarcutContext earcutContext;
    bool initialized = false;
    std::mutex mutex;

    unsigned int CreateTesselationContext()
    {
        {
            std::lock_guard<std::mutex> guard(mutex);
            if (!initialized) {
                earcutContext.used.fill(false);
                initialized = true;
            }
        }

        // 0 is an invalid identifier
        unsigned int contextId = 0;

        {
            std::lock_guard<std::mutex> guard(mutex);
            for (size_t i = 0; i < earcutContext.used.size(); ++i) {
                if (!earcutContext.used[i]) {
                    earcutContext.used[i] = true;
                    contextId = (unsigned int)i + 1;
                    break;
                }
            }
        }

        return contextId;
    }

    void ReleaseTesselationContext(unsigned int contextId)
    {
        if (contextId == 0) { return; }

        auto& earcut = earcutContext.earcuts[contextId - 1];
        earcut.indices.clear();
        earcut.vertices.clear();

        {
            std::lock_guard<std::mutex> guard(mutex);
            earcutContext.used[contextId - 1] = false;
        }
    }

    void TesselatePolygon(unsigned int contextId, char* pointsBuffer, char* ringsBuffer,
                          int nRings, unsigned int& nIndices, unsigned int& nVertices)
    {
        if (contextId == 0) { return; }

        int* rings = reinterpret_cast<int*>(ringsBuffer);

        PolygonArray polygon(nRings);

        for (int i = 0; i < nRings; ++i) {
            polygon[i].resize(rings[i]);
        }

        for (int i = 0, byteOffset = 0; i < nRings; ++i) {
            size_t ringByteSize = rings[i] * sizeof(Point);
            std::memcpy(polygon[i].data(), pointsBuffer + byteOffset, ringByteSize);
            byteOffset += ringByteSize;
        }

        auto& earcut = earcutContext.earcuts[contextId - 1];

        earcut(polygon);

        nIndices = (unsigned int)earcut.indices.size();
        nVertices = (unsigned int)earcut.vertices.size();
    }

    void GetIndices(unsigned int contextId, char* indices)
    {
        if (contextId == 0) { return; }

        auto& earcut = earcutContext.earcuts[contextId - 1];
        std::memcpy(indices, earcut.indices.data(), earcut.indices.size() * sizeof(N));
    }

    void GetVertices(unsigned int contextId, char* vertices)
    {
        if (contextId == 0) { return; }

        auto& earcut = earcutContext.earcuts[contextId - 1];
        std::memcpy(vertices, earcut.vertices.data(), earcut.vertices.size() * sizeof(Point));
    }
}
