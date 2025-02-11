﻿using System;
using UnityEngine;

public class Player : Unit {

    protected virtual void Start () {
        Animator = GetComponent<Animator>();
        Collider = GetComponent<BoxCollider2D>();
        RigidBody = GetComponent<Rigidbody2D>();

        IsGrounded = true;
        FacingRight = true;
	}

	public override void FixedUpdate() {
        base.FixedUpdate();

        //Check to see if the Unit needs to wait
        if(wait) {
            return;
        }

        //Check to see if it's Game Over
        if(GameManager.instance.playerLives <= 0) {
            disable();
        }

        if(Dead && GameManager.instance.playerLives > 0) {
            //Will need to Respawn
            PlayerRespawn.Instantiate();

            //Destroy the current Player
            Destroy(gameObject);
        }

        //Check if Unit moved
        Move();

        //Check if Unit Attacked
        Attack();
    }

    public virtual void Update() {
        //See if we are jumping
        if (GameManager.instance.platform.CheckJump() && IsGrounded) {
            //Set the animation
            Animate(Animation.Ground, false);

            MovementController.Jump(this);
        }

        //See if they are off the ground. They can 
        //push DOWN and fall faster
        if(!IsGrounded && GameManager.instance.platform.CheckDown()) {
            MovementController.FallFaster(this);
        }
    }

    /// <summary>
    /// Will move the Player if the input was detected
    /// </summary>
    private void Move() {
        //Check if Player moved
        Vector3 move = GameManager.instance.platform.CheckPlayerMovement();

        //Only update for moving if we actually moved
        if(!move.Equals(Vector3.zero)) {
            MovementController.Move(this, move);
        }
    }

    /// <summary>
    /// Damage the Player
    /// </summary>
    /// <param name="damage">Amount of Damage to Player</param>
    public override void ReceiveDamage(float damage) {
        CurrentHealth -= damage;

        //Play Animation for taking damage
        Animate(Animation.Damage, "");

        //Start the blinking and invulnerability
        StartCoroutine(blink(.15f, .2f, true));

        //Need to show Damage Taken Text
        FloatingText.Show(string.Format("-{0}", Convert.ToString(damage)), GUIUtils.damageStyle, new FromWorldPointTextPositioner(transform.position, 2f, 50));

        //Need to check and see if we died
        CheckHealth();
    }

    /// <summary>
    /// Will subtract a Life, call Death, and start back at MaxHealth. 
    /// </summary>
    public void SubtractLife() {
        Death();

        if(GameManager.instance.playerLives > 0) {
            GameManager.instance.playerLives -= 1;
            CurrentHealth = MaxHealth;
        }
    }

    /// <summary>
    /// Will add the supplied health to the health total
    /// </summary>
    /// <param name="amountOfHealth">Amount of health to add</param>
    public void AddHealth(int amountOfHealth) {
        CurrentHealth += amountOfHealth;

        //Can't go over 100, so set it back
        if(CurrentHealth > 100) {
            CurrentHealth = 100;
        }
    }

    /// <summary>
    /// Will kill the Unit
    /// </summary>
    public override void Death() {
        //Play death animation
        Dead = true;
        Animate(Animation.Dead, Dead);

        //Stop the Movement
        MovementController.StopMovement(this);

        //Wait until the Animation has finished
        StartCoroutine(UnitController.WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length + .6f, this));

        //TODO Add sound
    }

    /// <summary>
    /// Will Disable and Destroy the Unit
    /// </summary>
    private void disable() {
        enabled = false;
        Destroy(this.gameObject);

        //The game has ended if no lives
        GameManager.instance.GameOver();
    }

    /// <summary>
    /// Check if Health has dropped to 0 or below
    /// </summary>
    public override void CheckHealth() {
        if (CurrentHealth <= 0) {
            //TODO add Sound
            //TODO add Animation for using Life
            SubtractLife();
        }
    }

    public override void OnTriggerEnter2D(Collider2D collider) {
        GameObject colliderGameObject = collider.gameObject;

        if(colliderGameObject.tag.Equals("Item")) {
            //The Player has collided with an Item
            IItem item = colliderGameObject.GetComponent(colliderGameObject.name) as IItem;

            //Pickup the Item
            item.PickupAction(this.gameObject);

            //Disable the Item now, since it was picked up
            item.DisableOnPickup();
        }

        //TODO Need to figure out Exits
    }

    /// <summary>
    /// Will disable the Player and End the Level
    /// </summary>
    public void FinishLevel() {
        enabled = false;

        //TODO add more code for finishing level
    }
}
