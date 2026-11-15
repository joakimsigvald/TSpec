import { get, post, put, remove } from '../http';

export type Room = { roomNumber: string; bedCount: number };

const roomPath = (roomNumber: string) => `/rooms/${encodeURIComponent(roomNumber)}`;

export const listRooms = () => get<Room[]>('/rooms');

export const addRoom = (room: Room) => post<Room>('/rooms', room);

export const updateRoom = (room: Room) => put<Room>(roomPath(room.roomNumber), room);

export const deleteRoom = (roomNumber: string) => remove(roomPath(roomNumber));
