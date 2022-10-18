using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public Rigidbody2D rb;
    public Animator animator;
    public ParticleSystem dustEffect;
    
    private float walkSpeed;
    private float moveAcceleration;
    private float jumpSpeed;
    private float moveHorizontal;
    private float fallSpeed;
    private float aButtonCooldown;
    private float dButtonCooldown;
    private float dashSpeed;
    private float dashTime;
    private float dashCooldown;
    private float gravityScale;
    private float jumpTime;

    private bool doubleTap;
    private bool onGround;
    private bool canJump;
    private bool isDashing;
    private bool canDash;

    //Abilities
    private bool doubleJump;
    private bool dash;



    void Start()
    {
        //Setting values
        fallSpeed = -2f;
        walkSpeed = 7f;
        jumpSpeed = 15f;
        aButtonCooldown = 0.5f;
        dButtonCooldown = 0.5f;
        dashSpeed = 30f;
        dashTime = 0.1f;
        dashCooldown = 0.5f;
        jumpTime = 0.4f;

        doubleTap = false;
        onGround = false;
        canJump = true;
        isDashing = false;
        canDash = true;

        //Abilities
        doubleJump = true;
        dash = true;
    }


    void Update()
    {
        //Constantly set key input to moveHorizontal
        moveHorizontal = Input.GetAxisRaw("Horizontal");

        //Handle first jump
        if(onGround && Input.GetKeyDown(KeyCode.W))
        {
            StartCoroutine(Jump());
        }
        
        //Handle jump cutting
        if(rb.velocity.y > 0f && Input.GetKeyUp(KeyCode.W) && canJump)
        {
            rb.AddForce(new Vector2(0f, fallSpeed - (0.1f * rb.velocity.y) * 4f), ForceMode2D.Impulse);
        }

        //Handle double jump
        if(!onGround && doubleJump && canJump)
        {
            if(Input.GetKeyDown(KeyCode.W))
            {
            StartCoroutine(Jump());
            canJump = false;
            }
        }

        //Handle facing direction
        if(moveHorizontal != Mathf.Sign(rb.velocity.x) && moveHorizontal != 0f)
        {
            rb.transform.localScale = new Vector3(moveHorizontal * -1f, 1, 1);
        }

        //Handle dash
        if(Input.GetKeyDown(KeyCode.D) && dash)
        {
            if(dButtonCooldown > 0 && doubleTap)
            {
                if(canDash == true)
                {
                    StartCoroutine(Dash(1f));
                }
            }
            else
            {
                doubleTap = true;
                dButtonCooldown = .5f;
            }
        }
        if(Input.GetKeyDown(KeyCode.A) && dash)
        {
            if(aButtonCooldown > 0 && doubleTap)
            {
                if(canDash == true)
                {
                    StartCoroutine(Dash(-1f));
                }
            }
            else
            {
                doubleTap = true;
                aButtonCooldown = .5f;
            }
        }

        aButtonCooldown -= Time.deltaTime;
        dButtonCooldown -= Time.deltaTime;
    }


    void FixedUpdate()
    {
        //Acceleration decreases as x velocity reaches walk speed
        moveAcceleration = (walkSpeed - Mathf.Abs(rb.velocity.x))*30f; 

        if(rb.velocity != new Vector2(0f, 0f))
        {
            dustEffect.Play();
        }

        //Handle movement
        if(!isDashing)
        {
            if(moveHorizontal > 0.1f || moveHorizontal < -0.1f)
            {
                animator.SetBool("Running", true);
                rb.AddForce(new Vector2(moveHorizontal * moveAcceleration, 0f), ForceMode2D.Force);
            }
            //friction
            else
            {
                animator.SetBool("Running", false);
                if(moveHorizontal < 0.1f && moveHorizontal > -0.1f && onGround)
                {
                    rb.AddForce(new Vector2((0f-rb.velocity.x) *20f, 0f), ForceMode2D.Force);
                }
                else if(moveHorizontal < 0.1f && moveHorizontal > -0.1f && !onGround)
                {
                    rb.AddForce(new Vector2((0f-rb.velocity.x) *3f, 0f), ForceMode2D.Force);
                }
            }
        }
        //Increases fall velocity until reaches set fallSpeed
        if(rb.velocity.y < 0 && !onGround)
        {
            rb.AddForce(new Vector2(0f, fallSpeed - (0.1f * rb.velocity.y) * 2), ForceMode2D.Impulse);
        }
    }

    //Handles onGround with box collider
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            onGround = true;
            canJump = true;
        }
     }
    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.gameObject.tag == "Ground")
        {
            onGround = false;
        }
    }

    //Function to jump
    private IEnumerator Jump()
    {
        rb.velocity = new Vector2(0f, jumpSpeed);
        if(onGround)
        {
            yield return new WaitForSeconds(jumpTime);
        }
        else //Double jump is smaller
        {
            yield return new WaitForSeconds(jumpTime * .5f);
        }
        rb.AddForce(new Vector2(0f, fallSpeed - (0.1f * rb.velocity.y) * 2), ForceMode2D.Impulse); //Increases fall speed
    }

    //Function to dash
    private IEnumerator Dash(float dir)
    {
        canDash = false;
        gravityScale = rb.gravityScale;
        rb.gravityScale = 0f;
        isDashing = true;
        rb.velocity = new Vector2(dashSpeed * dir, 0f);
        yield return new WaitForSeconds(dashTime);
        isDashing = false;
        rb.gravityScale = gravityScale;
        rb.AddForce(new Vector2((0-rb.velocity.x) * 0.9f, 0f), ForceMode2D.Impulse);
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }
}
