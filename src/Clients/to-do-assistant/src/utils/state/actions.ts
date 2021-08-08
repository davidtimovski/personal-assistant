import store from "./store";
import * as Mutations from "./mutations";
import { ListsService } from "services/listsService";

async function getLists(service: ListsService, highPriorityText: string): Promise<void> {
  store.dispatch(Mutations.setListsLoading, true);

  const lists = await service.getAll();
  await store.dispatch(Mutations.getLists, lists, highPriorityText);

  return store.dispatch(Mutations.setListsLoading, false);
}

function reorderList(id: number, oldOrder: number, newOrder: number): Promise<void> {
  return store.dispatch(Mutations.reorderList, id, oldOrder, newOrder);
}

function completeTask(taskId: number): Promise<void> {
  return store.dispatch(Mutations.completeTask, taskId);
}

function uncompleteTask(taskId: number): Promise<void> {
  return store.dispatch(Mutations.uncompleteTask, taskId);
}

function deleteTask(taskId: number): Promise<void> {
  return store.dispatch(Mutations.deleteTask, taskId);
}

function reorderTask(listId: number, taskId: number, oldOrder: number, newOrder: number): Promise<void> {
  return store.dispatch(Mutations.reorderTask, listId, taskId, oldOrder, newOrder);
}

function updatePreferences(soundsEnabled: boolean, highPriorityListEnabled: boolean) {
  return store.dispatch(Mutations.updatePreferences, soundsEnabled, highPriorityListEnabled);
}

export { getLists, reorderList, completeTask, uncompleteTask, deleteTask, reorderTask, updatePreferences };
