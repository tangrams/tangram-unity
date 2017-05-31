using UnityEngine;
using System;
using System.IO;
using System.Collections;

public delegate void UnityIORequestCallback(object userData, Response response);

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

public class UnityIO 
{
	public IEnumerator UnityIORequest(string url, UnityIORequestCallback callback, object userData) {

		bool networkRequest = false;
		Response response;
		if (url.StartsWith("http")) {
			networkRequest = true;
		} else if (url.StartsWith("file")) {
			networkRequest = false;
		}

		if (networkRequest) {
			WWW request = new WWW(url);
			yield return request;
			response = new Response(request.error, request.bytes);
		} else {
			yield return 0;
			if (!File.Exists(url)) {
				response = new Response("File Does not exist", "");
			} else {
				response = new Response(null, File.ReadAllBytes(url));
			}
		}
		callback(userData, response);
	}
}

