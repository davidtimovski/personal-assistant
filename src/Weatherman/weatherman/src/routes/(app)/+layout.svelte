<script lang="ts">
	import '../../app.css';

	import { onMount, onDestroy } from 'svelte';

	import { UsersServiceBase } from '../../../../../Core/shared2/services/usersServiceBase';
	import { AuthService } from '../../../../../Core/shared2/services/authService';
	import Alert from '../../../../../Core/shared2/components/Alert.svelte';

	import { t } from '$lib/localization/i18n';
	import { isOffline, user, forecast } from '$lib/stores';
	import { ForecastsService } from '$lib/services/forecastsService';
	import type { WeathermanUser } from '$lib/models/weathermanUser';

	let usersService: UsersServiceBase;
	let forecastsService: ForecastsService;

	const now = new Date();

	function loadUser() {
		const cachedUser = usersService.getFromCache();
		if (cachedUser) {
			user.set(cachedUser);
			forecastsService.setPlaceholder(now, cachedUser.language, cachedUser.culture);
		}

		usersService.get<WeathermanUser>().then((currentUser) => {
			user.set(currentUser);
			usersService.cache(currentUser);

			forecastsService.get(now, currentUser.language, currentUser.culture);
		});
	}

	onMount(async () => {
		const authService = new AuthService();

		if (!(await authService.authenticated())) {
			await authService.signinRedirect();
			return;
		}

		await authService.silentLogin();

		usersService = new UsersServiceBase('Weatherman');
		forecastsService = new ForecastsService();
		loadUser();

		isOffline.set(!navigator.onLine);
		window.addEventListener('online', () => {
			isOffline.set(false);
		});
		window.addEventListener('offline', () => {
			isOffline.set(true);
		});

		// Reload weather data if app reopened after 15 or more minutes
		document.addEventListener(
			'visibilitychange',
			() => {
				if (document.hidden || $forecast === null || $forecast.lastRetrieved === null) {
					return;
				}

				const fifteenMinutesAgo = Date.now() - 900000;
				if ($forecast.lastRetrieved < fifteenMinutesAgo) {
					forecastsService.get(new Date(), $user.language, $user.culture);
				}
			},
			false
		);
	});

	onDestroy(() => {
		usersService?.release();
		forecastsService?.release();
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
