import { HttpProxy } from '../../../../shared2/services/httpProxy';
import { ErrorLogger } from '../../../../shared2/services/errorLogger';

import { lists } from '$lib/stores';
import { DerivedLists } from '$lib/services/listsService';
import { LocalStorageKeys, type LocalStorageUtil } from '$lib/utils/localStorageUtil';
import { List, type Task } from '$lib/models/entities';
import type { EditTaskModel } from '$lib/models/viewmodels/editTaskModel';
import { ListTask } from '$lib/models/viewmodels/listTask';
import { SharingState } from '$lib/models/viewmodels/sharingState';
import Variables from '$lib/variables';

export class TasksService {
	private readonly httpProxy = new HttpProxy('to-do-assistant2');
	private readonly logger = new ErrorLogger('ToDoAssistant', 'to-do-assistant2');

	get(id: number): Promise<Task> {
		return this.httpProxy.ajax<Task>(`${Variables.urls.api}/api/tasks/${id}`);
	}

	getForUpdate(id: number): Promise<EditTaskModel> {
		return this.httpProxy.ajax<EditTaskModel>(`${Variables.urls.api}/api/tasks/${id}/update`);
	}

	async create(listId: number, name: string, isOneTime: boolean, isPrivate: boolean): Promise<number> {
		try {
			const id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/api/tasks`, {
				method: 'post',
				body: window.JSON.stringify({
					listId: listId,
					name: name,
					isOneTime: isOneTime,
					isPrivate: isPrivate
				})
			});

			return id;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async bulkCreate(
		listId: number,
		tasksText: string,
		tasksAreOneTime: boolean,
		tasksArePrivate: boolean
	): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/tasks/bulk`, {
				method: 'post',
				body: window.JSON.stringify({
					listId: listId,
					tasksText: tasksText,
					tasksAreOneTime: tasksAreOneTime,
					tasksArePrivate: tasksArePrivate
				})
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async update(
		id: number,
		listId: number,
		name: string,
		isOneTime: boolean,
		isHighPriority: boolean,
		isPrivate: boolean,
		assignedToUserId: number | null
	): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/tasks`, {
				method: 'put',
				body: window.JSON.stringify({
					id: id,
					listId: listId,
					name: name,
					isOneTime: isOneTime,
					isHighPriority: isHighPriority,
					isPrivate: isPrivate,
					assignedToUserId: assignedToUserId
				})
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async delete(id: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/tasks/${id}`, {
				method: 'delete'
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	deleteLocal(id: number, listId: number, localLists: List[], localStorage: LocalStorageUtil) {
		const list = localLists.find((x) => x.id === listId);
		if (!list) {
			throw new Error('List not found');
		}

		const task = list.tasks.find((x) => x.id === id);
		if (!task) {
			throw new Error('Task not found');
		}

		const tasks = list.tasks.filter(
			(x) => x.isCompleted === task.isCompleted && x.isPrivate === task.isPrivate && x.order > task.order
		);
		tasks.forEach((task) => {
			task.order--;
		});

		const index = list.tasks.indexOf(task);
		list.tasks.splice(index, 1);

		if (task.isHighPriority && localStorage.getBool(LocalStorageKeys.HighPriorityListEnabled)) {
			const highPriorityList = localLists.find((x) => x.derivedListType === DerivedLists.HighPriority);
			if (!highPriorityList) {
				throw new Error(`Could not find derived list of type ${DerivedLists.HighPriority}`);
			}

			if (highPriorityList.tasks.length > 1) {
				const index = highPriorityList.tasks.indexOf(task);
				highPriorityList.tasks.splice(index, 1);
			} else {
				const index = localLists.indexOf(highPriorityList);
				localLists.splice(index, 1);
			}
		}

		if (localStorage.getBool(LocalStorageKeys.StaleTasksListEnabled)) {
			const staleTasksList = localLists.find((x) => x.derivedListType === DerivedLists.StaleTasks);
			if (!staleTasksList) {
				throw new Error(`Could not find derived list of type ${DerivedLists.StaleTasks}`);
			}

			const index = staleTasksList.tasks.indexOf(task);
			if (index === -1) {
				return;
			}

			if (staleTasksList.tasks.length > 1) {
				staleTasksList.tasks.splice(index, 1);
			} else {
				const index = localLists.indexOf(staleTasksList);
				localLists.splice(index, 1);
			}
		}

		lists.set(localLists);
	}

	async complete(id: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/tasks/complete`, {
				method: 'put',
				body: window.JSON.stringify({
					id: id
				})
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	completeLocal(id: number, listId: number, localLists: List[], localStorage: LocalStorageUtil) {
		const list = localLists.find((x) => x.id === listId);
		if (!list) {
			throw new Error('List not found');
		}

		const task = list.tasks.find((x) => x.id === id);
		if (!task) {
			throw new Error('Task not found');
		}

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
		task.modifiedDate = new Date().toISOString();

		if (task.isHighPriority && localStorage.getBool(LocalStorageKeys.HighPriorityListEnabled)) {
			const highPriorityList = localLists.find((x) => x.derivedListType === DerivedLists.HighPriority);
			if (!highPriorityList) {
				throw new Error(`Could not find derived list of type ${DerivedLists.HighPriority}`);
			}

			if (highPriorityList.tasks.length > 1) {
				const index = highPriorityList.tasks.indexOf(task);
				highPriorityList.tasks.splice(index, 1);
			} else {
				const index = localLists.indexOf(highPriorityList);
				localLists.splice(index, 1);
			}
		}

		if (localStorage.getBool(LocalStorageKeys.StaleTasksListEnabled)) {
			const staleTasksList = localLists.find((x) => x.derivedListType === DerivedLists.StaleTasks);
			if (!staleTasksList) {
				throw new Error(`Could not find derived list of type ${DerivedLists.StaleTasks}`);
			}

			const index = staleTasksList.tasks.indexOf(task);
			if (index === -1) {
				return;
			}

			if (staleTasksList.tasks.length > 1) {
				staleTasksList.tasks.splice(index, 1);
			} else {
				const index = localLists.indexOf(staleTasksList);
				localLists.splice(index, 1);
			}
		}

		lists.set(localLists);
	}

	async uncomplete(id: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/tasks/uncomplete`, {
				method: 'put',
				body: window.JSON.stringify({
					id: id
				})
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	uncompleteLocal(id: number, listId: number, localLists: List[], localStorage: LocalStorageUtil) {
		const list = localLists.find((x) => x.id === listId);
		if (!list) {
			throw new Error('List not found');
		}

		const task = list.tasks.find((x) => x.id === id);
		if (!task) {
			throw new Error('Task not found');
		}

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
		task.modifiedDate = new Date().toISOString();

		if (task.isHighPriority && localStorage.getBool(LocalStorageKeys.HighPriorityListEnabled)) {
			const highPriorityList = localLists.find((x) => x.derivedListType === DerivedLists.HighPriority);
			if (highPriorityList) {
				highPriorityList.tasks.push(task);
			} else {
				localLists.push(
					new List(
						0,
						null,
						null,
						false,
						false,
						SharingState.NotShared,
						0,
						false,
						DerivedLists.HighPriority,
						[task],
						null
					)
				);
			}
		}

		lists.set(localLists);
	}

	// async reorder(id: number, listId: number, oldOrder: number, newOrder: number): Promise<void> {
	// 	try {
	// 		await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/tasks/reorder`, {
	// 			method: 'put',
	// 			body: window.JSON.stringify({
	// 				id: id,
	// 				oldOrder: oldOrder,
	// 				newOrder: newOrder
	// 			})
	// 		});
	// 	} catch (e) {
	// 		this.logger.logError(e);
	// 		throw e;
	// 	}
	// }

	static getTasks(tasks: Task[]) {
		return tasks
			.filter((x) => !x.isCompleted && !x.isPrivate)
			.sort((a: Task, b: Task) => {
				return a.order - b.order;
			})
			.map((x) => {
				return ListTask.fromTask(x);
			});
	}

	static getPrivateTasks(tasks: Task[]) {
		return tasks
			.filter((x) => !x.isCompleted && x.isPrivate)
			.sort((a: Task, b: Task) => {
				return a.order - b.order;
			})
			.map((x) => {
				return ListTask.fromTask(x);
			});
	}

	static getCompletedTasks(tasks: Task[]) {
		return tasks
			.filter((x) => x.isCompleted && !x.isPrivate)
			.sort((a: Task, b: Task) => {
				return a.order - b.order;
			})
			.map((x) => {
				return ListTask.fromTask(x);
			});
	}

	static getCompletedPrivateTasks(tasks: Task[]) {
		return tasks
			.filter((x) => x.isCompleted && x.isPrivate)
			.sort((a: Task, b: Task) => {
				return a.order - b.order;
			})
			.map((x) => {
				return ListTask.fromTask(x);
			});
	}

	release() {
		this.httpProxy.release();
	}
}
