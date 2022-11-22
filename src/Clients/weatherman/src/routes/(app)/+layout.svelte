<script lang="ts">
	import '../../app.css';

	export const ssr = false;
	export const prerender = true;

	import { onMount, onDestroy } from 'svelte/internal';

	import { UsersServiceBase } from '../../../../shared2/services/usersServiceBase';
	import { AuthService } from '../../../../shared2/services/authService';
	import Alert from '../../../../shared2/components/Alert.svelte';

	import { t } from '$lib/localization/i18n';
	import { isOffline, user, forecast } from '$lib/stores';
	import { ForecastsService } from '$lib/services/forecastsService';
	import type { WeathermanUser } from '$lib/models/weathermanUser';
	import Variables from '$lib/variables';

	let usersService: UsersServiceBase;
	let forecastsService: ForecastsService;

	function loadUser() {
		const cachedUser = usersService.getFromCache();
		if (cachedUser) {
			user.set(cachedUser);
		}

		usersService.get<WeathermanUser>().then((currentUser) => {
			user.set(currentUser);
			usersService.cache(currentUser);

			forecastsService.get(currentUser.culture);
		});
	}

	onMount(async () => {
		const authService = new AuthService();
		await authService.initialize(Variables.urls.gateway);

		if (await authService.authenticated()) {
			await authService.setToken();
		} else {
			await authService.signinRedirect();
			return;
		}

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

		document.addEventListener(
			'visibilitychange',
			() => {
				if (document.hidden || $forecast === null || $forecast.lastRetrieved === null) {
					return;
				}

				const fifteenMinutesAgo = Date.now() - 900000;
				if ($forecast.lastRetrieved < fifteenMinutesAgo) {
					forecastsService.get($user.culture);
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
