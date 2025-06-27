using UnityEngine;
using UnityEngine.UI;

public class PlayerHPBar : MonoBehaviour
{
    [SerializeField] private Slider hpSlider;
    [SerializeField] private Character playerCharacter;

    private void Start()
    {
        if (playerCharacter == null)
        {
            // �v���C���[��Character�R���|�[�l���g�������擾
            var playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                playerCharacter = playerObj.GetComponent<Character>();
        }
        if (playerCharacter != null)
            hpSlider.maxValue = playerCharacter.MaxHealth;
    }

    private void Update()
    {
        if (playerCharacter != null)
        {
            hpSlider.value = playerCharacter.CurrentHealth;
        }
    }
}
