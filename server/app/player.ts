import { Schema, type } from '@colyseus/schema';
import { Client } from 'colyseus';

export class Player extends Schema {
  @type('string')
  sessionId: string;

  @type('string')
  name: string;

  @type('uint8')
  team: number;

  @type('uint16')
  score: number;

  @type('boolean')
  winner: boolean;

  lastInputAt: Date;
  client: Client;

  init(client: Client, name: string, team: number) {
    this.sessionId = client.sessionId;
    this.client = client;
    this.name = name;
    this.team = team;
    this.score = 0;
    this.winner = false;

    this.ping();
  }

  ping() {
    this.lastInputAt = new Date();
  }
}
