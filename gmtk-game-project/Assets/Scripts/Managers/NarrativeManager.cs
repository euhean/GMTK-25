using System.Collections.Generic;
using UnityEngine;
using BitWave_Labs.AnimatedTextReveal;

public class NarrativeManager : BaseManager
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
    /// Initializes the NarrativeManager from GameManager with placeholder values
    /// TODO: Implement proper narrative data loading from EventConfiguration
    /// </summary>
    public void InitializeFromGameManager()
    {
        if (GameManager.Instance != null)
        {
            // Use placeholder values since narrative system needs to be properly configured
            TextAsset narrativeCsv = csvFile; // Use existing csvFile for now
            int currentDay = 1; // TODO: Should come from DayManager
            bool narrativeStartEnd = true; // TODO: Should be configurable
            bool narrativeQuotaBool = true; // TODO: Should be based on actual game state
            
            // Update local values with defaults for now
            UpdateData(narrativeCsv, currentDay, narrativeStartEnd, narrativeQuotaBool);
            StartText();
        }
        else
        {
            Debug.LogWarning("[NarrativeManager] GameManager not found - using default values");
        }
    }

    /// <summary>
    /// Called when all text has been displayed
    /// </summary>
    public void OnTextComplete()
    {
        Debug.Log("All narrative text has been displayed.");
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
                // Debug.Log($"Processing row {i}, columns: {string.Join(", ", row)}");

                // Parse the day value
                if (int.TryParse(row[0], out int rowDay))
                {
                    if (rowDay == dayIndex)
                    {
                        // Parse start/end boolean
                        bool rowStartEnd = row[1].Trim().ToLower() == "true";
                        bool rowQuota = row[2].Trim().ToLower() == "true";

                        // DEBUG
                        // Debug.Log($"Row {i} matches Day={rowDay}, rowStartEnd={rowStartEnd}, currentStartEnd={startEnd}, rowQuota={rowQuota}, currentQuota={quotaBool}");

                        if (rowStartEnd == startEnd)
                        {
                            if (rowQuota == quotaBool)
                            {
                                // Debug log for matched row
                                Debug.Log($"Matched row {i}: Day={rowDay}, StartEnd={rowStartEnd}, Quota={rowQuota}");

                                // Split text by line breaks if any are encoded in the text
                                string[] splitLines = row[3].Split(new[] { "\\n" }, System.StringSplitOptions.None);
                                foreach (string line in splitLines)
                                {
                                    Debug.Log($"Adding line: {line.Trim()}");
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
            Debug.Log("No rows matched quotaBool. Using fallback lines.");
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
                Debug.Log("NARRATIVA FINALIZADA");
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
    
    #region BaseManager Implementation
    
    protected override void OnManagerStart()
    {
        Debug.Log($"[{ManagerID}] Narrative Manager started");
        
        // Auto-initialize from GameManager if not manually configured
        if (csvFile == null || dayIndex <= 0)
        {
            InitializeFromGameManager();
        }
    }
    
    protected override void OnManagerEnd()
    {
        Debug.Log($"[{ManagerID}] Narrative Manager ended");
    }
    
    protected override void OnManagerUpdate()
    {
        // Narrative manager update logic
        // Could handle input for advancing text, etc.
    }
    
    #endregion
}