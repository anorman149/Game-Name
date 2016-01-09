﻿using UnityEngine;

public class Player : Unit {
    private int coins;
    private int lives;
    private float knockBackPower = 6f;

    public float WeaponRange;

    // Use this for initialization
    protected virtual void Start () {
        Animator = GetComponent<Animator>();
        Collider = GetComponent<PolygonCollider2D>();
        RigidBody = GetComponent<Rigidbody2D>();

        //Setup default params
        coins = GameManager.instance.playerCoins;
        lives = GameManager.instance.playerLives;
        IsGrounded = true;
        FacingRight = true;
	}

	public override void FixedUpdate() {
        base.FixedUpdate();

        //Check to see if the Unit needs to wait
        if(wait) {
            return;
        }

        //Check to see if the Unit is Dead
        if(Dead) {
            disable();
        }

        //TODO Add audio or something here

        //Check if Player moved
        Move();
    }

    void Update() {
        //See if we are jumping
        if (GameManager.instance.platform.CheckPlayerJump() && IsGrounded) {
            //Set the animation
            Animate(Animation.Ground, false);

            MovementController.Jump(this);
        }
    }

    /// <summary>
    /// If the Player is disabled for some reason the 
    /// coins and lives will be transfered to Game Manager
    /// </summary>
    private void OnDisable() {
        GameManager.instance.playerCoins = coins;
        GameManager.instance.playerLives = lives;
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
    public override void TakeDamage(int damage) {
        CurrentHealth -= damage;

        //Play Animation for taking damage
        Animate(Animation.Damage, "");

        //Knock the character back a little
        StartCoroutine(MovementController.KnockBack(0.02f, knockBackPower, this));

        //Need to check and see if we died
        CheckHealth();
    }

    /// <summary>
    /// Will subtract a Life and start back at MaxHealth. 
    /// If there are no Lives left, Death is called.
    /// </summary>
    public void SubtractLife() {
        if(lives <= 0) {
            Death();
        } else {
            lives -= 1;
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
        StartCoroutine(UnitController.WaitForSeconds(Animator.GetCurrentAnimatorStateInfo(0).length + .5f, this));

        //TODO Add sound
    }

    /// <summary>
    /// Will Disable and Destroy the Unit
    /// </summary>
    private void disable() {
        enabled = false;
        Destroy(this.gameObject);
        GetComponent<Renderer>().enabled = false;

        //The game has ended if no lives
        GameManager.instance.GameOver();
    }

    /// <summary>
    /// Check whether the Player has Died
    /// If the Player has no more lives, it will end the game
    /// If another life exists, it will use it
    /// </summary>
    public override void CheckHealth() {
        if (CurrentHealth <= 0) {
            //TODO add Sound
            //TODO add Animation for using Life
            SubtractLife();
        }
    }

    /// <summary>
    /// Unit to Attack
    /// </summary>
    /// <param name="obj">Which Object the Unit will do damage to</param>
    public override void DealDamage(GameObject gameObject) {
        //Check to see if the Object is an Enemy AND if the Enemy is in Range of the Player's weapon
        if(gameObject.tag.Equals("Enemy") && MovementController.CheckDistanceFromUnit(this, gameObject.GetComponent<Enemy>()) <= WeaponRange) {
            Enemy enemy = gameObject.GetComponent<Enemy>();

            //If not Dead AND the Player is facing the same direction, then do some damage
            if(!enemy.IsDead() && UnitController.UnitsFacingEachOther(this, enemy)) {
                //Play Animation for taking damage
                Animate(Animation.Attack, "");

                enemy.TakeDamage(Damage);
            }
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
    /// Will subtract coins from Player
    /// </summary>
    /// <param name="coinsLost">Amount of coins to lose</param>
    public void SubtractCoins(int coinsLost) {
        coins -= coinsLost;

        //If coins are now below 0, let's set to 0
        if (coins < 0) {
            coins = 0;
        }
    }

    /// <summary>
    /// Will add the supplied Coins
    /// </summary>
    /// <param name="coinsToAdd">Amount of coins to add</param>
    public void AddCoins(int coinsToAdd) {
        coins += coinsToAdd;
    }

    /// <summary>
    /// Will disable the Player and End the Level
    /// </summary>
    public void FinishLevel() {
        enabled = false;

        //TODO add more code for finishing level
    }

    public override void OnCollisionEnter2D(Collision2D collision) {
        
    }
}
