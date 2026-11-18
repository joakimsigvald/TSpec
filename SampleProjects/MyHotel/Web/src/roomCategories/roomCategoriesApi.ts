import { get, post, put, remove } from '../http';

export type RoomCategory = { name: string; description: string };

export type RoomCategoryOverview = RoomCategory & { hasRooms: boolean };

const roomCategoryPath = (name: string) => `/room-categories/${encodeURIComponent(name)}`;

export const listRoomCategories = () => get<RoomCategoryOverview[]>('/room-categories');

export const addRoomCategory = (roomCategory: RoomCategory) =>
  post<RoomCategoryOverview>('/room-categories', roomCategory);

export const updateRoomCategory = (roomCategory: RoomCategory) =>
  put<RoomCategoryOverview>(roomCategoryPath(roomCategory.name), roomCategory);

export const deleteRoomCategory = (name: string) => remove(roomCategoryPath(name));
