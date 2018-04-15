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

		public long DataBufferSize;
		public long CurrentSizeBytes;

		public QuestionPool(int bufferResizeMultiplier)
		{
			data = default(byte*);
			DataBufferSize = 0;
			position = 0;
			this.bufferResizeMultiplier = bufferResizeMultiplier;
			CurrentSizeBytes = 0;
		}

		public void Add(int id, int[] tags, Dictionary<int, byte> terms)
		{
			// id + tagCount + tags + termCount + terms
			var bytes = 4 + 1 + (tags.Length * 4) + 2 + (terms.Count * 5);

			if (CurrentSizeBytes + bytes > DataBufferSize)
			{
				var expandBy = bytes * Math.Max(bufferResizeMultiplier, 1);

				DataBufferSize = CurrentSizeBytes + expandBy;

				if (data == default(byte*))
				{
					data = (byte*)Marshal.AllocHGlobal(expandBy);
				}
				else
				{
					data = (byte*)Marshal.ReAllocHGlobal((IntPtr)data, (IntPtr)DataBufferSize);
				}
			}

			var offset = CurrentSizeBytes;

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

			CurrentSizeBytes += bytes;
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

			if (position >= CurrentSizeBytes)
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
