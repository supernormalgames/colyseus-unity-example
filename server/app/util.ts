import { Clock } from 'colyseus';

export const MAX_DATE = new Date(8640000000000000);
export const MIN_DATE = new Date(-8640000000000000);
export const GUID_ALPHABET = '0123456789abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ';

export function decide(chance: number): boolean {
  return Math.random() * 100 < chance;
}

export function shuffleArray(a: Array<any>): Array<any> {
  for (let i = a.length - 1; i > 0; i--) {
    const j = Math.floor(Math.random() * (i + 1));
    [a[i], a[j]] = [a[j], a[i]];
  }
  return a;
}

export function sample(arr: Array<any>) {
  if (!arr) return null;
  return arr[Math.floor(Math.random() * arr.length)];
}

export function minutesBetween(date: Date, other: Date): number {
  const diffMs = other.getTime() - date.getTime();
  return Math.floor(diffMs / 1000 / 60);
}

export function dateWithMinutesFromNow(minutes: number): Date {
  const now = new Date().getTime();
  return new Date(now + minutes * 60000);
}

export function minutesSince(date: Date): number {
  return minutesBetween(date, new Date());
}

export function getRandomInt(min, max) {
  min = Math.ceil(min);
  max = Math.floor(max);
  return Math.floor(Math.random() * (max - min + 1)) + min;
}

export function getRandomDigit() {
  return getRandomInt(0, 9);
}

export function getRandomDigitString(length: number): string {
  let value = '';

  for (let i = 0; i < length; i++) {
    value += getRandomDigit();
  }

  return value;
}

export function flatMap(ary, lambda) {
  return Array.prototype.concat.apply([], ary.map(lambda));
}

export function unique(ary: Array<any>) {
  function onlyUnique(value, index, self) {
    return self.indexOf(value) === index;
  }

  return ary.filter(onlyUnique);
}

export async function delay(ms: number, clock: Clock) {
  return new Promise((resolve) => clock.setTimeout(resolve, ms));
}

export function hoursAndMinutes(minutes: number): string {
  const hours = minutes / 60;
  const displayHours = Math.floor(hours);
  const displayMinutes = Math.round((hours - displayHours) * 60);

  let formattedHours: string | null = null;
  let formattedMinutes: string | null = null;

  if (displayHours > 0) {
    formattedHours = displayHours + ' hr' + (displayHours > 1 ? 's' : '');
  }

  if (displayMinutes > 0) {
    formattedMinutes = displayMinutes + ' min' + (displayMinutes > 1 ? 's' : '');
  }

  if (formattedHours && formattedMinutes) {
    return `${formattedHours} ${formattedMinutes}`;
  } else if (formattedHours) {
    return formattedHours;
  } else if (formattedMinutes) {
    return formattedMinutes;
  } else {
    return '0 mins';
  }
}
