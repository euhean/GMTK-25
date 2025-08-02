using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using UnityEngine.SceneManagement;
using UnityEngine.UI;


public class DayManager : MonoBehaviour
{
    [Header("UI Reference")]
    [SerializeField] private Text dayText;
    public LoopManager.Day currentDay;

    private void Awake()
    {
    
    }
    
    private void Start()
    {
        setDay();
        
    }
    
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameManager.Instance.StartDay();

        }
    }
    
    public void SetDayText(string newText)
    {
        if (dayText != null)
        {

            Debug.Log("Dia: " + currentDay.dayName);
            dayText.text = newText;
        }
    }

    public void setDay()
    {
        currentDay = GameManager.Instance.GetCurrentDay();
        SetDayText(currentDay.dayName);
    }
}