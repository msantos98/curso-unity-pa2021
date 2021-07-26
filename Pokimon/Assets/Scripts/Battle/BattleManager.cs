using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum BattleState
{
    StartBattle,
    PlayerSelectAction,
    PlayerMove,
    EnemyMove,
    Busy
}

public class BattleManager : MonoBehaviour
{
    public BattleState battleState;

    public event Action<bool> OnBattleFinished;
    
    [SerializeField] private BattleUnit playerUnit;
    [SerializeField] private BattleHUD playerHUD;
    
    [SerializeField] private BattleUnit enemyUnit;
    [SerializeField] private BattleHUD enemyHUD;

    [SerializeField] private BattleDialogBox battleDialogBox;

    private int currentSelectedAction;
    private float timeSinceLastClick;
    [SerializeField] private float timeBetweenClicks = 1.0f;

    private int currentSelectedMovement;

    private float attackAnimationDuration = 0.6f;
    private float weakenedAnimationDuration = 2f;

    public void HandleStartBattle()
    {
        StartCoroutine(SetUpBattle());
    }

    public void HandleUpdate()
    {
        timeSinceLastClick += Time.deltaTime;

        if (battleDialogBox.isWriting)
        {
            return;
        }

        if (battleState == BattleState.PlayerSelectAction)
        {
            HandlePlayerActionSelection();
        }else if (battleState == BattleState.PlayerMove)
        {
            HandlePlayerMovementSelection();
        }
    }

    /// <summary>
    /// Empieza la batalla
    /// </summary>
    public IEnumerator SetUpBattle()
    {
        battleState = BattleState.StartBattle;
        
        playerUnit.SetUpPokemon();
        playerHUD.SetPokemon(playerUnit.Pokemon);
        battleDialogBox.SetPokemonMovements(playerUnit.Pokemon.Moves);
        
        enemyUnit.SetUpPokemon();
        enemyHUD.SetPokemon(enemyUnit.Pokemon);
        
        yield  return battleDialogBox.SetDialog($"Un {enemyUnit.Pokemon.Base.Name} salvaje apareció.");
        
        if (enemyUnit.Pokemon.Speed > playerUnit.Pokemon.Speed)
        {
            StartCoroutine(battleDialogBox.SetDialog("El enemigo ataca primero."));
            EnemyAction();
        }
        else
        {
            PlayerAction();
        }
    }

    /// <summary>
    /// Jugador elige acción
    /// </summary>
    private void PlayerAction()
    {
        battleState = BattleState.PlayerSelectAction;
        
        StartCoroutine(battleDialogBox.SetDialog("Selecciona una acción."));
        battleDialogBox.ToggleDialogText(true);
        battleDialogBox.ToggleActions(true);
        battleDialogBox.ToggleMovements(false);
        
        // Por defecto hacemos que se muestre seleccionada la primera acción
        currentSelectedAction = 0;
        battleDialogBox.SelectAction(currentSelectedAction);
    }

    private void PlayerMovement()
    {
        battleState = BattleState.PlayerMove;
        
        battleDialogBox.ToggleDialogText(false);
        battleDialogBox.ToggleActions(false);
        battleDialogBox.ToggleMovements(true);

        currentSelectedMovement = 0;
        battleDialogBox.SelectMovement(currentSelectedMovement, playerUnit.Pokemon.Moves[currentSelectedMovement]);
    }
    
    private IEnumerator PerformPlayerMovement()
    {
        Move move = playerUnit.Pokemon.Moves[currentSelectedMovement];
        move.PP--;
        yield return battleDialogBox.SetDialog($"{playerUnit.Pokemon.Base.Name} ha usado {move.Base.Name}.");
        
        int oldHPValue = enemyUnit.Pokemon.HP;
        
        playerUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(attackAnimationDuration);
        
        enemyUnit.PlayReceiveAttackAnimation();
        DamageDescription damageDescription = enemyUnit.Pokemon.ReceiveDamage(playerUnit.Pokemon, move);
        enemyHUD.UpdatePokemonData(oldHPValue);

        yield return ShowDamageDescription(damageDescription);
        
        if (damageDescription.Weakened)
        {
            yield return battleDialogBox.SetDialog($"{enemyUnit.Pokemon.Base.Name} ha sido debilitado.");
            enemyUnit.PlayWeakenedAnimation();
            yield return new WaitForSeconds(weakenedAnimationDuration);
            
            OnBattleFinished(true);
        }
        else
        {
            StartCoroutine(EnemyAction());
        }
    } 
    
