using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    bool myTurn=false;
    public int turnOrder;
    [SerializeField] public bool opponent = false;
    SpriteRenderer spriteRenderer;
    [SerializeField] SpriteRenderer subRenderer;
    Animator anim;
    Rigidbody2D rb;
    LineRenderer lineRenderer;
    HealthStat healthStat;
    float runDir;
    bool grounded;

    public float damage = 50;
    public float speed = 5f;
    public float jumpStrength = 5f;
    public float maxWalkDistance = 4f;
    [SerializeField] float distanceWalked = 0f;
    [SerializeField] bool firedShot = false;
    [SerializeField] bool jumped = false;
    public float maxJumpStrength=5;
    public float maxFireForce = 10;
    public float mouseScale = 5f;

    public Vector2 jumpForce=Vector2.zero;
    public Vector2 fireForce=Vector2.zero;

    bool isJumping = false;
    bool isShooting = false;
    bool isWalking = false;
    Vector2 lastPosition;
    float walkCommand = 0f;

    public Projectile equippedProjectile;
    public Transform projectileEmitter;
    public bool automated = false;

    private void Awake()
    {
        healthStat = GetComponent<HealthStat>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
        healthStat.OnDie.AddListener(() => { gameObject.layer = 6; if(Game.turn == turnOrder)EndTurn(); });
    }


    // Start is called before the first frame update
    void Start()
    {
        subRenderer.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        ReadPlayerInput(); 
        anim.SetBool("Grounded", grounded);
        anim.SetBool("BowEquipped", isShooting);
        if (grounded) anim.SetFloat("Speed", Mathf.Abs(runDir));
    }




    void ReadPlayerInput()
    {
        if (Game.turn != turnOrder) return;

        if (isWalking)
        {
            runDir = (opponent||automated)?walkCommand:Input.GetAxis("Horizontal");

            if (opponent || automated)
            {
                //turn around if we hit a wall
                RaycastHit2D hit = Physics2D.Raycast(this.transform.position, Vector2.right * Mathf.Sign(walkCommand),.75f,LayerMask.GetMask("Platform"));
                Debug.DrawRay(this.transform.position, Vector2.right * Mathf.Sign(runDir) * .75f, Color.red,1f);
                if(hit.collider != null)
                {
                    Debug.Log(hit.collider.name);
                    if(hit.collider.gameObject.layer == 3)
                    {
                        walkCommand *= -1f;
                    }
                }
            }

            if (Mathf.Abs(runDir) > 0.01f && distanceWalked<maxWalkDistance)
            {
                spriteRenderer.flipX = runDir < 0;
                if (rb.velocity.x != runDir*speed)
                { 
                    rb.velocity = new Vector2(runDir*speed, rb.velocity.y); 
                }
            }
            distanceWalked += Mathf.Abs(((Vector2)this.gameObject.transform.position - lastPosition).x);

            if (distanceWalked >= maxWalkDistance) { isWalking=false; runDir = 0;}

            lastPosition = this.gameObject.transform.position;
            if (grounded) anim.SetFloat("Speed", Mathf.Abs(runDir));
        }
        

        
        if (isJumping)
        {
            if (!opponent)
            {
                if (Input.GetMouseButton(0))
                {
                    jumpForce = mouseScale * (GetCenteredMousePos() - this.gameObject.transform.position);
                    jumpForce = jumpForce.normalized * Mathf.Min(jumpForce.magnitude, maxJumpStrength);
                    ShowBallisticTrajectory(this.gameObject.transform.position, jumpForce, 25);
                }

                if (Input.GetMouseButtonUp(0) && jumpForce != Vector2.zero)
                {
                    isJumping = false;
                    Jump(jumpForce);
                    lineRenderer.positionCount = 0;
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    isJumping = false;
                    lineRenderer.positionCount = 0;
                    jumpForce = Vector2.zero;
                }
            }
            spriteRenderer.flipX = jumpForce.x < 0 || rb.velocity.x<0;
        }

        
        
        if (isShooting)
        {
            if (!opponent)
            {
                if (Input.GetMouseButton(0))
                {
                    fireForce = mouseScale * (GetCenteredMousePos() - this.gameObject.transform.position);
                    fireForce = fireForce.normalized * Mathf.Min(fireForce.magnitude, maxFireForce);
                    ShowBallisticTrajectory(projectileEmitter.position, fireForce, 25);
                }
                if (Input.GetMouseButtonUp(0) && fireForce != Vector2.zero)
                {
                    isShooting = false;
                    FireProjectile(fireForce);
                    lineRenderer.positionCount = 0;
                }
                else if (Input.GetMouseButtonDown(1))
                {
                    CancelShot();
                }
            }
            
            subRenderer.transform.right = fireForce;
            spriteRenderer.flipX = fireForce.x < 0;
            subRenderer.flipY = spriteRenderer.flipX;
        }
    }


    public void InitiateJump()
    {
        if (jumped || Game.turn != turnOrder) return;
        isJumping = true;
        isShooting = false;
        jumpForce = Vector2.zero;
        isWalking = false;
    }

    public void Walk()
    {
        if (distanceWalked >= maxWalkDistance || (Game.turn != turnOrder && !Game.overrideTurn)) return;
        isWalking = true;
        isShooting = false;
        fireForce= jumpForce = Vector2.zero;
        isJumping = false;
        lastPosition = this.gameObject.transform.position;
    }

    public void WalkInDirection(float x)
    {
        Walk();
        walkCommand = x;
    }

    public void Shoot()
    {
        if (firedShot || Game.turn != turnOrder) return;
        isShooting = true;
        isJumping = false;
        isWalking = false;
        jumpForce = Vector2.zero;
        subRenderer.enabled = true;
    }

    public void FireProjectile(Vector2 force)
    {
        equippedProjectile.gameObject.layer = this.gameObject.layer;
        Rigidbody2D projectileRB = GameObject.Instantiate(equippedProjectile, projectileEmitter.position, projectileEmitter.rotation).GetComponent<Rigidbody2D>();
        projectileRB.velocity = force;
        projectileRB.GetComponent<Projectile>().damage = this.damage;
        fireForce = Vector2.zero;
        subRenderer.enabled = false;
        isShooting = false;
        lineRenderer.positionCount = 0;
        firedShot = true;
    }

    void CancelShot()
    {
        subRenderer.enabled = false;
        isShooting = false;
        fireForce=Vector2.zero;
        lineRenderer.positionCount = 0;
    }



    public void Jump(Vector2 vector)
    {
        anim.SetTrigger("Jump");
        //rb.velocity += (Vector2.up * jumpStrength);
        rb.velocity= vector;
        jumpForce= Vector2.zero;
        jumped = true;
    }



    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3) grounded = true;
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.layer == 3) grounded = false;
    }


    public void ShowBallisticTrajectory(Vector2 startPoint, Vector2 velocity, int divisions)
    {
        lineRenderer.positionCount = divisions;

        float timeOfFlight = velocity.y / 4.9f * 2f;

        for(int i = 0; i<divisions; i++)
        {
            float t = timeOfFlight / divisions * i;
            Vector2 pos = startPoint + new Vector2(velocity.x * t, velocity.y * t - 4.9f * (t * t));
            lineRenderer.SetPosition(i, pos);
            if (i > 0 )
            {
                RaycastHit2D hit = Physics2D.CircleCast((Vector2)lineRenderer.GetPosition(i - 1), 0.1f, pos - (Vector2)lineRenderer.GetPosition(i - 1), (pos - (Vector2)lineRenderer.GetPosition(i - 1)).magnitude);
                if(hit.collider!=null && !hit.collider.transform.IsChildOf(this.transform))
                {
                    Debug.Log(hit.collider.name);
                    lineRenderer.positionCount = i+1; 
                    lineRenderer.SetPosition(i, hit.point);
                    break;
                }
            }
        }
    }


    Vector3 GetCenteredMousePos()
    {
        return Camera.main.ScreenToWorldPoint(Input.mousePosition);
    }


    public void TakeTurn()
    {
        if (myTurn) return;
        Debug.Log("My Turn! " + turnOrder);
        if (healthStat.health <= 0) { EndTurn(); return; }
        distanceWalked = 0;
        myTurn = true;
        jumped = false;
        firedShot = false;
        if (opponent)
        {
            FindObjectOfType<Opponent>().TakeCharacterTurn(this);
        }

    }

    public void EndTurn()
    {
        myTurn = false;
        runDir = 0;
        anim.SetFloat("Speed", 0);
        Debug.Log(Game.turn == turnOrder);
        if (Game.turn != turnOrder) return;
        Debug.Log($"{name} Ending turn");
        isShooting = isJumping = false;
        Game.EndCurrentTurn(); 
    }


    public void PlayFallAnimation()
    {
        anim.SetTrigger("DamageTaken");
        //spriteRenderer.flipX = ((Vector2)this.transform.position - source.position).x > 0;
        //Debug.Log("Damage Taken");
    }

    public void PlayDeathAnimation()
    {
        anim.SetBool("Dead", true);
        //spriteRenderer.flipX = ((Vector2)this.transform.position - source.position).x > 0;
    }

    public void FaceTarget(Transform target)
    {
        spriteRenderer.flipX = (this.transform.position - target.position).x > 0;
    }

    public bool CantWalkFurther()
    {
        return distanceWalked >= maxWalkDistance;
    }

    public bool IsDead()
    {
        return healthStat.health <= 0;
    }

    public bool IsGrounded()
    {
        return grounded;
    }


    private void OnDrawGizmosSelected()
    {
        float range;
        Vector2 shotForce = Quaternion.Euler(0,0,45)*Vector2.right*maxFireForce;

        float time = shotForce.y / 9.81f * 2f;
        range = shotForce.x * time;
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(this.transform.position, range);


    }

}
