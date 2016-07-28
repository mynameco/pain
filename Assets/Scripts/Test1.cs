using UnityEngine;

public class Test1 : MonoBehaviour
{
	public enum UnitState
	{
		None,
		Seek,
		Move,
		Attack
	}

	public struct Cell
	{
		public bool Passability;
	}

	public struct Unit
	{
		public int Hp;
		public UnitState State;
		public int time;
	}

	private void Awake()
	{
		for (int x = 0; x < width; x++)
		{
			for (int y = 0; y < height; y++)
			{
				var pass = true;// random.NextDouble() < 0.7f;
				field[x, y] = new MyPathNode();
				field[x, y].IsWall = !pass;
			}
		}

		for (int index = 0; index < (maxUnits / 2); index++)
		{
			InjectUnit();
		}

		solver = new MySolver<MyPathNode, object>(field);
	}

	private void Update()
	{
		ComputeInjectUnits();
		ComputeSimulation();
	}

	private void ComputeInjectUnits()
	{
		if (unitCount < maxUnits)
		{
			if (random.NextDouble() < 0.5f)
			{
				InjectUnit();
				//Debug.LogWarning("Inject");
			}
		}
	}

	private void ComputeSimulation()
	{
		for (int index = 0; index < unitCount; index++)
		{
			ComputeUnit(index);
		}

		for (int index = 0; index < unitCount;)
		{
			if (units[index].Hp <= 0)
			{
				DestroyUnit(index);
			}
			else
			{
				index++;
			}
		}
	}

	private void InjectUnit()
	{
		if (unitCount >= maxUnits)
			return;

		var unit = new Unit();
		unit.Hp = random.Next(100, 200);

		units[unitCount] = unit;
		unitCount++;
	}

	private void DestroyUnit(int id)
	{
		if (id <= 0 || id >= maxUnits)
			return;

		units[id] = units[unitCount - 1];
		unitCount--;

		//Debug.LogWarning("Destroy : " + id);
	}

	private void ComputeUnit(int id)
	{
		if (units[id].time >= maxStateTime)
		{
			units[id].time = 0;
			units[id].State = (UnitState)random.Next((int)UnitState.None, (int)UnitState.Attack + 1);
		}
		else
		{
			units[id].time++;
		}

		if (units[id].State == UnitState.None)
		{
		}
		else if (units[id].State == UnitState.Seek)
		{
			if (units[id].time == 0)
			{
				GetPath();
			}
		}
		else if (units[id].State == UnitState.Move)
		{
		}
		else if (units[id].State == UnitState.Attack)
		{
			var targetId = random.Next(0, unitCount);
			if (targetId != id)
			{
				if ((units[id].time % 2) != 0)
				{
					units[targetId].Hp--;
					//Debug.LogWarning("Attack : " + targetId);
				}
			}
		}
	}

	private void GetPath()
	{
		var distance = 20;
		var x = random.Next(0, width - distance);
		var y = random.Next(0, height - distance);
		var path = solver.Search(x, y, x + distance - 2, y + distance - 2, null);
	}

	private MyPathNode[,] field = new MyPathNode[width, height];
	private Unit[] units = new Unit[maxUnits];
	[Inspectable]
	private int unitCount;

	private const int width = 100;
	private const int height = 100;
	private const int maxUnits = 1000;
	private const int maxStateTime = 10;


	private System.Random random = new System.Random();
	private MySolver<MyPathNode, object> solver;
}
