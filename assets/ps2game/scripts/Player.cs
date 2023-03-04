using Godot;
using System;

public partial class Player : CharacterBody2D {
    [ExportCategory("Player")]
    [ExportGroup("Movement")]
    [Export] private bool airControl = true;
    [Export] private float walkSpeed = 560f;
    [Export] private float runSpeed = 760f;
    [Export] private float jumpForce = 250f;
    [Export] private float wallJumpFroce = 150f;
    [Export] private float crouchSpeed = 50f;
    
    [ExportGroup("Physics")]
    [Export] private float wallSlideGravity = 1f;
    [Export] private float gravity = 4.5f;
    [Export] private CollisionShape2D collider;
    [Export] private RayCast2D wallCheck;
    [Export] private RayCast2D ceilingCheck;

    [ExportGroup("Health")]
    [Export] private bool isDead = false;
    [Export] private float currentHealth = 100f;

    private bool isFacingRight = true;
    private Vector2 velocity;
    private Vector2 colliderSize;
    private bool canWallJump = false;

    public override void _Ready() {
        RectangleShape2D shape = (RectangleShape2D) collider.Shape;
        colliderSize = shape.Size;
    }

    public override void _Process(double delta) {
        isDead = currentHealth < 0;
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
        if(GetAxis(moveConditionRight, moveConditionLeft, 1) > 0 && !isFacingRight) {
            Flip();
        } else if(GetAxis(moveConditionRight, moveConditionLeft, 1) < 0 && isFacingRight) {
            Flip();
        }

        if(!sprintCondition && !crouchCondition) {
            velocity.X = !airControl && !IsOnFloor ? (GetAxis(moveConditionRight, moveConditionLeft, walkSpeed) / 2) : GetAxis(moveConditionRight, moveConditionLeft, walkSpeed);
        } else if(sprintCondition && !crouchCondition) {
            velocity.X = !airControl && !IsOnFloor ? (GetAxis(moveConditionRight, moveConditionLeft, runSpeed) / 2) : GetAxis(moveConditionRight, moveConditionLeft, runSpeed);
        } else if(!sprintCondition && crouchCondition) {
            velocity.X = !airControl && !IsOnFloor ? (GetAxis(moveConditionRight, moveConditionLeft, crouchSpeed) / 2) : GetAxis(moveConditionRight, moveConditionLeft, crouchSpeed);
        } else {
            velocity.X = 0;
        }

        if(crouchCondition) {
            CrouchCollider(true);
        } else {
            CrouchCollider(false);
        }
    }

    void Flip() {
        isFacingRight = !isFacingRight;
        Vector2 scale = this.Scale;
        scale.X *= -1;
        this.Scale = scale;
    }

    float GetAxis(bool positive, bool negative, float speed) {
        float axis;
        if(positive) {
            axis = speed;
        } else if(negative) {
            axis = -speed;
        } else {
            axis = 0;
        }

        return axis;
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
