import store from "./store";
import * as Mutations from "./mutations";
import { ListsService } from "services/listsService";

async function getLists(service: ListsService): Promise<void> {
  store.dispatch(Mutations.setListsLoading, true);

  const lists = await service.getAll();

  await store.dispatch(Mutations.getLists, lists);
  await store.dispatch(Mutations.generateComputedLists);

  return store.dispatch(Mutations.setListsLoading, false);
}

function reorderList(id: number, oldOrder: number, newOrder: number): Promise<void> {
  return store.dispatch(Mutations.reorderList, id, oldOrder, newOrder);
}

async function completeTask(id: number, listId: number): Promise<void> {
  await store.dispatch(Mutations.completeTask, id, listId);
  return store.dispatch(Mutations.generateComputedLists);
}

async function uncompleteTask(id: number, listId: number): Promise<void> {
  await store.dispatch(Mutations.uncompleteTask, id, listId);
  return store.dispatch(Mutations.generateComputedLists);
}

async function deleteTask(id: number, listId: number): Promise<void> {
  await store.dispatch(Mutations.deleteTask, id, listId);
  return store.dispatch(Mutations.generateComputedLists);
}

function reorderTask(id: number, listId: number, oldOrder: number, newOrder: number): Promise<void> {
  return store.dispatch(Mutations.reorderTask, id, listId, oldOrder, newOrder);
}

function updatePreference(preference: string, enabled: boolean) {
  return store.dispatch(Mutations.updatePreference, preference, enabled);
}

export { getLists, reorderList, completeTask, uncompleteTask, deleteTask, reorderTask, updatePreference };
