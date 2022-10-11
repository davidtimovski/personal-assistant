<script lang="ts">
	import '../../app.css';

	export const ssr = false;
	export const prerender = true;

	import { onMount, onDestroy } from 'svelte/internal';
	import { page } from '$app/stores';
	import type { Unsubscriber } from 'svelte/store';

	import { Language } from '../../../../shared2/models/enums/language';
	import { AuthService } from '../../../../shared2/services/authService';
	import { CurrenciesService } from '../../../../shared2/services/currenciesService';

	import { LocalStorageUtil, LocalStorageKeys } from '$lib/utils/localStorageUtil';
	import { SyncService } from '$lib/services/syncService';
	import { locale, isOnline, loggedInUser, syncStatus } from '$lib/stores';
	import { AppEvents } from '$lib/models/appEvents';

	import Alert from '$lib/components/Alert.svelte';

	let localStorage: LocalStorageUtil;
	const unsubscriptions: Unsubscriber[] = [];

	let syncService: SyncService;
	let currenciesService: CurrenciesService;

	unsubscriptions.push(
		loggedInUser.subscribe((value) => {
			if (!value) {
				return;
			}

			sync();
		})
	);

	function sync() {
		if (!navigator.onLine) {
			return;
		}

		syncStatus.set(AppEvents.SyncStarted);

		syncService = new SyncService();
		const syncPromises = new Array<Promise<any>>();

		const lastSynced = localStorage.get(LocalStorageKeys.LastSynced);
		const syncPromise = syncService.sync(lastSynced).then((lastSyncedServer: string) => {
			localStorage.set(LocalStorageKeys.LastSynced, lastSyncedServer);
		});
		syncPromises.push(syncPromise);

		currenciesService = new CurrenciesService('Accountant', 'accountant2');
		const ratesPromise = currenciesService.loadRates();
		syncPromises.push(ratesPromise);

		Promise.all(syncPromises).then(() => {
			syncStatus.set(AppEvents.SyncFinished);
		});
	}

	onMount(() => {
		localStorage = new LocalStorageUtil();

		const lang = $page.url.searchParams.get('lang');
		if (lang && (lang === Language.English || lang === Language.Macedonian)) {
			localStorage.set('language', lang);
		}
		locale.set(localStorage.get('language'));

		new AuthService('accountant2').login();

		isOnline.set(navigator.onLine);
		window.addEventListener('online', () => {
			sync();
			isOnline.set(true);
		});
		window.addEventListener('offline', () => {
			isOnline.set(false);
		});

		unsubscriptions.push(
			syncStatus.subscribe((value) => {
				if (value === AppEvents.ReSync) {
					sync();
				}
			})
		);
	});

	onDestroy(() => {
		for (const unsubscribe of unsubscriptions) {
			unsubscribe();
		}
		syncService?.release();
		currenciesService?.release();
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
