<script lang="ts">
	import '../../app.css';

	export const ssr = false;
	export const prerender = true;

	import { onMount } from 'svelte/internal';
	import type { User } from 'oidc-client';

	import { AuthService } from '../../../../shared2/services/authService';

	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { locale } from '$lib/localization/i18n';
	import { isOnline, loggedInUser } from '$lib/stores';
	import { ListsService } from '$lib/services/listsService';
	import { SignalRClient } from '$lib/utils/signalRClient';

	import Alert from '$lib/components/Alert.svelte';

	let listsService: ListsService;
	let signalRClient: SignalRClient;
	let user: User | null = null;

	onMount(() => {
		const localStorage = new LocalStorageUtil();
		locale.set(localStorage.get('language'));

		listsService = new ListsService();
		signalRClient = new SignalRClient();

		loggedInUser.subscribe(async (value) => {
			if (!value) {
				return;
			}

			user = value;

			await listsService.getAll();

			signalRClient.initialize(user.access_token, parseInt(user.profile.sub, 10));
		});

		new AuthService('to-do-assistant2', window).login();

		isOnline.set(navigator.onLine);
		window.addEventListener('online', () => {
			isOnline.set(true);
		});
		window.addEventListener('offline', () => {
			isOnline.set(false);
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
