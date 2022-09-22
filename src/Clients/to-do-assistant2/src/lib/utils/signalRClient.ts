import { SignalRClientBase } from '../../../../shared2/utils/signalRClientBase';

import { syncStatus } from '$lib/stores';
import { ListsService } from '$lib/services/listsService';
import { SyncEvents } from '$lib/models/syncEvents';
import Variables from '$lib/variables';

export class SignalRClient extends SignalRClientBase {
	private readonly listsService = new ListsService();

	async initialize(accessToken: string, currentUserId: number) {
		await this.connect(`${Variables.urls.api}/toDoAssistantHub`, accessToken, Variables.debug);

		(<signalR.HubConnection>this.connection).onreconnected(async () => {
			// TODO
			//await Actions.getLists(this.listsService);
			//this.eventAggregator.publish(AppEvents.ListsChanged);

			syncStatus.set(SyncEvents.ReSync);
		});

		await (<signalR.HubConnection>this.connection).invoke('JoinGroups');

		this.on('TaskCompletedChanged', async (userId: number, id: number, listId: number, isCompleted: boolean) => {
			if (userId !== currentUserId) {
				// TODO
				if (isCompleted) {
					//await Actions.completeTask(id, listId);
				} else {
					//await Actions.uncompleteTask(id, listId);
				}

				// this.eventAggregator.publish(AppEvents.TaskCompletedChangedRemotely, {
				// 	id: id,
				// 	listId: listId,
				// 	isCompleted: isCompleted
				// });
			}
		});

		this.on('TaskDeleted', async (userId: number, id: number, listId: number) => {
			if (userId !== currentUserId) {
				// TODO
				//await Actions.deleteTask(id, listId);
				//this.eventAggregator.publish(AppEvents.TaskDeletedRemotely, { id: id, listId: listId });
			}
		});

		this.on('TaskReordered', async (userId: number, id: number, listId: number, oldOrder: number, newOrder: number) => {
			if (userId !== currentUserId) {
				// TODO
				//await Actions.reorderTask(id, listId, oldOrder, newOrder);
				//this.eventAggregator.publish(AppEvents.TaskReorderedRemotely, { id: id, listId: listId });
			}
		});

		this.on('TasksModified', async (userId: number) => {
			if (userId !== currentUserId) {
				// TODO
				//await Actions.getLists(this.listsService);
				//this.eventAggregator.publish(AppEvents.ListsChanged);
			}
		});
	}
}
