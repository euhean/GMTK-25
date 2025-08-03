using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using HisaGames.CutsceneManager;

namespace HisaGames.Cutscene
{
    public class EcCutscene : MonoBehaviour
    {
        [System.Serializable]
        public class CutsceneData
        {
            [Header("Cutscene Data")]
            public string name;
            [TextArea] public string dialogText;
            public CSUnityEvent onDialogStart;
            public CSUnityEvent onDialogEnd;
        }

        [System.Serializable]
        public class CSUnityEvent : UnityEvent { }

        [SerializeField] private CutsceneData[] cutsceneData;
        [SerializeField] private Text dialogText;
        
        private int currentID;
        private string currentDialogText;
        private bool isTyping;
        private float typingTimer;

        void Start()
        {
            StartCutscene();
        }

        void Update()
        {
            if (isTyping)
                TypeText();

            if (Input.GetMouseButtonDown(0)) 
                PlayNextCutscene();
        }

        public void StartCutscene()
        {
            currentID = 0;
            isTyping = false;
            typingTimer = EcCutsceneManager.instance.chatTypingDelay;
            PlayCutscene();
        }

        void PlayCutscene()
        {
            if(cutsceneData[currentID].onDialogStart != null)
                cutsceneData[currentID].onDialogStart.Invoke();

            dialogText.text = "";
            currentDialogText = cutsceneData[currentID].dialogText;
            isTyping = true;
        }

        void TypeText()
        {
            typingTimer -= Time.deltaTime;
            if (typingTimer <= 0)
            {
                if (dialogText.text.Length < currentDialogText.Length)
                {
                    dialogText.text += currentDialogText[dialogText.text.Length];
                    typingTimer = EcCutsceneManager.instance.chatTypingDelay;
                }
                else
                {
                    isTyping = false;
                }
            }
        }

        public void PlayNextCutscene()
        {
            if (dialogText.text == currentDialogText)
            {
                if(cutsceneData[currentID].onDialogEnd != null)
                    cutsceneData[currentID].onDialogEnd.Invoke();

                if (currentID < cutsceneData.Length - 1)
                {
                    currentID++;
                    PlayCutscene();
                }
                else
                {
                    EcCutsceneManager.instance.closeCutscenes();
                    if (DeskManager.Instance != null)
                        DeskManager.Instance.FinishDialogEvent();
                }
            }
            else
            {
                dialogText.text = currentDialogText;
                isTyping = false;
            }
        }
    }
}