using System.Collections;
using UnityEngine;

public class Shot : MonoBehaviour
{
    [System.NonSerialized]
    public Players player;

    // Editable Parameter
    private float ballSpeed = 1.0f;

    [SerializeField]
    private GameObject normalBallPrefab;
    [SerializeField]
    private GameObject taskBallPrefab;

    [System.NonSerialized]
    public Area opponentCourtArea;

    // �����ړ����ɁA�ł��������߂�ϐ�
    private float autoMoveLateralDirection;

    private AudioSource audioSource;
    public AudioClip[] hitSounds = new AudioClip[3];
    public AudioClip serveSound;

    private Animator animator;

    [Header("CV Shot")]
    public float cvMultiplierX = 1.0f;
    public float cvMultiplierY = 1.0f;
    public float cvMultiplierZ = 1.0f;
    public float cvMinUpwardSpeed = 8.0f;  
    public AudioClip cvHitSound;

    public float slowFactor = 1.0f;

    // Start is called before the first frame update
    void Start()
    {
        ballSpeed = Parameters.ballSpeed[(int)player] * slowFactor;

        audioSource = GetComponent<AudioSource>();
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        ballSpeed = Parameters.ballSpeed[(int)player] * slowFactor;
    }

    public void Toss()
    {
        float powerY = 28.0f;

        animator.SetTrigger("Toss");

        GameObject ballObject = Instantiate(normalBallPrefab);

        float posX = transform.position.x;
        float posZ = transform.position.z;

        // �v���C���̈ʒu�ɑ΂��āA�u���ʁv�́u����6.0f�v�́u�O��5.0f�v�̈ʒu�Ƀ{�[���𐶐�����
        ballObject.transform.position = new Vector3(posX, 6.0f, posZ + 5.0f * transform.forward.z);

        IBallController ball = ballObject.GetComponent<IBallController>();

        ball.SpeedY = powerY;

        ball.BallSpeed = Parameters.ballSpeed[(int)player];
    }

    public void Serve(GameObject ballObject, ServePosition servePosition, float x)
    {
        IBallController ball = ballObject.GetComponent<IBallController>();

        float powerX = 25.0f;
        float powerY = -6.0f;
        float powerZ = 100.0f;

        animator.SetTrigger("Serve");
        audioSource.PlayOneShot(serveSound);

        ball.Time = 0.0f;
        ball.SpeedX = powerX * -(int)servePosition * transform.forward.z + 6.0f * x;
        ball.SpeedY = powerY;
        ball.SpeedZ = powerZ * transform.forward.z;

        ball.BallSpeed = ballSpeed * slowFactor;
    }

    public void NormalShot(GameObject ballObject, DominantHand dominantHand)
    {
        IBallController ball = ballObject.GetComponent<IBallController>();

        float powerX = 2.0f;
        float powerY = 25.0f;
        float powerZ = 65.0f;

        // �{�[���̍��E�̃X�s�[�h�����߂邽�߂̕ϐ�
        float lateralDirection;

        // �����ړ����ɑł��������߂�ϐ���0.0f�i�l�������Ă��Ȃ��j�ꍇ
        if (autoMoveLateralDirection == 0.0f)
        {
            // �v���C���ƃ{�[���̑��ΓI�Ȉʒu�֌W�ɉ����āA�{�[���̉������̌��������߂�
            Vector3 objectPositionInPlayerLocal = transform.InverseTransformPoint(ballObject.transform.position);
            lateralDirection = objectPositionInPlayerLocal.x > 0 ? -1.0f : 1.0f;
        }
        else { lateralDirection = autoMoveLateralDirection; }

        if (lateralDirection == (int)dominantHand) { animator.SetTrigger("Forehand"); }
        else if (lateralDirection == -(int)dominantHand) { animator.SetTrigger("Backhand"); }

        int num = Random.Range(0, 3);
        audioSource.PlayOneShot(hitSounds[num]);

        float lateralSpeedMagnification = lateralDirection * (ballObject.transform.position.z - transform.position.z);

        ball.Time = 0.0f;
        ball.SpeedX = powerX * lateralSpeedMagnification;
        ball.SpeedY = powerY;
        ball.SpeedZ = powerZ * transform.forward.z;

        AdjustBallSpeed(ballObject);

        ball.BallSpeed = ballSpeed * slowFactor;
    }

