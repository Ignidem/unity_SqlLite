using UnityEngine;
using Utils.Serializers.CustomSerializers;

namespace SqlLite.Wrapper.Serialization
{
	public class Vector2IntSerializer : Serializer<Vector2Int, string>
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

	public class Vector3IntSerializer : Serializer<Vector3Int, string>
	{
		protected override Vector3Int Deserialize(string value)
		{
			string[] split = value.Split(',');
			int Parse(int i) => int.TryParse(split[i], out int v) ? v : 0;
			return new Vector3Int(Parse(0), Parse(1), Parse(2));
		}

		protected override string Serialize(Vector3Int input)
		{
			return $"{input.x},{input.y},{input.z}";
		}
	}

	public class Vector2Serializer : Serializer<Vector2, string>
	{
		protected override Vector2 Deserialize(string value)
		{
			string[] split = value.Split(',');
			float Parse(int i) => float.TryParse(split[i], out float v) ? v : 0;
			return new Vector2(Parse(0), Parse(1));
		}

		protected override string Serialize(Vector2 input)
		{
			return $"{input.x},{input.y}";
		}
	}

	public class Vector3Serializer : Serializer<Vector3, string>
	{
		protected override Vector3 Deserialize(string value)
		{
			string[] split = value.Split(',');
			float Parse(int i) => float.TryParse(split[i], out float v) ? v : 0;
			return new Vector3(Parse(0), Parse(1), Parse(2));
		}

		protected override string Serialize(Vector3 input)
		{
			return $"{input.x},{input.y},{input.z}";
		}
	}
}
