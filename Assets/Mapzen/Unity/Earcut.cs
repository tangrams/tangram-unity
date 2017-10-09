using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace Mapzen.Unity
{
    public class Earcut
    {
        private const string LibraryName =
        #if (UNITY_IOS || UNITY_WEBGL)
            "__Internal";
        #else
            "Earcut";
        #endif

        [DllImport(LibraryName, EntryPoint = "CreateTesselationContext")]
        private static extern uint CreateTesselationContext();

        [DllImport(LibraryName, EntryPoint = "ReleaseTesselationContext")]
        private static extern uint ReleaseTesselationContext(uint context);

        [DllImport(LibraryName, EntryPoint = "TesselatePolygon")]
        private static extern void TesselatePolygon(uint context, IntPtr points, IntPtr rings, int nRings, out int nIndices);

        [DllImport(LibraryName, EntryPoint = "GetIndices")]
        private static extern void GetIndices(uint context, IntPtr indices);

        private uint contextId;

        public int[] Indices { get; internal set; }

        public Earcut()
        {
            contextId = CreateTesselationContext();
            Debug.Assert(contextId > 0);
        }

        public void Tesselate(float[] points, int[] rings)
        {
            int nIndices = 0;

            GCHandle pointsBufferHandle = GCHandle.Alloc(points, GCHandleType.Pinned);
            GCHandle ringsBufferHandle = GCHandle.Alloc(rings, GCHandleType.Pinned);

            TesselatePolygon(contextId, pointsBufferHandle.AddrOfPinnedObject(), ringsBufferHandle.AddrOfPinnedObject(), rings.Length, out nIndices);

            pointsBufferHandle.Free();
            ringsBufferHandle.Free();

            Indices = new int[nIndices];

            GCHandle indicesBufferHandle = GCHandle.Alloc(Indices, GCHandleType.Pinned);

            GetIndices(contextId, indicesBufferHandle.AddrOfPinnedObject());

            indicesBufferHandle.Free();
        }

        public void Release()
        {
            ReleaseTesselationContext(contextId);
        }
    }
}