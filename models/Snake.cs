using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Media.Effects;
using SnakeGame.models;
using System.Xml.Linq;

public class Snake
{
    private List<Position> body = new List<Position>();
    public IReadOnlyList<Position> Body => body.AsReadOnly();
    public Position Head => body[0];
    public Direction CurrentDirection { get; private set; }
    private Direction lastMovedDirection;

    public Snake(Position startPosition)
    {
        body.Add(startPosition);
        CurrentDirection = Direction.Right;
        lastMovedDirection = Direction.Right;
    }

    public void ChangeDirection(Direction newDirection)
    {
        // Prevent 180-degree turns based on last moved direction
        if ((lastMovedDirection == Direction.Up && newDirection == Direction.Down) ||
            (lastMovedDirection == Direction.Down && newDirection == Direction.Up) ||
            (lastMovedDirection == Direction.Left && newDirection == Direction.Right) ||
            (lastMovedDirection == Direction.Right && newDirection == Direction.Left))
            return;

        CurrentDirection = newDirection;
    }

    public Position GetNextHeadPosition()
    {
        Position newHead = new Position(Head.X, Head.Y);
        switch (CurrentDirection)
        {
            case Direction.Up:
                newHead.Y--;
                break;
            case Direction.Down:
                newHead.Y++;
                break;
            case Direction.Left:
                newHead.X--;
                break;
            case Direction.Right:
                newHead.X++;
                break;
        }
        return newHead;
    }

    public void Move(bool grow)
    {
        Position newHead = GetNextHeadPosition();
        body.Insert(0, newHead);
        if (!grow)
            body.RemoveAt(body.Count - 1);
        lastMovedDirection = CurrentDirection;
    }

    public bool CollidesWith(Position position)
    {
        // Check collision with all body segments except the head
        for (int i = 1; i < body.Count; i++)
        {
            if (Math.Abs(body[i].X - position.X) < 0.5 &&
                Math.Abs(body[i].Y - position.Y) < 0.5)
            {
                return true;
            }
        }
        return false;
    }
}


