using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Controller2D))]
public class Player : MonoBehaviour
{
	[Header("HpOptins")]
	[TooltipAttribute("キャラクターの体力")]
	public int maxHp = 100;

	[TooltipAttribute("キャラクターが耐えられる温度")]
	public int maxTemperature = 1000;

	[TooltipAttribute("ダメージ後何秒で回復を始めるか")]
	public float recoveryTime = 5;

	[TooltipAttribute("何秒に一度回復するか")]
	public float recoverInterval = 0.1f;

	[TooltipAttribute("回復するHP")]
	public int recoveryHp = 1;

	[TooltipAttribute("HPゲージの取得")]
	public TemperatureGage temperatureGage;

	[Header("SpeedOptions")]

	[TooltipAttribute("キャラクターの速度")]
	public float moveSpeed = 6;

	[TooltipAttribute("空中にいる時の加速時間")]
	public float accelerationTimeAirborne = .2f;

	[TooltipAttribute("着地しているときの加速時間")]
	public float accelerationTimeGrounded = .1f;

	[TooltipAttribute("インプットを停止させておくか")]
	public bool defaultInputStop = false;

	[Header("JumpOptions")]
	[TooltipAttribute("ジャンプの高さ")]
	public float jumpHeight = 4;

	[TooltipAttribute("ジャンプの滞空時間")]
	public float timeToJumpApex = .4f;

	[Header("ClimbOptions")]
	[TooltipAttribute("登る速度")]
	public float climbSpeed = 2;

	[Header("AttackOptions")]
	[TooltipAttribute("下に樽があるかどうかを調べる距離")]
	public float findDistance = 1;

	[Header("Reference")]
	[TooltipAttribute("スパインスケルトンアニメーションの参照")]
	public Spine.Unity.SkeletonAnimation skeletonAnimation;

	[TooltipAttribute("アニメーターの参照")]
	public Animator animator;

	[TooltipAttribute("コントローラー2Dの参照")]
	public Controller2D controller;

	[TooltipAttribute("剣の参照")]
	public Sword sword;

	[TooltipAttribute("カメラコントローラーの参照")]
	public CameraController cameraController;

	[HideInInspector]
	public int hp;

	void Start()
	{

		//重力の初期化
		gravity = -(2 * jumpHeight) / Mathf.Pow(timeToJumpApex, 2);
		jumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;

		right = true;
		tow_jump = false;
		hp = maxHp;
		inputStop = defaultInputStop;

		shadow = transform.GetChild(3).GetComponent<SpriteRenderer>();
		defAlpha = shadow.color.a;
	}

