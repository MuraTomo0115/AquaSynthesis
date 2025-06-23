using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExpPopup : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI expText;

    public void SetExp(int exp)
    {
        expText.text = $"+{exp}";
    }
}