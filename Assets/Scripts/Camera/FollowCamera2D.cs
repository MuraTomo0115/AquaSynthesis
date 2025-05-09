using UnityEngine;

public class FollowCamera2D : MonoBehaviour
{
    //�Ǐ]�Ώۂ�Transform�i�v���C���[�j
    private Transform _target;

    //�J�����̒Ǐ]�̊��炩���i�l��傫������Ƒ����ǂ����j
    [SerializeField] private float _smoothSpeed = 5f;

    //�J�����̈ʒu�␳�iZ��2D�ł�-10�Œ肪��{�j
    [SerializeField] private Vector3 _offset = new Vector3(0, 0, -10f);

    /// <summary>
    /// �ŏ��Ƀv���C���[���^�O����T���Ď擾
    /// </summary>
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            _target = player.transform;
        }
        else
        {
            Debug.LogWarning("Player�^�O�̃I�u�W�F�N�g��������܂���I");
        }
    }

    /// <summary>
    /// ���t���[���̌�ɃJ�������X�V�iLateUpdate�̕����J�����Ǐ]�ɓK���Ă�j
    /// </summary>
    private void LateUpdate()
    {
        if (_target == null) return;

        //�v���C���[�̈ʒu�ɃI�t�Z�b�g���������ڕW�ʒu
        Vector3 desiredPosition = new Vector3(_target.position.x, _target.position.y, _offset.z);

        //���݂̃J�����ʒu����ڕW�ʒu�֊��炩�ɕ�Ԃ��Ĉړ�
        transform.position = Vector3.Lerp(transform.position, desiredPosition, _smoothSpeed * Time.deltaTime);
    }
}

