import { HttpProxy } from '../../../../../Core/shared2/services/httpProxy';
import { ErrorLogger } from '../../../../../Core/shared2/services/errorLogger';
import { ValidationUtil, ValidationResult } from '../../../../../Core/shared2/utils/validationUtils';

import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
import { localState } from '$lib/stores';
import { List, type Task } from '$lib/models/entities';
import type { ListWithShares } from '$lib/models/viewmodels/listWithShares';
import type { ShareRequest } from '$lib/models/viewmodels/shareRequest';
import type { ListOption } from '$lib/models/viewmodels/listOption';
import type { CanShareList } from '$lib/models/viewmodels/canShareList';
import type { Assignee } from '$lib/models/viewmodels/assignee';
import { ListIcon } from '$lib/models/viewmodels/listIcon';
import type { EditListModel } from '$lib/models/viewmodels/editListModel';
import { ArchivedList } from '$lib/models/viewmodels/archivedList';
import { ListModel } from '$lib/models/viewmodels/listModel';
import { SharingState } from '$lib/models/viewmodels/sharingState';
import type { CreateList } from '$lib/models/server/requests/createList';
import type { UpdateList } from '$lib/models/server/requests/updateList';
import type { UpdateSharedList } from '$lib/models/server/requests/updateSharedList';
import type { ShareList } from '$lib/models/server/requests/shareList';
import type { CopyList } from '$lib/models/server/requests/copyList';
import type { SetIsArchived } from '$lib/models/server/requests/setIsArchived';
import { SetTasksAsNotCompleted } from '$lib/models/server/requests/setTasksAsNotCompleted';
import type { SetShareIsAccepted } from '$lib/models/server/requests/setShareIsAccepted';
import Variables from '$lib/variables';
import { LocalState } from '$lib/models/localState';

export enum DerivedLists {
	HighPriority = 'high-priority',
	StaleTasks = 'stale-tasks'
}

export class ListsService {
	private readonly httpProxy = new HttpProxy();
	private readonly logger = new ErrorLogger('To Do Assistant');
	private readonly localStorage = new LocalStorageUtil();

	async getAll(includeCache = false) {
		if (includeCache) {
			const cachedLists = this.localStorage.getObject<List[]>('homePageData');
			if (cachedLists) {
				localState.set(new LocalState(cachedLists, true));
			}
		}

		const allLists = await this.httpProxy.ajax<List[]>(`${Variables.urls.api}/lists`);

		const highPriorityListEnabled = this.localStorage.getBool(LocalStorageKeys.HighPriorityListEnabled);
		const staleTasksListEnabled = this.localStorage.getBool(LocalStorageKeys.StaleTasksListEnabled);

		if (highPriorityListEnabled || staleTasksListEnabled) {
			const allTasks: Task[] = allLists
				.filter((x) => !x.isArchived && !x.derivedListType)
				.reduce((a: Task[], b: List) => {
					return a.concat(b.tasks);
				}, []);

			if (highPriorityListEnabled) {
				this.generateHighPriorityList(allLists, allTasks);
			}

			if (staleTasksListEnabled) {
				this.generateStaleTasksList(allLists, allTasks);
			}
		}

		this.localStorage.set('homePageData', JSON.stringify(allLists));
		localState.set(new LocalState(allLists, false));
	}

	private generateHighPriorityList(allLists: List[], allTasks: Task[]) {
		const uncompletedHighPriorityTasks = allTasks.filter((x) => !x.isCompleted && x.isHighPriority);
		const highPriorityList = allLists.find((x) => x.derivedListType === DerivedLists.HighPriority);
		if (uncompletedHighPriorityTasks.length > 0) {
			if (highPriorityList) {
				highPriorityList.tasks = uncompletedHighPriorityTasks;
			} else {
				allLists.push(
					new List(0, null, null, false, false, SharingState.NotShared, 1, false, DerivedLists.HighPriority, uncompletedHighPriorityTasks, null)
				);
			}
		} else if (highPriorityList) {
			const index = allLists.indexOf(highPriorityList);
			allLists.splice(index, 1);
		}
	}

	private generateStaleTasksList(allLists: List[], allTasks: Task[]) {
		const now = new Date();
		const month = now.getMonth() - 1;
		const aMonthAgo = new Date(now.getFullYear(), month, now.getDate());

		const uncompletedStaleTasks = allTasks.filter((x) => !x.isCompleted && new Date(x.modifiedDate) < aMonthAgo);
		const staleTasksList = allLists.find((x) => x.derivedListType === DerivedLists.StaleTasks);
		if (uncompletedStaleTasks.length > 0) {
			if (staleTasksList) {
				staleTasksList.tasks = uncompletedStaleTasks;
			} else {
				allLists.push(new List(0, null, null, false, false, SharingState.NotShared, 2, false, DerivedLists.StaleTasks, uncompletedStaleTasks, null));
			}
		} else if (staleTasksList) {
			const index = allLists.indexOf(staleTasksList);
			allLists.splice(index, 1);
		}
	}

