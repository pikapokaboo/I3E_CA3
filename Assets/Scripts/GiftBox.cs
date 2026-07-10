using System.Collections;
using UnityEngine;

public class GiftBox : MonoBehaviour, IInteractable
{
    [SerializeField] private int interactionsBeforeOpening = 3;
    [SerializeField] private float shakeDuration = 0.45f;
    [SerializeField] private float shakeAmount = 0.09f;
    [SerializeField] private float shakeRotation = 8f;
    [SerializeField] private float hopHeight = 1.25f;
    [SerializeField] private float hopDuration = 0.55f;
    [SerializeField] private GameObject ballPrefab;

    private int interactionCount;
    private bool isAnimating;
    private Vector3 startPosition;
    private Quaternion startRotation;

    private void Awake()
    {
        startPosition = transform.position;
        startRotation = transform.rotation;
    }

    public void Interact()
    {
        if (isAnimating)
        {
            return;
        }

        interactionCount++;

        if (interactionCount >= interactionsBeforeOpening)
        {
            StartCoroutine(OpenRoutine());
        }
        else
        {
            StartCoroutine(ShakeRoutine());
        }
    }

    private IEnumerator ShakeRoutine()
    {
        isAnimating = true;

        float elapsed = 0f;
        while (elapsed < shakeDuration)
        {
            float pulse = Mathf.Sin(elapsed * 75f);
            transform.position = startPosition + transform.right * (pulse * shakeAmount);
            transform.rotation = startRotation * Quaternion.Euler(0f, 0f, pulse * shakeRotation);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = startPosition;
        transform.rotation = startRotation;
        isAnimating = false;
    }

    private IEnumerator OpenRoutine()
    {
        isAnimating = true;

        float elapsed = 0f;
        while (elapsed < hopDuration)
        {
            float t = Mathf.Clamp01(elapsed / hopDuration);
            float hop = Mathf.Sin(t * Mathf.PI) * hopHeight;
            float pulse = Mathf.Sin(elapsed * 85f);

            transform.position = startPosition + Vector3.up * hop + transform.right * (pulse * shakeAmount);
            transform.rotation = startRotation * Quaternion.Euler(pulse * shakeRotation, 0f, pulse * shakeRotation);

            elapsed += Time.deltaTime;
            yield return null;
        }

        SpawnBall();
        Destroy(gameObject);
    }

    private void SpawnBall()
    {
        if (ballPrefab == null)
        {
            return;
        }

        Vector3 spawnPosition = startPosition + Vector3.up * 0.5f;
        Instantiate(ballPrefab, spawnPosition, Quaternion.identity);
    }
}
