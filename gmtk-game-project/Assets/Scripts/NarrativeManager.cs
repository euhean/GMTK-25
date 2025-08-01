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
                    if (rowDay == dayIndex)
                    {
                        // Parse start/end boolean
                        bool rowStartEnd = row[1].Trim().ToLower() == "true";

                        Debug.Log($"Row {i} matches Day={rowDay}, StartEnd={rowStartEnd}, currentStartEnd={startEnd}, Quota={quotaBool}");

                        if (rowStartEnd == startEnd)
                        {
                            // Parse variant boolean
                            bool rowQuota = row[2].Trim().ToLower() == "true";

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
                        }
                    }
                }
            }
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