	getAllAsOptions(): Promise<ListOption[]> {
		return this.httpProxy.ajax<ListOption[]>(`${Variables.urls.api}/lists/options`);
	}

	async getNonArchivedAsOptions(): Promise<ListOption[]> {
		const listOptions = await this.httpProxy.ajax<ListOption[]>(`${Variables.urls.api}/lists/options`);
		return listOptions.filter((x) => !x.isArchived);
	}

	get(id: number): Promise<EditListModel> {
		return this.httpProxy.ajax<EditListModel>(`${Variables.urls.api}/lists/${id}`);
	}

	getWithShares(id: number): Promise<ListWithShares> {
		return this.httpProxy.ajax<ListWithShares>(`${Variables.urls.api}/lists/${id}/with-shares`);
	}

	getShareRequests(): Promise<ShareRequest[]> {
		return this.httpProxy.ajax<ShareRequest[]>(`${Variables.urls.api}/lists/share-requests`);
	}

	getPendingShareRequestsCount(): Promise<number> {
		return this.httpProxy.ajax<number>(`${Variables.urls.api}/lists/pending-share-requests-count`);
	}

	getIsShared(id: number): Promise<boolean> {
		return this.httpProxy.ajax<boolean>(`${Variables.urls.api}/lists/${id}/shared`);
	}

	getMembersAsAssigneeOptions(id: number): Promise<Assignee[]> {
		return this.httpProxy.ajax<Assignee[]>(`${Variables.urls.api}/lists/${id}/members`);
	}

	async create(dto: CreateList): Promise<number> {
		try {
			const id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/lists`, {
				method: 'post',
				body: window.JSON.stringify(dto)
			});

			await this.getAll();

			return id;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	static validateEdit(name: string): ValidationResult {
		const result = new ValidationResult();

		if (ValidationUtil.isEmptyOrWhitespace(name)) {
			result.fail('name');
		}

		return result;
	}

	async update(dto: UpdateList): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/lists`, {
				method: 'put',
				body: window.JSON.stringify(dto)
			});

			await this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async updateShared(dto: UpdateSharedList): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/lists/shared`, {
				method: 'put',
				body: window.JSON.stringify(dto)
			});

			await this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async delete(id: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/lists/${id}`, {
				method: 'delete'
			});

			await this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	canShareListWithUser(email: string): Promise<CanShareList> {
		return this.httpProxy.ajax<CanShareList>(`${Variables.urls.api}/lists/can-share-with-user/${email}`);
	}

	static validateShare(selectedShareEmail: string, userEmail: string): ValidationResult {
		const result = new ValidationResult();

		if (ValidationUtil.isEmptyOrWhitespace(selectedShareEmail)) {
			result.fail('email');
		}

		if (selectedShareEmail.trim().toLowerCase() === userEmail) {
			result.fail('email');
		}

		return result;
	}

