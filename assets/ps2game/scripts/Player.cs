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
    [Export] private CollisionShape2D collider;
    [Export] private RayCast2D wallCheck;
    [Export] private RayCast2D ceilingCheck;
    [Export] private bool isDead = false;
    [Export] private float currentHealth = 100f;

    private Vector2 velocity;
    private Vector2 colliderSize;
    private bool canWallJump = false;
    private bool wasFlipped = false;

    public override void _Ready() {
        RectangleShape2D shape = (RectangleShape2D) collider.Shape;
        colliderSize = shape.Size;
    }

    public override void _Process(double delta) {
        isDead = currentHealth < 0;
        if(!wasFlipped) {
            this.Scale = new Vector2(this.Scale.X == -1 ? 1 : -1, this.Scale.Y);
            wasFlipped = true;
        }
    }
    
    public override void _PhysicsProcess(double delta) {
        Control(
            isDead,
            Input.IsActionPressed("left"),
            Input.IsActionPressed("right"),
            Input.IsActionPressed("sprint"),
            Input.IsActionPressed("crouch"),
            airControl,
            Input.IsActionPressed("jump"),
            delta,
            wallCheck.IsColliding(),
            ceilingCheck.IsColliding(),
            this.IsOnFloor()
        );
    }

    void Control(bool isDead, bool moveConditionLeft, bool moveConditionRight, bool sprintCondition, bool crouchCondition, bool airControl, bool jumpCondition, double delta, bool IsOnWall, bool IsOnCeiling, bool IsOnFloor) {
        if(!isDead) {
            FallAndWallSlide((float) delta, IsOnWall, IsOnCeiling, IsOnFloor);
            Jump(jumpCondition, IsOnFloor);
            WallJump(jumpCondition, IsOnWall);
            MoveSprintCrouch(moveConditionLeft, moveConditionRight, sprintCondition, crouchCondition, airControl, IsOnFloor);

            Velocity = velocity;
            MoveAndSlide();
        }
    }
    
    void FallAndWallSlide(float delta, bool IsOnWall, bool IsOnCeiling, bool IsOnFloor) {
        if(IsOnWall) {
            velocity.Y += delta * (wallSlideGravity * 100f);
        } else if(!IsOnFloor | IsOnCeiling) {
            velocity.Y += delta * (gravity * 100f);
        }
    }

    void Jump(bool jumpCondition, bool IsOnFloor) {
        if(jumpCondition && IsOnFloor) {
            velocity.Y = -jumpForce;
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
    }

    void Flip(bool IsFacingRight) {
        if(IsFacingRight && this.Scale.X == -1) {
            wasFlipped = false;
        } else if(!IsFacingRight && this.Scale.X == 1) {
            wasFlipped = false;
        } else if(!IsFacingRight && this.Scale.X == -1) {
            wasFlipped = true;
        } else if(IsFacingRight && this.Scale.X == 1) {
            wasFlipped = true;
        }
    }

    public void RemoveHp(float hp) {
        if(hp > 0f && hp <= 100f) {
            currentHealth -= hp;
        } 
    }

    public void Die() {
        currentHealth = 0f;
    }

    void CrouchCollider(bool IsCrouched) {
        if(IsCrouched) {
            RectangleShape2D shape = (RectangleShape2D) collider.Shape;
            shape.Size = new Vector2(colliderSize.X, (colliderSize.Y / 2));
            collider.Position = new Vector2(0, (colliderSize.Y / 2));
        } else {
            RectangleShape2D shape = (RectangleShape2D) collider.Shape;
            shape.Size = colliderSize;
            collider.Position = new Vector2(0, 0);
        }
    }
}
