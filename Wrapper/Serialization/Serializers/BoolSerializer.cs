using System.Collections;
using System.Collections.Generic;

namespace SqlLite.Wrapper.Serialization
{
	public class BoolListSerializer : SqlSerializer<IList<bool>, byte[]>
	{
		protected override byte[] Serialize(IList<bool> input)
		{
			BitArray bits = new BitArray(input.Count);
			for (int i = 0; i < input.Count; i++)
			{
				bits[i] = input[i];
			}

			byte[] byteArray = new byte[1];
			bits.CopyTo(byteArray, 0);
			return byteArray;
		}

		protected override IList<bool> Deserialize(byte[] value)
		{
			BitArray bits = new BitArray(value);
			bool[] bools = new bool[bits.Length];
			for (int i = 0; i < bits.Length; i++)
			{
				bools[i] = bits[i];
			}

			return bools;
		}
	}

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

