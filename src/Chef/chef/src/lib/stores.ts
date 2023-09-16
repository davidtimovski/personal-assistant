import { writable } from 'svelte/store';

import type { AuthInfo } from '../../../../Core/shared2/models/authInfo';
import { AlertState } from '../../../../Core/shared2/models/alertState';
import { AlertStatus } from '../../../../Core/shared2/models/enums/alertEvents';

import { ChefUser } from './models/chefUser';
import Variables from '$lib/variables';
import { State } from '$lib/models/state';

export const isOffline = writable<boolean>(false);
export const authInfo = writable<AuthInfo | null>(null);
export const user = writable<ChefUser>(
	new ChefUser('', '', 'en-US', 'en-US', Variables.urls.defaultProfileImageUrl, false)
);
export const alertState = writable<AlertState>(new AlertState(AlertStatus.Hidden, null, []));
export const state = writable<State>(new State(null, true));
