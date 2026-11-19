import { get } from '../http';

export type Rules = { minBedCount: number; maxBedCount: number };

export const getRules = () => get<Rules>('/rules');
