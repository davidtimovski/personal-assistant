import store from "./store";
import { State } from "./state";
import { List, Task } from "models/entities";
import { ListsService } from "services/listsService";

function getLists(state: State, lists: List[]) {
  if (!lists) {
    return;
  }

  return Object.assign({}, state, { lists: [...lists] });
}

function generateComputedLists(state: State) {
  if (!state.highPriorityListEnabled) {
    return;
  }

  const newState = Object.assign({}, state, {});

  const allTasks: Task[] = newState.lists
    .filter((x) => !x.isArchived && !x.computedListType)
    .reduce((a, b) => {
      return a.concat(b.tasks);
    }, []);

  const uncompletedHighPriorityTasks = allTasks.filter((x) => !x.isCompleted && x.isHighPriority);
  const highPriorityList = newState.lists.find((x) => x.computedListType);
  if (uncompletedHighPriorityTasks.length > 0) {
    if (highPriorityList) {
      highPriorityList.tasks = uncompletedHighPriorityTasks;
    } else {
      newState.lists.push(
        new List(
          0,
          null,
          null,
          false,
          false,
          null,
          0,
          false,
          ListsService.highPriorityComputedListMoniker,
          uncompletedHighPriorityTasks,
          null
        )
      );
    }
  } else if (highPriorityList) {
    const index = newState.lists.indexOf(highPriorityList);
    newState.lists.splice(index, 1);
  }

  return newState;
}

function setListsLoading(state: State, loading: boolean) {
  return Object.assign({}, state, { loading: loading });
}

function reorderList(state: State, id: number, oldOrder: number, newOrder: number) {
  const newState = Object.assign({}, state, {});
  const list = newState.lists.find((x) => x.id === id);

  if (newOrder > oldOrder) {
    const nonArchivedLists = newState.lists.filter((x) => !x.isArchived && x.order >= oldOrder && x.order <= newOrder);
    nonArchivedLists.forEach((list) => {
      list.order--;
    });
  } else {
    const nonArchivedLists = newState.lists.filter((x) => !x.isArchived && x.order <= oldOrder && x.order >= newOrder);
    nonArchivedLists.forEach((list) => {
      list.order++;
    });
  }

  list.order = newOrder;

  return newState;
}

function completeTask(state: State, id: number, listId: number) {
  const newState = Object.assign({}, state, {});

  const list = newState.lists.find((x) => x.id === listId);
  const task = list.tasks.find((x) => x.id === id);

  if (task.isPrivate) {
    const completedPrivateTasks = list.tasks.filter((x) => x.isCompleted && x.isPrivate);
    completedPrivateTasks.forEach((task) => {
      task.order++;
    });

    task.isCompleted = true;

    const privateTasks = list.tasks.filter((x) => !x.isCompleted && x.isPrivate && x.order > task.order);
    privateTasks.forEach((task) => {
      task.order--;
    });
  } else {
    const completedTasks = list.tasks.filter((x) => x.isCompleted && !x.isPrivate);
    completedTasks.forEach((task) => {
      task.order++;
    });

    task.isCompleted = true;

    const tasks = list.tasks.filter((x) => !x.isCompleted && !x.isPrivate && x.order > task.order);
    tasks.forEach((task) => {
      task.order--;
    });
  }

  task.order = 1;

  return newState;
}

function uncompleteTask(state: State, id: number, listId: number) {
  const newState = Object.assign({}, state, {});

  const list = newState.lists.find((x) => x.id === listId);
  const task = list.tasks.find((x) => x.id === id);

  let newOrder: number;

  if (task.isPrivate) {
    const privateTasks = list.tasks.filter((x) => !x.isCompleted && x.isPrivate);
    newOrder = ++privateTasks.length;

    const completedPrivateTasks = list.tasks.filter((x) => x.isCompleted && x.isPrivate && x.order > task.order);
    completedPrivateTasks.forEach((task) => {
      task.order--;
    });
  } else {
    const tasks = list.tasks.filter((x) => !x.isCompleted && !x.isPrivate);
    newOrder = ++tasks.length;

    const completedTasks = list.tasks.filter((x) => x.isCompleted && !x.isPrivate && x.order > task.order);
    completedTasks.forEach((task) => {
      task.order--;
    });
  }

  task.isCompleted = false;
  task.order = newOrder;

  return newState;
}

function deleteTask(state: State, id: number, listId: number) {
  const newState = Object.assign({}, state, {});

  const list = newState.lists.find((x) => x.id === listId);
  const task = list.tasks.find((x) => x.id === id);

  const tasks = list.tasks.filter(
    (x) => x.isCompleted === task.isCompleted && x.isPrivate === task.isPrivate && x.order > task.order
  );
  tasks.forEach((task) => {
    task.order--;
  });

  const index = list.tasks.indexOf(task);
  list.tasks.splice(index, 1);

  return newState;
}

function reorderTask(state: State, id: number, listId: number, oldOrder: number, newOrder: number) {
  const newState = Object.assign({}, state, {});

  const list = newState.lists.find((x) => x.id === listId);
  const task = list.tasks.find((x) => x.id === id);

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

function updatePreference(state: State, preference: string, enabled: boolean) {
  const newState = Object.assign({}, state, {});

  window.localStorage.setItem(preference, enabled.toString());

  newState[preference] = enabled;

  return newState;
}

store.registerAction("getLists", getLists);
store.registerAction("generateComputedLists", generateComputedLists);
store.registerAction("setListsLoading", setListsLoading);
store.registerAction("reorderList", reorderList);
store.registerAction("completeTask", completeTask);
store.registerAction("uncompleteTask", uncompleteTask);
store.registerAction("deleteTask", deleteTask);
store.registerAction("reorderTask", reorderTask);
store.registerAction("updatePreference", updatePreference);

export {
  getLists,
  generateComputedLists,
  setListsLoading,
  reorderList,
  completeTask,
  uncompleteTask,
  deleteTask,
  reorderTask,
  updatePreference,
};
