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
            // Verificar si hay eventos en el día actual
            var currentDay = GameManager.Instance.GetCurrentDay();
            if (currentDay != null && currentDay.events.Count > 0)
            {
                GameManager.Instance.runEvent(); // Ir al primer evento
            }
            else
            {
                
                GameManager.Instance.AdvanceToNextDay();
            }
        }
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
        SetDayText(currentDay.dayName);
    }
}