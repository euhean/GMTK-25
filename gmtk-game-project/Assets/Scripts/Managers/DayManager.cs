using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using TMPro;

public class DayManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private float waitTimeForEvent = 5f;
    public GameManager.Day currentDay;
    private Coroutine autoEventCoroutine;

    private void Awake()
    {
    
    }
    
    private void Start()
    {
        setDay();
        StartAutoEventCoroutine();
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
        {
            if (autoEventCoroutine != null)
            {
                StopCoroutine(autoEventCoroutine);
                autoEventCoroutine = null;
            }

            var currentDay = GameManager.Instance.GetCurrentDay();
            if (currentDay != null && currentDay.events.Count > 0)
            {
                GameManager.Instance.runEvent();
            }
            else
            {
                GameManager.Instance.AdvanceToNextDay();
            }
        }
    }

    private void StartAutoEventCoroutine()
    {
        if (autoEventCoroutine != null)
        {
            StopCoroutine(autoEventCoroutine);
        }
        autoEventCoroutine = StartCoroutine(AutoEventCoroutine());
    }

    private IEnumerator AutoEventCoroutine()
    {
        yield return new WaitForSeconds(waitTimeForEvent);
        
        var currentDay = GameManager.Instance.GetCurrentDay();
        if (currentDay != null && currentDay.events.Count > 0)
        {
            GameManager.Instance.runEvent();
        }
        
        autoEventCoroutine = null;
    }

    public void SetDayText(string newText)
    {
        if (dayText != null)
        {

            
            dayText.text = newText;
        }
    }

    public void setDay()
    {
        currentDay = GameManager.Instance.GetCurrentDay();
        if (currentDay != null)
        {
            SetDayText(currentDay.dayName);
        }
        else
        {
            SetDayText("No Day Available");
        }
    }
}