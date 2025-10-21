using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

public class ButtonHandling : MonoBehaviour {
    public UDPReceiver udpReceiver;

    [Header("Number Buttons")]
    public Button[] numberButtons;   // 0–9 buttons

    [Header("Operator Buttons")]
    public Button plusButton;
    public Button minusButton;
    public Button multiplyButton;
    public Button divideButton;
    public Button powerButton;
    public Button openParenButton;
    public Button closeParenButton;
    public Button equalButton;
    public Button clearButton;
    public Button undoButton;

    private Color normalColor = Color.white;
    private Color highlightColor = new Color(0.3f, 0.7f, 1f);

    private string lastSymbol = "";
    private float debounceTime = 1.0f;
    private float lastActionTime = 0f;

    void Update() {
        if (udpReceiver == null) {
            Debug.LogWarning("⚠ ButtonHandling: Missing UDPReceiver reference!");
            return;
        }

        string symbolText = udpReceiver.symbolData;
        if (string.IsNullOrEmpty(symbolText))
            return;

        try {
            if (symbolText == lastSymbol && Time.time - lastActionTime < debounceTime)
                return;

            lastSymbol = symbolText;
            lastActionTime = Time.time;

            ClickButtonBySymbol(symbolText);
        }
        catch (Exception e) {
            Debug.LogWarning($"⚠ ButtonHandling JSON parse error: {e.Message}");
        }
    }

    private void ClickButtonBySymbol(string symbol) {
        Button targetButton = null;

        // --- Handle numbers (0–9)
        if (int.TryParse(symbol, out int num)) {
            if (num >= 0 && num <= 9 && num < numberButtons.Length)
                targetButton = numberButtons[num];
        } else {
            // --- Handle operators and functions
            switch (symbol) {
                case "+": targetButton = plusButton; break;
                case "-": targetButton = minusButton; break;
                case "*": targetButton = multiplyButton; break;
                case "/": targetButton = divideButton; break;
                case "^": targetButton = powerButton; break;
                case "(": targetButton = openParenButton; break;
                case ")": targetButton = closeParenButton; break;
                case "=": targetButton = equalButton; break;
                case "C":
                case "c": targetButton = clearButton; break;
                case "U":
                case "u":
                case "undo": targetButton = undoButton; break;
                default:
                    Debug.LogWarning($"Unknown symbol: {symbol}");
                    break;
            }
        }

        if (targetButton != null) {
            targetButton.onClick.Invoke();
            Debug.Log($"Clicked button: {symbol}");
            StartCoroutine(FlashButton(targetButton));
        } else {
            Debug.LogWarning($"⚠ No button assigned for symbol: {symbol}");
        }
    }

    private IEnumerator FlashButton(Button button) {
        Image img = button.GetComponent<Image>();
        if (img != null) {
            Color original = img.color;

            // Flash immediately
            img.color = highlightColor;

            // Wait briefly
            yield return new WaitForSeconds(0.2f); // faster flash feels better

            // Restore original color
            img.color = original;

            // Force Button's transition system to reapply normal state
            var colors = button.colors;
            button.targetGraphic.color = colors.normalColor;
        }
    }
}
