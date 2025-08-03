using System.Collections.Generic;
using UnityEngine;
using BitWave_Labs.AnimatedTextReveal;

public class NarrativeManager : BaseManager
{
    [SerializeField] public AnimateText animateText;
    [SerializeField] public TextAsset csvFile;

    [Header("Filtering Criteria")]
    [Tooltip("Matches with 'Dia' column")]
    public int dayIndex = 1;

    [SerializeField] private bool startEnd = true;
    [SerializeField] private bool quotaBool = true;

    [SerializeField] public List<string> textLines = new List<string>();
    public bool isTextComplete = false;

    private void Awake()
    {
        // Ensure AnimateText component exists and is assigned.
        if (animateText == null)
        {
            animateText = GetComponent<AnimateText>();
            if (animateText == null)
            {
                animateText = gameObject.AddComponent<AnimateText>();
                Debug.LogWarning("[NarrativeManager] AnimateText component was missing and has been added automatically.");
            }
        }
    }

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
    /// Initializes the NarrativeManager from GameManager with actual game data
    /// This method should be called when the narrative scene is loaded
    /// </summary>
    public void InitializeFromGameManager()
    {
        Debug.Log("[NarrativeManager] Initializing from GameManager");
        
        if (GameManager.Instance == null)
        {
            Debug.LogError("[NarrativeManager] GameManager.Instance is null - using fallback values");
            UpdateData(csvFile, 1, true, true);
            StartText();
            return;
        }
        
        // Get actual values from GameManager
        var currentDay = GameManager.Instance.GetCurrentDay();
        var currentEvent = GameManager.Instance.GetCurrentEvent();
        
        int dayNumber = 1; // Default fallback
        bool narrativeStartEnd = true; // Default to start narrative
        bool narrativeQuotaBool = true; // Default quota state
        
        // Get current day number from DayManager via GameManager
        var dayManager = GameManager.Instance.GetManager<DayManager>();
        if (dayManager != null)
        {
            dayNumber = dayManager.currentDayNumber;
        }
        else if (currentDay != null)
        {
            // Fallback: try to get day index from LoopManager
            var loopManager = GameManager.Instance.GetManager<LoopManager>();
            if (loopManager != null)
            {
                dayNumber = loopManager.GetCurrentDayIndex() + 1; // +1 for display (0-based to 1-based)
            }
        }
        
        // Determine if this is start or end narrative based on current event
        if (currentEvent != null)
        {
            var eventConfig = currentEvent.GetEventConfiguration();
            if (eventConfig != null)
            {
                // If event name contains "end" or "complete", it's an end narrative
                narrativeStartEnd = !eventConfig.eventName.ToLower().Contains("end") && 
                                   !eventConfig.eventName.ToLower().Contains("complete");
            }
        }
        
        // TODO: Implement quota logic based on actual game state
        // For now, use default value
        
        Debug.Log($"[NarrativeManager] Loading narrative for day {dayNumber}, startEnd: {narrativeStartEnd}, quota: {narrativeQuotaBool}");
        UpdateData(csvFile, dayNumber, narrativeStartEnd, narrativeQuotaBool);
        StartText();
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
        int searchDay = dayIndex;
        
        // If dayIndex is 0, try to find the first available day in CSV
        if (dayIndex == 0)
        {
            for (int i = 1; i < data.Length; i++)
            {
                string[] row = data[i].Split(';');
                if (row.Length >= 4 && int.TryParse(row[0], out int csvDay))
                {
                    searchDay = csvDay;
                    Debug.Log($"[NarrativeManager] Day 0 not found in CSV, using first available day: {searchDay}");
                    break;
                }
            }
        }

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
                    if (rowDay == searchDay)
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
        
        // If still no lines, provide a default message
        if (textLines.Count == 0)
        {
            Debug.LogWarning($"[NarrativeManager] No text found for Day={searchDay}, StartEnd={startEnd}, Quota={quotaBool}. Adding default text.");
            textLines.Add("Welcome to the game!");
            textLines.Add("Let's get started with your adventure.");
        }

        Debug.Log($"Loaded {textLines.Count} text lines from CSV");
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
        // Debug the specific issue
        if (animateText == null)
        {
            Debug.LogError("[NarrativeManager] AnimateText component is null! Running auto-fix...");
            // Try to auto-fix the AnimateText reference
            TryFixAnimateTextReference();
        }
        
        if (textLines.Count == 0)
        {
            Debug.LogError($"[NarrativeManager] No text lines loaded! CSV file: {(csvFile != null ? csvFile.name : "null")}, Day: {dayIndex}, StartEnd: {startEnd}, Quota: {quotaBool}");
        }
        
        if (animateText != null && textLines.Count > 0)
        {
            Debug.Log($"[NarrativeManager] Starting text animation with {textLines.Count} lines");
            isTextComplete = false;
            animateText.SetTextLines(textLines);
            animateText.StartText();
        }
        else
        {
            Debug.LogError($"[NarrativeManager] Cannot start text - AnimateText: {(animateText != null ? "OK" : "NULL")}, TextLines: {textLines.Count}");
        }
    }
    
    /// <summary>
    /// Try to automatically fix the AnimateText reference
    /// </summary>
    private void TryFixAnimateTextReference()
    {
        var animateTextComponent = GetComponent<BitWave_Labs.AnimatedTextReveal.AnimateText>();
        if (animateTextComponent == null)
        {
            Debug.Log("[NarrativeManager] Adding missing AnimateText component...");
            try
            {
                animateTextComponent = gameObject.AddComponent<BitWave_Labs.AnimatedTextReveal.AnimateText>();
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"[NarrativeManager] Failed to add AnimateText component: {ex.Message}");
                return;
            }
        }
        
        animateText = animateTextComponent;
        if (animateText != null)
        {
            animateText.OnAllTextComplete = OnTextComplete;
            Debug.Log("[NarrativeManager] ✅ Fixed AnimateText reference!");
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