    public void LobShot(GameObject ballObject, DominantHand dominantHand)
    {
        IBallController ball = ballObject.GetComponent<IBallController>();

        float powerX = 1.25f;
        float powerY = 45.0f;
        float powerZ = 45.0f;

        // �{�[���̍��E�̃X�s�[�h�����߂邽�߂̕ϐ�
        float lateralDirection;

        // �����ړ����ɑł��������߂�ϐ���0.0f�i�l�������Ă��Ȃ��j�ꍇ
        if (autoMoveLateralDirection == 0.0f)
        {
            // �v���C���ƃ{�[���̑��ΓI�Ȉʒu�֌W�ɉ����āA�{�[���̉������̌��������߂�
            Vector3 objectPositionInPlayerLocal = transform.InverseTransformPoint(ballObject.transform.position);
            lateralDirection = objectPositionInPlayerLocal.x > 0 ? -1.0f : 1.0f;
        }
        else { lateralDirection = autoMoveLateralDirection; }

        if (lateralDirection == (int)dominantHand) { animator.SetTrigger("Forehand"); }
        else if (lateralDirection == -(int)dominantHand) { animator.SetTrigger("Backhand"); }

        int num = Random.Range(0, 3);
        audioSource.PlayOneShot(hitSounds[num]);

        float lateralSpeedMagnification = lateralDirection * (ballObject.transform.position.z - transform.position.z);

        ball.Time = 0.0f;
        ball.SpeedX = powerX * lateralSpeedMagnification;
        ball.SpeedY = powerY;
        ball.SpeedZ = powerZ * transform.forward.z;

        AdjustBallSpeed(ballObject);

        ball.BallSpeed = ballSpeed * slowFactor;
    }

    public void FastShot(GameObject ballObject, DominantHand dominantHand)
    {
        IBallController ball = ballObject.GetComponent<IBallController>();

        float powerX = 2.5f;
        float powerY = 14.0f;
        float powerZ = 100.0f;

        // �{�[���̍��E�̃X�s�[�h�����߂邽�߂̕ϐ�
        float lateralDirection;

        // �����ړ����ɑł��������߂�ϐ���0.0f�i�l�������Ă��Ȃ��j�ꍇ
        if (autoMoveLateralDirection == 0.0f)
        {
            // �v���C���ƃ{�[���̑��ΓI�Ȉʒu�֌W�ɉ����āA�{�[���̉������̌��������߂�
            Vector3 objectPositionInPlayerLocal = transform.InverseTransformPoint(ballObject.transform.position);
            lateralDirection = objectPositionInPlayerLocal.x > 0 ? -1.0f : 1.0f;
        }
        else { lateralDirection = autoMoveLateralDirection; }

        if (lateralDirection == (int)dominantHand) { animator.SetTrigger("Forehand"); }
        else if (lateralDirection == -(int)dominantHand) { animator.SetTrigger("Backhand"); }

        int num = Random.Range(0, 3);
        audioSource.PlayOneShot(hitSounds[num]);

        float lateralSpeedMagnification = lateralDirection * (ballObject.transform.position.z - transform.position.z);

        ball.Time = 0.0f;
        ball.SpeedX = powerX * lateralSpeedMagnification;
        ball.SpeedY = powerY;
        ball.SpeedZ = powerZ * transform.forward.z;

        AdjustUpwardSpeed(ballObject);
        AdjustBallSpeed(ballObject);

        ball.BallSpeed = ballSpeed;
    }

