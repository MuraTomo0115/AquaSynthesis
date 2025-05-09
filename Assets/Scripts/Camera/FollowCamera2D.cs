using UnityEngine;

public class FollowCamera2D : MonoBehaviour
{
    //�Ǐ]�Ώۂ�Transform�i�v���C���[�j
    private Transform target;

    //�J�����̒Ǐ]�̊��炩���i�l��傫������Ƒ����ǂ����j
    [SerializeField] private float smoothSpeed = 5f;

    //�J�����̈ʒu�␳�iZ��2D�ł�-10�Œ肪��{�j
    [SerializeField] private Vector3 offset = new Vector3(0, 0, -10f);

    //�ŏ��Ƀv���C���[���^�O����T���Ď擾
    private void Start()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            target = player.transform;
        }
        else
        {
            Debug.LogWarning("Player�^�O�̃I�u�W�F�N�g��������܂���I");
        }
    }

    //���t���[���̌�ɃJ�������X�V�iLateUpdate�̕����J�����Ǐ]�ɓK���Ă�j
    private void LateUpdate()
    {
        if (target == null) return;

        //�v���C���[�̈ʒu�ɃI�t�Z�b�g���������ڕW�ʒu
        Vector3 desiredPosition = new Vector3(target.position.x, target.position.y, offset.z);

        //���݂̃J�����ʒu����ڕW�ʒu�֊��炩�ɕ�Ԃ��Ĉړ�
        transform.position = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
    }
}

