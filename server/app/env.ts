export const DEV_MODE = process.env.NODE_ENV === 'development';
export const PORT = parseInt(process.env.PORT ?? '2567');
export const MAX_PLAYERS: number = 2;
export const MIN_PLAYERS: number = 2;
