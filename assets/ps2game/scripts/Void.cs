using Godot;
using System;

public class Void : Area2D
{
    [Export] private NodePath playerNode;

    public override void _Process(float delta)
    {
        this.Position = new Vector2(GetNode<KinematicBody2D>(playerNode).Position.x, this.Position.y);
    }
}
