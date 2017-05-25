using System;
using System.Runtime.InteropServices;

public class Earcut
{
    [DllImport("UnityNativePlugin", EntryPoint = "CreateTesselationContext")]
    private static extern uint CreateTesselationContext();

    [DllImport("UnityNativePlugin", EntryPoint = "ReleaseTesselationContext")]
    private static extern uint ReleaseTesselationContext(uint context);

    [DllImport("UnityNativePlugin", EntryPoint = "TesselatePolygon")]
    private static extern void TesselatePolygon(uint context, IntPtr points, IntPtr rings, int nRings, out int nIndices, out int nVertices);

    [DllImport("UnityNativePlugin", EntryPoint = "GetIndices")]
    private static extern void GetIndices(uint context, IntPtr indices);

    [DllImport("UnityNativePlugin", EntryPoint = "GetVertices")]
    private static extern void GetVertices(uint context, IntPtr vertices);

    private uint contextId;

    public int[] indices { get; internal set; }

    public float[] vertices { get; internal set; }

    public Earcut()
    {
        contextId = CreateTesselationContext();
    }

    public void Tesselate(float[] points, int[] rings)
    {
        int nVertices = 0;
        int nIndices = 0;

        GCHandle pointsBufferHandle = GCHandle.Alloc(points, GCHandleType.Pinned);
        GCHandle ringsBufferHandle = GCHandle.Alloc(rings, GCHandleType.Pinned);

        TesselatePolygon(contextId, pointsBufferHandle.AddrOfPinnedObject(), ringsBufferHandle.AddrOfPinnedObject(), rings.Length, out nIndices, out nVertices);

        pointsBufferHandle.Free();
        ringsBufferHandle.Free();

        indices = new int[nIndices];
        vertices = new float[nVertices * 2];

        GCHandle indicesBufferHandle = GCHandle.Alloc(indices, GCHandleType.Pinned);
        GCHandle verticesBufferHandle = GCHandle.Alloc(vertices, GCHandleType.Pinned);

        GetIndices(contextId, indicesBufferHandle.AddrOfPinnedObject());
        GetVertices(contextId, verticesBufferHandle.AddrOfPinnedObject());

        indicesBufferHandle.Free();
        verticesBufferHandle.Free();
    }

    public void Release()
    {
        ReleaseTesselationContext(contextId);
    }
}

