// 
// THIS FILE HAS BEEN GENERATED AUTOMATICALLY
// DO NOT CHANGE IT MANUALLY UNLESS YOU KNOW WHAT YOU'RE DOING
// 
// GENERATED USING @colyseus/schema 1.0.9
// 

using Colyseus.Schema;

namespace state {
	public partial class GameState : Schema {
		[Type(0, "map", typeof(MapSchema<Player>))]
		public MapSchema<Player> players = new MapSchema<Player>();

		[Type(1, "string")]
		public string playState = default(string);

		[Type(2, "boolean")]
		public bool resolving = default(bool);

		[Type(3, "map", typeof(MapSchema<Token>))]
		public MapSchema<Token> tokens = new MapSchema<Token>();

		[Type(4, "uint8")]
		public uint teamTurn = default(uint);

		[Type(5, "uint8")]
		public uint boardWidth = default(uint);

		[Type(6, "uint8")]
		public uint boardHeight = default(uint);

		[Type(7, "uint8")]
		public uint passCount = default(uint);
	}
}
