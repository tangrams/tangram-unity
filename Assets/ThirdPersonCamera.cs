using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {

	public GameObject FollowedObject;
	public float InputSensitivity = 100.0f;

	float theta = 0.0f;
	float phi = 0.0f;

	float distance = 5.0f;

	void Start () {
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
	
	void Update () {
		float mouseX = Input.GetAxis("Mouse X");
		float mouseY = Input.GetAxis("Mouse Y");

		theta += mouseY * InputSensitivity * Time.deltaTime;
		phi   += mouseX * InputSensitivity * Time.deltaTime;

		theta = Mathf.Clamp(theta, 10.0f, 120.0f);
		phi   = phi % 360.0f;

		Quaternion rotation = Quaternion.Euler(theta, phi, 0.0f);
		Vector3 dir = new Vector3(0.0f, 1.0f, 0.0f);
		Vector3 pointOnSphere = rotation * dir * distance;

		transform.position = FollowedObject.transform.position + pointOnSphere;
		transform.LookAt(FollowedObject.transform);
	}

	void OnDrawGizmos() {
		var frustrumPoints = ExtractFrustrumNearPlanePoints();
		for (int i = 0; i < 4; ++i) {
			Gizmos.DrawSphere(frustrumPoints[i], 0.1f);
		}

		Matrix4x4 cameraToWorld = Camera.main.transform.localToWorldMatrix;
		Matrix4x4 projection = Camera.main.projectionMatrix;
		Vector4 axisX = cameraToWorld.GetColumn(0);
		Vector4 axisY = cameraToWorld.GetColumn(1);
		Vector4 axisZ = cameraToWorld.GetColumn(2);

		Gizmos.DrawLine(new Vector3(0.0f, 0.0f, 0.0f), axisX);
		Gizmos.DrawLine(new Vector3(0.0f, 0.0f, 0.0f), axisY);
		Gizmos.DrawLine(new Vector3(0.0f, 0.0f, 0.0f), axisZ);
    }

	Vector3[] ExtractFrustrumNearPlanePoints()
	{
		Matrix4x4 cameraToWorld = Camera.main.transform.localToWorldMatrix;
		Matrix4x4 projection = Camera.main.projectionMatrix;

		float zn = Camera.main.nearClipPlane;
		float fovRadians = Camera.main.fieldOfView * (Mathf.PI / 180.0f);
		float e = Mathf.Tan(fovRadians * 0.5f);

		Vector4 axisX = cameraToWorld.GetColumn(0);
		Vector4 axisY = cameraToWorld.GetColumn(1);
		Vector4 axisZ = cameraToWorld.GetColumn(2);

		Vector3 nearCenter = axisZ * zn;

		float nearExtX = e * zn;
		float nearExtY = nearExtX / Camera.main.aspect;

		Vector4 cameraPos = Camera.main.transform.position;

		Vector4 BL = cameraPos + axisX * -nearExtX + axisY * -nearExtY + axisZ * zn;
		Vector4 BR = cameraPos + axisX *  nearExtX + axisY * -nearExtY + axisZ * zn;
		Vector4 TR = cameraPos + axisX *  nearExtX + axisY *  nearExtY + axisZ * zn;
		Vector4 TL = cameraPos + axisX * -nearExtX + axisY *  nearExtY + axisZ * zn;

		return new Vector3[] { BL, BR, TR, TL };
	}
}