<script lang="ts">
	import '../../app.css';

	export const ssr = false;
	export const prerender = true;

	import { onMount } from 'svelte/internal';

	import { AuthService } from '../../../../shared2/services/authService';

	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { locale } from '$lib/localization/i18n';
	import { isOnline, loggedInUser } from '$lib/stores';
	import { AppEvents } from '$lib/models/appEvents';

	import Alert from '$lib/components/Alert.svelte';

	onMount(() => {
		const localStorage = new LocalStorageUtil();
		locale.set(localStorage.get('language'));

		new AuthService('to-do-assitant2', window).login();

		loggedInUser.subscribe((value) => {
			if (!value) {
				return;
			}
		});

		isOnline.set(navigator.onLine);
		window.addEventListener('online', () => {
			isOnline.set(true);
		});
		window.addEventListener('offline', () => {
			isOnline.set(false);
		});

		// syncStatus.subscribe((value) => {
		// 	if (value === AppEvents.ReSync) {
		// 		sync(localStorage);
		// 	}
		// });
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