    private IEnumerator EnemyAction()
    {
        battleState = BattleState.EnemyMove;

        Move move = enemyUnit.Pokemon.RandomMove();
        move.PP--;
        yield return battleDialogBox.SetDialog($"{enemyUnit.Pokemon.Base.Name} ha usado {move.Base.Name}.");

        int oldHPValue = playerUnit.Pokemon.HP;
        
        enemyUnit.PlayAttackAnimation();
        yield return new WaitForSeconds(attackAnimationDuration);

        playerUnit.PlayReceiveAttackAnimation();
        DamageDescription damageDescription = playerUnit.Pokemon.ReceiveDamage(enemyUnit.Pokemon, move);
        playerHUD.UpdatePokemonData(oldHPValue);
        
        yield return ShowDamageDescription(damageDescription);
        
        if (damageDescription.Weakened)
        {
            yield return battleDialogBox.SetDialog($"¡Oh, no! {playerUnit.Pokemon.Base.Name} ha sido debilitado.");
            playerUnit.PlayWeakenedAnimation();
            
            yield return new WaitForSeconds(weakenedAnimationDuration);
            OnBattleFinished(false);
        }
        else
        {
            PlayerAction();
        }
    }
    
    private void HandlePlayerActionSelection()
    {
        if (timeSinceLastClick < timeBetweenClicks)
        {
            return;
        }
        
        if (Input.GetAxisRaw("Vertical") != 0)
        {
            timeSinceLastClick = 0;
            
            // Subimos o bajamos
            // Solamente tenemos en cuenta 2 acciones (Luchar o Huir)
            currentSelectedAction = (currentSelectedAction + 1) % 2;
            
            battleDialogBox.SelectAction(currentSelectedAction);
        }

        if (Input.GetAxisRaw("Submit") != 0)
        {
            timeSinceLastClick = 0;
            
            // Seleccionamos acción
            if (currentSelectedAction == 0)
            {
                PlayerMovement();
                
            }else if (currentSelectedAction == 1)
            {
                // TODO: Implementar huir
            }
        }
    }
    
    private void HandlePlayerMovementSelection()
    {
        if (timeSinceLastClick < timeBetweenClicks)
        {
            return;
        }

        if (Input.GetAxisRaw("Vertical") != 0)
        {
            timeSinceLastClick = 0;
            
            // Tenemos en cuenta 4 movimientos
            int oldSelectedMovement = currentSelectedMovement;
            currentSelectedMovement = (currentSelectedMovement + 2) % 4;

            if (currentSelectedMovement >= playerUnit.Pokemon.Moves.Count)
            {
                currentSelectedMovement = oldSelectedMovement;
            }
            
            battleDialogBox.SelectMovement(currentSelectedMovement, playerUnit.Pokemon.Moves[currentSelectedMovement]);
            
        } else if (Input.GetAxisRaw("Horizontal") != 0)
        {
            timeSinceLastClick = 0;
            int oldSelectedMovement = currentSelectedMovement;

            currentSelectedMovement = (currentSelectedMovement + 1) % 2 + 2 * (currentSelectedMovement / 2);
            
            if (currentSelectedMovement >= playerUnit.Pokemon.Moves.Count)
            {
                currentSelectedMovement = oldSelectedMovement;
            }
            
            battleDialogBox.SelectMovement(currentSelectedMovement, playerUnit.Pokemon.Moves[currentSelectedMovement]);
        }

        if (Input.GetAxisRaw("Submit") != 0)
        {
            timeSinceLastClick = 0;
            
            battleDialogBox.ToggleDialogText(true);
            battleDialogBox.ToggleActions(false);
            battleDialogBox.ToggleMovements(false);

            StartCoroutine(PerformPlayerMovement());
        }
    }

    private IEnumerator ShowDamageDescription(DamageDescription damageDescription)
    {
        if (damageDescription.Critical > 1)
        {
            yield return battleDialogBox.SetDialog("¡Golpe crítico!");
        }

        if (damageDescription.Type > 1)
        {
            yield return battleDialogBox.SetDialog("¡Es súper efectivo!");
            
        }else if (damageDescription.Type < 1)
        {
            yield return battleDialogBox.SetDialog("No es muy efectivo...");
        }
    }
}