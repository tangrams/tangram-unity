using System;
using System.Runtime.InteropServices;
using System.Diagnostics;

public class Earcut
{
    #if UNITY_IOS
    [DllImport("__Internal")]
    #else
    [DllImport("Earcut", EntryPoint = "CreateTesselationContext")]
    #endif
    private static extern uint CreateTesselationContext();

    #if UNITY_IOS
    [DllImport("__Internal")]
    #else
    [DllImport("Earcut", EntryPoint = "ReleaseTesselationContext")]
    #endif
    private static extern uint ReleaseTesselationContext(uint context);

    #if UNITY_IOS
    [DllImport("__Internal")]
    #else
    [DllImport("Earcut", EntryPoint = "TesselatePolygon")]
    #endif
    private static extern void TesselatePolygon(uint context, IntPtr points, IntPtr rings, int nRings, out int nIndices, out int nVertices);

    #if UNITY_IOS
    [DllImport("__Internal")]
    #else
    [DllImport("Earcut", EntryPoint = "GetIndices")]
    #endif
    private static extern void GetIndices(uint context, IntPtr indices);

    #if UNITY_IOS
    [DllImport("__Internal")]
    #else
    [DllImport("Earcut", EntryPoint = "GetVertices")]
    #endif
    private static extern void GetVertices(uint context, IntPtr vertices);

    private uint contextId;

    public int[] indices { get; internal set; }

    public float[] vertices { get; internal set; }

    public Earcut()
    {
        contextId = CreateTesselationContext();
        Debug.Assert(contextId > 0);
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

