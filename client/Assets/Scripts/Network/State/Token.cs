// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.9
// 

using Colyseus.Schema;

namespace state {
	public partial class Token : Schema {
		[Type(0, "string")]
		public string id = default(string);

		[Type(1, "uint8")]
		public uint team = default(uint);

		[Type(2, "uint8")]
		public uint tokenType = default(uint);

		[Type(3, "uint8")]
		public uint x = default(uint);

		[Type(4, "uint8")]
		public uint y = default(uint);

		[Type(5, "boolean")]
		public bool revealed = default(bool);
	}
}
