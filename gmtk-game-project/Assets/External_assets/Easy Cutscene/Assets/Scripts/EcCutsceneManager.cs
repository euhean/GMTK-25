using UnityEngine;
using HisaGames.Cutscene;

namespace HisaGames.CutsceneManager
{
    public class EcCutsceneManager : MonoBehaviour
    {
        public static EcCutsceneManager instance;

        [Header("Cutscene Settings")]
        [SerializeField] private EcCutscene[] cutscenes;
        [SerializeField] private string currentCutscene;
        [SerializeField] private GameObject dialogPanel;

        [Header("Text Settings")]
        public float chatTypingDelay = 0.05f;

        private void Awake()
        {
            instance = this;
            InitCutscenes(currentCutscene);
        }

        public void closeCutscenes()
        {
            dialogPanel.SetActive(false);
            foreach(var cutscene in cutscenes)
            {
                cutscene.gameObject.SetActive(false);
            }
        }

        public void InitCutscenes(string cutsceneName)
        {
            closeCutscenes();
            currentCutscene = cutsceneName;
            
            EcCutscene cutscene = getCutscenesObject(currentCutscene);
            if (cutscene != null)
            {
                dialogPanel.SetActive(true);
                cutscene.gameObject.SetActive(true);
                cutscene.StartCutscene();
            }
        }

        public EcCutscene getCutscenesObject(string name)
        {
            foreach(var cutscene in cutscenes)
            {
                if (name == cutscene.name)
                    return cutscene;
            }
            return null;
        }
    }
}