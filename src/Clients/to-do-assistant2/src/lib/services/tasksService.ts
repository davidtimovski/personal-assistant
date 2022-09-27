import { HttpProxy } from '../../../../shared2/services/httpProxy';
import { ErrorLogger } from '../../../../shared2/services/errorLogger';

import { lists } from '$lib/stores';
import { ListsService } from '$lib/services/listsService';
import { List, type Task } from '$lib/models/entities';
import type { EditTaskModel } from '$lib/models/viewmodels/editTaskModel';
import type { BulkAddTasksModel } from '$lib/models/viewmodels/bulkAddTasksModel';
import { ListTask } from '$lib/models/viewmodels/listTask';
import Variables from '$lib/variables';
import { SharingState } from '$lib/models/viewmodels/sharingState';

export class TasksService {
	private readonly httpProxy = new HttpProxy();
	private readonly logger = new ErrorLogger('ToDoAssistant');

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

	async bulkCreate(model: BulkAddTasksModel, listsService: ListsService): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/tasks/bulk`, {
				method: 'post',
				body: window.JSON.stringify({
					listId: model.listId,
					tasksText: model.tasksText,
					tasksAreOneTime: model.tasksAreOneTime,
					tasksArePrivate: model.tasksArePrivate
				})
			});

			//await Actions.getLists(listsService);
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

	deleteLocal(id: number, listId: number, localLists: List[]) {
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

		if (task.isHighPriority) {
			const allTasks: Task[] = localLists
				.filter((x) => !x.isArchived && !x.computedListType)
				.reduce((a: Task[], b: List) => {
					return a.concat(b.tasks);
				}, []);
			const uncompletedHighPriorityTasks = allTasks.filter((x) => !x.isCompleted && x.isHighPriority);

			const highPriorityList = localLists.find(
				(x) => x.computedListType === ListsService.highPriorityComputedListMoniker
			);
			if (highPriorityList) {
				if (uncompletedHighPriorityTasks.length > 0) {
					const index = highPriorityList.tasks.indexOf(task);
					highPriorityList.tasks.splice(index, 1);
				} else {
					const index = localLists.indexOf(highPriorityList);
					localLists.splice(index, 1);
				}
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

	completeLocal(id: number, listId: number, localLists: List[]) {
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

		if (task.isHighPriority) {
			const allTasks: Task[] = localLists
				.filter((x) => !x.isArchived && !x.computedListType)
				.reduce((a: Task[], b: List) => {
					return a.concat(b.tasks);
				}, []);
			const uncompletedHighPriorityTasks = allTasks.filter((x) => !x.isCompleted && x.isHighPriority);

			const highPriorityList = localLists.find(
				(x) => x.computedListType === ListsService.highPriorityComputedListMoniker
			);
			if (highPriorityList) {
				if (uncompletedHighPriorityTasks.length > 0) {
					const index = highPriorityList.tasks.indexOf(task);
					highPriorityList.tasks.splice(index, 1);
				} else {
					const index = localLists.indexOf(highPriorityList);
					localLists.splice(index, 1);
				}
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

	uncompleteLocal(id: number, listId: number, localLists: List[]) {
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

		if (task.isHighPriority) {
			const highPriorityList = localLists.find(
				(x) => x.computedListType === ListsService.highPriorityComputedListMoniker
			);
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
						ListsService.highPriorityComputedListMoniker,
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
}
