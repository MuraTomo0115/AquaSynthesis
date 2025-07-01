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
            // プレイヤーのCharacterコンポーネントを自動取得
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

            // 色変化（緑→黄→赤）
            if (rate > 0.5f)
            {
                float t = (rate - 0.5f) / 0.5f; // 0.5～1.0→0～1
                _fillImage.color = Color.Lerp(Color.yellow, Color.green, t);
            }
            else
            {
                float t = rate / 0.5f; // 0～0.5→0～1
                _fillImage.color = Color.Lerp(Color.red, Color.yellow, t);
            }
        }
    }
}
