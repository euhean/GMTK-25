using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BitWave_Labs.AnimatedTextReveal;

public class NarrativeManager : MonoBehaviour
{
    [SerializeField] private AnimateText animateText;
    [SerializeField] private TextAsset csvFile;

    [Header("Filtering Criteria")]
    [Tooltip("Matches with 'Dia' column")]
    public int dayIndex = 1;

    [SerializeField] private bool startEnd = true;
    [SerializeField] private bool quotaBool = true;

    [SerializeField] private List<string> textLines = new List<string>();
    private bool isTextComplete = false;

    private void Start()
    {
        // Register the callback for text completion
        if (animateText != null)
        {
            animateText.OnAllTextComplete = OnTextComplete;
        }
        
        // Try to initialize from GameManager if it exists
        InitializeFromGameManager();
    }

    /// <summary>
    /// Initializes the NarrativeManager with data from GameManager
    /// </summary>
    public void InitializeFromGameManager()
    {
        if (GameManager.Instance != null)
        {
            // Get values from GameManager
            TextAsset narrativeCsv = GameManager.Instance.GetNarrativeCsv();
            int currentDay = GameManager.Instance.currentDay;
            bool narrativeStartEnd = GameManager.Instance.GetNarrativeStartEnd();
            bool narrativeQuotaBool = GameManager.Instance.GetNarrativeQuotaBool();
            
            // Update local values
            UpdateData(narrativeCsv, currentDay, narrativeStartEnd, narrativeQuotaBool);
            
            // Start displaying the text
            StartText();
        }
        else
        {
            Debug.LogWarning("GameManager instance not found. Using default values for narrative.");
            LoadTextFromCSV();
            StartText();
        }
    }

    /// <summary>
    /// Called when all text has been displayed
    /// </summary>
    public void OnTextComplete()
    {
        
        isTextComplete = true;
        // Any additional actions when text is complete can go here
    }

    /// <summary>
    /// Loads text from CSV file based on filtering criteria and sends it to AnimateText
    /// </summary>
    public void LoadTextFromCSV()
    {
        // Clear previous lines
        textLines.Clear();
        isTextComplete = false;

        string[] data;

        // Get CSV data from TextAsset
        if (csvFile != null)
        {
            data = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        }
        else
        {
            Debug.LogError("No CSV file assigned!");
            return;
        }

        List<string> fallbackLines = new List<string>();

        // Skip header row
        for (int i = 1; i < data.Length; i++)
        {
            string[] row = data[i].Split(';');
            if (row.Length >= 4)
            {
                // DEBUG log for current row and column
                // 

                // Parse the day value
                if (int.TryParse(row[0], out int rowDay))
                {
                    if (rowDay == dayIndex)
                    {
                        // Parse start/end boolean
                        bool rowStartEnd = row[1].Trim().ToLower() == "true";
                        bool rowQuota = row[2].Trim().ToLower() == "true";

                        // DEBUG
                        // 

                        if (rowStartEnd == startEnd)
                        {
                            if (rowQuota == quotaBool)
                            {
                                // Debug log for matched row
                                

                                // Split text by line breaks if any are encoded in the text
                                string[] splitLines = row[3].Split(new[] { "\\n" }, System.StringSplitOptions.None);
                                foreach (string line in splitLines)
                                {
                                    
                                    textLines.Add(line.Trim());
                                }
                            }
                            else
                            {
                                // Save fallback lines if quotaBool doesn't match
                                string[] splitLines = row[3].Split(new[] { "\\n" }, System.StringSplitOptions.None);
                                foreach (string line in splitLines)
                                {
                                    fallbackLines.Add(line.Trim());
                                }
                            }
                        }
                    }
                }
            }
        }

        // If no lines were added, use fallback lines
        if (textLines.Count == 0 && fallbackLines.Count > 0)
        {
            
            textLines.AddRange(fallbackLines);
        }
    }

    // Se agrega el método UpdateData para actualizar los datos
    public void UpdateData(TextAsset newCsvFile, int newDayIndex, bool newStartEnd, bool newQuotaBool)
    {
        csvFile = newCsvFile;
        dayIndex = newDayIndex;
        startEnd = newStartEnd;
        quotaBool = newQuotaBool;

        LoadTextFromCSV();
    }

    private void Update()
    {
        // When Space is pressed, advance to the next line if there's text to display
        if (Input.GetKeyDown(KeyCode.Space) && animateText != null && textLines.Count > 0)
        {
            if (isTextComplete)
            {
                
                GameManager.Instance.AdvanceToNextEvent();
            }
            else
            {
                animateText.AdvanceToNextLine();
            }
        }
    }

    /// <summary>
    /// Start showing text from the beginning
    /// </summary>
    public void StartText()
    {
        if (animateText != null && textLines.Count > 0)
        {
            isTextComplete = false;
            animateText.SetTextLines(textLines);
            animateText.StartText();
        }
        else
        {
            Debug.LogError("AnimateText reference is missing or no text lines available!");
        }
    }
}