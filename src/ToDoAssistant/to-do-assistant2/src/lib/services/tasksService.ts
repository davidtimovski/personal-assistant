import { HttpProxy } from '../../../../../Core/shared2/services/httpProxy';
import { ErrorLogger } from '../../../../../Core/shared2/services/errorLogger';
import { ValidationResult, ValidationUtil } from '../../../../../Core/shared2/utils/validationUtils';

import { state } from '$lib/stores';
import { DerivedLists } from '$lib/services/listsService';
import { LocalStorageKeys, LocalStorageUtil } from '$lib/utils/localStorageUtil';
import { List, type Task } from '$lib/models/entities';
import type { EditTaskModel } from '$lib/models/viewmodels/editTaskModel';
import { ListTask } from '$lib/models/viewmodels/listTask';
import { SharingState } from '$lib/models/viewmodels/sharingState';
import type { CreateTask } from '$lib/models/server/requests/createTask';
import type { BulkCreate } from '$lib/models/server/requests/bulkCreate';
import type { UpdateTask } from '$lib/models/server/requests/updateTask';
import { CompleteUncomplete } from '$lib/models/server/requests/completeUncomplete';
import { State } from '$lib/models/state';
import Variables from '$lib/variables';

export class TasksService {
	private static readonly urlRegex =
		/^(?:(?:(?:https?|ftp):)?\/\/)(?:\S+(?::\S*)?@)?(?:(?!(?:10|127)(?:\.\d{1,3}){3})(?!(?:169\.254|192\.168)(?:\.\d{1,3}){2})(?!172\.(?:1[6-9]|2\d|3[0-1])(?:\.\d{1,3}){2})(?:[1-9]\d?|1\d\d|2[01]\d|22[0-3])(?:\.(?:1?\d{1,2}|2[0-4]\d|25[0-5])){2}(?:\.(?:[1-9]\d?|1\d\d|2[0-4]\d|25[0-4]))|(?:(?:[a-z0-9\u00a1-\uffff][a-z0-9\u00a1-\uffff_-]{0,62})?[a-z0-9\u00a1-\uffff]\.)+(?:[a-z\u00a1-\uffff]{2,}\.?))(?::\d{2,5})?(?:[/?#]\S*)?$/i;

	private readonly httpProxy = new HttpProxy();
	private readonly logger = new ErrorLogger('To Do Assistant');
	private readonly localStorage = new LocalStorageUtil();

	get(id: number): Promise<Task> {
		return this.httpProxy.ajax<Task>(`${Variables.urls.api}/tasks/${id}`);
	}

	getForUpdate(id: number): Promise<EditTaskModel> {
		return this.httpProxy.ajax<EditTaskModel>(`${Variables.urls.api}/tasks/${id}/update`);
	}

	static validateEdit(name: string, url: string | null): ValidationResult {
		const result = new ValidationResult();

		if (ValidationUtil.isEmptyOrWhitespace(name)) {
			result.fail('name');
		}

		if (url && !this.isUrl(url)) {
			result.fail('url');
		}

		return result;
	}

	async create(dto: CreateTask): Promise<number> {
		try {
			const id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/tasks`, {
				method: 'post',
				body: window.JSON.stringify(dto)
			});

			return id;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	static validateBulkCreate(tasksText: string): ValidationResult {
		const result = new ValidationResult();

		if (ValidationUtil.isEmptyOrWhitespace(tasksText)) {
			result.fail('tasksText');
		}

		return result;
	}

	async bulkCreate(dto: BulkCreate): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/tasks/bulk`, {
				method: 'post',
				body: window.JSON.stringify(dto)
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async update(dto: UpdateTask): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/tasks`, {
				method: 'put',
				body: window.JSON.stringify(dto)
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async delete(id: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/tasks/${id}`, {
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

		const tasks = list.tasks.filter((x) => x.isCompleted === task.isCompleted && x.isPrivate === task.isPrivate && x.order > task.order);
		tasks.forEach((task) => {
			task.order--;
		});

		const index = list.tasks.indexOf(task);
		list.tasks.splice(index, 1);

		if (task.isHighPriority && this.localStorage.getBool(LocalStorageKeys.HighPriorityListEnabled)) {
			const highPriorityList = localLists.find((x) => x.derivedListType === DerivedLists.HighPriority);
			if (highPriorityList) {
				if (highPriorityList.tasks.length > 1) {
					const index = highPriorityList.tasks.indexOf(task);
					highPriorityList.tasks.splice(index, 1);
				} else {
					const index = localLists.indexOf(highPriorityList);
					localLists.splice(index, 1);
				}
			}
		}

		if (this.localStorage.getBool(LocalStorageKeys.StaleTasksListEnabled)) {
			const staleTasksList = localLists.find((x) => x.derivedListType === DerivedLists.StaleTasks);
			if (staleTasksList) {
				const index = staleTasksList.tasks.indexOf(task);

				if (index > -1) {
					if (staleTasksList.tasks.length > 1) {
						staleTasksList.tasks.splice(index, 1);
					} else {
						const index = localLists.indexOf(staleTasksList);
						localLists.splice(index, 1);
					}
				}
			}
		}

		state.set(new State(localLists, false));
	}

	async complete(id: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/tasks/complete`, {
				method: 'put',
				body: window.JSON.stringify(new CompleteUncomplete(id))
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
		task.modifiedDate = new Date().toISOString();

		if (task.isHighPriority && this.localStorage.getBool(LocalStorageKeys.HighPriorityListEnabled)) {
			const highPriorityList = localLists.find((x) => x.derivedListType === DerivedLists.HighPriority);
			if (highPriorityList) {
				if (highPriorityList.tasks.length > 1) {
					const index = highPriorityList.tasks.indexOf(task);
					highPriorityList.tasks.splice(index, 1);
				} else {
					const index = localLists.indexOf(highPriorityList);
					localLists.splice(index, 1);
				}
			}
		}

		if (this.localStorage.getBool(LocalStorageKeys.StaleTasksListEnabled)) {
			const staleTasksList = localLists.find((x) => x.derivedListType === DerivedLists.StaleTasks);
			if (staleTasksList) {
				const index = staleTasksList.tasks.indexOf(task);

				if (index > -1) {
					if (staleTasksList.tasks.length > 1) {
						staleTasksList.tasks.splice(index, 1);
					} else {
						const index = localLists.indexOf(staleTasksList);
						localLists.splice(index, 1);
					}
				}
			}
		}

		state.set(new State(localLists, false));
	}

	async uncomplete(id: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/tasks/uncomplete`, {
				method: 'put',
				body: window.JSON.stringify(new CompleteUncomplete(id))
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
		task.modifiedDate = new Date().toISOString();

		if (task.isHighPriority && this.localStorage.getBool(LocalStorageKeys.HighPriorityListEnabled)) {
			const highPriorityList = localLists.find((x) => x.derivedListType === DerivedLists.HighPriority);
			if (highPriorityList) {
				highPriorityList.tasks.push(task);
			} else {
				localLists.push(new List(0, null, null, false, false, SharingState.NotShared, 0, false, DerivedLists.HighPriority, [task], null));
			}
		}

		state.set(new State(localLists, false));
	}

	// async reorder(id: number, listId: number, oldOrder: number, newOrder: number): Promise<void> {
	// 	try {
	// 		await this.httpProxy.ajaxExecute(`${Variables.urls.api}/tasks/reorder`, {
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

	static getTasks(tasks: Task[], disabled: boolean) {
		return tasks
			.filter((x) => !x.isCompleted && !x.isPrivate)
			.sort((a: Task, b: Task) => {
				return a.order - b.order;
			})
			.map((x) => {
				return ListTask.fromTask(x, disabled);
			});
	}

	static getPrivateTasks(tasks: Task[], disabled: boolean) {
		return tasks
			.filter((x) => !x.isCompleted && x.isPrivate)
			.sort((a: Task, b: Task) => {
				return a.order - b.order;
			})
			.map((x) => {
				return ListTask.fromTask(x, disabled);
			});
	}

	static getCompletedTasks(tasks: Task[], disabled: boolean) {
		return tasks
			.filter((x) => x.isCompleted && !x.isPrivate)
			.sort((a: Task, b: Task) => {
				return a.order - b.order;
			})
			.map((x) => {
				return ListTask.fromTask(x, disabled);
			});
	}

	static getCompletedPrivateTasks(tasks: Task[], disabled: boolean) {
		return tasks
			.filter((x) => x.isCompleted && x.isPrivate)
			.sort((a: Task, b: Task) => {
				return a.order - b.order;
			})
			.map((x) => {
				return ListTask.fromTask(x, disabled);
			});
	}

	static isUrl(text: string) {
		return this.urlRegex.test(text);
	}

	release() {
		this.httpProxy.release();
		this.logger.release();
	}
}
