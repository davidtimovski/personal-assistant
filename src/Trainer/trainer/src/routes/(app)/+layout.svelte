<script lang="ts">
	import '../../app.css';

	import { onMount, onDestroy } from 'svelte';

	import { UsersServiceBase } from '../../../../../Core/shared2/services/usersServiceBase';
	import { AuthService } from '../../../../../Core/shared2/services/authService';
	import Alert from '../../../../../Core/shared2/components/Alert.svelte';

	import { t } from '$lib/localization/i18n';
	import { isOffline, user } from '$lib/stores';
	import type { TrainerUser } from '$lib/models/trainerUser';

	let usersService: UsersServiceBase;
	//let forecastsService: ForecastsService;

	function loadUser() {
		const cachedUser = usersService.getFromCache();
		if (cachedUser) {
			user.set(cachedUser);
		}

		usersService.get<TrainerUser>().then((currentUser) => {
			user.set(currentUser);
			usersService.cache(currentUser);
		});
	}

	onMount(async () => {
		const authService = new AuthService();

		if (!(await authService.authenticated())) {
			await authService.signinRedirect();
			return;
		}

		await authService.silentLogin();

		usersService = new UsersServiceBase('Trainer');
		loadUser();

		isOffline.set(!navigator.onLine);
		window.addEventListener('online', () => {
			isOffline.set(false);
		});
		window.addEventListener('offline', () => {
			isOffline.set(true);
		});
	});

	onDestroy(() => {
		usersService?.release();
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
