using System;
using SettlersEngine;

public struct MyPathNode : IPathNode<object>
{
	public Int32 X { get; set; }
	public Int32 Y { get; set; }
	public Boolean IsWall { get; set; }

	public bool IsWalkable(Object unused)
	{
		return !IsWall;
	}
}

public class MySolver<TPathNode, TUserContext> : SpatialAStar<TPathNode, TUserContext> where TPathNode : IPathNode<TUserContext>
{
	protected override Double Heuristic(PathNode inStart, PathNode inEnd)
	{
		return Math.Abs(inStart.X - inEnd.X) + Math.Abs(inStart.Y - inEnd.Y);
	}

	protected override Double NeighborDistance(PathNode inStart, PathNode inEnd)
	{
		return Heuristic(inStart, inEnd);
	}

	public MySolver(TPathNode[,] inGrid)
		: base(inGrid)
	{
	}
}