    public void DropShot(GameObject ballObject, DominantHand dominantHand)
    {
        IBallController ball = ballObject.GetComponent<IBallController>();

        float powerX = 2.0f;
        float powerY = 25.0f;
        float powerZ = 48.0f;

        // �{�[���̍��E�̃X�s�[�h�����߂邽�߂̕ϐ�
        float lateralDirection;

        // �����ړ����ɑł��������߂�ϐ���0.0f�i�l�������Ă��Ȃ��j�ꍇ
        if (autoMoveLateralDirection == 0.0f)
        {
            // �v���C���ƃ{�[���̑��ΓI�Ȉʒu�֌W�ɉ����āA�{�[���̉������̌��������߂�
            Vector3 objectPositionInPlayerLocal = transform.InverseTransformPoint(ballObject.transform.position);
            lateralDirection = objectPositionInPlayerLocal.x > 0 ? -1.0f : 1.0f;
        }
        else { lateralDirection = autoMoveLateralDirection; }

        if (lateralDirection == (int)dominantHand) { animator.SetTrigger("Forehand"); }
        else if (lateralDirection == -(int)dominantHand) { animator.SetTrigger("Backhand"); }

        int num = Random.Range(0, 3);
        audioSource.PlayOneShot(hitSounds[num]);

        float lateralSpeedMagnification = lateralDirection * (ballObject.transform.position.z - transform.position.z);

        ball.Time = 0.0f;
        ball.SpeedX = powerX * lateralSpeedMagnification;
        ball.SpeedY = powerY;
        ball.SpeedZ = powerZ * transform.forward.z;

        AdjustBallSpeed(ballObject);

        ball.BallSpeed = ballSpeed;
    }

    public void Smash(GameObject ballObject)
    {
        IBallController ball = ballObject.GetComponent<IBallController>();

        float powerX = 4.0f;
        float powerY = -20.0f;
        float powerZ = 120.0f;

        animator.SetTrigger("Smash");

        int num = Random.Range(0, 3);
        audioSource.PlayOneShot(hitSounds[num]);

        // �{�[���̍��E�̃X�s�[�h�����߂邽�߂̕ϐ�
        float lateralDirection;

        // �����ړ����ɑł��������߂�ϐ���0.0f�i�l�������Ă��Ȃ��j�ꍇ
        if (autoMoveLateralDirection == 0.0f)
        {
            // �v���C���ƃ{�[���̑��ΓI�Ȉʒu�֌W�ɉ����āA�{�[���̉������̌��������߂�
            Vector3 objectPositionInPlayerLocal = transform.InverseTransformPoint(ballObject.transform.position);
            lateralDirection = objectPositionInPlayerLocal.x > 0 ? -1.0f : 1.0f;
        }
        else { lateralDirection = autoMoveLateralDirection; }

        float lateralSpeedMagnification = lateralDirection * (ballObject.transform.position.z - transform.position.z);

        ball.Time = 0.0f;
        ball.SpeedX = powerX * lateralSpeedMagnification;
        ball.SpeedY = powerY;
        ball.SpeedZ = powerZ * transform.forward.z;

        AdjustUpwardSpeed(ballObject);
        AdjustBallSpeed(ballObject);

        ball.BallSpeed = ballSpeed;
    }

    public void Volley(GameObject ballObject, DominantHand dominantHand)
    {
        IBallController ball = ballObject.GetComponent<IBallController>();

        float powerX = 2.0f;
        float powerY = 5.0f;
        float powerZ = 55.0f;

        // �{�[���̍��E�̃X�s�[�h�����߂邽�߂̕ϐ�
        float lateralDirection;

        // �����ړ����ɑł��������߂�ϐ���0.0f�i�l�������Ă��Ȃ��j�ꍇ
        if (autoMoveLateralDirection == 0.0f)
        {
            // �v���C���ƃ{�[���̑��ΓI�Ȉʒu�֌W�ɉ����āA�{�[���̉������̌��������߂�
            Vector3 objectPositionInPlayerLocal = transform.InverseTransformPoint(ballObject.transform.position);
            lateralDirection = objectPositionInPlayerLocal.x > 0 ? -1.0f : 1.0f;
        }
        else { lateralDirection = autoMoveLateralDirection; }

        if (lateralDirection == (int)dominantHand) { animator.SetTrigger("ForeVolley"); }
        else if (lateralDirection == -(int)dominantHand) { animator.SetTrigger("BackVolley"); }

        int num = Random.Range(0, 3);
        audioSource.PlayOneShot(hitSounds[num]);

        float lateralSpeedMagnification = lateralDirection * (ballObject.transform.position.z - transform.position.z);

        ball.Time = 0.0f;
        ball.SpeedX = powerX * lateralSpeedMagnification;
        ball.SpeedY = powerY;
        ball.SpeedZ = powerZ * transform.forward.z;

        AdjustUpwardSpeed(ballObject);
        AdjustBallSpeed(ballObject);

        ball.BallSpeed = ballSpeed;
    }

