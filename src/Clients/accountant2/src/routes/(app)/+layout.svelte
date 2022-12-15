<script lang="ts">
	import '../../app.css';

	export const ssr = false;
	export const prerender = true;

	import { onMount, onDestroy } from 'svelte/internal';
	import type { Unsubscriber } from 'svelte/store';

	import { AuthService } from '../../../../shared2/services/authService';
	import { UsersServiceBase } from '../../../../shared2/services/usersServiceBase';
	import Alert from '../../../../shared2/components/Alert.svelte';

	import { SyncService } from '$lib/services/syncService';
	import { user, isOnline, syncStatus } from '$lib/stores';
	import type { AccountantUser } from '$lib/models/accountantUser';
	import { AppEvents } from '$lib/models/appEvents';

	let usersService: UsersServiceBase;
	let syncService: SyncService;

	const unsubscriptions: Unsubscriber[] = [];

	function loadUser() {
		const cachedUser = usersService.getFromCache();
		if (cachedUser) {
			user.set(cachedUser);
		}

		usersService.get<AccountantUser>().then((currentUser) => {
			user.set(currentUser);
			usersService.cache(currentUser);
		});
	}

	onMount(async () => {
		if (navigator.onLine) {
			const authService = new AuthService();
			await authService.initialize();

			if (await authService.authenticated()) {
				await authService.setToken();
			} else {
				await authService.signinRedirect();
				return;
			}

			usersService = new UsersServiceBase('Accountant');
			syncService = new SyncService();

			loadUser();
			syncService.sync();
		}

		isOnline.set(navigator.onLine);
		window.addEventListener('online', () => {
			syncService.sync();
			isOnline.set(true);
		});
		window.addEventListener('offline', () => {
			isOnline.set(false);
		});

		unsubscriptions.push(
			syncStatus.subscribe((value) => {
				if (value === AppEvents.ReSync) {
					syncService.sync();
				}
			})
		);
	});

	onDestroy(() => {
		for (const unsubscribe of unsubscriptions) {
			unsubscribe();
		}
		usersService?.release();
		syncService?.release();
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
