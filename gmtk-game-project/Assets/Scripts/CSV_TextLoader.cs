using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BitWave_Labs.AnimatedTextReveal;

public class CSV_TextLoader : MonoBehaviour
{
    [SerializeField] private AnimateText animateText;
    [SerializeField] private TextAsset csvFile;
    [SerializeField] private string csvFilePath;
    
    [Header("Filtering Criteria")]
    [Tooltip("Matches with 'Dia' column")]
    public int dayIndex = 1;
    
    [Tooltip("Matches with 'Variant' column")]
    public bool quotaBool = true;
    
    // Internal toggle for matching Inici/Final column
    private bool startEnd = true;
    
    private void Start()
    {
        // Optionally load on start
        // LoadTextFromCSV();
    }
    
    /// <summary>
    /// Loads text from CSV file based on filtering criteria and sends it to AnimateText
    /// </summary>
    public void LoadTextFromCSV()
    {
        List<string> textLines = new List<string>();
        string[] data;
        
        // Get CSV data from either TextAsset or file path
        if (csvFile != null)
        {
            data = csvFile.text.Split(new[] { '\r', '\n' }, System.StringSplitOptions.RemoveEmptyEntries);
        }
        else if (!string.IsNullOrEmpty(csvFilePath))
        {
            data = File.ReadAllLines(csvFilePath);
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
                        // Split text by line breaks if any are encoded in the text
                        string[] splitLines = row[3].Split(new[] { "\\n" }, System.StringSplitOptions.None);
                        foreach (string line in splitLines)
                        {
                            textLines.Add(line.Trim());
                        }
                        
                        // Toggle startEnd for next match
                        startEnd = !startEnd;
                    }
                }
            }
        }
        
        // Send the collected lines to AnimateText
        if (animateText != null)
        {
            animateText.SetLines(textLines);
        }
        else
        {
            Debug.LogError("AnimateText reference is missing!");
        }
    }
}