	async share(dto: ShareList): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/lists/share`, {
				method: 'put',
				body: window.JSON.stringify(dto)
			});

			await this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async leave(id: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/lists/${id}/leave`, {
				method: 'delete'
			});

			await this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	static validateCopy(name: string): ValidationResult {
		const result = new ValidationResult();

		if (ValidationUtil.isEmptyOrWhitespace(name)) {
			result.fail('name');
		}

		return result;
	}

	async copy(dto: CopyList): Promise<number> {
		try {
			const id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/lists/copy`, {
				method: 'post',
				body: window.JSON.stringify(dto)
			});

			await this.getAll();

			return id;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	copyAsText(name: string, allTasks: Task[]) {
		try {
			let text = name;

			const tasks = allTasks
				.filter((x) => !x.isCompleted && !x.isPrivate)
				.sort((a: Task, b: Task) => {
					return a.order - b.order;
				});
			const privateTasks = allTasks
				.filter((x) => !x.isCompleted && x.isPrivate)
				.sort((a: Task, b: Task) => {
					return a.order - b.order;
				});
			const completedTasks = allTasks
				.filter((x) => x.isCompleted && !x.isPrivate)
				.sort((a: Task, b: Task) => {
					return a.order - b.order;
				});
			const completedPrivateTasks = allTasks
				.filter((x) => x.isCompleted && x.isPrivate)
				.sort((a: Task, b: Task) => {
					return a.order - b.order;
				});

			if (privateTasks.length + tasks.length > 0) {
				text += '\n';

				for (const task of privateTasks) {
					text += `\n${task.name} ☐`;
				}
				for (const task of tasks) {
					text += `\n${task.name} ☐`;
				}
			}

			if (completedPrivateTasks.length + completedTasks.length > 0) {
				if (tasks.length > 0) {
					text += '\n----------';
				}

				for (const task of completedPrivateTasks) {
					text += `\n${task.name} 🗹`;
				}
				for (const task of completedTasks) {
					text += `\n${task.name} 🗹`;
				}
			}

			const textArea = document.createElement('textarea');
			textArea.value = text;
			textArea.style.position = 'fixed'; // Avoid scrolling to bottom
			document.body.appendChild(textArea);
			textArea.focus();
			textArea.select();

			document.execCommand('copy');

			document.body.removeChild(textArea);
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async setIsArchived(dto: SetIsArchived): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/lists/is-archived`, {
				method: 'put',
				body: window.JSON.stringify(dto)
			});

			await this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async uncompleteAllTasks(id: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/lists/uncomplete-all`, {
				method: 'put',
				body: window.JSON.stringify(new SetTasksAsNotCompleted(id))
			});

			await this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async setShareIsAccepted(dto: SetShareIsAccepted): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/lists/share-is-accepted`, {
				method: 'put',
				body: window.JSON.stringify(dto)
			});

			await this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	// async reorder(id: number, oldOrder: number, newOrder: number): Promise<void> {
	// 	try {
	// 		await this.httpProxy.ajaxExecute(`${Variables.urls.api}/lists/reorder`, {
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

	static getForHomeScreen(lists: List[]): ListModel[] {
		return lists
			.filter((x) => !x.isArchived && !x.derivedListType)
			.sort((a: List, b: List) => {
				return a.order - b.order;
			})
			.map(
				(x) =>
					new ListModel(
						x.id,
						<string>x.name,
						<string>x.icon,
						x.sharingState,
						x.order,
						x.derivedListType,
						null,
						x.tasks.filter((x) => !x.isCompleted).length
					)
			);
	}

	static getDerivedForHomeScreen(lists: List[], derivedListNameLookup: Map<string, string>): ListModel[] {
		return lists
			.filter((x) => x.derivedListType)
			.sort((a: List, b: List) => {
				return a.order - b.order;
			})
			.map(
				(x) =>
					new ListModel(
						x.id,
						derivedListNameLookup.get(x.derivedListType)!,
						<string>x.icon,
						x.sharingState,
						x.order,
						x.derivedListType,
						this.getDerivedListIconClass(x.derivedListType),
						x.tasks.filter((x) => !x.isCompleted).length
					)
			);
	}

	static getArchived(lists: List[]): ArchivedList[] {
		return lists
			.filter((x) => x.isArchived)
			.sort((a: List, b: List) => {
				const aDate = new Date(<string>a.modifiedDate);
				const bDate = new Date(<string>b.modifiedDate);
				if (aDate > bDate) return -1;
				if (aDate < bDate) return 1;
				return 0;
			})
			.map((x) => new ArchivedList(x.id, <string>x.name, <string>x.icon, x.sharingState));
	}

	static getIconOptions(): ListIcon[] {
		return [
			new ListIcon('list', 'fas fa-list'),
			new ListIcon('shopping-cart', 'fas fa-shopping-cart'),
			new ListIcon('shopping-bag', 'fas fa-bag-shopping'),
			new ListIcon('home', 'fas fa-home'),
			new ListIcon('birthday', 'fas fa-birthday-cake'),
			new ListIcon('cheers', 'fas fa-glass-cheers'),
			new ListIcon('vacation', 'fas fa-umbrella-beach'),
			new ListIcon('passport', 'fas fa-passport'),
			new ListIcon('plane', 'fas fa-plane'),
			new ListIcon('car', 'fas fa-car'),
			new ListIcon('pickup-truck', 'fas fa-truck-pickup'),
			new ListIcon('world', 'fas fa-globe-americas'),
			new ListIcon('camping', 'fas fa-campground'),
			new ListIcon('tree', 'fas fa-tree'),
			new ListIcon('motorcycle', 'fas fa-motorcycle'),
			new ListIcon('bicycle', 'fas fa-bicycle'),
			new ListIcon('workout', 'fas fa-dumbbell'),
			new ListIcon('ski', 'fas fa-skiing'),
			new ListIcon('snowboard', 'fas fa-snowboarding'),
			new ListIcon('swimming', 'fas fa-person-swimming'),
			new ListIcon('work', 'fas fa-briefcase'),
			new ListIcon('baby', 'fas fa-baby-carriage'),
			new ListIcon('dog', 'fas fa-dog'),
			new ListIcon('cat', 'fas fa-cat'),
			new ListIcon('bird', 'fas fa-dove'),
			new ListIcon('fish', 'fas fa-fish'),
			new ListIcon('camera', 'fas fa-camera'),
			new ListIcon('medicine', 'fas fa-prescription-bottle-alt'),
			new ListIcon('file', 'fas fa-file-alt'),
			new ListIcon('book', 'fas fa-book'),
			new ListIcon('mountain', 'fas fa-mountain'),
			new ListIcon('facebook', 'fab fa-facebook'),
			new ListIcon('twitter', 'fab fa-twitter'),
			new ListIcon('instagram', 'fab fa-instagram'),
			new ListIcon('tiktok', 'fab fa-tiktok')
		];
	}

	static derivedListsIcons = new Map<string, string>([
		[DerivedLists.HighPriority, 'fas fa-exclamation-triangle'],
		[DerivedLists.StaleTasks, 'fas fa-inbox']
	]);

	static getDerivedListIconClass(type: string): string {
		const icon = ListsService.derivedListsIcons.get(type);
		if (!icon) {
			throw 'No such derived list type';
		}

		return icon;
	}

	release() {
		this.httpProxy.release();
		this.logger.release();
	}
}
