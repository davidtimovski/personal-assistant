<script lang="ts">
	import '../../app.css';

	export const ssr = false;
	export const prerender = true;

	import { onMount } from 'svelte/internal';

	import { AuthService } from '../../../../shared2/services/authService';
	import { CurrenciesService } from '../../../../shared2/services/currenciesService';

	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { locale } from '$lib/localization/i18n';
	import { SyncService } from '$lib/services/syncService';
	import { isOnline, loggedInUser, syncStatus } from '$lib/stores';
	import { AppEvents } from '$lib/models/appEvents';

	import Alert from '$lib/components/Alert.svelte';

	function sync(localStorage: LocalStorageUtil) {
		if (!navigator.onLine) {
			return;
		}

		syncStatus.set(AppEvents.SyncStarted);

		const syncService = new SyncService();
		const syncPromises = new Array<Promise<any>>();

		const lastSynced = localStorage.get(LocalStorageKeys.LastSynced);
		const syncPromise = syncService.sync(lastSynced).then((lastSyncedServer: string) => {
			localStorage.set(LocalStorageKeys.LastSynced, lastSyncedServer);
		});
		syncPromises.push(syncPromise);

		const currenciesService = new CurrenciesService('Accountant');
		const ratesPromise = currenciesService.loadRates();
		syncPromises.push(ratesPromise);

		Promise.all(syncPromises).then(() => {
			syncStatus.set(AppEvents.SyncFinished);
		});
	}

	onMount(() => {
		const localStorage = new LocalStorageUtil();
		locale.set(localStorage.get('language'));

		new AuthService(window).login();

		loggedInUser.subscribe((value) => {
			if (!value) {
				return;
			}

			sync(localStorage);
		});

		isOnline.set(navigator.onLine);
		window.addEventListener('online', () => {
			sync(localStorage);
			isOnline.set(true);
		});
		window.addEventListener('offline', () => {
			isOnline.set(false);
		});

		syncStatus.subscribe((value) => {
			if (value === AppEvents.ReSync) {
				sync(localStorage);
			}
		});
	});
</script>

<main>
	<div />
	<div class="center">
		<slot />
		<Alert />
	</div>
	<div />
</main>
