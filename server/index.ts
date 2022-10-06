import dotenv from 'dotenv';
dotenv.config();

import http from 'http';
import express from 'express';
import basicAuth from 'express-basic-auth';
import { PORT } from './app/env';
import { Server, LocalPresence, matchMaker } from 'colyseus';
import { GameRoom } from './app/game-room';
import cors from 'cors';
import { monitor } from '@colyseus/monitor';

const corsOptions = {
  allowedHeaders: 'Content-Type,Authorization,X-Total-Count',
  exposedHeaders: 'X-Total-Count',
};

const app = express();

app.use(cors(corsOptions));

const basicAuthMiddleware = basicAuth({
  users: { admin: 'yoursupersecretpassword' },
  challenge: true,
});

const server = http.createServer(app);
const presence = new LocalPresence();

const gameServer = new Server({
  server,
  express: app,
  presence,
  pingInterval: 3000,
  pingMaxRetries: 2,
  verifyClient: (info, next) => {
    next(true); // accept everyone for now
  },
});

gameServer.define('game', GameRoom).filterBy(['metadata.joinCode']);

gameServer.onShutdown(() => {
  console.log('...server has shutdown gracefully.');
});

const roomhelper = express.Router();
roomhelper.use(express.json());
roomhelper.get('/', async (req, res) => {
  const joinCode = req.query.joinCode ?? null;
  const foundRoomId = await findRoomByJoinCode(joinCode);
  res.json({ roomId: foundRoomId });
});

app.use('/roomhelper', roomhelper);
app.use('/monitor', basicAuthMiddleware, monitor());

gameServer.listen(PORT).then(() => {
  console.log(`Ready on port ${PORT}`);
});

// locate room by join code helper

async function findRoomByJoinCode(joinCode: string) {
  // brute forcing this for now so we don't have to use MongoDB
  // const rooms = await matchMaker.query({ 'metadata.joinCode': joinCode });
  const rooms = await matchMaker.query({});

  for (const room of rooms) {
    if (room.metadata.joinCode === joinCode) return room.roomId;
  }

  return null;
}
