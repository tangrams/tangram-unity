using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {

	public GameObject FollowedObject;
	public float InputSensitivity = 100.0f;
	public const float Distance = 5.0f;

	float theta = 0.0f;
	float phi = 0.0f;

	float adjustedDistance = Distance;

	struct CameraDesc
	{
		public Matrix4x4 view;
		public Matrix4x4 projection;
		public float nearClipPlane;
		public float fieldOfView;
		public float aspect;
		public Vector3 position;
	}

	void Start()
	{
		Cursor.lockState = CursorLockMode.Locked;
		Cursor.visible = false;
	}
	
	void Update()
	{
		float mouseX = Input.GetAxis("Mouse X");
		float mouseY = Input.GetAxis("Mouse Y");

		theta += mouseY * InputSensitivity * Time.deltaTime;
		phi   += mouseX * InputSensitivity * Time.deltaTime;

		theta = Mathf.Clamp(theta, 10.0f, 120.0f);
		phi   = phi % 360.0f;

		Quaternion rotation = Quaternion.Euler(theta, phi, 0.0f);
		Vector3 dir = new Vector3(0.0f, 1.0f, 0.0f);
		Vector3 pointOnSphere = rotation * dir * Distance;

		CameraDesc cameraDesc = new CameraDesc();
		cameraDesc.view = Camera.main.cameraToWorldMatrix;
		cameraDesc.projection = Camera.main.projectionMatrix;
		cameraDesc.nearClipPlane = Camera.main.nearClipPlane;
		cameraDesc.fieldOfView = Camera.main.fieldOfView;
		cameraDesc.aspect = Camera.main.aspect;
		cameraDesc.position = FollowedObject.transform.position + pointOnSphere;
		cameraDesc.view = Matrix4x4.LookAt(Camera.main.transform.position,
			FollowedObject.transform.position,
			Vector3.up);

		var frustrumPoints = ExtractFrustrumNearPlanePoints(cameraDesc);
		float distance = GetMinimumCollisionDistanceFromTarget(frustrumPoints);

		if (distance != -1)
		{
			adjustedDistance = distance;
		}
		else
		{
			adjustedDistance = Distance;
		}

		pointOnSphere = rotation * dir * adjustedDistance;

		transform.position = FollowedObject.transform.position + pointOnSphere;
		transform.LookAt(FollowedObject.transform);
	}

	float GetMinimumCollisionDistanceFromTarget(Vector3[] nearPlaneFrustrumPoints)
	{
		float distance = -1.0f;
		for (int i = 0; i < nearPlaneFrustrumPoints.Length; ++i)
		{
			Vector3 targetToFrustrumPoint = nearPlaneFrustrumPoints[i] - FollowedObject.transform.position;
			float maxDistance = targetToFrustrumPoint.magnitude;
			RaycastHit raycastHitInfo;

			bool hit = Physics.Raycast(FollowedObject.transform.position,
				targetToFrustrumPoint.normalized,
				out raycastHitInfo, maxDistance);

			if (hit)
			{
				if (raycastHitInfo.collider.gameObject != FollowedObject)
				{
					if (distance == -1.0f)
					{
						distance = raycastHitInfo.distance;
					}
					else
					{
						if (raycastHitInfo.distance < distance)
						{
							distance = raycastHitInfo.distance;
						}
					}
				}
			}
		}

		return distance;
	}

	Vector3[] ExtractFrustrumNearPlanePoints(CameraDesc camera)
	{
		float zn = camera.nearClipPlane;
		float fovRadians = camera.fieldOfView * (Mathf.PI / 180.0f);
		float e = Mathf.Tan(fovRadians * 0.5f);

		Vector4 axisX = camera.view.GetColumn(0);
		Vector4 axisY = camera.view.GetColumn(1);
		Vector4 axisZ = camera.view.GetColumn(2);

		Vector3 nearCenter = axisZ * zn;

		float nearExtX = e * zn;
		float nearExtY = nearExtX / camera.aspect;

		Vector4 cameraPos = camera.position;

		Vector4 BL = cameraPos + axisX * -nearExtX + axisY * -nearExtY + axisZ * zn;
		Vector4 BR = cameraPos + axisX *  nearExtX + axisY * -nearExtY + axisZ * zn;
		Vector4 TR = cameraPos + axisX *  nearExtX + axisY *  nearExtY + axisZ * zn;
		Vector4 TL = cameraPos + axisX * -nearExtX + axisY *  nearExtY + axisZ * zn;

		return new Vector3[] { BL, BR, TR, TL };
	}
}