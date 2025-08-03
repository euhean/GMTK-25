using System.Collections.Generic;
using System.IO;
using System.Text;
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
        if (animateText == null)
        {
            animateText = GetComponent<AnimateText>();
            if (animateText == null)
            {
                Debug.LogError("AnimateText component not found! Please add it to the same GameObject.");
                return;
            }
        }

        // Register the callback for text completion
        animateText.OnAllTextComplete = OnTextComplete;
        
        // Try to initialize from GameManager if it exists
        InitializeFromGameManager();
    }

    /// <summary>
    /// Initializes the NarrativeManager with data from GameManager
    /// </summary>
    public void InitializeFromGameManager()
    {
        if (GameManager.Instance == null)
        {
            Debug.LogWarning("GameManager instance not found. Using default values for narrative.");
            LoadTextFromCSV();
            StartText();
            return;
        }

        // Get values from GameManager
        TextAsset narrativeCsv = GameManager.Instance.GetNarrativeCsv();
        if (narrativeCsv == null)
        {
            Debug.LogError("No narrative CSV file found in GameManager!");
            return;
        }

        int currentDay = GameManager.Instance.currentDay;
        bool narrativeStartEnd = GameManager.Instance.GetNarrativeStartEnd();
        bool narrativeQuotaBool = GameManager.Instance.GetNarrativeQuotaBool();
        
        // Update local values
        UpdateData(narrativeCsv, currentDay, narrativeStartEnd, narrativeQuotaBool);
        
        // Start displaying the text
        StartText();
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
        textLines.Clear();
        isTextComplete = false;

        string[] data;

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
            // Split by comma but respect quoted fields
            string[] row = SplitCSVLine(data[i]);
            
            if (row.Length >= 4)
            {
                // Parse the day value
                if (int.TryParse(row[0], out int rowDay))
                {
                    if (rowDay == dayIndex)
                    {
                        bool rowStartEnd = row[1].Trim().ToLower() == "true";
                        bool rowQuota = row[2].Trim().ToLower() == "true";

                        if (rowStartEnd == startEnd)
                        {
                            string text = row[3].Trim().Trim('"'); // Remove surrounding quotes
                            if (rowQuota == quotaBool)
                            {
                                string[] splitLines = text.Split(new[] { "/n" }, System.StringSplitOptions.None);
                                foreach (string line in splitLines)
                                {
                                    if (!string.IsNullOrWhiteSpace(line))
                                    {
                                        textLines.Add(line.Trim());
                                    }
                                }
                            }
                            else
                            {
                                string[] splitLines = text.Split(new[] { "/n" }, System.StringSplitOptions.None);
                                foreach (string line in splitLines)
                                {
                                    if (!string.IsNullOrWhiteSpace(line))
                                    {
                                        fallbackLines.Add(line.Trim());
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        if (textLines.Count == 0 && fallbackLines.Count > 0)
        {
            textLines.AddRange(fallbackLines);
        }

        Debug.Log($"Loaded {textLines.Count} lines of text");
    }

    private string[] SplitCSVLine(string line)
    {
        List<string> result = new List<string>();
        bool inQuotes = false;
        StringBuilder currentField = new StringBuilder();
        
        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (line[i] == ',' && !inQuotes)
            {
                result.Add(currentField.ToString());
                currentField.Clear();
            }
            else
            {
                currentField.Append(line[i]);
            }
        }
        
        result.Add(currentField.ToString());
        return result.ToArray();
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
        // When Space or left mouse button is pressed, advance to the next line if there's text to display
        if ((Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0)) && animateText != null && textLines.Count > 0)
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
        if (animateText == null)
        {
            Debug.LogError("AnimateText component is missing! Please add it to the same GameObject.");
            return;
        }

        if (textLines == null || textLines.Count == 0)
        {
            Debug.LogError("No text lines available! Make sure the CSV file is properly loaded.");
            return;
        }

        isTextComplete = false;
        animateText.SetTextLines(textLines);
        animateText.StartText();
    }
}