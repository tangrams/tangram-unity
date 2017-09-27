//
// @CMAKE_DOCS@
//

#include "unity_earcut.h"
#include "earcut.h"
#include <array>
#include <mutex>
#include <cstring>

#define CAPACITY 32

extern "C" {
    using Coord = float;
    using N = int;
    using Point = std::array<Coord, 2>;
    using PolygonArray = std::vector<std::vector<Point>>;

    struct EarcutContext {
        std::array<bool, CAPACITY> used;
        std::array<std::vector<N>, CAPACITY> indices;
        std::array<PolygonArray, CAPACITY> polygons;
    };

    EarcutContext earcutContext;
    bool initialized = false;
    std::mutex mutex;

    unsigned int CreateTesselationContext()
    {
        std::lock_guard<std::mutex> guard(mutex);
        if (!initialized) {
            earcutContext.used.fill(false);
            initialized = true;
        }

        // 0 is an invalid identifier
        unsigned int contextId = 0;
        for (size_t i = 0; i < earcutContext.used.size(); ++i) {
            if (!earcutContext.used[i]) {
                earcutContext.used[i] = true;
                contextId = (unsigned int)i + 1;
                break;
            }
        }

        return contextId;
    }

    void ReleaseTesselationContext(unsigned int contextId)
    {
        if (contextId == 0) { return; }

        std::lock_guard<std::mutex> guard(mutex);
        earcutContext.used[contextId - 1] = false;
    }

    void TesselatePolygon(unsigned int contextId, char* pointsBuffer, char* ringsBuffer,
                          int nRings, unsigned int& nIndices)
    {
        if (contextId == 0) { return; }

        int* rings = reinterpret_cast<int*>(ringsBuffer);

        auto& polygon = earcutContext.polygons[contextId - 1];
        polygon.resize(nRings);

        for (int i = 0; i < nRings; ++i) {
            polygon[i].resize(rings[i]);
        }

        for (int i = 0, byteOffset = 0; i < nRings; ++i) {
            size_t ringByteSize = polygon[i].size() * sizeof(Point);
            std::memcpy(polygon[i].data(), pointsBuffer + byteOffset, ringByteSize);
            byteOffset += ringByteSize;
        }

        auto& indices = earcutContext.indices[contextId - 1];
        indices = mapbox::earcut<N>(polygon);
        nIndices = (unsigned int)indices.size();
    }

    void GetIndices(unsigned int contextId, char* indices)
    {
        if (contextId == 0) { return; }

        auto& earcutIndices = earcutContext.indices[contextId - 1];
        std::memcpy(indices, earcutIndices.data(), earcutIndices.size() * sizeof(N));
    }
}
