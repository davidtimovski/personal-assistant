<script lang="ts">
	import { onMount, onDestroy } from 'svelte';
	import { goto } from '$app/navigation';

	import { AuthService } from '../../../../../../Core/shared2/services/authService';

	import { t } from '$lib/localization/i18n';
	import { LocalStorageUtil } from '$lib/utils/localStorageUtil';
	import { RecipesService } from '$lib/services/recipesService';
	import Variables from '$lib/variables';

	let pendingShareRequestCount = 0;
	let preferencesButtonIsLoading = $state(false);
	const personalAssistantUrl = Variables.urls.account;
	let version = $state('--');

	let localStorage: LocalStorageUtil;
	let recipesService: RecipesService;

	async function goToPreferences() {
		preferencesButtonIsLoading = true;
		await goto('/preferences');
	}

	async function logOut() {
		localStorage.clear();

		const authService = new AuthService();
		await authService.logout();
	}

	onMount(async () => {
		localStorage = new LocalStorageUtil();
		recipesService = new RecipesService();

		recipesService.getPendingShareRequestsCount().then((pending) => {
			pendingShareRequestCount = pending;
		});

		caches.keys().then((cacheNames) => {
			if (cacheNames.length > 0) {
				version = cacheNames.sort().reverse()[0];
			}
		});
	});

	onDestroy(() => {
		recipesService?.release();
	});
</script>

<section class="container">
	<div class="page-title-wrap">
		<div class="side inactive">
			<i class="fas fa-bars"></i>
		</div>
		<div class="page-title">{$t('menu.menu')}</div>
		<a href="/recipes" class="back-button">
			<i class="fas fa-times"></i>
		</a>
	</div>

	<div class="content-wrap">
		<div class="horizontal-buttons-wrap">
			<!-- <a href="/archivedLists" class="wide-button">{$t('menu.archivedLists')}</a> -->

			<!-- <a href="/shareRequests" class="wide-button with-badge">
				<span>{$t('menu.shareRequests')}</span>
				{#if pendingShareRequestCount > 0}
					<span class="badge">{pendingShareRequestCount}</span>
				{/if}
			</a> -->

			<button type="button" onclick={goToPreferences} class="wide-button with-badge">
				<span class="button-loader" class:loading={preferencesButtonIsLoading}>
					<i class="fas fa-circle-notch fa-spin"></i>
				</span>
				<span>{$t('menu.preferences')}</span>
			</button>

			<a href="/help" class="wide-button">{$t('menu.help')}</a>
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