	void Update()
	{
		if (Input.GetKeyDown(KeyCode.R))
		{
			ApplyDamage(maxHp);
		}
		if (controller.collisions.above || controller.collisions.below)
		{
			velocity.y = 0;
		}

		input = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));

		if (Input.GetKey(KeyCode.Escape))
		{
			Quit();
		}
		CompulsionMove(ref input);
		Move(input);
		Jump();
		Attack();
		FindToggleCollision();
		Recovary();
		controller.Move(velocity * Time.deltaTime);
	}

	#region Move

	Vector3 velocity;
	float velocityXSmoothing;
	[HideInInspector]
	public bool right = true;
	[HideInInspector]
	public bool inputStop = false;
	[HideInInspector]
	public Vector2 compulsionInput = Vector3.zero;

	Vector2 input;

	void Move(Vector2 input)
	{
		//移動しているかを確認
		if (0 != input.x)
		{
			right = (0 < input.x);
			animator.SetBool("isWalk", true);
		}
		else
		{
			animator.SetBool("isWalk", false);
		}

		//2Dゲームであるため左方向の移動の場合は絵を左右反転させる
		skeletonAnimation.skeleton.FlipX = !right;
		float targetVelocityX = input.x * moveSpeed;
		velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
	}

	#endregion

	#region Jump

	float gravity;
	float jumpVelocity;
	[HideInInspector]
	public bool ignoreGravity = false;
	[HideInInspector]
	public bool tow_jump;

	void Jump()
	{
		if ((Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown("joystick button 0")) && controller.IsGrounded)
		{
			tow_jump = false;
			velocity.y = jumpVelocity;
		}

		//ジャンプ時に梯子などがある場合は上る
		ClimbInput();

		if (!ignoreGravity)
		{
			velocity.y += gravity * Time.deltaTime;
		}

		//影を落とす
		if (controller.IsGrounded && !oldIsGrounded) 
		{
			StartCoroutine(Shadow(true));
		}
		if (!controller.IsGrounded && oldIsGrounded) 
		{
			StartCoroutine(Shadow(false));
		}

		oldIsGrounded = controller.IsGrounded;
	}

	public void TaruJump(float x)
	{
		velocity.y = jumpVelocity * x;
		velocity.x = jumpVelocity * x * input.x;
	}

	#endregion

	#region Climb

	[HideInInspector]
	public bool climb = false;

	void ClimbInput()
	{
		if (climb)
		{
			//上る場合はキャラクターのVeloxity.yにclimbSpeedを代入する
			float axis = Input.GetAxis("Vertical");
			if (axis > 0)
			{
				velocity.y = climbSpeed;
			}
			if (axis < 0)
			{
				velocity.y = -climbSpeed;
			}
			if (axis == 0)
			{
				velocity.y = 0;
			}
		}
	}

	public void Climb()
	{
		ignoreGravity = true;
		velocity.y = 0;
		climb = true;
	}

	public void ResetClimb()
	{
		ignoreGravity = false;
		climb = false;
	}

	#endregion

	#region Attack

	private bool attack = false;
	void Attack()
	{
		//攻撃
		if (Input.GetKeyDown(KeyCode.Space) || Input.GetAxis("RightTrigger") == 1 && !attack)
		{
			attack = true;
			SwitchAttack();
		}

		//攻撃キャンセル
		if (Input.GetKeyUp(KeyCode.Space) || Input.GetAxis("RightTrigger") != 1 && attack)
		{
			attack = false;
			sword.AttackFinish();
			animator.SetBool("isUnderAttack", false);
			animator.SetBool("isAttack", false);
		}
	}

	void SwitchAttack()
	{
		//樽がある場合は下方向の攻撃をする
		if (FindBarrels())
		{
			animator.SetBool("isUnderAttack", true);
			sword.Attack(true, controller.collisionMask);
		}
		else
		{
			//通常の左右攻撃
			animator.SetBool("isAttack", true);
			sword.Attack(false, controller.collisionMask);
		}
	}

	bool FindBarrels()
	{
		RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, findDistance, controller.collisionMask);

		if (!hit) return false;

		Barrels barrels = hit.collider.GetComponent<Barrels>();

		if (!barrels) false;

		return true;
	}

	#endregion

	#region FindToggleCollision

	void FindToggleCollision()
	{
		if (inputStop) return;

		//カメラトリガーはToggleのため、Rayで探す
		Vector2 size = controller.collider.bounds.size;
		Vector3 center = transform.position + new Vector3(0, size.y, 0);
		RaycastHit2D hit = Physics2D.BoxCast(center, size, 0, Vector2.zero, 0, controller.collisionMask);

		if (!hit) return;

		CameraTrigger cameraTrigger = hit.collider.gameObject.GetComponent<CameraTrigger>();

		if (cameraTrigger) cameraController.SetCameraTrigger(cameraTrigger);
	}

	void CompulsionMove(ref Vector2 input)
	{
		if (inputStop) input = compulsionInput;
	}

	/// <summary>
	/// コリジョンマスクをセットする
	/// </summary>
	/// <param name="layerMask">更新するレイヤーマスク</param>
	public void SetCollisionMask(LayerMask layerMask)
	{
		if (controller.collisionMask != layerMask)
		{
			controller.collisionMask = layerMask;
		}
	}


	#endregion

	#region Damage

	/// <summary>
	/// ダメージの更新
	/// </summary>
	/// <param name="damage">追加するダメージ</param>
	public void ApplyDamage(int damage)
	{
		//ダメージを受けなかった時間を0にする
		timeWithoutDamage = 0;

		hp -= damage;
		if (hp <= 0)
		{

		}
	}

	#endregion

	#region Layer

	/// <summary>
	/// レイヤーとコライダーマスクを比較する
	/// </summary>
	/// <param name="layer">レイヤー（int）</param>
	public bool CompareLayerAndColliderMask(int layer)
	{
		string str = LayerMask.LayerToName(layer);
		LayerMask layerMask = LayerMask.GetMask(str);
		return controller.collisionMask.value == layerMask.value;
	}

	#endregion

	#region Recovery

	private float timeWithoutDamage = 0;
	private float recovaryCurrentTime = 0;

	private void Recovary()
	{
		timeWithoutDamage += Time.deltaTime;

		//ある一定時間ダメージを受けなければ自動回復される
		if (timeWithoutDamage > recoveryTime)
		{
			temperatureGage.glowing = true;
			recovaryCurrentTime += Time.deltaTime;

			//一定時間回復
			if (recovaryCurrentTime > recoverInterval)
			{
				recovaryCurrentTime = 0;
				hp += recoveryHp;
				hp = Mathf.Clamp(hp, 0, maxHp);
			}
		}
		else
		{
			temperatureGage.glowing = false;
		}
	}

	#endregion

	#region Shadow

	private SpriteRenderer shadow;
	private float shadowSpeed = 5;
	private float defAlpha;
	private bool oldIsGrounded;

	private IEnumerator Shadow(bool fadeIn)
	{
		float alpha = defAlpha;
		while (alpha > 0)
		{
			alpha -= shadowSpeed * Time.deltaTime;
			Color color = shadow.color;
			color.a = fadeIn ? defAlpha - alpha : alpha;
			shadow.color = color;
			yield return null;
		}
		yield break;
	}

	#endregion

	void Quit()
	{
#if UNITY_EDITOR
		UnityEditor.EditorApplication.isPlaying = false;
#elif UNITY_STANDALONE
      UnityEngine.Application.Quit();
#endif
	}

	void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Vector2 to = transform.position;
		to += Vector2.down * findDistance;
		Gizmos.DrawLine(transform.position, to);
	}
}