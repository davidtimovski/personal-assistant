import { writable } from 'svelte/store';

import type { AuthInfo } from '../../../../Core/shared2/models/authInfo';
import { AlertState } from '../../../../Core/shared2/models/alertState';
import { AlertStatus } from '../../../../Core/shared2/models/enums/alertEvents';

import { TrainerUser } from '$lib/models/trainerUser';
import Variables from '$lib/variables';

export const isOffline = writable<boolean>(false);
export const authInfo = writable<AuthInfo | null>(null);
export const user = writable<TrainerUser>(
	new TrainerUser('', '', 'en-US', 'en-US', Variables.urls.defaultProfileImageUrl)
);
export const alertState = writable<AlertState>(new AlertState(AlertStatus.Hidden, null, []));
