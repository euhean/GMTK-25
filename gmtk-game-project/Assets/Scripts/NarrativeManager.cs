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
    
    [Tooltip("Matches with 'Variant' column")]
    public bool quotaBool = true;
    
    // Internal toggle for matching Inici/Final column
    [SerializeField] private bool startEnd = true;
    
    [SerializeField] private List<string> textLines = new List<string>();
    private bool isTextComplete = false;

    private void Start()
    {
        // Register the callback for text completion
        if (animateText != null)
        {
            animateText.OnAllTextComplete = OnTextComplete;
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

        // Reset the alternating boolean
        startEnd = true;

        // Skip header row
        for (int i = 1; i < data.Length; i++)
        {
            string[] row = data[i].Split(';');
            if (row.Length >= 4)
            {
                // Debug log for current row and column
                Debug.Log($"Processing row {i}, columns: {string.Join(", ", row)}");

                // Parse the day value
                if (int.TryParse(row[0], out int rowDay))
                {
                    // Parse start/end boolean
                    bool rowStartEnd = row[1].Trim().ToLower() == "true";

                    // Parse variant boolean
                    bool rowVariant = row[2].Trim().ToLower() == "true";

                    // Match conditions
                    if (rowDay == dayIndex && rowStartEnd == startEnd && rowVariant == quotaBool)
                    {
                        // Debug log for matched row
                        Debug.Log($"Matched row {i}: Day={rowDay}, StartEnd={rowStartEnd}, Variant={rowVariant}");

                        // Split text by line breaks if any are encoded in the text
                        string[] splitLines = row[3].Split(new[] { "\\n" }, System.StringSplitOptions.None);
                        foreach (string line in splitLines)
                        {
                            Debug.Log($"Adding line: {line.Trim()}");
                            textLines.Add(line.Trim());
                        }
                    }
                }
            }
        }
    }

    // Se agrega el método UpdateData para actualizar los datos
    public void UpdateData(TextAsset newCsvFile, int newDayIndex, bool newQuotaBool, bool newStartEnd)
    {
        csvFile = newCsvFile;
        dayIndex = newDayIndex;
        quotaBool = newQuotaBool;
        startEnd = newStartEnd;

        LoadTextFromCSV();
    }
    
    private void Update()
    {
        // When Space is pressed, advance to the next line if there's text to display
        if (Input.GetKeyDown(KeyCode.Space) && animateText != null && textLines.Count > 0)
        {
            if (isTextComplete)
            {
                Debug.Log("All text already displayed. No more lines to advance.");
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
            Debug.LogError("Narrativa finalizada!");
        }
    }
}