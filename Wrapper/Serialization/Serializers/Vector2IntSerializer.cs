using UnityEngine;

namespace SqlLite.Wrapper.Serialization
{
	internal class Vector2IntSerializer : SqlSerializer<Vector2Int, string>
	{
		protected override Vector2Int Deserialize(string value)
		{
			string[] split = value.Split(',');
			int Parse(int i) => int.TryParse(split[i], out int v) ? v : 0;
			return new Vector2Int(Parse(0), Parse(1));
		}

		protected override string Serialize(Vector2Int input)
		{
			return $"{input.x},{input.y}";
		}
	}
}
