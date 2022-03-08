import { inject } from "aurelia-framework";
import { EventAggregator } from "aurelia-event-aggregator";

import { SignalRClientBase } from "../../../shared/src/utils/signalRClientBase";

import { ListsService } from "services/listsService";
import * as Actions from "utils/state/actions";
import * as environment from "../../config/environment.json";
import { AppEvents } from "models/appEvents";

@inject(ListsService, EventAggregator)
export class SignalRClient extends SignalRClientBase {
  constructor(private readonly listsService: ListsService, private readonly eventAggregator: EventAggregator) {
    super();
  }

  async initialize(accessToken: string, currentUserId: number) {
    const envConfig = JSON.parse(<any>environment);

    await this.connect(`${envConfig.urls.api}/toDoAssistantHub`, accessToken);

    this.connection.onreconnected(async () => {
      await Actions.getLists(this.listsService);
      this.eventAggregator.publish(AppEvents.ListsChanged);
    });

    await this.connection.invoke("JoinGroups");

    this.on("TaskCompletedChanged", async (userId: number, id: number, listId: number, isCompleted: boolean) => {
      if (userId !== currentUserId) {
        if (isCompleted) {
          await Actions.completeTask(id, listId);
        } else {
          await Actions.uncompleteTask(id, listId);
        }

        this.eventAggregator.publish(AppEvents.TaskCompletedChangedRemotely, {
          id: id,
          listId: listId,
          isCompleted: isCompleted,
        });
      }
    });

    this.on("TaskDeleted", async (userId: number, id: number, listId: number) => {
      if (userId !== currentUserId) {
        await Actions.deleteTask(id, listId);
        this.eventAggregator.publish(AppEvents.TaskDeletedRemotely, { id: id, listId: listId });
      }
    });

    this.on("TaskReordered", async (userId: number, id: number, listId: number, oldOrder: number, newOrder: number) => {
      if (userId !== currentUserId) {
        await Actions.reorderTask(id, listId, oldOrder, newOrder);
        this.eventAggregator.publish(AppEvents.TaskReorderedRemotely, { id: id, listId: listId });
      }
    });

    this.on("TasksModified", async (userId: number) => {
      if (userId !== currentUserId) {
        await Actions.getLists(this.listsService);
        this.eventAggregator.publish(AppEvents.ListsChanged);
      }
    });
  }
}
