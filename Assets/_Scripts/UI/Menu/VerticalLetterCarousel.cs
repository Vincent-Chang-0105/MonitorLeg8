using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VerticalLetterCarousel : MonoBehaviour
{
    [SerializeField] private string word = " ROTINOM";
    [SerializeField] private TextMeshProUGUI letterPrefab;
    [SerializeField] private float spacing = 105f;
    [SerializeField] private float speed = 50f;
    [SerializeField] private float beltHeight = 1200f; // Height before recycling to top
    
    // Store letter objects in order of appearance
    private List<LetterObject> letters = new List<LetterObject>();
    
    // Class to track each letter
    private class LetterObject
    {
        public TextMeshProUGUI textComponent;
        public RectTransform rectTransform;
        public char character;
        
        public LetterObject(TextMeshProUGUI text, char c)
        {
            textComponent = text;
            rectTransform = text.GetComponent<RectTransform>();
            character = c;
        }
    }

    private void Start()
    {
        CreateCarousel();
    }

    void CreateCarousel()
    {
        Vector3 currentPos = Vector3.zero;
        
        // Create the letter sequence - MONITORMONITORMONITOR
        string repeatedWord = "";
        for (int i = 0; i < 2; i++) // Repeat 3 times
        {
            repeatedWord += word;
        }
        // Create each letter
        for (int i = 0; i < repeatedWord.Length; i++)
        {
            char c = repeatedWord[i];
            // Create letter
            TextMeshProUGUI newLetter = Instantiate(letterPrefab, transform);
            newLetter.text = c.ToString();
            
            // Position letter
            RectTransform rect = newLetter.GetComponent<RectTransform>();
            rect.anchoredPosition = currentPos;
            
            // Random rotation for visual interest
            float zRotation = Random.Range(-15f, 15f);
            rect.localRotation = Quaternion.Euler(0, 0, zRotation);
            
            // Add to tracking list
            letters.Add(new LetterObject(newLetter, c));
            
            // Move Up for next letter
            currentPos.y += spacing;
        }
    }

    private void Update()
    {
        // Track the highest letter for recycling
        float highestY = float.MinValue;
        foreach (var letter in letters)
        {
            if (letter.rectTransform.anchoredPosition.y > highestY)
                highestY = letter.rectTransform.anchoredPosition.y;
        }
        
        // Move and recycle each letter
        foreach (var letter in letters)
        {
            // Move downward
            letter.rectTransform.anchoredPosition += Vector2.down * speed * Time.deltaTime;
            
            // If below threshold, recycle to top
            if (letter.rectTransform.anchoredPosition.y < -beltHeight)
            {
                // Move to top of stack
                letter.rectTransform.anchoredPosition = new Vector2(
                    letter.rectTransform.anchoredPosition.x,
                    highestY + spacing
                );
                
                // Ensure text is correct
                letter.textComponent.text = letter.character.ToString();
                
                // New random rotation
                float zRotation = Random.Range(-15f, 15f);
                letter.rectTransform.localRotation = Quaternion.Euler(0, 0, zRotation);
                
                // Update highest Y after repositioning
                highestY = letter.rectTransform.anchoredPosition.y;
            }
        }
    }
}