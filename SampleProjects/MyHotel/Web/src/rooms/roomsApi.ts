import { get, post, put, remove } from '../http';

export type Room = { roomNumber: string; bedCount: number; roomCategory: string };

export type RoomOverview = Room & { hasOpenBookings: boolean };

const roomPath = (roomNumber: string) => `/rooms/${encodeURIComponent(roomNumber)}`;

export const listRooms = () => get<RoomOverview[]>('/rooms');

export const addRoom = (room: Room) => post<RoomOverview>('/rooms', room);

export const updateRoom = (room: Room) => put<RoomOverview>(roomPath(room.roomNumber), room);

export const deleteRoom = (roomNumber: string) => remove(roomPath(roomNumber));
