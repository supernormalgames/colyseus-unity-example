import { Client, Delayed, matchMaker, Room } from 'colyseus';
import { MAX_PLAYERS, MIN_PLAYERS } from './env';
import { GameProtocol, Turn } from './game-protocol';
import { GameState, PlayState } from './game-state';
import { Player } from './player';
import { delay, getRandomDigitString, minutesSince } from './util';

export class GameRoom extends Room<GameState> {
  maxClients = MAX_PLAYERS;
  idlePlayerInterval: Delayed | null = null;

  async onCreate(options: any) {
    this.setState(new GameState());

    const joinCode = await this.findAvailableRoomCode();

    if (joinCode === null) throw new Error('room create failure');

    this.setMetadata({ joinCode });
    this.setPrivate(options.private === true);

    this.onMessage(GameProtocol.PLACE_TOKEN, (client, move: Turn) => {
      const player = this.state.playerWithClient(client);
      player.ping();

      if (player.team !== this.state.teamTurn) return;
      if (this.state.resolving) return;
      if (this.state.playState !== PlayState.playing) return;

      const result = this.state.tryPlaceToken(player.team, move.tokenType, move.x, move.y);

      if (result.success) {
        this.resolveBoard(move.x, move.y);
      } else {
        client.send(GameProtocol.MESSAGE, result.message);
      }
    });

    this.onMessage(GameProtocol.PASS, (client) => {
      const player = this.state.playerWithClient(client);
      player.ping();
      this.pass(player);
    });

    this.onMessage(GameProtocol.RESIGN, (client) => {
      const player = this.state.playerWithClient(client);
      player.ping();
      this.resign(player);
    });

    this.onMessage(GameProtocol.REMATCH, (client) => {
      if (this.state.playState === PlayState.endgame) {
        this.newGame();
      }
    });

    this.onMessage(GameProtocol.CHAT, (client, message) => {
      const player = this.state.playerWithClient(client);
      player.ping();
      this.broadcast(GameProtocol.CHAT, { from: client.sessionId, message });
    });

    this.autoTerminateIdlePlayers(10000);
  }

  onJoin(client: Client, options: any) {
    if (!options.name || options.name.legnth === 0) {
      options.name = `player ${getRandomDigitString(4)}`;
    }

    this.state.addPlayer(client, options.name);
    this.checkQuorum();

    // TODO: add to schema instead
    // Implemented this way for backwards compatibility with older clients
    this.clock.setTimeout(() => {
      client.send(GameProtocol.JOIN_CODE, this.metadata.joinCode);
    }, 500);
  }

  onLeave(client: Client, consented: boolean) {
    const player: Player = this.state.playerWithClient(client);
    this.state.removePlayer(client);
    this.checkQuorum();
  }

  startGame() {
    this.state.playState = PlayState.playing;
    this.state.nextTurn();
  }

  newGame() {
    this.state.newGame();
    this.startGame();
  }

  async resolveBoard(x: number, y: number) {
    this.state.resolving = true;

    await delay(400, this.clock);

    const capturedSomething = await this.state.resolveCapturesFrom(x, y, this.state.teamTurn, this.clock);

    if (capturedSomething) {
      this.broadcast(GameProtocol.CAPTURE, 1);
    }

    if (this.state.playState === PlayState.playing) {
      this.state.nextTurn();
    }

    this.state.resolving = false;
  }

  async pass(player: Player) {
    this.state.resolving = true;
    this.broadcast(GameProtocol.MESSAGE, `${player.name} passed`);

    await delay(1000, this.clock);

    this.state.incrementPass();

    if (this.state.playState === PlayState.playing) {
      this.state.nextTurn();
    }

    this.state.resolving = false;
  }

  async resign(player: Player) {
    this.state.resolving = true;
    this.broadcast(GameProtocol.MESSAGE, `${player.name} resigned!`);

    await delay(1000, this.clock);

    this.state.resignPlayer(player);

    if (this.state.playState === PlayState.playing) {
      this.state.nextTurn();
    }

    this.state.resolving = false;
  }

  autoTerminateIdlePlayers(checkInterval: number) {
    this.idlePlayerInterval = this.clock.setInterval(() => {
      this.state.players.forEach((player: Player) => {
        if (minutesSince(player.lastInputAt) > 5) {
          player.client.leave();
        }
      });
    }, checkInterval);
  }

  onDispose() {}

  checkQuorum() {
    const playerCount = this.state.playerCount();
    const currentPlayState = this.state.playState;
    const quorum = playerCount >= MIN_PLAYERS;

    if (currentPlayState === PlayState.waiting && quorum) {
      this.startGame();
    } else if ((currentPlayState === PlayState.playing || currentPlayState === PlayState.endgame) && !quorum) {
      this.newGame();
      this.state.playState = PlayState.waiting;
    }
  }

  async findAvailableRoomCode(): Promise<string | null> {
    const maxAttempts = 100;

    for (let i = 0; i < maxAttempts; i++) {
      const joinCode = getRandomDigitString(4);
      const rooms = await matchMaker.query({ joinCode });

      if (rooms.length === 0) {
        return joinCode;
      }
    }

    return null;
  }
}