    // public void AIShot(GameObject ballObject, Vector3 power, DominantHand dominantHand)
    // {
    //     IBallController ball = ballObject.GetComponent<IBallController>();

    //     float powerX = power.x;
    //     float powerY = power.y;
    //     float powerZ = power.z;

    //     Vector3 objectPositionInPlayerLocal = transform.InverseTransformPoint(ballObject.transform.position);
    //     float lateralDirection = objectPositionInPlayerLocal.x > 0 ? -1.0f : 1.0f;

    //     if (lateralDirection == (int)dominantHand) { animator.SetTrigger("Forehand"); }
    //     else if (lateralDirection == -(int)dominantHand) { animator.SetTrigger("Backhand"); }

    //     int num = Random.Range(0, 3);
    //     audioSource.PlayOneShot(hitSounds[num]);

    //     ball.Time = 0.0f;
    //     ball.SpeedX = powerX;
    //     ball.SpeedY = powerY;
    //     ball.SpeedZ = powerZ;

    //     ball.BallSpeed = ballSpeed;
    // }

  public void AIShot(GameObject ballObject, Vector3 power, DominantHand dominantHand)
  {
      IBallController ball = ballObject.GetComponent<IBallController>();
      Vector3 ballPos = ballObject.transform.position;

      GameObject targetPlayer = (player == Players.p1)
          ? GameData.character2
          : GameData.character1;

      Vector3 targetPos = targetPlayer.transform.position;

      float aiLandingOffset = 3.0f;  

      float landingZ = targetPos.z + (player == Players.p1 ? aiLandingOffset : -aiLandingOffset);

      float landingX = Random.Range(opponentCourtArea.xNegativeLimit,
                                    opponentCourtArea.xPositiveLimit);

      float T = 1.25f;  

      float vx = (landingX - ballPos.x) / T;
      float vz = (landingZ - ballPos.z) / T;

      float g = ball.Gravity;
      float landingY = 0.6f;  

      float vy = (landingY - ballPos.y + 0.5f * g * T * T) / T;

      ball.SpeedX = vx;
      ball.SpeedY = vy;
      ball.SpeedZ = vz;
      ball.BallSpeed = ballSpeed * slowFactor; 
      GameData.lastShooter = gameObject.name;
      GameData.foul = FoulState.NoFoul;
      GameData.ballBoundCount = 0;
  }


    // Task�ESetting���[�h��p����
    public void ServeBall()
    {
        Vector3 position = transform.position;

        GameObject ballObject = Instantiate(taskBallPrefab);
        ballObject.transform.position = new Vector3(position.x - 1.0f, position.y + 5.0f, position.z - 3.0f);

        IBallController ball = ballObject.GetComponent<IBallController>();

        animator.SetTrigger("Forehand");

        int num = Random.Range(0, 3);
        audioSource.PlayOneShot(hitSounds[num]);

        ball.Time = 0.0f;

        Vector3 ballSpeed = DecideBallSpeed(ballObject);
        ball.SpeedX = ballSpeed.x;
        ball.SpeedY = ballSpeed.y;
        ball.SpeedZ = -ballSpeed.z;

        ball.BallSpeed = Parameters.ballSpeed[(int)player];
    }

    private void AdjustUpwardSpeed(GameObject ballObject)
    {
        IBallController ball = ballObject.GetComponent<IBallController>();

        Vector3 ballPosition = ballObject.transform.position;
        float netDistance = Mathf.Abs(ballPosition.z);

        float gravity = ball.Gravity;

        float netArrivalTime = netDistance / Mathf.Abs(ball.SpeedZ);
        float netArrivalHight = ball.SpeedY * netArrivalTime - 0.50f * gravity * netArrivalTime * netArrivalTime + ballPosition.y;

        // (GameData���Œ�`����Ă���netHight���g�p���Ă��邪�ATaskData���Œ�`����Ă�����̂��g���Ă��悢)
        if (GameData.netHight / 2.0f < netArrivalHight && netArrivalHight < GameData.netHight)
        {
            float targetHight = 7.0f;
            ball.SpeedZ *= 0.60f;

            float adjustedNetArrivalTime = netDistance / Mathf.Abs(ball.SpeedZ);
            float adjustedSpeedY = ((targetHight - ballPosition.y) + 0.50f * gravity * adjustedNetArrivalTime * adjustedNetArrivalTime) / adjustedNetArrivalTime;

            ball.SpeedY = adjustedSpeedY;
        }
    }

