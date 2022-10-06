// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.9
// 

using Colyseus.Schema;

namespace state {
	public partial class Player : Schema {
		[Type(0, "string")]
		public string sessionId = default(string);

		[Type(1, "string")]
		public string name = default(string);

		[Type(2, "uint8")]
		public uint team = default(uint);

		[Type(3, "uint16")]
		public ushort score = default(ushort);

		[Type(4, "boolean")]
		public bool winner = default(bool);
	}
}
