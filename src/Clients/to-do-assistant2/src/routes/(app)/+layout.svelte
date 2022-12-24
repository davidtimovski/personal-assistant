<script lang="ts">
	import '../../app.css';

	export const ssr = false;
	export const prerender = true;

	import { onMount, onDestroy } from 'svelte/internal';

	import { AuthService } from '../../../../shared2/services/authService';
	import Alert from '../../../../shared2/components/Alert.svelte';

	import { t } from '$lib/localization/i18n';
	import { isOffline, authInfo, user } from '$lib/stores';
	import { UsersService } from '$lib/services/usersService';
	import { ListsService } from '$lib/services/listsService';
	import { SignalRClient } from '$lib/utils/signalRClient';
	import type { ToDoAssistantUser } from '$lib/models/toDoAssistantUser';

	let usersService: UsersService;
	let listsService: ListsService;

	const authInfoUnsub = authInfo.subscribe(async (value) => {
		if (!value) {
			return;
		}

		await new SignalRClient(listsService).initialize(value.token, value.profile.sub);
	});

	function loadUser() {
		const cachedUser = usersService.getFromCache<ToDoAssistantUser>();
		if (cachedUser) {
			user.set(cachedUser);
		}

		usersService.get<ToDoAssistantUser>().then((currentUser) => {
			user.set(currentUser);
			usersService.cache(currentUser);
		});
	}

	onMount(async () => {
		const authService = new AuthService();
		await authService.initialize();

		if (await authService.authenticated()) {
			await authService.setToken();
		} else {
			await authService.signinRedirect();
			return;
		}

		usersService = new UsersService();
		loadUser();

		listsService = new ListsService();
		listsService.getAll(true);

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
		usersService?.release();
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
