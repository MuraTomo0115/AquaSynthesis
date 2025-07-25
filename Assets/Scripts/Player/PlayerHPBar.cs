using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBar : MonoBehaviour
{
    [SerializeField] private Slider _hpSlider;
    [SerializeField] private Image _fillImage;
    [SerializeField] private Character _playerCharacter;

    private void Start()
    {
        if (_playerCharacter == null)
        {
            // vC[ฬCharacterR|[lg๐ฉฎๆพ
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                _playerCharacter = playerObj.GetComponent<Character>();
        }
        if (_playerCharacter != null)
            _hpSlider.maxValue = _playerCharacter.MaxHealth;
    }

    private void Update()
    {
        if (_playerCharacter != null)
        {
            _hpSlider.value = _playerCharacter.CurrentHealth;

            float rate = (float)_playerCharacter.CurrentHealth / _playerCharacter.MaxHealth;

            // Fฯปiฮจฉจิj
            if (rate > 0.5f)
            {
                float t = (rate - 0.5f) / 0.5f; // 0.5`1.0จ0`1
                _fillImage.color = Color.Lerp(Color.yellow, Color.green, t);
            }
            else
            {
                float t = rate / 0.5f; // 0`0.5จ0`1
                _fillImage.color = Color.Lerp(Color.red, Color.yellow, t);
            }
        }
    }
}
