import { Client, Clock } from 'colyseus';
import { Schema, MapSchema, type } from '@colyseus/schema';
import { Player } from './player';
import { MAX_PLAYERS } from './env';
import { nanoid } from 'nanoid';
import { CellInfo, TurnResult } from './game-protocol';
import { delay, flatMap, unique } from './util';

const EMPTY = -1;

export const NOONE = 99;

export enum PlayState {
  waiting = 'waiting',
  playing = 'playing',
  endgame = 'endgame',
}

export class Token extends Schema {
  @type('string')
  id: string;

  @type('uint8')
  team: number;

  @type('uint8')
  tokenType: number;

  @type('uint8')
  x: number;

  @type('uint8')
  y: number;

  @type('boolean')
  revealed: boolean;
}

export class GameState extends Schema {
  @type({ map: Player })
  players: MapSchema<Player>;

  @type('string')
  playState: PlayState;

  @type('boolean')
  resolving: boolean;

  @type({ map: Token })
  tokens: MapSchema<Token>;

  @type('uint8')
  teamTurn: number;

  @type('uint8')
  boardWidth: number;

  @type('uint8')
  boardHeight: number;

  @type('uint8')
  passCount: number;

  cells: Array<Array<CellInfo>>;

  constructor() {
    super();
    this.players = new MapSchema<Player>();
    this.tokens = new MapSchema<Token>();
    this.boardWidth = 5;
    this.boardHeight = 5;

    this.reset();
  }

  reset() {
    this.playState = PlayState.waiting;
    this.teamTurn = NOONE;
    this.passCount = 0;
    this.removeAllTokens();

    this.cells = new Array<Array<CellInfo>>(this.boardHeight);

    for (let x = 0; x < this.boardWidth; x++) {
      this.cells[x] = new Array<CellInfo>(this.boardWidth);

      for (let y = 0; y < this.boardHeight; y++) {
        this.cells[x][y] = {
          x,
          y,
          team: -1,
          tokenType: EMPTY,
          newlyPlayed: false,
        };
      }
    }
  }

  newGame() {
    this.reset();
    this.players.forEach((player) => {
      player.score = 0;
      player.winner = false;
    });
  }

  addPlayer(client: Client, name: string): Player {
    const player = new Player();
    const availableTeam = this.findAvailableTeam();

    player.init(client, name, availableTeam);

    this.players[client.id] = player;

    return player;
  }

  removePlayer(client: Client) {
    delete this.players[client.sessionId];
  }

  allPlayers(): Array<Player> {
    return Array.from(this.players.values());
  }

  allTokens(): Array<Token> {
    return Array.from(this.tokens.values());
  }

  playerCount(): number {
    return this.players.size;
  }

  playerWithTeam(team: number): Player | null {
    return this.allPlayers().find((player) => player.team === team) ?? null;
  }

  playerWithClient(client: Client): Player {
    return this.players[client.sessionId];
  }

  createToken(team: number, tokenType: number, x: number, y: number) {
    const token = new Token();
    token.id = nanoid(6);
    token.team = team;
    token.tokenType = tokenType;
    token.x = x;
    token.y = y;
    token.revealed = false;

    this.tokens.set(token.id, token);
    this.cells[x][y] = {
      x,
      y,
      team,
      tokenType,
      newlyPlayed: true,
    };

    return token;
  }

  removeTokenAt(x: number, y: number) {
    const cell = this.getCell(x, y);

    if (!cell) return;

    cell.team = EMPTY;
    cell.tokenType = EMPTY;
    cell.newlyPlayed = false;

    const tokenAtThisPosition = this.allTokens().find((token) => token.x === x && token.y === y);

    if (tokenAtThisPosition) {
      this.tokens.delete(tokenAtThisPosition.id);
    }
  }

  removeTokenWithId(id: string) {
    const token = this.tokens.get(id);

    if (token) {
      this.tokens.delete(id);
    }
  }

  removeAllTokens() {
    const allTokens = this.allTokens();

    for (const token of allTokens) {
      this.tokens.delete(token.id);
    }
  }

  getNeighbors(x: number, y: number): Array<CellInfo> {
    const neighbors = new Array<CellInfo>();

    const up = this.getCell(x, y + 1);
    if (up) neighbors.push(up);

    const right = this.getCell(x + 1, y);
    if (right) neighbors.push(right);

    const down = this.getCell(x, y - 1);
    if (down) neighbors.push(down);

    const left = this.getCell(x - 1, y);
    if (left) neighbors.push(left);

    return neighbors;
  }

  getLiberties(x: number, y: number) {
    const group: Array<CellInfo> = this.groupAt(x, y);
    const emptyCells = flatMap(group, (groupCell) => {
      return this.getNeighbors(groupCell.x, groupCell.y).filter(
        (neighbor) => neighbor.team === EMPTY || neighbor.newlyPlayed
      );
    });

    return unique(emptyCells).length;
  }

  groupAt(x: number, y: number) {
    const startingCell = this.getCell(x, y);

    if (startingCell !== null) {
      const [group, _] = this.partitionTraverse(startingCell, (neighbor: CellInfo) => {
        return neighbor.team === startingCell.team;
      });

      return group;
    } else {
      return [];
    }
  }

