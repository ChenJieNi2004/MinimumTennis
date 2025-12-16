using UnityEngine;

public class Move : MonoBehaviour, ICalculateSpeed, ICharacterMover
{
    [System.NonSerialized]
    public Players player;

    private readonly float baseMaximumSpeed = 30.0f;

    // Editable Parameters
    private float maximumSpeed = 30.0f;
    private float acceleration = 1.0f;

    private float speedX = 0.0f;
    private float speedZ = 0.0f;

    [System.NonSerialized]
    public Area movableArea;

    private Vector3 defaultRotation;

    private Animator animator;

    // Start is called before the first frame update
    void Start()
    {
        maximumSpeed = Parameters.maximumSpeed[(int)player];
        acceleration = Parameters.acceleration[(int)player];

        defaultRotation = transform.eulerAngles;

        animator = gameObject.GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        maximumSpeed = Parameters.maximumSpeed[(int)player];
        acceleration = Parameters.acceleration[(int)player];
    }

    void FixedUpdate()
    {
        // �ړ����x�ƈړ������ɉ����āA�A�j���[�V�����̃p�����[�^��ύX
        animator.SetFloat("SpeedX", speedX / maximumSpeed);
        animator.SetFloat("SpeedY", speedZ / maximumSpeed);
        animator.SetFloat("AnimationSpeed", maximumSpeed / baseMaximumSpeed);
    }

    // �L�����N�^�̑���IK�œK���p�ɗp����֐�
    public float CalculateSpeed()
    {
        float speedRateX = Mathf.Abs(speedX / maximumSpeed);
        float speedRateZ;

        if (speedZ < 0) { speedRateZ = 2.0f * Mathf.Abs(speedZ / maximumSpeed); }
        else { speedRateZ = Mathf.Abs(speedZ / maximumSpeed); }

        return Mathf.Max(speedRateX, speedRateZ);
    }

    public void ResetCharacterPosition(Vector3 position)
    {
        // Reset position
        Vector3 currentPosition = new Vector3(transform.position.x, 0.0f, transform.position.z);
        transform.Translate(position - currentPosition, Space.World);

        // Reset rotation
        Vector3 currentRotation = transform.eulerAngles;
        transform.Rotate(defaultRotation - currentRotation);
    }

    public void MovePlayer(float x, float z)
    {
        // �������ւ̈ړ�
        if (x < -0.10f)
        {
            if (speedX - acceleration > maximumSpeed * x) { speedX -= acceleration; }
            else { speedX = maximumSpeed * x; }
        }
        // �E�����ւ̈ړ�
        else if (x > 0.10f)
        {
            if (speedX + acceleration < maximumSpeed * x) { speedX += acceleration; }
            else { speedX = maximumSpeed * x; }
        }
        // �������͂���Ă��Ȃ��ꍇ�͌���
        else { speedX *= 0.90f; }

        // �O�����ւ̈ړ�
        if (z > 0.10f)
        {
            if (speedZ + acceleration < maximumSpeed * z) { speedZ += acceleration; }
            else { speedZ = maximumSpeed * z; }
        }
        // ������ւ̈ړ�
        else if (z < -0.10f)
        {
            if (speedZ - acceleration > maximumSpeed * z) { speedZ -= acceleration; }
            else { speedZ = maximumSpeed * z; }
        }
        // �������͂���Ă��Ȃ��ꍇ�͌���
        else { speedZ *= 0.90f; }

        // �ړ�����
        transform.Translate(speedX * Time.deltaTime, 0.0f, speedZ * Time.deltaTime);

        // �ړ��\�͈͂𒴂��Ȃ��悤�ɂ���
        Vector3 currentPosition = transform.position;
        currentPosition.x = Mathf.Clamp(currentPosition.x, movableArea.xNegativeLimit, movableArea.xPositiveLimit);
        currentPosition.z = Mathf.Clamp(currentPosition.z, movableArea.zNegativeLimit, movableArea.zPositiveLimit);
        transform.position = currentPosition;
    }

    public void StopPlayer()
    {
        speedX *= 0.90f;
        speedZ *= 0.90f;
    }
}
