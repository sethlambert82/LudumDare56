using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public Rigidbody2D rb;
    public float damage;
    public AudioClip fireNoise;
    public AudioClip hitNoise;


    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        Game.audioSource.PlayOneShot(fireNoise);
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.right = rb.velocity;
    }


    private void OnCollisionEnter2D(Collision2D collision)
    {
        //check the target
        collision.gameObject.SendMessage("ReceiveDamage", damage);
        collision.gameObject.SendMessage("FaceTarget", rb.transform);
        Game.audioSource.PlayOneShot(hitNoise);
        GameObject.Destroy(this.gameObject);

    }

}
