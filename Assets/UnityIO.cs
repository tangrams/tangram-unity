using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class UnityIO
{
    public class Response {
        public string error;
        public byte[] data;

        public Response(string e, byte[] d) {
            error = e;
            data = d;
        }
        public Response(string e, string d) {
            error = e;
            data = Convert.FromBase64String(d);
        }
    }

    public delegate void IORequestCallback(object userData, Response response);

    public IEnumerator FetchData(Uri uri, IORequestCallback callback, object userData) {

        bool networkRequest = false;
        Response response;
        if (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps) {
            networkRequest = true;
        }

        if (networkRequest) {
            WWW request = new WWW(uri.AbsoluteUri);
            yield return request;
            response = new Response(request.error, request.bytes);
        } else {
            yield return 0;
            var obj = Resources.Load(uri.AbsolutePath);
            if (obj == null) {
                response = new Response("Resource not found", "");
            } else {
                response = new Response(null, obj.ToString());
            }
        }
        callback(userData, response);
    }
}

