import { HttpProxy } from '../../../../shared2/services/httpProxy';
import { ErrorLogger } from '../../../../shared2/services/errorLogger';

import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
import { lists } from '$lib/stores';
import { List, type Task } from '$lib/models/entities';
import type { ListWithShares } from '$lib/models/viewmodels/listWithShares';
import type { Share } from '$lib/models/viewmodels/share';
import type { ShareRequest } from '$lib/models/viewmodels/shareRequest';
import type { ListOption } from '$lib/models/viewmodels/listOption';
import type { CanShareList } from '$lib/models/viewmodels/canShareList';
import type { AssigneeOption } from '$lib/models/viewmodels/assigneeOption';
import { ListIcon } from '$lib/models/viewmodels/listIcon';
import type { EditListModel } from '$lib/models/viewmodels/editListModel';
import { ArchivedList } from '$lib/models/viewmodels/archivedList';
import { ListModel } from '$lib/models/viewmodels/listModel';
import { SharingState } from '$lib/models/viewmodels/sharingState';
import Variables from '$lib/variables';

export class ListsService {
	private readonly httpProxy = new HttpProxy();
	private readonly logger = new ErrorLogger('ToDoAssistant');
	private readonly localStorage = new LocalStorageUtil();

	async getAll(includeCache = false) {
		if (includeCache) {
			let cache = this.localStorage.getObject<List[]>('homePageData');
			if (cache) {
				lists.set(cache);
			}
		}

		const allLists = await this.httpProxy.ajax<List[]>(`${Variables.urls.api}/api/lists`);

		if (this.localStorage.getBool(LocalStorageKeys.HighPriorityListEnabled)) {
			this.generateComputedLists(allLists);
		}

		lists.set(allLists);
		this.localStorage.set('homePageData', JSON.stringify(allLists));
	}

