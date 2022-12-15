import { writable } from 'svelte/store';

import type { AuthInfo } from '../../../shared2/models/authInfo';
import { AlertState } from '../../../shared2/models/alertState';
import { AlertStatus } from '../../../shared2/models/enums/alertEvents';

import { WeathermanUser } from '$lib/models/weathermanUser';
import { Forecast } from '$lib/models/forecast';
import Variables from '$lib/variables';

export const isOffline = writable<boolean>(false);
export const authInfo = writable<AuthInfo | null>(null);
export const user = writable<WeathermanUser>(
	new WeathermanUser('', '', 'en-US', 'en-US', Variables.urls.defaultProfileImageUrl)
);
export const alertState = writable<AlertState>(new AlertState(AlertStatus.Hidden, null, []));
export const forecast = writable<Forecast>(new Forecast(null, null, null, null, null, null, [], []));
