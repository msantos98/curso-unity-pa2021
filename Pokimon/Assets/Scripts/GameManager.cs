using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum GameState
{
    PlayerMove,
    Battle
}
public class GameManager : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private BattleManager battleManager;
    [SerializeField] private Camera worldMainCamera;

    private GameState _gameState;

    private void Awake()
    {
        _gameState = GameState.PlayerMove;
    }

    private void Start()
    {
        playerController.OnPokemonEncountered += StartPokemonBattle;
        battleManager.OnBattleFinished += FinishPokemonBattle;
    }

    private void Update()
    {
        if (_gameState == GameState.PlayerMove)
        {
           playerController.HandleUpdate();
           
        }else if (_gameState == GameState.Battle)
        {
            battleManager.HandleUpdate();
        }
    }

    private void StartPokemonBattle()
    {
        _gameState = GameState.Battle;
        battleManager.gameObject.SetActive(true);
        worldMainCamera.gameObject.SetActive(false);
        
        battleManager.HandleStartBattle();
    }
    
    private void FinishPokemonBattle(bool playerHasWon)
    {
        _gameState = GameState.PlayerMove;
        battleManager.gameObject.SetActive(false);
        worldMainCamera.gameObject.SetActive(true);
        
        if (!playerHasWon)
        {
            // TODO: Diferenciar entre victoria y derrota}
        }
    }
}