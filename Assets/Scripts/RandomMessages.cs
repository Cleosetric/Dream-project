using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RandomMessages : MonoBehaviour
{
    public TextMeshProUGUI textComponent;
    public float typingSpeed = 0.05f; // Speed of typing
    public float waitTime = 2f; // Time before switching text

    [TextArea(3, 5)]
    public List<string> messages = new List<string>(); // List of messages
    private string currentText = "";
    private bool isTyping = false;

    private void Update()
    {
        if (GetComponent<BubblesInteracts>() != null)
        {
            BubblesInteracts bubble = GetComponent<BubblesInteracts>();

            if (bubble.isVisible && !isTyping)
            {
                // Stop any ongoing coroutine to restart properly
                StopAllCoroutines();

                // Start the typing effect
                StartCoroutine(ShowRandomText());
            }
        }
    }

    IEnumerator ShowRandomText()
    {
        isTyping = true; // Prevent multiple runs
        // Pick a random message from the list
        string newMessage = messages[Random.Range(0, messages.Count)];
        currentText = newMessage;
        textComponent.text = "";

        // Typing effect
        foreach (char letter in currentText)
        {
            textComponent.text += letter;
            yield return new WaitForSeconds(typingSpeed);
        }

        // Wait before changing text
        yield return new WaitForSeconds(waitTime);
        isTyping = false; // Allow new coroutine after finishing
    }
}
