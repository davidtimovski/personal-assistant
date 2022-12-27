<script lang="ts">
	import { onMount } from 'svelte/internal';
	import { goto } from '$app/navigation';

	import { AuthService } from '../../../../../shared2/services/authService';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import Variables from '$lib/variables';

	let preferencesButtonIsLoading = false;
	const personalAssistantUrl = Variables.urls.account;
	let version = '--';

	let localStorage: LocalStorageUtil;

	async function goToPreferences() {
		preferencesButtonIsLoading = true;
		await goto('/preferences');
	}

	async function logOut() {
		localStorage.clear();

		const authService = new AuthService();
		await authService.initialize();
		await authService.logout();
	}

	onMount(async () => {
		localStorage = new LocalStorageUtil();

		caches.keys().then((cacheNames) => {
			if (cacheNames.length > 0) {
				version = cacheNames.sort().reverse()[0];
			}
		});
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive">
			<i class="fas fa-bars" />
		</div>
		<div class="page-title">{$t('menu.menu')}</div>
		<a href="/" class="back-button">
			<i class="fas fa-times" />
		</a>
	</div>

	<div class="content-wrap">
		<div class="horizontal-buttons-wrap">
			<button type="button" on:click={goToPreferences} class="wide-button with-badge">
				<span class="button-loader" class:loading={preferencesButtonIsLoading}>
					<i class="fas fa-circle-notch fa-spin" />
				</span>
				<span>{$t('menu.preferences')}</span>
			</button>
		</div>

		<hr />

		<div class="horizontal-buttons-wrap">
			<a href={personalAssistantUrl} class="wide-button">{$t('menu.goToPersonalAssistant')}</a>
			<button type="button" on:click={logOut} class="wide-button">{$t('menu.logOut')}</button>
		</div>

		<hr />

		<div class="version"><span>{$t('menu.version')}</span> {version}</div>
	</div>
</section>
