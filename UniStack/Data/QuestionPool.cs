using System;
using System.Collections.Generic;
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
	public unsafe struct QuestionPool
	{
		private byte* data;
		private long position;
		private int bufferResizeMultiplier;

		public int Count;
		public long RealSize;
		public long Size;

		public QuestionPool(int bufferResizeMultiplier)
		{
			data = default(byte*);
			position = 0;
			this.bufferResizeMultiplier = bufferResizeMultiplier;

			Count = 0;
			RealSize = 0;
			Size = 0;
		}

		public void Add(int id, int[] tags, Dictionary<int, byte> terms)
		{
			// id + tagCount + tags + termCount + terms
			var bytes = 4 + 1 + (tags.Length * 4) + 2 + (terms.Count * 5);

			if (Size + bytes > RealSize)
			{
				var expandBy = bytes * Math.Max(bufferResizeMultiplier, 1);

				RealSize = Size + expandBy;

				if (data == default(byte*))
				{
					data = (byte*)Marshal.AllocHGlobal(expandBy);
				}
				else
				{
					data = (byte*)Marshal.ReAllocHGlobal((IntPtr)data, (IntPtr)RealSize);
				}
			}

			var offset = Size;

			SetInt(id, ref offset);
			data[offset++] = (byte)tags.Length;

			for (var i = 0; i < tags.Length; i++)
			{
				SetInt(tags[i], ref offset);
			}

			data[offset++] = (byte)(terms.Count >> 8);
			data[offset++] = (byte)terms.Count;

			foreach (var kv in terms)
			{
				SetInt(kv.Key, ref offset);
				data[offset++] = kv.Value;
			}

			Size += bytes;
			Count++;
		}

		public void Remove(uint[] indices)
		{
			//TODO: Implement.
			throw new NotImplementedException();
		}

		public bool Next(out Question question, bool fromStart = false)
		{
			if (fromStart)
			{
				position = 0;
			}

			if (position >= Size)
			{
				question = default(Question);

				return false;
			}

			var q = new Question(data + position);

			// id
			position += 4;
			// tag count
			position += 1;
			// tags
			position += 4 * data[position - 1];
			// term count
			position += 2;
			// terms
			position += 5 * (data[position - 2] << 8 ^ data[position - 1]);

			question = q;

			return true;
		}

		public void Destroy() => Marshal.FreeHGlobal((IntPtr)data);



		private void SetInt(int val, ref long offset)
		{
			data[offset++] = (byte)(val >> 24);
			data[offset++] = (byte)(val >> 16);
			data[offset++] = (byte)(val >> 8);
			data[offset++] = (byte)val;
		}
	}
}
