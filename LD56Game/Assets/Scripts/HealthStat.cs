using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthStat : MonoBehaviour
{
    bool dead = false;
    public float health = 100;
    public float maxHealth = 100;
    public UnityEngine.Events.UnityEvent OnDie=new UnityEngine.Events.UnityEvent();
    public UnityEngine.Events.UnityEvent OnDamage=new UnityEngine.Events.UnityEvent();

    public AudioClip onHitNoise;
    public AudioClip onDieNoise;

    [SerializeField] UnityEngine.UI.Slider slider;


    private void Update()
    {
        slider.value = health / maxHealth;
    }

    public void ReceiveDamage(float damage)
    {
        health-=damage;
        if (health <= 0)
        {
            Die();
            gameObject.SendMessage("PlayDeathAnimation");
            slider.enabled = false;
        }
        else
        {
            gameObject.SendMessage("PlayFallAnimation");
            Game.audioSource.PlayOneShot(onHitNoise);
            OnDamage.Invoke();
        }
    }

    void Die()
    {
        Game.audioSource.PlayOneShot(onDieNoise);
        OnDie.Invoke();
    }

    public void Destroy()
    {
        GameObject.Destroy(gameObject);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag.Equals("Boundary"))
        {
            Die();
        }
    }

}
