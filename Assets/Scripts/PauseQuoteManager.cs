using TMPro;
using UnityEngine;

public class PauseQuoteManager : MonoBehaviour
{
    public TextMeshProUGUI quoteText;

    [TextArea]
    public string[] quotes = {
        "Even legends need a break.",
        "Paused… but not stopped.",
        "Rest is part of the grind.",
        "Breathe. You got this.",
        "Recharge mode: ON.",
        "The game waits… life doesn’t. Smile!",
        "Even heroes take coffee breaks."
    };

    public void ShowRandomQuote()
    {
        int index = Random.Range(0, quotes.Length);
        quoteText.text = quotes[index];
    }
}
