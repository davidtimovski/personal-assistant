import store from "./store";
import { State } from "./state";
import { List } from "models/entities/list";

function getLists(state: State, lists: Array<List>) {
  const newState = Object.assign({}, state, { lists: [...lists] });

  return newState;
}

function setListsLoading(state: State, loading: boolean) {
  const newState = Object.assign({}, state, { loading: loading });

  return newState;
}

function reorderList(state: State, id: number, oldOrder: number, newOrder: number) {
  const newState = Object.assign({}, state, {});
  const list = newState.lists.find((x) => x.id === id);

  if (newOrder > oldOrder) {
    const nonArchivedLists = newState.lists.filter(
      (x) => !x.isArchived && x.order >= oldOrder && x.order <= newOrder
    );
    nonArchivedLists.forEach((list) => {
      list.order--;
    });
  } else {
    const nonArchivedLists = newState.lists.filter(
      (x) => !x.isArchived && x.order <= oldOrder && x.order >= newOrder
    );
    nonArchivedLists.forEach((list) => {
      list.order++;
    });
  }

  list.order = newOrder;

  return newState;
}

function completeTask(state: State, listId: number, taskId: number) {
  const newState = Object.assign({}, state, {});
  const list = newState.lists.find((x) => x.id === listId);
  const task = list.tasks.find((x) => x.id === taskId);

  if (task.isPrivate) {
    const completedPrivateTasks = list.tasks.filter(
      (x) => x.isCompleted && x.isPrivate
    );
    completedPrivateTasks.forEach((task) => {
      task.order++;
    });

    task.isCompleted = true;
    task.order = 1;

    const privateTasks = list.tasks.filter(
      (x) => !x.isCompleted && x.isPrivate && x.order > task.order
    );
    privateTasks.forEach((task) => {
      task.order--;
    });
  } else {
    const completedTasks = list.tasks.filter(
      (x) => x.isCompleted && !x.isPrivate
    );
    completedTasks.forEach((task) => {
      task.order++;
    });

    task.isCompleted = true;
    task.order = 1;

    const tasks = list.tasks.filter(
      (x) => !x.isCompleted && !x.isPrivate && x.order > task.order
    );
    tasks.forEach((task) => {
      task.order--;
    });
  }

  return newState;
}

function uncompleteTask(state: State, listId: number, taskId: number) {
  const newState = Object.assign({}, state, {});
  const list = newState.lists.find((x) => x.id === listId);
  const task = list.tasks.find((x) => x.id === taskId);

  let order: number;

  if (task.isPrivate) {
    const privateTasks = list.tasks.filter(
      (x) => !x.isCompleted && x.isPrivate
    );
    order = ++privateTasks.length;

    const completedPrivateTasks = list.tasks.filter(
      (x) => x.isCompleted && x.isPrivate && x.order > task.order
    );
    completedPrivateTasks.forEach((task) => {
      task.order--;
    });
  } else {
    const tasks = list.tasks.filter((x) => !x.isCompleted && !x.isPrivate);
    order = ++tasks.length;

    const completedTasks = list.tasks.filter(
      (x) => x.isCompleted && !x.isPrivate && x.order > task.order
    );
    completedTasks.forEach((task) => {
      task.order--;
    });
  }

  task.isCompleted = false;
  task.order = order;

  return newState;
}

function deleteTask(state: State, listId: number, taskId: number) {
  const newState = Object.assign({}, state, {});
  const list = newState.lists.find((x) => x.id === listId);
  const task = list.tasks.find((x) => x.id === taskId);

  const tasks = list.tasks.filter(
    (x) =>
      x.isCompleted === task.isCompleted &&
      x.isPrivate === task.isPrivate &&
      x.order > task.order
  );
  tasks.forEach((task) => {
    task.order--;
  });

  const index = list.tasks.indexOf(task);
  list.tasks.splice(index, 1);

  return newState;
}

function reorderTask(state: State, listId: number, taskId: number, oldOrder: number, newOrder: number) {
  const newState = Object.assign({}, state, {});
  const list = newState.lists.find((x) => x.id === listId);
  const task = list.tasks.find((x) => x.id === taskId);

  if (task.isPrivate) {
    if (newOrder > oldOrder) {
      const privateTasks = list.tasks.filter(
        (x) => x.isPrivate && x.isCompleted == task.isCompleted && x.order >= oldOrder && x.order <= newOrder
      );
      privateTasks.forEach((task) => {
        task.order--;
      });
    } else {
      const privateTasks = list.tasks.filter(
        (x) => x.isPrivate && x.isCompleted == task.isCompleted && x.order <= oldOrder && x.order >= newOrder
      );
      privateTasks.forEach((task) => {
        task.order++;
      });
    }
  } else {
    if (newOrder > oldOrder) {
      const publicTasks = list.tasks.filter(
        (x) => !x.isPrivate && x.isCompleted == task.isCompleted && x.order >= oldOrder && x.order <= newOrder
      );
      publicTasks.forEach((task) => {
        task.order--;
      });
    } else {
      const publicTasks = list.tasks.filter(
        (x) => !x.isPrivate && x.isCompleted == task.isCompleted && x.order <= oldOrder && x.order >= newOrder
      );
      publicTasks.forEach((task) => {
        task.order++;
      });
    }
  }

  task.order = newOrder;

  return newState;
}

store.registerAction("getLists", getLists);
store.registerAction("setListsLoading", setListsLoading);
store.registerAction("reorderList", reorderList);
store.registerAction("completeTask", completeTask);
store.registerAction("uncompleteTask", uncompleteTask);
store.registerAction("deleteTask", deleteTask);
store.registerAction("reorderTask", reorderTask);

export { getLists, setListsLoading, reorderList, completeTask, uncompleteTask, deleteTask, reorderTask };
