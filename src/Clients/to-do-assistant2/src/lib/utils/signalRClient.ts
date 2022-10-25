import { SignalRClientBase } from '../../../../shared2/utils/signalRClientBase';

import { remoteEvents } from '$lib/stores';
import type { ListsService } from '$lib/services/listsService';
import { RemoteEvent, RemoteEventType } from '$lib/models/remoteEvents';
import Variables from '$lib/variables';

export class SignalRClient extends SignalRClientBase {
	constructor(private readonly listsService: ListsService) {
		super();
	}

	async initialize(accessToken: string, currentUserId: string) {
		await this.connect(`${Variables.urls.api}/toDoAssistantHub`, accessToken, Variables.debug);

		(<signalR.HubConnection>this.connection).onreconnected(async () => {
			await this.listsService.getAll();
		});

		await (<signalR.HubConnection>this.connection).invoke('JoinGroups');

		this.on('TaskCompletedChanged', async (userId: string, id: number, listId: number, isCompleted: boolean) => {
			if (userId !== currentUserId) {
				if (isCompleted) {
					remoteEvents.set(
						new RemoteEvent(RemoteEventType.TaskCompletedRemotely, {
							id: id,
							listId: listId
						})
					);
				} else {
					remoteEvents.set(
						new RemoteEvent(RemoteEventType.TaskUncompletedRemotely, {
							id: id,
							listId: listId
						})
					);
				}
			}
		});

		this.on('TaskDeleted', async (userId: string, id: number, listId: number) => {
			if (userId !== currentUserId) {
				remoteEvents.set(
					new RemoteEvent(RemoteEventType.TaskDeletedRemotely, {
						id: id,
						listId: listId
					})
				);
			}
		});

		// TODO
		// this.on('TaskReordered', async (userId: string, id: number, listId: number, oldOrder: number, newOrder: number) => {
		// 	if (userId !== currentUserId) {
		// 		// TODO
		// 		await Actions.reorderTask(id, listId, oldOrder, newOrder);
		// 		this.eventAggregator.publish(RemoteEventType.TaskReorderedRemotely, { id: id, listId: listId });
		// 	}
		// });

		this.on('TasksModified', async (userId: string) => {
			if (userId !== currentUserId) {
				await this.listsService.getAll();
			}
		});
	}
}
