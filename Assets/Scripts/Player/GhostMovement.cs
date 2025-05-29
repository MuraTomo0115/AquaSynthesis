using System.Collections;
using UnityEngine;

public class GhostMovement : MonoBehaviour
{
    [Header("攻撃関連")]
    [SerializeField] private GameObject _attackSensorPrefab;  // ← プレハブに変更
    private GameObject _attackSensorInstance;

    [SerializeField] private GameObject _bullet;
    [SerializeField] private Transform _firePoint;

    private Vector2 _recordedPosition;
    private Vector2 _recordedInput;
    private bool _recordedJump;
    private bool _recordedAttack;
    private bool _recordedPistol;
    private bool _recordedSummonA;
    private bool _recordedSummonB;

    private Animator _animator;
    private SpriteRenderer _spriteRenderer;
    private Character _charaState;

    public void Initialize(Vector2 position, Vector2 input, bool jump, bool attack, bool pistol, bool summonA, bool summonB)
    {
        _recordedPosition = position;
        _recordedInput = input;
        _recordedJump = jump;
        _recordedAttack = attack;
        _recordedPistol = pistol;
        _recordedSummonA = summonA;
        _recordedSummonB = summonB;
    }

    private void Start()
    {
        _animator = GetComponent<Animator>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _charaState = GetComponent<Character>();

        transform.position = _recordedPosition;

        // 攻撃センサー生成（ゴースト用）
        if (_attackSensorPrefab != null)
        {
            _attackSensorInstance = Instantiate(_attackSensorPrefab, transform);
            _attackSensorInstance.SetActive(false);
        }

        // 攻撃再現
        if (_recordedAttack)
        {
            _animator.SetTrigger("AttackSword");
        }

        if (_recordedPistol)
        {
            _animator.SetTrigger("AttackPistol");
            ShootPistol();
        }

        // 向き
        if (_recordedInput.x < 0)
        {
            _spriteRenderer.flipX = true;
        }
        else if (_recordedInput.x > 0)
        {
            _spriteRenderer.flipX = false;
        }
    }

    private void FixedUpdate()
    {
        if (Mathf.Abs(_recordedInput.x) > Mathf.Epsilon)
        {
            _animator.SetInteger("AnimState", 1);
        }
        else
        {
            _animator.SetInteger("AnimState", 0);
        }
    }

    private void ShootPistol()
    {
        GameObject bullet = Instantiate(_bullet, _firePoint.position, Quaternion.identity);
        Vector2 direction = _spriteRenderer.flipX ? Vector2.left : Vector2.right;
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        bulletScript.SetDirection(direction);
    }

    public void StartAttack()
    {
        if (_attackSensorInstance != null)
        {
            _attackSensorInstance.SetActive(true);
            _attackSensorInstance.transform.localScale = new Vector3(
                _spriteRenderer.flipX ? -1 : 1, 1, 1
            );
        }
    }

    public void EndAttack()
    {
        if (_attackSensorInstance != null)
        {
            _attackSensorInstance.transform.localScale = Vector3.zero;
            _attackSensorInstance.SetActive(false);
        }
    }

    public void OwnAttackHit(Collider2D other)
    {
        Character hitObject = other.GetComponent<Character>();
        if (hitObject != null)
        {
            hitObject.HitAttack(_charaState.AttackPower);
        }
    }

    public void trueAttack()
    {
        // アニメーションイベント用のダミー
    }

    public void falseAttack()
    {
        // アニメーションイベント用のダミー
    }
}
