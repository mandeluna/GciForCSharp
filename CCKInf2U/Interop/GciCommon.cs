namespace CCKInf2U.Interop;

/// <summary>
/// gcicmn.ht
/// </summary>
internal static unsafe class GciCommon
{
	public struct GciStoreTravDoArgsSType
	{
		// TODO(AB): This union is... An ouch. Deal with it later. Won't be traversing for a while anyway.
	}

	public struct GciClampedTravArgsSType
	{
		public OopType clampSpec;
		public OopType resultOop; /* Result of GciPerformTrav/GciExecuteStrTrav */
		public GciTravBufType* travBuff;
		public int level;
		public int retrievalFlags;
		public BoolType isRpc; /* private, for use by implementation of GCI */

		// TODO(AB): Constructor if necessary...
		/*
			GciClampedTravArgsSType() {
			clampSpec = OOP_NIL;
			resultOop = OOP_NIL;
			travBuff = NULL;
			level = 0;
			retrievalFlags = 0;
			isRpc = 1;
			}
		 */
	}

	public struct GciTravBufType
	{
		public uint allocatedBytes;
		public uint usedBytes;
		public fixed ByteType body[8];
	}
}
