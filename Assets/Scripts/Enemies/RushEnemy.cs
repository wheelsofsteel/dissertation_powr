﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class RushEnemy : MonoBehaviour
{
    private ParticleSystem fireBurst;
    private bool assetSpawned;
    private GameObject player;
    private PlayerDamageController dmgController;
    private EnemyAI enemyAIController;
    private Transform cachedTransform;
    private Collider m_collider;
    private MeshRenderer m_renderer;
    private AudioSource audioSource;
    public AudioClip thrusterClip;
    public AudioClip explosionClip;
    public float spawnDelay = 5f;

    private bool passedPlayer;
    private bool shouldDie;


    public GameObject Explosion;

    public int Health = 1;

    private float timer;
    private float maxTime = 10f;

    private float moveSpeed = 5f;
    // Start is called before the first frame update
    void Start()
    {
        fireBurst = GetComponentInChildren<ParticleSystem>();
        audioSource = GetComponent<AudioSource>();
        player = GameObject.Find("BodyColliderDamage");
        dmgController = player.GetComponent<PlayerDamageController>();
        enemyAIController = GetComponent<EnemyAI>();
        m_collider = GetComponent<Collider>();
        m_renderer = GetComponentInChildren<MeshRenderer>();
        enemyAIController.Health = Health;
        cachedTransform = transform;
    }

    void OnEnable()
    {
        if (player == null) return;
        fireBurst.Stop();
        m_collider.enabled = false;
        m_renderer.enabled = false;
        StartCoroutine(SpawnDelay());
    }

    private IEnumerator SpawnDelay()
    {
        yield return new WaitForSecondsRealtime(spawnDelay);
        assetSpawned = true;
        m_collider.enabled = true;
        fireBurst.Play();
        m_renderer.enabled = true;
        audioSource.clip = thrusterClip;
        audioSource.loop = true;
        audioSource.Play();
    }

    void Update()
    {
        if (!assetSpawned) return;
        cachedTransform.position += cachedTransform.forward * (moveSpeed * Time.deltaTime);
        if (timer < maxTime)
        {
            timer += Time.deltaTime;
        }
        else
        {
            if (shouldDie) return;
            shouldDie = true;
            StartCoroutine(WaitForExplosion());
            return;
        }
        if (Vector3.Distance(player.transform.position, transform.position) > 10f && !passedPlayer)
        {
            transform.LookAt(player.transform);
        }
        else
        {
            passedPlayer = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject == player)
        {
            dmgController.TakeDamage();
        }

        StartCoroutine(WaitForExplosion());

    }

    private IEnumerator WaitForExplosion()
    {
        audioSource.Stop();
        audioSource.clip = explosionClip;
        audioSource.loop = false;
        audioSource.Play();
        m_renderer.enabled = false;
        m_collider.enabled = false;
        GameObject explosionClone = Instantiate(Explosion, transform.position, cachedTransform.rotation);
        Destroy(explosionClone, 1f);
        yield return new WaitForSecondsRealtime(1f);
        enemyAIController.enemyWaveController.RemoveEnemy(gameObject);
        Destroy(gameObject);
    }
}