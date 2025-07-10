using UnityEngine;

public class WaterWallController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer _renderer;      // �ǂ̌����ڗpSpriteRenderer
    [SerializeField] private float _normalAlpha = 0.8f;     // �ʏ펞�̕s�����x
    [SerializeField] private float _transparentAlpha = 0.2f;// �L�^���̍ŏ������x
    [SerializeField] private float _flickerSpeed = 1.5f;    // �_�Łi�����x�ω��j�̑���

    private bool _isFlicker = false;    // �L�^���̓_�ŉ��o�t���O
    private bool _isDisabled = false;   // �ǂ����������ꂽ���ǂ���
    private Collider2D _collider;       // �ǂ̃R���W����

    private void Awake()
    {
        _collider = GetComponent<Collider2D>();
    }

    /// <summary>
    /// Inspector�ŁuReset�v��������SpriteRenderer�������擾
    /// </summary>
    private void Reset()
    {
        _renderer = GetComponent<SpriteRenderer>();
    }

    /// <summary>
    /// �L�^���Ȃǂŕǂ�_�Łi�����x�ω��j�����邩�؂�ւ�
    /// </summary>
    /// <param name="isTransparent">true�œ_�ŊJ�n�Afalse�Œʏ�\��</param>
    public void SetTransparent(bool isTransparent)
    {
        _isFlicker = isTransparent;
        if (!isTransparent && _renderer != null)
        {
            // �ʏ펞�͕s�����x��߂�
            var color = _renderer.color;
            color.a = _normalAlpha;
            _renderer.color = color;
        }
    }

    /// <summary>
    /// �L�^���͓����x�������I�ɕω������ē_�ŉ��o
    /// </summary>
    private void Update()
    {
        // ����������Ă��Ȃ����_�Œ��̂ݓ����x��ω�
        if (_isFlicker && _renderer != null && !_isDisabled)
        {
            // sin�g��0.2�`0.8�̊Ԃ�����
            float t = (Mathf.Sin(Time.time * _flickerSpeed) + 1f) / 2f;
            float alpha = Mathf.Lerp(_transparentAlpha, _normalAlpha, t);
            var color = _renderer.color;
            color.a = alpha;
            _renderer.color = color;
        }
    }

    /// <summary>
    /// �m�Y��������Ă΂��ƕǂ𖳌����i�������R���W���������j
    /// </summary>
    public void DisableWall()
    {
        _isDisabled = true;
        if (_renderer != null)
        {
            // ���S�ɓ�����
            var color = _renderer.color;
            color.a = 0f;
            _renderer.color = color;
        }
        if (_collider != null)
        {
            // �R���W������������
            _collider.enabled = false;
        }
        // �K�v�Ȃ�Q�[���I�u�W�F�N�g���̂��A�N�e�B�u�ɂ��Ă�OK
        // gameObject.SetActive(false);
    }
}
