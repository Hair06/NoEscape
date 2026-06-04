using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClickableSlider : MonoBehaviour, IPointerDownHandler
{
    private Slider slider;
    private RectTransform sliderRect;

    private void Awake()
    {
        slider = GetComponent<Slider>();
        sliderRect = GetComponent<RectTransform>();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (slider == null || sliderRect == null) return;

        Vector2 localPoint;

        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
            sliderRect,
            eventData.position,
            eventData.pressEventCamera,
            out localPoint))
        {
            float percent = Mathf.InverseLerp(
                sliderRect.rect.xMin,
                sliderRect.rect.xMax,
                localPoint.x
            );

            slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, percent);
        }
    }
}