using UnityEngine;
using TMPro; // Importar TextMeshPro
using System.IO;

public class LoopManager : MonoBehaviour
{
    public TextMeshProUGUI textComponent; // Variable asignable desde el inspector para TMP
    public TextAsset csvFile; // Referencia directa al asset del archivo CSV
    public bool useRandomName = true; // Nuevo bool para decidir si usar nombre random

    public void Continue()
    {
        var currentLoop = GameManager.Instance.GetCurrentLoop();
        if (currentLoop != null && currentLoop.days.Count > 0)
        {
            GameManager.Instance.goToDayScene();
        }
        else
        {
            Debug.LogWarning("No loops available to continue.");
            GameManager.Instance.goToMenuScene();
        }
    }

    void Start()
    {
        if (textComponent == null)
        {
            Debug.LogError("Text component is not assigned!");
            return;
        }

        LoadRandomTextFromCSV();
    }

    public void LoadRandomTextFromCSV()
    {
        if (csvFile != null)
        {
            var lines = csvFile.text.Split('\n'); // Leer líneas del contenido del archivo CSV
            if (lines.Length > 0)
            {
                string selectedLine;
                if (useRandomName)
                {
                    selectedLine = lines[Random.Range(0, lines.Length)];
                }
                else
                {
                    selectedLine = lines[0];
                }
                textComponent.text = selectedLine; // Asignar texto
            }
            else
            {
                Debug.LogWarning("El archivo CSV está vacío.");
            }
        }
        else
        {
            Debug.LogError("No se ha asignado un archivo CSV.");
        }
    }
}