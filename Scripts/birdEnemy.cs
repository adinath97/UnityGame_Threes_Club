﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class birdEnemy : MonoBehaviour
{
    public float gravity;
    public Vector2 velocity;
    public bool isWalkingLeft = true;
    //private bool grounded = false;
    public LayerMask floorMask;
    private RaycastHit2D hitRay;
    private RaycastHit2D hitRay1;
    public LayerMask wallMask;
    public bool shouldDie = false;
    private float deathTimer = 0;
    public float timeBeforeDestruction = 1.0f;
    private Vector2 direction;
    private Animator animator;

    private enum EnemyState
    {
        flying,
        dead
    }

    private EnemyState state = EnemyState.flying;

    // Start is called before the first frame update
    void Start()
    {
        animator = this.GetComponent<Animator>();
        enabled = false; //until it comes in camera, disable
        //Fall();
        direction = Vector2.left;
    }

    // Update is called once per frame
    void Update()
    {
        if(!LevelManager.gameOver && !PlayerRotator.playerOutOfRotation)
        {
            UpdatePosition();
            animator.enabled = true;
        }
        else
        {
            animator.enabled = false;
        }
    }

    private void UpdatePosition()
    {
        Vector3 scale = transform.localScale;

        if (direction == Vector2.right)
        {
            scale.x = 1;
            transform.position = new Vector2(transform.position.x + velocity.x * Time.deltaTime, transform.position.y);
        }
        else if (direction == Vector2.left)
        {
            scale.x = -1;
            transform.position = new Vector2(transform.position.x - velocity.x * Time.deltaTime, transform.position.y);
        }

        //transform.localPosition = pos;
        transform.localScale = scale;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "playerBullet")
        {
            Destroy(collision.gameObject);
            Destroy(this.gameObject);
        }

        Debug.Log("COLLISION OCCURRED!");
        if (direction == Vector2.right)
        {
            direction = Vector2.left;
        }

        else if (direction == Vector2.left)
        {
            direction = Vector2.right;
        }
    }

    public void Crush()
    {
        state = EnemyState.dead;
        //trigger death animation for enemy instance
        GetComponent<Collider2D>().enabled = false;
        shouldDie = true;
    }

    void CheckCrushed()
    {
        if (shouldDie)
        {
            if (deathTimer <= timeBeforeDestruction)
            {
                deathTimer += Time.deltaTime;
            }
            else
            {
                shouldDie = false;
                Destroy(this.gameObject);
            }
        }
    }

    void UpdateEnemyPosition()
    {
        if (state != EnemyState.dead)
        {
            //update position
            Vector3 pos = transform.localPosition;
            Vector3 scale = transform.localScale;

            if (state == EnemyState.flying)
            {
                if (isWalkingLeft)
                {
                    pos.x -= velocity.x * Time.deltaTime;
                    scale.x = -1;
                }
                else
                {
                    pos.x += velocity.x * Time.deltaTime;
                    scale.x = 1;
                }
            }

            CheckWalls(pos, scale.x);

            transform.localPosition = pos;
            transform.localScale = scale;
        }
    }

    Vector3 CheckGround(Vector3 pos)
    {
        //left center and right origins to pass rays downwards
        Vector2 originLeft = new Vector2(pos.x - .5f + .2f, pos.y - .5f);
        Vector2 originCenter = new Vector2(pos.x, pos.y - .5f);
        Vector2 originRight = new Vector2(pos.x + .5f - .2f, pos.y - .5f);

        RaycastHit2D groundLeft = Physics2D.Raycast(originLeft, Vector2.down, velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D groundMiddle = Physics2D.Raycast(originCenter, Vector2.down, velocity.y * Time.deltaTime, floorMask);
        RaycastHit2D groundRight = Physics2D.Raycast(originRight, Vector2.down, velocity.y * Time.deltaTime, floorMask);

        if (groundLeft.collider != null || groundMiddle.collider != null || groundRight.collider != null)
        {
            if (groundLeft)
            {
                hitRay = groundLeft;
            }
            else if (groundMiddle)
            {
                hitRay = groundMiddle;
            }
            else if (groundRight)
            {
                hitRay = groundRight;
            }

            if (hitRay.collider.tag == "Speedster")
            {
                ///load game over screen;
                Debug.Log("Player Hit!");
            }

            pos.y = hitRay.collider.bounds.center.y + hitRay.collider.bounds.size.y / 2 + .5f;

            //grounded = true;
            velocity.y = 0;
        }

        return pos;
    }

    void CheckWalls(Vector3 pos, float direction)
    {
        Vector2 originTop = new Vector2(pos.x + direction * .4f, pos.y + .5f - .2f);
        Vector2 originMiddle = new Vector2(pos.x + direction * .4f, pos.y);
        Vector2 originBottom = new Vector2(pos.x + direction * .4f, pos.y - .5f + .2f);

        RaycastHit2D wallTop = Physics2D.Raycast(originTop, new Vector2(direction, 0), velocity.x * Time.deltaTime, wallMask);
        RaycastHit2D wallMiddle = Physics2D.Raycast(originMiddle, new Vector2(direction, 0), velocity.x * Time.deltaTime, wallMask);
        RaycastHit2D wallBottom = Physics2D.Raycast(originBottom, new Vector2(direction, 0), velocity.x * Time.deltaTime, wallMask);

        if (wallTop.collider != null || wallMiddle.collider != null || wallBottom.collider != null)
        {
            if (wallTop)
            {
                hitRay1 = wallTop;
            }
            else if (wallMiddle)
            {
                hitRay1 = wallMiddle;
            }
            else if (wallBottom)
            {
                hitRay1 = wallBottom;
            }

            isWalkingLeft = !isWalkingLeft;
        }
    }

    private void OnBecameVisible()
    {
        //enable once viewable by camera
        enabled = true;
    }
}
