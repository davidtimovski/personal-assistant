import { writable } from 'svelte/store';

import type { AuthInfo } from '../../../shared2/models/authInfo';
import { AlertState } from '../../../shared2/models/alertState';
import { AlertStatus } from '../../../shared2/models/enums/alertEvents';

import { ToDoAssistantUser } from './models/toDoAssistantUser';
import { RemoteEvent, RemoteEventType } from './models/remoteEvents';
import Variables from '$lib/variables';
import { State } from '$lib/models/state';

export const isOffline = writable<boolean>(false);
export const authInfo = writable<AuthInfo | null>(null);
export const user = writable<ToDoAssistantUser>(
	new ToDoAssistantUser('', '', 'en-US', 'en-US', Variables.urls.defaultProfileImageUrl, false)
);
export const remoteEvents = writable<RemoteEvent>(new RemoteEvent(RemoteEventType.None, null));
export const alertState = writable<AlertState>(new AlertState(AlertStatus.Hidden, null, []));
export const state = writable<State>(new State([], true));
