using UnityEngine;

public class ManagerTester : MonoBehaviour
{
    // Añadido para emular el GameManager: referencia a NarrativeManager y CSV de prueba
    public NarrativeManager narrativeManager;
    public TextAsset testCsv;

    // Parámetro adicional para startEnd
    public int testDayIndex = 1;
    public bool testStartEnd = true;
    public bool testQuotaBool = true;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        TestNarrativeManager();
    }

    // Método público para probar NarrativeManager desde el inspector
    [ContextMenu("Test Narrative Manager")]
    public void TestNarrativeManager()
    {
        Debug.Log("Testing NarrativeManager...");

        // Si no hay referencia, intenta buscar el NarrativeManager en la escena.
        if (narrativeManager == null)
        {
            narrativeManager = FindFirstObjectByType<NarrativeManager>();
        }

        if (narrativeManager != null)
        {
            // Emula el envío de datos desde el GameManager al NarrativeManager:
            narrativeManager.UpdateData(testCsv, testDayIndex, testStartEnd, testQuotaBool);
            narrativeManager.StartText();
        }
        else
        {
            Debug.LogError("NarrativeManager not found!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
