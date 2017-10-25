using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThirdPersonCamera : MonoBehaviour {

	public GameObject FollowedObject;
	public float InputSensitivity = 100.0f;
	public float Distance = 5.0f;
	public float CameraPitchAngle = 45.0f;
	public float CameraPitchAngleMin = 10.0f;
	public float CameraPitchAngleMax = 120.0f;
	public float AimingBlendFactor = 0.05f;


	Vector3 targetLastPosition;

	Vector2 dir2d;

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
		dir2d = Vector2.right;
	}
	
	void Update()
	{
		float mouseY = Input.GetAxis("Mouse Y");
		float mouseX = Input.GetAxis("Mouse X");

		CameraPitchAngle += mouseY * InputSensitivity * Time.deltaTime;
		CameraPitchAngle = Mathf.Clamp(CameraPitchAngle, CameraPitchAngleMin, CameraPitchAngleMax);

		Debug.Log(mouseY);
		if (FollowedObject.transform.position != targetLastPosition)
		{
			Vector3 direction = FollowedObject.transform.position - targetLastPosition;
			Vector2 newDir2d = new Vector2(direction.x, direction.z).normalized;
			dir2d = Vector2.Lerp(dir2d, newDir2d, AimingBlendFactor);
		}

		Vector3 cameraPosition = new Vector3(-dir2d.x, Mathf.Sin(CameraPitchAngle * Mathf.Deg2Rad), -dir2d.y) * Distance;

		CameraDesc cameraDesc = new CameraDesc();
		cameraDesc.view = Camera.main.cameraToWorldMatrix;
		cameraDesc.projection = Camera.main.projectionMatrix;
		cameraDesc.nearClipPlane = Camera.main.nearClipPlane;
		cameraDesc.fieldOfView = Camera.main.fieldOfView;
		cameraDesc.aspect = Camera.main.aspect;
		cameraDesc.position = FollowedObject.transform.position + cameraPosition;
		cameraDesc.view = Matrix4x4.LookAt(Camera.main.transform.position,
			FollowedObject.transform.position,
			Vector3.up);

		var frustrumPoints = ExtractFrustrumNearPlanePoints(cameraDesc);
		float distance = GetMinimumCollisionDistanceFromTarget(frustrumPoints);

		float adjustedDistance;
		if (distance != -1)
		{
			adjustedDistance = distance;
		}
		else
		{
			adjustedDistance = Distance;
		}

		cameraPosition = new Vector3(-dir2d.x, Mathf.Sin(CameraPitchAngle * Mathf.Deg2Rad), -dir2d.y) * adjustedDistance;

		transform.position = FollowedObject.transform.position + cameraPosition;
		transform.LookAt(FollowedObject.transform);

		targetLastPosition = FollowedObject.transform.position;
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
		float fovRadians = Mathf.Deg2Rad * camera.fieldOfView;
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
