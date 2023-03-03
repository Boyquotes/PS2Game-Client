using Godot;
using System;

public partial class Player : CharacterBody2D {
    [Export] private bool airControl = true;
    [Export] private float walkSpeed = 560f;
    [Export] private float runSpeed = 760f;
    [Export] private float jumpForce = 250f;
    [Export] private float wallJumpFroce = 150f;
    [Export] private float wallSlideGravity = 1f;
    [Export] private float crouchSpeed = 50f;
    [Export] private float gravity = 4.5f;
    [Export] private NodePath collider;
    [Export] private NodePath wallCheckRight;
    [Export] private NodePath wallCheckLeft;
    [Export] private NodePath ceilingCheck;
    [Export] private bool isDead = false;
    [Export] private float currentHealth = 100f;

    private Vector2 colliderSize;
    private Vector2 velocity;
    private bool canWallJump = false;

    public override void _Ready() {
        RectangleShape2D shape = (RectangleShape2D) GetNode<CollisionShape2D>(collider).Shape;
        colliderSize = shape.Extents;
    }

    public override void _Process(double delta) {
        isDead = currentHealth < 0;
    }
    
    public override void _PhysicsProcess(double delta) {
        if(!isDead) {
            FallAndWallSlide((float) delta, GetNode<RayCast2D>(wallCheckRight).IsColliding() | GetNode<RayCast2D>(wallCheckLeft).IsColliding(), GetNode<RayCast2D>(ceilingCheck).IsColliding(), this.IsOnFloor());
            Jump(Input.IsActionPressed("jump"), this.IsOnFloor());
            WallJump(Input.IsActionPressed("jump"), GetNode<RayCast2D>(wallCheckRight).IsColliding() | GetNode<RayCast2D>(wallCheckLeft).IsColliding());
            MoveSprintCrouch(Input.IsActionPressed("left"), Input.IsActionPressed("right"), Input.IsActionPressed("sprint"), Input.IsActionPressed("crouch"), airControl, this.IsOnFloor());
        }
    }

    void MoveSprintCrouch(bool moveConditionLeft, bool moveConditionRight, bool sprintCondition, bool crouchCondition, bool airControl, bool IsOnFloor) {
        
        if(moveConditionLeft && !moveConditionRight && !sprintCondition && !crouchCondition) {
            velocity.X = !airControl && !IsOnFloor ? -(walkSpeed / 2) : -walkSpeed;
            Flip(true);
        } else if(!moveConditionLeft && moveConditionRight && !sprintCondition && !crouchCondition) {
            velocity.X = !airControl && !IsOnFloor ? (walkSpeed / 2) : walkSpeed;
            Flip(false);
        } else if(moveConditionLeft && !moveConditionRight && !sprintCondition && crouchCondition) {
            velocity.X = !airControl && !IsOnFloor ? -(crouchSpeed / 2) : -crouchSpeed;
            Flip(true);
        } else if(!moveConditionLeft && moveConditionRight && !sprintCondition && crouchCondition) {
            velocity.X = !airControl && !IsOnFloor ? (crouchSpeed / 2) : crouchSpeed;
            Flip(false);
        } else if(moveConditionLeft && !moveConditionRight && sprintCondition && !crouchCondition) {
            velocity.X = !airControl && !IsOnFloor ? -(runSpeed / 2) : -runSpeed;
            Flip(true);
        } else if(!moveConditionLeft && moveConditionRight && sprintCondition && !crouchCondition) {
            velocity.X = !airControl && !IsOnFloor ? (runSpeed / 2) : runSpeed;
            Flip(false);
        } else {
            velocity.X = 0;
        }

        if(crouchCondition) {
            CrouchCollider(true);
        } else {
            CrouchCollider(false);
        }

        MoveAndSlide(velocity, new Vector2(0, -1));
    }

    void Jump(bool jumpCondition, bool IsOnFloor) {
        if(jumpCondition && IsOnFloor) {
            velocity.Y = -jumpForce;
        }
    }

    void FallAndWallSlide(float delta, bool IsOnWall, bool IsOnCeiling, bool IsOnFloor) {
        if(IsOnWall) {
            velocity.Y += delta * (wallSlideGravity * 100f);
        } else if(!IsOnFloor | IsOnCeiling) {
            velocity.Y += delta * (gravity * 100f);
        }
    }

    void WallJump(bool wallClimbJump, bool IsOnWall) {
        if(!IsOnWall) {
            this.canWallJump = true;
        }
        if(wallClimbJump && IsOnWall && this.canWallJump) {
            velocity.Y = -wallJumpFroce;
            this.canWallJump = false;
        }
    }

    void Flip(bool IsFacingRight) {
        
    }

    public void RemoveHp(double hp) {
        if(hp > 0f && hp <= 100f) {
            currentHealth -= hp;
        } 
    }

    public void Die() {
        currentHealth = 0f;
    }

    void CrouchCollider(bool IsCrouched) {
        if(IsCrouched) {
            RectangleShape2D shape = (RectangleShape2D) GetNode<CollisionShape2D>(collider).Shape;
            shape.Extents = new Vector2(colliderSize.X, (colliderSize.Y / 2));
            GetNode<CollisionShape2D>(collider).Position = new Vector2(0, (colliderSize.Y / 2));
        } else {
            RectangleShape2D shape = (RectangleShape2D) GetNode<CollisionShape2D>(collider).Shape;
            shape.Extents = colliderSize;
            GetNode<CollisionShape2D>(collider).Position = new Vector2(0, 0);
        }
    }
}
