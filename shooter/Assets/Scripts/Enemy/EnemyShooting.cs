using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShooting : MonoBehaviour
{
    public GameObject prefab;
    public GameObject shootingPoint;

    private ParticleSystem shootingEffect;
    private Animator _animator;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
        shootingEffect = shootingPoint.gameObject.GetComponentInChildren<ParticleSystem>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            _animator.SetBool("Shoot Bullet", true);
            FireEnemyBullet();
        }
        else
        {
            _animator.SetBool("Shoot Bullet", false);
        }
    }

    void FireEnemyBullet()
    {
        GameObject bullet = ObjectPool.SharedInstance.GetFirstPooleableObject();
        bullet.transform.position = shootingPoint.transform.position;
        bullet.transform.rotation = shootingPoint.transform.rotation;
        bullet.SetActive(true);
        
        shootingEffect.Play();
    }
}