﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

// Enemy type. Aims at enemy and fires a burst of three bullets. Each burst can occur any time between a set range.
public class BurstFireEnemyWeapon : MonoBehaviour
{
    private bool loaded;
    private bool assetSpawned;
    public AudioClip enemyActivate;
    public GameObject Player;
    public GameObject Nozzle;
    public GameObject Bullet;
    public ParticleSystem flames;
    public GameObject laser;
    public int Health = 2;
    public float spawnDelay = 2f;

    private List<EnemyBullet> _bulletStore;
    private EnemyAI enemyAIController;
    private float _bulletTimer;
    private float bulletDelay;
    private readonly int defaultBulletPool = 3;
    private int _bulletPool;
    private Transform cachedTransform;
    private Transform cachedNozzleTransform;
    private AudioSource weaponSound;

    private void Awake()
    {
        GameEvents.current.onSceneLoaded += OnceSceneLoaded;
        _bulletPool = defaultBulletPool;
        cachedTransform = transform;
        cachedNozzleTransform = Nozzle.transform;
        _bulletStore = new List<EnemyBullet>();
        weaponSound = transform.GetChild(0).GetComponent<AudioSource>();
        bulletDelay = Random.Range(2f, 6f);
        enemyAIController = GetComponent<EnemyAI>() ?? GetComponentInParent<EnemyAI>();
        enemyAIController.Health = Health;
    }

    private void OnceSceneLoaded()
    {
        Player = GameObject.Find("VRCamera");
        loaded = true;
    }

    private void OnEnable()
    {
        for (var i = 0; i < 10; i++)
        {
            _bulletStore.Add(BulletManager.instance.RequestBullet());
            _bulletStore[i].parent = gameObject;
            _bulletStore[i].gameObject.SetActive(false);
        }

        enemyAIController.ToggleRenderersAndColliders(false);
        laser.SetActive(false);
        flames.Stop();
        StartCoroutine(SpawnDelay());
    }

    private IEnumerator SpawnDelay()
    {
        yield return new WaitForSecondsRealtime(spawnDelay);
        laser.SetActive(true);
        flames.Play();
        assetSpawned = true;
        enemyAIController.ToggleRenderersAndColliders(true);
        GetComponent<AudioSource>().PlayOneShot(enemyActivate, 1);

    }

    private void OnDestroy()
    {
        if (BulletManager.instance != null)
        {
            BulletManager.instance.ReturnBullet(_bulletStore);
        }
    }

    private void Fire()
    {
        foreach (var bullet in _bulletStore.Where(bullet => !bullet.gameObject.activeSelf))
        {
            var go = bullet.gameObject;
            go.SetActive(true);
            go.transform.position = cachedNozzleTransform.position;
            go.transform.rotation = cachedNozzleTransform.rotation;
            break;
        }

        weaponSound.Play();
    }

    void Update()
    {
        if (!assetSpawned || !loaded) return;
        if (enemyAIController.Dead) return;
        cachedTransform.LookAt(Player.transform);

        // Shoots a burst of three bullets in quick succession then waits 1 second.
        if (_bulletTimer < bulletDelay)
        {
            _bulletTimer += Time.deltaTime;
        }
        else
        {
            if (_bulletPool > 0)
            {
                _bulletTimer = bulletDelay - 0.3f;
                Fire();
                _bulletPool--;
            }
            else
            {
                bulletDelay = Random.Range(2f, 6f);
                _bulletTimer = 0;
                _bulletPool = defaultBulletPool;
            }
        }
        
    }
}
