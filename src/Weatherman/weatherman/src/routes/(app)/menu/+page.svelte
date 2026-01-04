<script lang="ts">
	import { onMount } from 'svelte';
	import { goto } from '$app/navigation';

	import { AuthService } from '../../../../../../Core/shared2/services/authService';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import Variables from '$lib/variables';

	let preferencesButtonIsLoading = $state(false);
	const personalAssistantUrl = Variables.urls.account;
	let version = $state('--');

	let localStorage: LocalStorageUtil;

	async function goToPreferences() {
		preferencesButtonIsLoading = true;
		await goto('/preferences');
	}

	async function logOut() {
		localStorage.clear();

		const authService = new AuthService();
		await authService.logout();
	}

	onMount(() => {
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
			<i class="fas fa-bars"></i>
		</div>
		<div class="page-title">{$t('menu.menu')}</div>
		<a href="/weather" class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="content-wrap">
		<div class="horizontal-buttons-wrap">
			<button type="button" onclick={goToPreferences} class="wide-button with-badge">
				<span class="button-loader" class:loading={preferencesButtonIsLoading}>
					<i class="fas fa-circle-notch fa-spin"></i>
				</span>
				<span>{$t('menu.preferences')}</span>
			</button>
		</div>

		<hr />

		<div class="horizontal-buttons-wrap">
			<a href={personalAssistantUrl} class="wide-button">{$t('menu.goToPersonalAssistant')}</a>
			<button type="button" onclick={logOut} class="wide-button">{$t('menu.logOut')}</button>
		</div>

		<hr />

		<div class="version"><span>{$t('menu.version')}</span> {version}</div>
	</div>
</section>
