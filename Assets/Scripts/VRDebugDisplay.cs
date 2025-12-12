using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

/// <summary>
/// Displays debug messages in VR as a floating canvas.
/// Attach to any GameObject and it will create a world-space canvas showing recent logs.
/// </summary>

// not in use
public class VRDebugDisplay : MonoBehaviour
{
    private GameObject canvasObj;
    private Text debugText;
    private List<string> logMessages = new List<string>();
    private int maxMessages = 15;

    [Header("Display Settings")]
    public Vector3 displayPosition = new Vector3(0, 1.5f, 2f);
    public Vector3 displayRotation = new Vector3(0, 0, 0);
    public float displayScale = 0.005f;

    void Start()
    {
        CreateDebugCanvas();
        Application.logMessageReceived += HandleLog;

        AddLog("=== VR Debug Display Active ===");
    }

    void CreateDebugCanvas()
    {
        canvasObj = new GameObject("VRDebugCanvas");
        Canvas canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;

        canvasObj.transform.position = displayPosition;
        canvasObj.transform.rotation = Quaternion.Euler(displayRotation);
        canvasObj.transform.localScale = Vector3.one * displayScale;

        RectTransform canvasRect = canvasObj.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(800, 600);

        GameObject panelObj = new GameObject("Panel");
        panelObj.transform.SetParent(canvasObj.transform, false);
        Image panelImage = panelObj.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.8f);
        RectTransform panelRect = panelObj.GetComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.sizeDelta = Vector2.zero;

        GameObject textObj = new GameObject("DebugText");
        textObj.transform.SetParent(panelObj.transform, false);
        debugText = textObj.AddComponent<Text>();
        debugText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        debugText.fontSize = 14;
        debugText.color = Color.white;
        debugText.alignment = TextAnchor.UpperLeft;
        debugText.horizontalOverflow = HorizontalWrapMode.Wrap;
        debugText.verticalOverflow = VerticalWrapMode.Overflow;

        RectTransform textRect = textObj.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.sizeDelta = new Vector2(-20, -20);
        textRect.anchoredPosition = Vector2.zero;
    }

    void HandleLog(string logString, string stackTrace, LogType type)
    {
        if (logString.Contains("[CUBE]") ||
            logString.Contains("[VR ROTATION]") ||
            logString.Contains("[FLOOR]") ||
            logString.Contains("XR") ||
            logString.Contains("Interactor") ||
            logString.Contains("Grab") ||
            logString.Contains("Hover") ||
            logString.Contains("SELECT"))
        {
            string prefix = type == LogType.Error ? "[ERROR] " :
                           type == LogType.Warning ? "[WARN] " : "";
            AddLog(prefix + logString);
        }
    }

    void AddLog(string message)
    {
        logMessages.Add(System.DateTime.Now.ToString("HH:mm:ss") + " " + message);

        if (logMessages.Count > maxMessages)
        {
            logMessages.RemoveAt(0);
        }

        UpdateDisplay();
    }

    void UpdateDisplay()
    {
        if (debugText != null)
        {
            debugText.text = string.Join("\n", logMessages);
        }
    }

    void OnDestroy()
    {
        Application.logMessageReceived -= HandleLog;
    }

    void Update()
    {
        // Press Space to toggle visibility (for testing in editor)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            canvasObj.SetActive(!canvasObj.activeSelf);
        }
    }
}