    private void AdjustBallSpeed(GameObject ballObject)
    {
        IBallController ball = ballObject.GetComponent<IBallController>();

        Vector3 ballPosition = ballObject.transform.position;

        float speedY = ball.SpeedY;
        float gravity = ball.Gravity;
        float arrivalTime = (speedY + Mathf.Sqrt(speedY * speedY + 2.0f * gravity * ballPosition.y)) / gravity;

        float arrivalPointX = ball.SpeedX * arrivalTime + ballPosition.x;
        float arrivalPointZ = ball.SpeedZ * arrivalTime + ballPosition.z;

        Area area = opponentCourtArea;
        float margin = 3.0f;

        if (area.xPositiveLimit < arrivalPointX && arrivalPointX < area.xPositiveLimit + 10.0f)
        {
            // X���̐������ւ̃A�E�g��␳����
            ball.SpeedX = ((area.xPositiveLimit - margin) - ballPosition.x) / arrivalTime;
        }
        else if (area.xNegativeLimit - 10.0f < arrivalPointX && arrivalPointX < area.xNegativeLimit)
        {
            // X���̕������ւ̃A�E�g��␳����
            ball.SpeedX = ((area.xNegativeLimit + margin) - ballPosition.x) / arrivalTime;
        }

        if (area.zPositiveLimit < arrivalPointZ && arrivalPointZ < area.zPositiveLimit + 10.0f)
        {
            // Z���̐������ւ̃A�E�g��␳����
            ball.SpeedZ = ((area.zPositiveLimit - margin) - ballPosition.z) / arrivalTime;
        }
        else if (area.zNegativeLimit - 10.0f < arrivalPointZ && arrivalPointZ < area.zNegativeLimit)
        {
            // Z���̕������ւ̃A�E�g��␳����
            ball.SpeedZ = ((area.zNegativeLimit + margin) - ballPosition.z) / arrivalTime;
        }
    }
    private void ApplySlowMotion(IBallController ball, float slowFactor)
    {
        ball.SpeedX *= slowFactor;
        ball.SpeedY *= slowFactor;
        ball.SpeedZ *= slowFactor;

        ball.BallSpeed /= slowFactor;
    }

    // Task�ESetting���[�h��p����
    private Vector3 DecideBallSpeed(GameObject ballObject)
    {
        TaskBallController ball = ballObject.GetComponent<TaskBallController>();
        ITaskManager taskManagerInterface = TaskData.taskManagerInterface;

        Vector3 arrivalPosition = taskManagerInterface.DecideArrivalPosition(ball);
        Vector3 decidedBallSpeed;

        decidedBallSpeed.y = 25.0f;
        decidedBallSpeed.x = taskManagerInterface.DecideLateralSpeed(ballObject, ball, decidedBallSpeed.y, arrivalPosition);
        decidedBallSpeed.z = taskManagerInterface.DecideDepthSpeed(ballObject, ball, decidedBallSpeed.y, arrivalPosition);

        return decidedBallSpeed;
    }

    public void Takeback(bool fore, bool back, float lateralDirection)
    {
        autoMoveLateralDirection = lateralDirection;
        animator.SetBool("TakebackFore", fore);
        animator.SetBool("TakebackBack", back);
    }

    public void ExitServeAnimation()
    {
        if (GameData.server != name) { return; }
        StartCoroutine(LayerWeightCoroutine());
    }

    private IEnumerator LayerWeightCoroutine()
    {
        float weight = 0.0f;
        float weightDelta = 0.010f;

        while (animator.GetLayerWeight(1) < 1.0f)
        {
            weight += weightDelta;
            animator.SetLayerWeight(1, weight);

            yield return new WaitForEndOfFrame();
        }
    }
}
