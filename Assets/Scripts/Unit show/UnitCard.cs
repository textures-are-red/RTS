using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UnitCard : MonoBehaviour
{
    [SerializeField] private Image _image;
    [SerializeField] private TextMeshProUGUI _text;
    [SerializeField] private Button _button;

    public Image Image => _image;
    public TextMeshProUGUI Text => _text;
    public Button Button => _button;
}