  partitionTraverse(startingCell: CellInfo, inclusionCondition) {
    const checkedCells: Array<CellInfo> = [];
    const boundaryCells: Array<CellInfo> = [];
    const cellsToCheck: Array<CellInfo> = [];

    cellsToCheck.push(startingCell);

    while (cellsToCheck.length > 0) {
      const cell = cellsToCheck.pop()!;

      if (cellsToCheck.indexOf(cell) > -1) {
        // skip it, we already checked
      } else {
        checkedCells.push(cell);

        this.getNeighbors(cell.x, cell.y).forEach((neighbor) => {
          if (checkedCells.indexOf(neighbor) > -1) {
            // skip this neighbor, we already checked it
          } else {
            if (inclusionCondition(neighbor)) {
              cellsToCheck.push(neighbor);
            } else {
              boundaryCells.push(neighbor);
            }
          }
        });
      }
    }

    return [checkedCells, unique(boundaryCells)];
  }

  getCell(x: number, y: number): CellInfo | null {
    if (x < 0 || x >= this.boardWidth || y < 0 || y >= this.boardHeight) return null;
    return this.cells[x][y];
  }

  allSpacesTaken(): boolean {
    for (let x = 0; x < this.boardWidth; x++) {
      for (let y = 0; y < this.boardHeight; y++) {
        if (this.cells[x][y].team === EMPTY) {
          return false;
        }
      }
    }

    return true;
  }

  tryPlaceToken(team: number, tokenType: number, x: number, y: number): TurnResult {
    // Check bounds and if taken
    const targetCell = this.getCell(x, y);

    if (targetCell === null || targetCell.team !== -1) {
      return { success: false, message: 'not available.', id: null };
    }

    // Check suicide rule
    if (this.wouldBeSuicide(x, y, team)) {
      return { success: false, message: 'that would be suicide!', id: null };
    }

    const token = this.createToken(team, tokenType, x, y);

    this.passCount = 0;

    return { success: true, message: '', id: token.id };
  }

  incrementPass() {
    this.passCount++;
    this.checkForEndgame();
  }

  resignPlayer(player: Player) {
    const otherPlayer = this.allPlayers().find((p) => p.sessionId !== player.sessionId) ?? null;

    if (otherPlayer === null) {
      // if no one else is left I gues the resigning player wins
      player.winner = true;
    } else {
      otherPlayer.winner = true;
    }

    this.endGame();
  }

  async resolveCapturesFrom(x: number, y: number, team: number, clock: Clock): Promise<boolean> {
    await delay(100, clock);

    const playedAt = this.getCell(x, y);
    let capturedSomething = false;

    if (!playedAt) return false;

    // checking for "1" because newly placed token won't be detected
    const capturedNeighbors = this.getNeighbors(x, y).filter((cell) => {
      return !cell.newlyPlayed && cell.team !== EMPTY && cell.team !== team && this.getLiberties(cell.x, cell.y) === 1;
    });

    playedAt.newlyPlayed = false;

    const capturedCells = flatMap(capturedNeighbors, (cell) => this.groupAt(cell.x, cell.y));

    capturedCells.forEach((cell) => {
      capturedSomething = true;
      this.removeTokenAt(cell.x, cell.y);
    });

    this.checkForEndgame();

    return capturedSomething;
  }

  inAtari(x: number, y: number): boolean {
    return this.getLiberties(x, y) === 1;
  }

  wouldBeSuicide(x: number, y: number, team: number): boolean {
    const cell = this.getCell(x, y);

    if (!cell) return true;

    const surroundedEmptyPoint =
      cell.team === EMPTY && this.getNeighbors(x, y).filter((neighbor) => neighbor.team === EMPTY).length === 0;

    if (!surroundedEmptyPoint) return false;

    const someFriendlyNotInAtari = this.getNeighbors(x, y).some((neighbor) => {
      const inAtari = this.inAtari(neighbor.x, neighbor.y);
      const friendly = neighbor.team === team;

      return friendly && !inAtari;
    });

    if (someFriendlyNotInAtari) return false;

    const someEnemyInAtari = this.getNeighbors(x, y).some((neighbor) => {
      const inAtari = this.inAtari(neighbor.x, neighbor.y);
      const enemy = neighbor.team !== EMPTY && neighbor.team !== team;

      return enemy && inAtari;
    });

    if (someEnemyInAtari) return false;

    return true;
  }

  checkForEndgame() {
    if (this.passCount >= 2) {
      this.endGame();
      return;
    }

    if (this.allSpacesTaken()) {
      this.endGame();
      return;
    }

    // TODO: check for other conditions?
  }

  endGame() {
    this.playState = PlayState.endgame;

    // TODO: score game if someone isn't already a winner (someone resigned)
    let winner = this.allPlayers().find((player) => player.winner) ?? null;

    if (winner === null) {
      let team0Points = 0;
      let team1Points = 0;
      let winningTeam = EMPTY;

      for (let x = 0; x < this.boardWidth; x++) {
        for (let y = 0; y < this.boardHeight; y++) {
          if (this.cells[x][y].team === 0) team0Points++;
          else if (this.cells[x][y].team === 1) team1Points++;
        }
      }

      if (team0Points > team1Points) {
        winningTeam = 0;
      } else if (team1Points > team0Points) {
        winningTeam = 1;
      }

      winner = this.allPlayers().find((player) => player.team === winningTeam) ?? null;

      if (winner) {
        winner.winner = true;
      }
    }
  }

  nextTurn() {
    if (this.teamTurn === NOONE) {
      this.teamTurn = 0;
    } else {
      this.teamTurn += 1;
    }

    if (this.teamTurn >= this.playerCount()) {
      this.teamTurn = 0;
    }
  }

  findAvailableTeam(): number {
    const allPlayers = this.allPlayers();

    for (let i = 0; i < MAX_PLAYERS; i++) {
      const playerWithTeam = allPlayers.find((player) => player.team === i);

      if (!playerWithTeam) return i;
    }

    return 0;
  }
}
