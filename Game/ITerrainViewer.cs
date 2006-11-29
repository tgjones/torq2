using System;
using Microsoft.Xna.Framework;

namespace Torq2
{
	public interface ITerrainViewer
	{
		Vector3 Position
		{
			get;
		}

		Vector2 Position2D
		{
			get;
		}
	}
}
