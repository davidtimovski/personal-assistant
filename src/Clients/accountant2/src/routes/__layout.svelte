<script lang="ts">
	import { onMount } from 'svelte/internal';
	import '../app.css';
	import { AuthService } from '../../../shared2/services/authService';
	import { CurrenciesService } from '../../../shared2/services/currenciesService';
	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { locale } from '$lib/localization/i18n';
	import { SyncService } from '$lib/services/syncService';
	import { loggedInUser, syncStatus } from '$lib/stores';
	import { AppEvents } from '$lib/models/appEvents';

	function sync(localStorage: LocalStorageUtil) {
		if (!navigator.onLine) {
			return;
		}

		syncStatus.set(AppEvents.SyncStarted);

		const syncService = new SyncService();
		const syncPromises = new Array<Promise<any>>();

		const lastSynced = localStorage.get('lastSynced');
		const syncPromise = syncService.sync(lastSynced);
		syncPromises.push(syncPromise);
		syncPromise.then((lastSyncedServer: string) => {
			localStorage.set('lastSynced', lastSyncedServer);
		});

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

		window.addEventListener('online', () => {
			sync(localStorage);
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
		<!-- <alert></alert> -->
	</div>
	<div />
</main>

<!-- 
<Header /> -->
<style>
</style>
