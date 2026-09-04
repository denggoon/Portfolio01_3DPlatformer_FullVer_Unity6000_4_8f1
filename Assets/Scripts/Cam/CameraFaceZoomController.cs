using UnityEngine;

// 마우스 휠로 캐릭터 얼굴 클로즈업 <-> 기본 쿼터뷰 카메라를 전환하고,
// 우클릭 드래그로 캐릭터를 중심으로 카메라를 수평 공전(궤도 회전)시킨다.
// 스크롤량을 0~1 사이 zoomAmount로 누적하고, 매 프레임 기본값과 클로즈업값을 zoomAmount로 보간한 뒤,
// 그 결과에 orbitAngle만큼 월드 Y축 회전을 한 번 더 얹는 방식으로 두 기능을 합성한다
// (오프셋 벡터를 Y축으로 회전시키면 높이(Y)는 그대로 유지되면서 수평으로만 도는, "공전" 형태가 그대로 나온다).
// SideScrollCamera.CameraFollow()는 위치의 Y축에 자체 스무딩을 걸기 때문에, 회전과 위치를 같은 프레임에
// 완전히 동기화해서 바꾸려면 줌 동작 중엔 CameraFollow()를 멈추고(HoldCameraFunction) 위치도 직접 계산해야 한다.
// (공전만 하는 경우엔 Y가 바뀌지 않으므로 CameraFollow()에 맡겨도 스무딩 지연 문제가 없다.)
public class CameraFaceZoomController : MonoBehaviour
{
	public SideScrollCamera sideCam;

	[Header("클로즈업 카메라 위치 - 기본 쿼터뷰 오프셋 대비 상대값")]
	public float closeUpElevation = 2F;        // distancePos.y 목표값 (기본 고도보다 낮게)
	public float closeUpDistanceScale = 0.3F;  // 기본 distancePos.x/z에 곱하는 배율 (부호 유지 = 방향 안 틀어짐)

	[Header("클로즈업 시 카메라가 바라볼 지점 - 플레이어 기준 상대 위치(대략 얼굴 높이)")]
	public Vector3 faceLookOffset = new Vector3(0F, 1.6F, 0F);

	[Header("휠 반응 속도")]
	public float scrollSensitivity = 4F; // 휠 한 틱당 목표 zoomAmount 증가폭 배율
	public float zoomMoveSpeed = 2F;     // 목표 zoomAmount를 따라가는 속도 (초당 zoomAmount 변화량)

	[Header("우클릭 드래그 공전(orbit) 설정")]
	public float orbitSensitivity = 6F; // 마우스 X 이동량 대비 orbitAngle 증가폭 배율

	private bool hasCachedDefaults = false;
	private Vector3 defaultDistancePos;
	private Quaternion defaultRotation;

	private Vector3 closeUpDistancePos;
	private Quaternion closeUpRotation;

	private float targetZoomAmount = 0F; // 0 = 기본 쿼터뷰, 1 = 완전 클로즈업
	private float zoomAmount = 0F;
	private float orbitAngle = 0F; // 캐릭터를 중심으로 한 수평 공전 각도(도), 우클릭 드래그로 계속 누적됨
	private bool weAreHolding = false; // 우리가 HoldCameraFunction(true)를 걸어둔 상태인지

	void Update()
	{
		if (sideCam == null)
		{
			sideCam = GameRuleManager.instance.sideCam;
			if (sideCam == null) return;
		}

		if (!hasCachedDefaults)
			CacheDefaults();

		if (GameRuleManager.instance.eGameStatus != E_GAME_STATUS.IN_PLAY) return;
		if (sideCam.IsCameraFunctionHeld && !weAreHolding) return; // 다른 카메라 이벤트가 이미 카메라를 점유 중이면 무시

		float scroll = Input.GetAxis("Mouse ScrollWheel");
		targetZoomAmount = Mathf.Clamp01(targetZoomAmount + scroll * scrollSensitivity);
		zoomAmount = Mathf.MoveTowards(zoomAmount, targetZoomAmount, Time.deltaTime * zoomMoveSpeed);

		if (Input.GetMouseButton(1)) // 우클릭을 누르고 있는 동안만 드래그로 공전
		{
			orbitAngle = Mathf.Repeat(orbitAngle + Input.GetAxis("Mouse X") * orbitSensitivity, 360F);
		}

		// 줌은 CameraFollow()의 Y축 스무딩을 우회해야 하므로 홀드가 필요하지만,
		// 공전은 오프셋의 Y값을 바꾸지 않아 CameraFollow()에 맡겨도 스무딩 지연이 생기지 않는다.
		bool zoomActive = zoomAmount > 0F || targetZoomAmount > 0F;
		if (zoomActive != weAreHolding)
		{
			sideCam.HoldCameraFunction(zoomActive);
			weAreHolding = zoomActive;
		}

		Vector3 zoomedDistancePos = Vector3.Lerp(defaultDistancePos, closeUpDistancePos, zoomAmount);
		Quaternion zoomedRotation = Quaternion.Slerp(defaultRotation, closeUpRotation, zoomAmount);

		Quaternion orbitDelta = Quaternion.Euler(0F, orbitAngle, 0F);
		Vector3 finalDistancePos = orbitDelta * zoomedDistancePos;
		Quaternion finalRotation = orbitDelta * zoomedRotation;

		sideCam.SetCameraDistance(finalDistancePos);
		sideCam.SetCameraQuaternionAngle(finalRotation);

		if (weAreHolding)
		{
			Vector3 playerPos = GameRuleManager.instance.playerMove.transform.position;
			sideCam.transform.position = playerPos + finalDistancePos + sideCam.slidePos;
		}
	}

	private void CacheDefaults()
	{
		defaultDistancePos = sideCam.distancePos;
		defaultRotation = sideCam.transform.rotation;

		closeUpDistancePos = new Vector3(defaultDistancePos.x * closeUpDistanceScale, closeUpElevation, defaultDistancePos.z * closeUpDistanceScale);

		// 카메라 위치, 바라볼 지점 모두 플레이어 기준 상대 오프셋이라 플레이어 월드 위치와 무관하게 고정된 방향이 나온다.
		closeUpRotation = Quaternion.LookRotation(faceLookOffset - closeUpDistancePos);

		hasCachedDefaults = true;
	}
}
