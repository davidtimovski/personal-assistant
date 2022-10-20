<script lang="ts">
	import '../../app.css';

	export const ssr = false;
	export const prerender = true;

	import { onMount, onDestroy } from 'svelte/internal';
	import { page } from '$app/stores';

	import { Language } from '../../../../shared2/models/enums/language';
	import { AuthService } from '../../../../shared2/services/authService';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { locale, isOffline, authInfo } from '$lib/stores';
	import { ListsService } from '$lib/services/listsService';
	import { SignalRClient } from '$lib/utils/signalRClient';

	import Alert from '$lib/components/Alert.svelte';

	let listsService: ListsService;

	const authInfoUnsub = authInfo.subscribe(async (value) => {
		if (!value) {
			return;
		}

		listsService = new ListsService();
		await listsService.getAll(true);

		await new SignalRClient(listsService).initialize(value.token, value.profile.sub);
	});

	onMount(async () => {
		const localStorage = new LocalStorageUtil();

		const lang = $page.url.searchParams.get('lang');
		if (lang && (lang === Language.English || lang === Language.Macedonian)) {
			localStorage.set('language', lang);
		}
		locale.set(localStorage.get('language'));

		const authService = new AuthService();
		await authService.initialize();

		if (await authService.authenticated()) {
			await authService.setToken();
		} else {
			await authService.signinRedirect();
			return;
		}

		isOffline.set(!navigator.onLine);
		window.addEventListener('online', () => {
			isOffline.set(false);
		});
		window.addEventListener('offline', () => {
			isOffline.set(true);
		});
	});

	onDestroy(() => {
		authInfoUnsub();
		listsService?.release();
	});
</script>

<main>
	<div />
	<div class="center">
		<slot />
		<Alert />
	</div>
	<div />

	<div class="connection-warning-overlay" class:visible={$isOffline}>
		<div class="connection-warning">
			<i class="fas fa-wifi" />
			<br />
			<span>{$t('waitingForConnection')}</span>
		</div>
	</div>
</main>

<style lang="scss">
	.connection-warning-overlay {
		display: none;
		position: fixed;
		z-index: 2;
		width: 100%;
		height: 100%;
		background: rgba(0, 0, 0, 0.7);

		&.visible {
			display: block;
		}

		.connection-warning {
			padding: 0 25px;
			margin-top: 190px;
			font-size: 2rem;
			line-height: 2.5rem;
			text-align: center;
			color: #fafafa;
			user-select: none;
		}
		.connection-warning i {
			margin-bottom: 15px;
			animation: flashColor 1.5s infinite;
		}
	}
</style>
