export const MIN_PROTOCOL = 1;

export enum GameProtocol {
  PLACE_TOKEN = 1,
  MESSAGE = 2,
  PASS = 3,
  RESIGN = 4,
  REMATCH = 5,
  CHAT = 6,
  CAPTURE = 7,
  JOIN_CODE = 8,
}

export interface Point {
  x: number;
  y: number;
}

export interface CellInfo {
  x: number;
  y: number;
  team: number;
  tokenType: number;
  newlyPlayed: boolean;
}

export interface Turn {
  tokenType: number;
  x: number;
  y: number;
}

export interface TurnResult {
  id: string | null;
  success: boolean;
  message: string;
}
