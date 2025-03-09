using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class BubblesInteracts : MonoBehaviour
{
    public Transform player; // Assign player object
    public float showDistance = 3f;
    public float floatSpeed = 1f;
    public float floatAmount = 0.2f;

    [Space]
    public CanvasGroup canvasGroup; // Used for fade in/out
    public KeyCode hotKey = KeyCode.E;

    [Space]
    public UnityEvent onBubbleInteract; // Event exposed to Inspector

    private Vector3 initialPosition;
    public bool isVisible = false;

    void Start()
    {
        if (!player)
        {
            GameObject playergo = GameObject.FindGameObjectWithTag("Player");
            if (playergo)
            {
                player = playergo.transform;
            }
        }

        initialPosition = transform.localPosition;
        canvasGroup.alpha = 0; // Hide initially
    }

    void Update()
    {
        // Distance check
        float distance = Vector2.Distance(player.position, transform.position);
        bool shouldShow = distance < showDistance;

        // Show/Hide with fade effect
        if (shouldShow && !isVisible)
        {
            isVisible = true;
            StopAllCoroutines();
            StartCoroutine(FadeUI(1));
        }
        else if (!shouldShow && isVisible)
        {
            isVisible = false;
            StopAllCoroutines();
            StartCoroutine(FadeUI(0));
        }

        if (isVisible && Input.GetKeyDown(hotKey))
        {
            onBubbleInteract?.Invoke(); // Trigger the event
        }
        // Floating effect
        transform.localPosition =
            initialPosition + new Vector3(0, Mathf.Sin(Time.time * floatSpeed) * floatAmount, 0);
    }

    private IEnumerator FadeUI(float targetAlpha)
    {
        float startAlpha = canvasGroup.alpha;
        float duration = 0.3f;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / duration);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;
    }

    public void Log(string message)
    {
        Debug.Log(message);
    }

    public void ChangeScene(string sceneName)
    {
        GameManager.Instance.ChangeScene(sceneName);
    }

    public void StartCombat()
    {
        GameManager.Instance.StartCombat();
    }
}
