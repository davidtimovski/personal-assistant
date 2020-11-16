import store from "./store";
import * as Mutations from "./mutations";
import { ListsService } from "services/listsService";

async function getLists(service: ListsService): Promise<void> {
  store.dispatch(Mutations.setListsLoading, true);

  const lists = await service.getAll();
  await store.dispatch(Mutations.getLists, lists);

  return store.dispatch(Mutations.setListsLoading, false);
}

function completeTask(listId: number, taskId: number): Promise<void> {
  return store.dispatch(Mutations.completeTask, listId, taskId);
}

function uncompleteTask(listId: number, taskId: number): Promise<void> {
  return store.dispatch(Mutations.uncompleteTask, listId, taskId);
}

function deleteTask(listId: number, taskId: number): Promise<void> {
  return store.dispatch(Mutations.deleteTask, listId, taskId);
}

export { getLists, completeTask, uncompleteTask, deleteTask };
