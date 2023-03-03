using Godot;
using System;

public partial class Void : Area2D
{
    [Export] private NodePath playerNode;

    public override void _Process(double delta)
    {
        this.Position = new Vector2(GetNode<CharacterBody2D>(playerNode).Position.X, this.Position.Y);
    }
}
