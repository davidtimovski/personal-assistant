import store from "./store";
import { State } from "./state";
import { List } from "models/entities/list";
import { Task } from "models/entities/task";
import { ListsService } from "services/listsService";

function getLists(state: State, lists: List[], highPriorityText: string) {
  if (!lists) {
    return;
  }

  if (state.highPriorityListEnabled) {
    const allTasks: Task[] = lists.reduce((a, b) => {
      return a.concat(b.tasks);
    }, []);

    const uncompletedHighPriorityTasks = allTasks.filter((x) => !x.isCompleted && x.isHighPriority);
    if (uncompletedHighPriorityTasks.length > 0) {
      lists.push(
        new List(
          0,
          highPriorityText,
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
  }

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

function completeTask(state: State, taskId: number) {
  const newState = Object.assign({}, state, {});
  const allTasks: Task[] = newState.lists.reduce((a, b) => {
    return a.concat(b.tasks);
  }, []);
  const task = allTasks.find((x) => x.id === taskId);
  const list = newState.lists.find((x) => x.id === task.listId);

  if (task.isPrivate) {
    const completedPrivateTasks = list.tasks.filter((x) => x.isCompleted && x.isPrivate);
    completedPrivateTasks.forEach((task) => {
      task.order++;
    });

    task.isCompleted = true;
    task.order = 1;

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
    task.order = 1;

    const tasks = list.tasks.filter((x) => !x.isCompleted && !x.isPrivate && x.order > task.order);
    tasks.forEach((task) => {
      task.order--;
    });
  }

  if (task.isHighPriority) {
    const highPriorityList = newState.lists.find(
      (x) => x.computedListType === ListsService.highPriorityComputedListMoniker
    );

    const index = highPriorityList.tasks.indexOf(task);
    highPriorityList.tasks.splice(index, 1);

    if (highPriorityList.tasks.length === 0) {
      newState.lists = newState.lists.filter(
        (x) => x.computedListType !== ListsService.highPriorityComputedListMoniker
      );
    }
  }

  return newState;
}

function uncompleteTask(state: State, taskId: number) {
  const newState = Object.assign({}, state, {});
  const allTasks: Task[] = newState.lists.reduce((a, b) => {
    return a.concat(b.tasks);
  }, []);
  const task = allTasks.find((x) => x.id === taskId);
  const list = newState.lists.find((x) => x.id === task.listId);

  let order: number;

  if (task.isPrivate) {
    const privateTasks = list.tasks.filter((x) => !x.isCompleted && x.isPrivate);
    order = ++privateTasks.length;

    const completedPrivateTasks = list.tasks.filter((x) => x.isCompleted && x.isPrivate && x.order > task.order);
    completedPrivateTasks.forEach((task) => {
      task.order--;
    });
  } else {
    const tasks = list.tasks.filter((x) => !x.isCompleted && !x.isPrivate);
    order = ++tasks.length;

    const completedTasks = list.tasks.filter((x) => x.isCompleted && !x.isPrivate && x.order > task.order);
    completedTasks.forEach((task) => {
      task.order--;
    });
  }

  task.isCompleted = false;
  task.order = order;

  if (task.isHighPriority) {
    let highPriorityList = newState.lists.find(
      (x) => x.computedListType === ListsService.highPriorityComputedListMoniker
    );
    if (highPriorityList) {
      highPriorityList.tasks.push(task);
    } else {
      highPriorityList = new List(
        0,
        "High Priority",
        null,
        false,
        false,
        null,
        0,
        false,
        ListsService.highPriorityComputedListMoniker,
        [task],
        null
      );
      newState.lists.push(highPriorityList);
    }
  }

  return newState;
}

function deleteTask(state: State, taskId: number) {
  const newState = Object.assign({}, state, {});
  const allTasks: Task[] = newState.lists.reduce((a, b) => {
    return a.concat(b.tasks);
  }, []);
  const task = allTasks.find((x) => x.id === taskId);
  const list = newState.lists.find((x) => x.id === task.listId);

  const tasks = list.tasks.filter(
    (x) => x.isCompleted === task.isCompleted && x.isPrivate === task.isPrivate && x.order > task.order
  );
  tasks.forEach((task) => {
    task.order--;
  });

  const index = list.tasks.indexOf(task);
  list.tasks.splice(index, 1);

  if (task.isHighPriority) {
    const highPriorityList = newState.lists.find(
      (x) => x.computedListType === ListsService.highPriorityComputedListMoniker
    );

    const index = highPriorityList.tasks.indexOf(task);
    highPriorityList.tasks.splice(index, 1);

    if (highPriorityList.tasks.length === 0) {
      newState.lists = newState.lists.filter(
        (x) => x.computedListType !== ListsService.highPriorityComputedListMoniker
      );
    }
  }

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

function updatePreferences(state: State, soundsEnabled: boolean, highPriorityListEnabled: boolean) {
  const newState = Object.assign({}, state, {});

  window.localStorage.setItem("soundsEnabled", soundsEnabled.toString());
  window.localStorage.setItem("highPriorityListEnabled", highPriorityListEnabled.toString());

  newState.soundsEnabled = soundsEnabled;
  newState.highPriorityListEnabled = highPriorityListEnabled;

  return newState;
}

store.registerAction("getLists", getLists);
store.registerAction("setListsLoading", setListsLoading);
store.registerAction("reorderList", reorderList);
store.registerAction("completeTask", completeTask);
store.registerAction("uncompleteTask", uncompleteTask);
store.registerAction("deleteTask", deleteTask);
store.registerAction("reorderTask", reorderTask);
store.registerAction("updatePreferences", updatePreferences);

export {
  getLists,
  setListsLoading,
  reorderList,
  completeTask,
  uncompleteTask,
  deleteTask,
  reorderTask,
  updatePreferences,
};