	private generateComputedLists(allLists: List[]) {
		const allTasks: Task[] = allLists
			.filter((x) => !x.isArchived && !x.computedListType)
			.reduce((a: Task[], b: List) => {
				return a.concat(b.tasks);
			}, []);

		const uncompletedHighPriorityTasks = allTasks.filter((x) => !x.isCompleted && x.isHighPriority);
		const highPriorityList = allLists.find((x) => x.computedListType === ListsService.highPriorityComputedListMoniker);
		if (uncompletedHighPriorityTasks.length > 0) {
			if (highPriorityList) {
				highPriorityList.tasks = uncompletedHighPriorityTasks;
			} else {
				allLists.push(
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
						uncompletedHighPriorityTasks,
						null
					)
				);
			}
		} else if (highPriorityList) {
			const index = allLists.indexOf(highPriorityList);
			allLists.splice(index, 1);
		}
	}

	getAllAsOptions(): Promise<ListOption[]> {
		return this.httpProxy.ajax<ListOption[]>(`${Variables.urls.api}/api/lists/options`);
	}

	get(id: number): Promise<EditListModel> {
		return this.httpProxy.ajax<EditListModel>(`${Variables.urls.api}/api/lists/${id}`);
	}

	getWithShares(id: number): Promise<ListWithShares> {
		return this.httpProxy.ajax<ListWithShares>(`${Variables.urls.api}/api/lists/${id}/with-shares`);
	}

	getShareRequests(): Promise<ShareRequest[]> {
		return this.httpProxy.ajax<ShareRequest[]>(`${Variables.urls.api}/api/lists/share-requests`);
	}

	getPendingShareRequestsCount(): Promise<number> {
		return this.httpProxy.ajax<number>(`${Variables.urls.api}/api/lists/pending-share-requests-count`);
	}

	getIsShared(id: number): Promise<boolean> {
		return this.httpProxy.ajax<boolean>(`${Variables.urls.api}/api/lists/${id}/shared`);
	}

	getMembersAsAssigneeOptions(id: number): Promise<AssigneeOption[]> {
		return this.httpProxy.ajax<AssigneeOption[]>(`${Variables.urls.api}/api/lists/${id}/members`);
	}

	async create(name: string, icon: string, isOneTimeToggleDefault: boolean, tasksText: string): Promise<number> {
		try {
			const id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/api/lists`, {
				method: 'post',
				body: window.JSON.stringify({
					name: name,
					icon: icon,
					isOneTimeToggleDefault: isOneTimeToggleDefault,
					tasksText: tasksText
				})
			});

			await this.getAll();

			return id;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async update(
		id: number,
		name: string,
		icon: string,
		tasksText: string,
		notificationsEnabled: boolean,
		isOneTimeToggleDefault: boolean,
		isArchived: boolean,
		sharingState: SharingState
	): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/lists`, {
				method: 'put',
				body: window.JSON.stringify({
					id: id,
					name: name,
					icon: icon,
					tasksText: tasksText,
					notificationsEnabled: notificationsEnabled,
					isOneTimeToggleDefault: isOneTimeToggleDefault,
					isArchived: isArchived,
					sharingState: sharingState
				})
			});

			await this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async updateShared(id: number, notificationsEnabled: boolean): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/lists/shared`, {
				method: 'put',
				body: window.JSON.stringify({
					id: id,
					notificationsEnabled: notificationsEnabled
				})
			});

			await this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async delete(id: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/lists/${id}`, {
				method: 'delete'
			});

			await this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	canShareListWithUser(email: string): Promise<CanShareList> {
		return this.httpProxy.ajax<CanShareList>(`${Variables.urls.api}/api/lists/can-share-with-user/${email}`);
	}

	async share(id: number, newShares: Share[], editedShares: Share[], removedShares: Share[]): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/lists/share`, {
				method: 'put',
				body: window.JSON.stringify({
					listId: id,
					newShares: newShares,
					editedShares: editedShares,
					removedShares: removedShares
				})
			});

			await this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async leave(id: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/lists/${id}/leave`, {
				method: 'delete'
			});

			await this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async copy(list: List): Promise<number> {
		try {
			const id = await this.httpProxy.ajax<number>(`${Variables.urls.api}/api/lists/copy`, {
				method: 'post',
				body: window.JSON.stringify({
					id: list.id,
					name: list.name,
					icon: list.icon
				})
			});

			await this.getAll();

			return id;
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	copyAsText(listName: string, listToCopy: List) {
		try {
			let text = listName;

			const tasks = listToCopy.tasks
				.filter((x) => !x.isCompleted && !x.isPrivate)
				.sort((a: Task, b: Task) => {
					return a.order - b.order;
				});
			const privateTasks = listToCopy.tasks
				.filter((x) => !x.isCompleted && x.isPrivate)
				.sort((a: Task, b: Task) => {
					return a.order - b.order;
				});
			const completedTasks = listToCopy.tasks
				.filter((x) => x.isCompleted && !x.isPrivate)
				.sort((a: Task, b: Task) => {
					return a.order - b.order;
				});
			const completedPrivateTasks = listToCopy.tasks
				.filter((x) => x.isCompleted && x.isPrivate)
				.sort((a: Task, b: Task) => {
					return a.order - b.order;
				});

			if (privateTasks.length + tasks.length > 0) {
				text += '\n';

				for (let task of privateTasks) {
					text += `\n${task.name} ☐`;
				}
				for (let task of tasks) {
					text += `\n${task.name} ☐`;
				}
			}

			if (completedPrivateTasks.length + completedTasks.length > 0) {
				if (tasks.length > 0) {
					text += '\n----------';
				}

				for (let task of completedPrivateTasks) {
					text += `\n${task.name} 🗹`;
				}
				for (let task of completedTasks) {
					text += `\n${task.name} 🗹`;
				}
			}

			const textArea = document.createElement('textarea');
			textArea.value = text;
			textArea.style.position = 'fixed'; // avoid scrolling to bottom
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

	async setIsArchived(id: number, isArchived: boolean): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/lists/is-archived`, {
				method: 'put',
				body: window.JSON.stringify({
					listId: id,
					isArchived: isArchived
				})
			});

			await this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async setTasksAsNotCompleted(id: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/lists/set-tasks-as-not-completed`, {
				method: 'put',
				body: window.JSON.stringify({
					listId: id
				})
			});

			await this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async setShareIsAccepted(id: number, isAccepted: boolean): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/lists/share-is-accepted`, {
				method: 'put',
				body: window.JSON.stringify({
					listId: id,
					isAccepted: isAccepted
				})
			});

			await this.getAll();
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	async reorder(id: number, oldOrder: number, newOrder: number): Promise<void> {
		try {
			await this.httpProxy.ajaxExecute(`${Variables.urls.api}/api/lists/reorder`, {
				method: 'put',
				body: window.JSON.stringify({
					id: id,
					oldOrder: oldOrder,
					newOrder: newOrder
				})
			});
		} catch (e) {
			this.logger.logError(e);
			throw e;
		}
	}

	static getForHomeScreen(lists: List[]): ListModel[] {
		return lists
			.filter((x) => !x.isArchived && !x.computedListType)
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
						x.computedListType,
						null,
						x.tasks.filter((x) => !x.isCompleted).length
					)
			);
	}

	static getComputedForHomeScreen(lists: List[], computedListNameLookup: any): ListModel[] {
		return lists
			.filter((x) => x.computedListType)
			.map(
				(x) =>
					new ListModel(
						x.id,
						computedListNameLookup.get(x.computedListType),
						<string>x.icon,
						x.sharingState,
						x.order,
						x.computedListType,
						this.getComputedListIconClass(x.computedListType),
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
			new ListIcon('shopping', 'fas fa-shopping-cart'),
			new ListIcon('home', 'fas fa-home'),
			new ListIcon('birthday', 'fas fa-birthday-cake'),
			new ListIcon('cheers', 'fas fa-glass-cheers'),
			new ListIcon('vacation', 'fas fa-umbrella-beach'),
			new ListIcon('plane', 'fas fa-plane-departure'),
			new ListIcon('car', 'fas fa-car'),
			new ListIcon('pickup-truck', 'fas fa-truck-pickup'),
			new ListIcon('world', 'fas fa-globe-americas'),
			new ListIcon('camping', 'fas fa-campground'),
			new ListIcon('motorcycle', 'fas fa-motorcycle'),
			new ListIcon('bicycle', 'fas fa-bicycle'),
			new ListIcon('ski', 'fas fa-skiing'),
			new ListIcon('snowboard', 'fas fa-snowboarding'),
			new ListIcon('work', 'fas fa-briefcase'),
			new ListIcon('baby', 'fas fa-baby-carriage'),
			new ListIcon('dog', 'fas fa-dog'),
			new ListIcon('cat', 'fas fa-cat'),
			new ListIcon('fish', 'fas fa-fish'),
			new ListIcon('camera', 'fas fa-camera'),
			new ListIcon('medicine', 'fas fa-prescription-bottle-alt'),
			new ListIcon('file', 'fas fa-file-alt'),
			new ListIcon('book', 'fas fa-book'),
			new ListIcon('mountain', 'fas fa-mountain')
		];
	}

	static highPriorityComputedListMoniker = 'high-priority';

	static getComputedListIconClass(type: string): string {
		if (type === this.highPriorityComputedListMoniker) {
			return 'fas fa-exclamation-triangle';
		}

		throw 'No such computed list type';
	}
}
