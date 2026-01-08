using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

// 鼠标指针悬停时让 UI 按钮轻微抖动（用于地图场景的按钮）
public class ButtonHoverShake : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	[Tooltip("抖动强度（像素）")]
	public float intensity = 6f;
	[Tooltip("抖动频率")]
	public float frequency = 20f;

	private RectTransform rectTransform;
	private Vector3 originalAnchoredPos;
	private Coroutine shakeCoroutine;

	void Awake()
	{
		rectTransform = GetComponent<RectTransform>();
		if (rectTransform != null)
		{
			originalAnchoredPos = rectTransform.anchoredPosition3D;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (rectTransform == null) return;
		if (shakeCoroutine == null) shakeCoroutine = StartCoroutine(ShakeLoop());
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		if (shakeCoroutine != null)
		{
			StopCoroutine(shakeCoroutine);
			shakeCoroutine = null;
		}
		if (rectTransform != null) rectTransform.anchoredPosition3D = originalAnchoredPos;
	}

	private IEnumerator ShakeLoop()
	{
		// 使用 PerlinNoise 产生较平滑的抖动
		while (true)
		{
			float t = Time.time * frequency;
			float nx = Mathf.PerlinNoise(t, 0f) * 2f - 1f;
			float ny = Mathf.PerlinNoise(0f, t) * 2f - 1f;
			rectTransform.anchoredPosition3D = originalAnchoredPos + new Vector3(nx * intensity, ny * intensity, 0f);
			yield return null;
		}
	}
}


