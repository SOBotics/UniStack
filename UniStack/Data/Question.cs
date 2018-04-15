using System.Runtime.InteropServices;

#region question data structure

// int id

// byte tagCount
// int tag1
// ...

// short termCount
// int term1Val
// byte term1Freq
// ...

#endregion

namespace UniStack.Data
{
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public unsafe struct Question
	{
		private byte* data;

		public int Id => GetInt(0);

		public byte TagCount => data[4];

		public int GetTag(int index) => GetInt(5 + (index * 4));

		public short TermCount =>
			(short)(
				(data[
					4 +            // Id
					1 +            // Tag count
					(4 * TagCount) // Tags
				] << 8) ^
				data[
					4 +              // Id
					1 +              // Tag count
					(4 * TagCount) + // Tags
					1                // Offset
				]
			);

		public int GetTermValue(int index) => 
			GetInt(
				4 +              // Id
				1 +              // Tag count
				(TagCount * 4) + // Tags
				2 +              // Term count
				(index * 5)      // Terms
			);

		public byte GetTermFrequency(int index) =>
			data[
				4 +              // Id
				1 +              // Tag count
				(TagCount * 4) + // Tags
				2 +              // Term count
				(index * 5) +    // Terms
				4                //Term value
			];



		internal Question(byte* data) => this.data = data;



		private int GetInt(int offset) =>
			data[offset]     << 24 ^
			data[offset + 1] << 16 ^
			data[offset + 2] <<  8 ^
			data[offset + 3];
	}
}