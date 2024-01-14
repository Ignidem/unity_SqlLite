namespace SqlLite.Wrapper.Serialization
{
	public class BoolSerializer : SqlSerializer<bool, int>
	{
		protected override bool Deserialize(int value)
		{
			return value != 0;
		}

		protected override int Serialize(bool input)
		{
			return input ? 1 : 0;
		}
	}
}

