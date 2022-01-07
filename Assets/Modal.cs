using System.Collections;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Modal : MonoBehaviour
{
    public static Modal Instance;

    public Text GameName;
    public Text GamePlaytime;
    public Text GameGenres;


    public GameObject SliderContainer;
    public Text MinPlaytimeText;
    public Text MaxPlaytimeText;
    public Slider MinSlider;
    public Slider MaxSlider;

    public Transform ToggleTransform;

    public Transform PointContainer;
    public Transform MinPoint;
    public Transform MaxPoint;

    public GameObject PointPrefab;


    public void Awake()
    {
        Instance = this;
        _sliderUpdate = SliderUpdate();
        StartCoroutine(_sliderUpdate);
    }

    IEnumerator _sliderUpdate;
    IEnumerator SliderUpdate()
    {
        for(; ; )
        {
            yield return new WaitForSecondsRealtime(0.1f);
            if (DataManager.Instance.IsDone)
                break;
        }

        SliderContainer.SetActive(true);

        // make setup - First 
        MaxSlider.minValue = 0;
        MaxSlider.maxValue = DataManager.Instance.Games.games.Max(item => item.playtime_forever);
        // Add 1 to the max so it can be selected
        MaxSlider.maxValue = (MaxSlider.maxValue / 60) + 1;
        MinSlider.minValue = 0;
        MinSlider.maxValue = MaxSlider.maxValue;
        MinSlider.onValueChanged.AddListener(delegate { ValueChanged(); });
        MaxSlider.onValueChanged.AddListener(delegate { ValueChanged(); });

        int maxTime = DataManager.Instance.Games.games.Max(item => item.playtime_forever);

        foreach (DataManager.Game game in DataManager.Instance.Games.games)
        {
            var point = Instantiate(PointPrefab, PointContainer);
            point.gameObject.transform.position = Vector2.Lerp(MinPoint.position, MaxPoint.position, (game.playtime_forever * 1.0f) /  maxTime);
        }

        for (; ; )
        {
            // Sanitize input from sliders so minSlider wont pass maxSlider and vice versa
            if (MinSlider.value > MaxSlider.value)
                MinSlider.value = MaxSlider.value;

            MinPlaytimeText.text = (MinSlider.value).ToString() + "h";
            MaxPlaytimeText.text = (MaxSlider.value).ToString() + "h";

            yield return new WaitForEndOfFrame();
        }
    }

    public void ValueChanged()
    {
        Debug.Log("AAAAAAAAAAAAAAA");
        ChartRenderer.Instance.DrawSunburst(MinSlider.value * 60, MaxSlider.value * 60);        
    }

}
