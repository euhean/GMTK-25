using System.Collections.Generic;
using UnityEngine;


namespace HisaGames.Character
{
    [System.Serializable]
    public class EcCharacter : MonoBehaviour
    {
        [Header("Sprite Settings")]
        [Tooltip("SpriteRenderer component for displaying character sprites.")]
        public SpriteRenderer spriteRenderer;

        [Tooltip("Array of sprites used for the character.")]
        public Sprite[] spriteImages;

        [Tooltip("Dictionary to map sprite names to their corresponding sprites.")]
        private Dictionary<string, Sprite> spriteDictionary;

        void Awake()
        {
            // Initialize the sprite dictionary & Populate the dictionary with sprites
            spriteDictionary = new Dictionary<string, Sprite>();
            foreach (var sprite in spriteImages)
            {
                spriteDictionary[sprite.name] = sprite;
            }

            // Get the sprite renderer component
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        /// <summary>
        /// Change the sprite of the character by name.
        /// </summary>
        /// <param name="spriteName">Name of the sprite to change to.</param>
        public void ChangeSpriteByName(string spriteName)
        {
            if (spriteRenderer != null)
            {
                if (spriteDictionary.TryGetValue(spriteName, out var newSprite))
                {
                    spriteRenderer.sprite = newSprite;
                }
                else
                {
                    Debug.LogWarning($"Sprite with name {spriteName} not found.");
                }
            }
            else
            {
                Debug.LogWarning("spriteRenderer is null.");
            }
        }
    }
}